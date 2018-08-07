using System;
using System.Collections.Generic;

namespace Huygens
{
    /// <summary>
    /// A fully-encapsulated HTTP response that can cross AppDomain boundaries
    /// </summary>
    [Serializable]
    public class SerialisableResponse : MarshalByRefObject
    {
        /// <summary>
        /// Binary response from website
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Status header message from server
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Headers returned from server
        /// </summary>
        public Dictionary<string, string[]> Headers { get; set; }

        /// <summary>
        /// Create a new empty response object
        /// </summary>
        public static SerialisableResponse CreateEmpty()
        {
            return new SerialisableResponse{
                Headers = new Dictionary<string, string[]>(),
                Content = null,
                StatusCode = 200,
                StatusMessage = "OK"
            };
        }
    }
}