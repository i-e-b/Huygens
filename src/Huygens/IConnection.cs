using System;
using System.Collections.Generic;
using Huygens.Internal;

namespace Huygens
{
    /// <summary>
    /// Interface for exposing hosted site to an external connection (e.g. IP Socket or proxy)
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Is the connection active (always true for non-network connections)
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// Unique ID for the connection
        /// </summary>
        Guid Id { get; }
        /// <summary>
        /// Local listening IP
        /// </summary>
        string LocalIP { get; }
        /// <summary>
        /// Remote connecting IP, if any
        /// </summary>
        string RemoteIP { get; }
        /// <summary>
        /// Log of requests made with this connection
        /// </summary>
        LogInfo RequestLog { get; }
        /// <summary>
        /// Log of responses given on this connection
        /// </summary>
        LogInfo ResponseLog { get; }

        /// <summary>
        /// Close connection
        /// </summary>
        void Close();

        /// <summary>
        /// Add requet to log
        /// </summary>
        void LogRequest(string pathTranslated, string url);
        /// <summary>
        /// Add body to log
        /// </summary>
        void LogRequestBody(byte[] content);
        /// <summary>
        /// Add headers to log
        /// </summary>
        void LogRequestHeaders(string headers);

        /// <summary>
        /// Read bytes from the incoming request (including http query and headers)
        /// </summary>
        /// <param name="maxBytes">Maximum size for network transfer. Ignored for memory connections</param>
        byte[] ReadRequestBytes(int maxBytes);
        /// <summary>
        /// Wait until bytes are ready, and return buffer size
        /// </summary>
        int WaitForRequestBytes();

        /// <summary>
        /// Write a 100 continue message to the response
        /// </summary>
        void Write100Continue();

        /// <summary>
        /// Write data to the response body
        /// </summary>
        void WriteBody(byte[] data, int offset, int length);

        /// <summary>
        /// Send a file to the response
        /// </summary>
        void WriteEntireResponseFromFile(string fileName, bool keepAlive);


        /// <summary>
        /// Write a string to the response
        /// </summary>
        void WriteEntireResponseFromString(int statusCode, IDictionary<string, string> extraHeaders, string body, bool keepAlive);
        /// <summary>
        /// Write an error to the response
        /// </summary>
        void WriteErrorAndClose(int statusCode);
        /// <summary>
        /// Write an error to the response
        /// </summary>
        void WriteErrorAndClose(int statusCode, string message);
        /// <summary>
        /// Write an error to the response
        /// </summary>
        void WriteErrorWithExtraHeadersAndKeepAlive(int statusCode, IDictionary<string, string> extraHeaders);
        /// <summary>
        /// Output headers to the response
        /// </summary>
        void WriteHeaders(int statusCode, IDictionary<string, string> extraHeaders, string responseStatusString);
    }
}