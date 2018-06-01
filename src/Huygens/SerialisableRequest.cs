using System;
using System.Collections.Generic;

namespace Huygens
{
    /// <summary>
    /// A fully-encapsulated HTTP request that can cross AppDomain boundaries
    /// </summary>
    [Serializable]
    public class SerialisableRequest : MarshalByRefObject
    {
        /// <summary>
        /// Used to perform functions directly on the child host
        /// </summary>
        public string CommandControl { get; set; }

        /// <summary>
        /// Body content bytes
        /// </summary>
        public byte[] Content { get; set; }
        
        /// <summary>
        /// HTTP method verb
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// Uri to be passed to the web app
        /// </summary>
        public string RequestUri { get; set; }

        /// <summary>
        /// Header dictionary
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }
    }
}