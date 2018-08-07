using System;
using System.Collections.Generic;

namespace Huygens.Compatibility
{
    /// <summary>
    /// IHeaderCollection that wraps a Dictionary of single values
    /// </summary>
    public class DictionaryHeaderWrapper : IHeaderCollection
    {
        private readonly Dictionary<string, string> _headers;

        /// <summary>
        /// Wrap a dictionary
        /// </summary>
        public DictionaryHeaderWrapper(Dictionary<string, string> headers)
        {
            _headers = headers;
        }

        /// <inheritdoc />
        public string Get(string name)
        {
            if ( ! _headers.TryGetValue(name, out var value)) return null;
            return value;
        }

        /// <inheritdoc />
        public void Remove(string name)
        {
            _headers.Remove(name);
        }

        /// <inheritdoc />
        public void Add(string name, string value)
        {
            try {
                _headers.Add(name, value);
            } catch {
                _headers[name] += ", " + value;
            }
        }

        /// <inheritdoc />
        public void Set(string name, string value)
        {
            try {
                _headers.Add(name, value);
            } catch {
                _headers[name] = value;
            }
        }
        
        /// <inheritdoc />
        public string this[string name]
        {
            get { return Get(name); }
            set { Set(name, value); }
        }

        /// <inheritdoc />
        public IEnumerable<string> AllKeys => _headers.Keys;
    }
}