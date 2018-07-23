using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Huygens.Internal
{
    /// <summary>
    /// Connection to memory structures.
    /// </summary>
    [Serializable]
    public class MemoryConnection : MarshalByRefObject, IConnection
    {
        private readonly SerialisableRequest _request;
        private readonly MemoryStream _resultBody;
        private readonly SerialisableResponse _result;
        private bool _headersRead;

        /// <summary>
        /// Create a connection for a single serialised request
        /// </summary>
        public MemoryConnection(SerialisableRequest request)
        {
            _request = request;
            if (_request.Headers == null) _request.Headers = new Dictionary<string, string>();

            _headersRead = false;

            Id = Guid.NewGuid();
            LocalIP = "0.0.0.0";
            RemoteIP = "*";
            RequestLog = new LogInfo();
            ResponseLog = new LogInfo();

            _resultBody = new MemoryStream();
            _result = new SerialisableResponse{
                Headers = new Dictionary<string, string[]>(),
                StatusCode = 200,
                StatusMessage = "OK"
            };
        }


        /// <summary>
        /// Set this header, clearing any existing values
        /// </summary>
        private void SetHeader(string key, string value) {
            if (_result.Headers.ContainsKey(key)) _result.Headers[key] = new []{ value };
            else _result.Headers.Add(key, new []{ value });
        }

        /// <summary>
        /// Set this header, Appending to any existing values
        /// </summary>
        private void AddHeader(string key, string value) {
            if (_result.Headers.ContainsKey(key)) _result.Headers[key] = _result.Headers[key].Concat(new []{ value }).ToArray();
            else _result.Headers.Add(key, new []{ value });
        }

        /// <summary>
        /// Create a serialised response after processing a request
        /// </summary>
        public SerialisableResponse GenerateResponse()
        {
            _result.Content = _resultBody.ToArray();
            return _result;
        }

        /// <inheritdoc />
        public bool Connected { get{return true; } }
        /// <inheritdoc />
        public Guid Id { get; set; }
        /// <inheritdoc />
        public string LocalIP { get; set; }
        /// <inheritdoc />
        public string RemoteIP { get; set; }
        /// <inheritdoc />
        public LogInfo RequestLog { get;set; }
        /// <inheritdoc />
        public LogInfo ResponseLog { get;set; }
        /// <inheritdoc />
        public void Close() { }

        /// <summary>
        /// Lifetime service
        /// </summary>
        public override object InitializeLifetimeService()
        {
            return null;
        }

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
            if (!_headersRead) {
                _headersRead = true;
                return RequestHeaderBytes();
            }
            return _request.Content ?? new byte[0];
        }
        
        private byte[] RequestHeaderBytes()
        {
            var sb = new StringBuilder();
            //GET https://www.fiddler2.com/UpdateCheck.aspx?isBeta=False HTTP/1.1
            sb.Append(_request.Method);
            sb.Append(' ');
            sb.Append(_request.RequestUri);
            sb.Append(" HTTP/1.1\r\n");
            foreach (var header in _request.Headers)
            {
                sb.Append(header.Key);
                sb.Append(": ");
                sb.Append(header.Value);
                sb.Append("\r\n");
            }
            sb.Append("\r\n");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        /// <inheritdoc />
        public int WaitForRequestBytes()
        {
            return _request.Content.Length;
        }

        /// <inheritdoc />
        public void Write100Continue()
        {
            WriteEntireResponseFromString(100, null, null, true);
        }

        /// <inheritdoc />
        public void WriteBody(byte[] data, int offset, int length)
        {
            _resultBody.Write(data, offset, length);
        }

        /// <inheritdoc />
        public void WriteEntireResponseFromFile(string fileName, bool keepAlive)
        {
            if (!File.Exists(fileName))
            {
                WriteErrorAndClose(404);
                return;
            }

            // Deny the request if the contentType cannot be recognized.

            SetHeader("Content-Type", NetworkUtils.GetContentType(fileName));

            bool completed = false;
            FileStream fs = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                int len = (int) fs.Length;
                byte[] fileBytes = new byte[len];
                int bytesRead = fs.Read(fileBytes, 0, len);

                MakeResponseHeaders(200, null, bytesRead, keepAlive);
                ResponseLog.Headers = StringifyResponseHeaders();
                ResponseLog.StatusCode = 200;

                _resultBody.Write(fileBytes, 0, bytesRead);

                completed = true;
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

        private string StringifyResponseHeaders()
        {
            // read the dictionary into a string
            var sb = new StringBuilder();
            foreach (var header in _result.Headers)
            {
                sb.Append(header.Key);
                sb.Append(": ");
                sb.Append(string.Join(", ", header.Value));
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        /// <inheritdoc />
        public void WriteEntireResponseFromString(int statusCode, IDictionary<string, string> extraHeaders, string body, bool keepAlive)
        {
            var buf = Encoding.UTF8.GetBytes(body ?? "");

            MakeResponseHeaders(statusCode, extraHeaders, buf.Length, keepAlive);

            ResponseLog.Headers = StringifyResponseHeaders();
            ResponseLog.StatusCode = statusCode;
            _resultBody.Write(buf, 0, buf.Length);
        }

        /// <inheritdoc />
        public void WriteErrorAndClose(int statusCode)
        {
            WriteErrorAndClose(statusCode, null);
        }

        /// <inheritdoc />
        public void WriteErrorAndClose(int statusCode, string message)
        {
            WriteEntireResponseFromString(statusCode, null, GetErrorResponseBody(statusCode, message), false);
        }

        /// <inheritdoc />
        public void WriteErrorWithExtraHeadersAndKeepAlive(int statusCode, IDictionary<string, string> extraHeaders)
        {
            WriteEntireResponseFromString(statusCode, extraHeaders, GetErrorResponseBody(statusCode, null), true);
        }

        /// <inheritdoc />
        public void WriteHeaders(int statusCode, IDictionary<string, string> extraHeaders)
        {
            MakeResponseHeaders(statusCode, extraHeaders, -1, false);
            
            ResponseLog.Headers = StringifyResponseHeaders();
            ResponseLog.StatusCode = statusCode;
        }
        
        
        private string GetErrorResponseBody(int statusCode, string message)
        {
            string body = Messages.FormatErrorMessageBody(statusCode, "/");

            if (!string.IsNullOrEmpty(message))
            {
                body += "\r\n<!--\r\n" + message + "\r\n-->";
            }

            return body;
        }

        private void MakeResponseHeaders(int statusCode, IDictionary<string, string> extraHeaders, int contentLength, bool keepAlive)
        {
            _result.StatusCode = statusCode;
            _result.StatusMessage = HttpWorkerRequest.GetStatusDescription(statusCode);
            AddHeader("Date", DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo));

            if (contentLength >= 0)
            {
                SetHeader("Content-Length", contentLength.ToString());
            }

            if (extraHeaders != null) {
                foreach (var header in extraHeaders)
                {
                    AddHeader(header.Key, header.Value);
                }
            }

            if (!keepAlive)
            {
                SetHeader("Connection", "Close");
            }
        }
    }
}