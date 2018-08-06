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

        /// <summary>
        /// Make a deep copy of this request
        /// </summary>
        public SerialisableRequest Clone()
        {
            var result = new SerialisableRequest
            {
                CommandControl = CommandControl,
                Method = Method,
                RequestUri = RequestUri,
                Headers = CopyOf(Headers)
            };
            if (Content != null) result.Content = (byte[])Content.Clone();
            return result;
        }

        private Dictionary<string, string> CopyOf(Dictionary<string, string> original)
        {
            var result = new Dictionary<string,string>();
            foreach (var kvp in original)
            {
                result.Add(kvp.Key, kvp.Value);
            }
            return result;
        }
    }
}