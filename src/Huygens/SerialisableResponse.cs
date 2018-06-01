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
    }
}