//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace Huygens.Internal
{
    /// <summary>
    /// Communicate with the outside world
    /// </summary>
    public class SocketConnection : MarshalByRefObject, IConnection
    {
        private const int HttpOK = 200;

        private readonly MemoryStream _responseContent;
        private readonly GenericServer _server;
        private Socket _socket;

        internal SocketConnection(GenericServer server, Socket socket)
        {
            Id = Guid.NewGuid();
            _responseContent = new MemoryStream();
            _server = server;
            _socket = socket;
            InitializeLogInfo();
        }

        /// <summary>
        /// True if IP socket has a connected state
        /// </summary>
        public bool Connected
        {
            get { return _socket.Connected; }
        }

        /// <summary>
        /// Unique id
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Local listener IP address
        /// </summary>
        public string LocalIP
        {
            get
            {
                IPEndPoint ep = (IPEndPoint) _socket.LocalEndPoint;
                return (ep != null && ep.Address != null) ? ep.Address.ToString() : "127.0.0.1";
            }
        }

        /// <inheritdoc />
        public string RemoteIP
        {
            get
            {
                IPEndPoint ep = (IPEndPoint) _socket.RemoteEndPoint;
                return (ep != null && ep.Address != null) ? ep.Address.ToString() : "127.0.0.1";
            }
        }

        /// <inheritdoc />
        public LogInfo RequestLog { get; private set; }

        /// <inheritdoc />
        public LogInfo ResponseLog { get; private set; }
        
        /// <inheritdoc />
        public void Close()
        {
            FinalizeLogInfo();

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }
            finally
            {
                _socket = null;
            }
        }
        
        /// <inheritdoc />
        public override object InitializeLifetimeService() { return null; }
        
        /// <inheritdoc />
        public void LogRequest(string pathTranslated, string url)
        {
            RequestLog.PathTranslated = pathTranslated;

            RequestLog.Url = url;
        }
        
        /// <inheritdoc />
        public void LogRequestBody(byte[] content)
        {
            RequestLog.Body = content;
        }

        /// <inheritdoc />
        public void LogRequestHeaders(string headers)
        {
            RequestLog.Headers = headers;
        }
        
        /// <inheritdoc />
        public byte[] ReadRequestBytes(int maxBytes)
        {
            try
            {
                if (WaitForRequestBytes() == 0)
                {
                    return null;
                }

                int numBytes = _socket.Available;

                if (numBytes > maxBytes)
                {
                    numBytes = maxBytes;
                }

                int numReceived = 0;

                byte[] buffer = new byte[numBytes];

                if (numBytes > 0)
                {
                    numReceived = _socket.Receive(buffer, 0, numBytes, SocketFlags.None);
                }

                if (numReceived < numBytes)
                {
                    byte[] tempBuffer = new byte[numReceived];

                    if (numReceived > 0)
                    {
                        Buffer.BlockCopy(buffer, 0, tempBuffer, 0, numReceived);
                    }

                    buffer = tempBuffer;
                }

                return buffer;
            }
            catch
            {
                return null;
            }
        }
        
        /// <inheritdoc />
        public int WaitForRequestBytes()
        {
            int availBytes = 0;

            try
            {
                if (_socket.Available == 0)
                {
                    _socket.Poll(100000, SelectMode.SelectRead);

                    if (_socket.Available == 0 && _socket.Connected)
                    {
                        _socket.Poll(30000000, SelectMode.SelectRead);
                    }
                }

                availBytes = _socket.Available;
            }
            catch { Ignore(); }

            return availBytes;
        }

        private void Ignore() { }
        
        /// <inheritdoc />
        public void Write100Continue()
        {
            WriteEntireResponseFromString(100, null, null, true);
        }

        internal void Write200Continue()
        {
            WriteEntireResponseFromString(200, null, string.Empty, true);
        }
        
        /// <inheritdoc />
        public void WriteBody(byte[] data, int offset, int length)
        {
            try
            {
                _responseContent.Write(data, 0, data.Length);
                _socket.Send(data, offset, length, SocketFlags.None);
            }
            catch (SocketException)
            {
            }
        }

        /// <inheritdoc />
        public void WriteEntireResponseFromFile(String fileName, bool keepAlive)
        {
            if (!File.Exists(fileName))
            {
                WriteErrorAndClose(404);
                return;
            }

            string contentType = NetworkUtils.GetContentType(fileName);

            var contentTypeHeader = new Dictionary<string, string>{
                    {"Content-Type", contentType}
            };

            bool completed = false;
            FileStream fs = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                int len = (int) fs.Length;
                byte[] fileBytes = new byte[len];
                int bytesRead = fs.Read(fileBytes, 0, len);

                String headers = MakeResponseHeaders(HttpOK, contentTypeHeader, bytesRead, keepAlive);
                ResponseLog.Headers = headers;
                ResponseLog.StatusCode = HttpOK;
                _socket.Send(Encoding.UTF8.GetBytes(headers));

                _socket.Send(fileBytes, 0, bytesRead, SocketFlags.None);

                completed = true;
            }
            catch (SocketException)
            {
            }
            finally
            {
                if (!keepAlive || !completed)
                {
                    Close();
                }

                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        /// <inheritdoc />
        public void WriteEntireResponseFromString(int statusCode, IDictionary<string, string> extraHeaders, String body, bool keepAlive)
        {
            try
            {
                int bodyLength = (body != null) ? Encoding.UTF8.GetByteCount(body) : 0;
                string headers = MakeResponseHeaders(statusCode, extraHeaders, bodyLength, keepAlive);

                ResponseLog.Headers = headers;
                ResponseLog.StatusCode = statusCode;
                _socket.Send(Encoding.UTF8.GetBytes(headers + body));
            }
            catch (SocketException)
            {
            }
            finally
            {
                if (!keepAlive)
                {
                    Close();
                }
            }
        }
        
        /// <inheritdoc />
        public void WriteErrorAndClose(int statusCode, string message)
        {
            WriteEntireResponseFromString(statusCode, null, GetErrorResponseBody(statusCode, message), false);
        }
        
        /// <inheritdoc />
        public void WriteErrorAndClose(int statusCode)
        {
            WriteErrorAndClose(statusCode, null);
        }
        
        /// <inheritdoc />
        public void WriteErrorWithExtraHeadersAndKeepAlive(int statusCode, IDictionary<string, string> extraHeaders)
        {
            WriteEntireResponseFromString(statusCode, extraHeaders, GetErrorResponseBody(statusCode, null), true);
        }
        
        /// <inheritdoc />
        public void WriteHeaders(int statusCode, IDictionary<string, string> extraHeaders)
        {
            string headers = MakeResponseHeaders(statusCode, extraHeaders, -1, false);

            ResponseLog.Headers = headers;
            ResponseLog.StatusCode = statusCode;

            try
            {
                _socket.Send(Encoding.UTF8.GetBytes(headers));
            }
            catch (SocketException)
            {
            }
        }


        private void FinalizeLogInfo()
        {
            try
            {
                ResponseLog.Body = _responseContent.ToArray();
                _responseContent.Dispose();
                ResponseLog.Created = DateTime.Now;
                ResponseLog.Url = RequestLog.Url;
                ResponseLog.PathTranslated = RequestLog.PathTranslated;
                ResponseLog.Identity = RequestLog.Identity;
                ResponseLog.PhysicalPath = RequestLog.PhysicalPath;
            }
            catch { Ignore(); }
        }

        private string GetErrorResponseBody(int statusCode, string message)
        {
            string body = Messages.FormatErrorMessageBody(statusCode, _server.VirtualPath);

            if (!string.IsNullOrEmpty(message))
            {
                body += "\r\n<!--\r\n" + message + "\r\n-->";
            }

            return body;
        }

        private void InitializeLogInfo()
        {
            RequestLog = new LogInfo
                {
                    Created = DateTime.Now,
                    ConversationId = Id,
                    RowType = 1,
                    Identity = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                    PhysicalPath = _server.PhysicalPath
                };

            ResponseLog = new LogInfo
                {
                    ConversationId = Id,
                    RowType = 2
                };
        }

        private static string MakeResponseHeaders(int statusCode, IDictionary<string, string> moreHeaders, int contentLength, bool keepAlive)
        {
            var sb = new StringBuilder();

            sb.Append("HTTP/1.1 " + statusCode + " " + HttpWorkerRequest.GetStatusDescription(statusCode) + "\r\n");
            sb.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo) + "\r\n");

            if (contentLength >= 0)
            {
                sb.Append("Content-Length: " + contentLength + "\r\n");
            }

            if (moreHeaders != null)
            {
                foreach (var header in moreHeaders)
                {
                    sb.Append(header.Key + ": " + header.Value + "\r\n");
                }
            }

            if (!keepAlive)
            {
                sb.Append("Connection: Close\r\n");
            }

            sb.Append("\r\n");

            return sb.ToString();
        }
    }
}