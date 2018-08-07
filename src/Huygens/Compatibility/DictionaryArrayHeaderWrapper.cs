using System.Collections.Generic;
using System.Linq;

namespace Huygens.Compatibility
{
    /// <summary>
    /// IHeaderCollection that wraps a Dictionary of arrays
    /// </summary>
    public class DictionaryArrayHeaderWrapper : IHeaderCollection
    {
        private readonly Dictionary<string, string[]> _headers;

        /// <summary>
        /// Wrap a dictionary
        /// </summary>
        public DictionaryArrayHeaderWrapper(Dictionary<string, string[]> headers)
        {
            _headers = headers;
        }

        /// <inheritdoc />
        public string Get(string name)
        {
            if ( ! _headers.TryGetValue(name, out var value)) return null;
            return string.Join(", ", value);
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
                _headers.Add(name, new[] { value });
            }
            catch {
                _headers[name] = _headers[name].Concat(new[] { value }).ToArray();
            }
        }

        /// <inheritdoc />
        public void Set(string name, string value)
        {
            try {
                _headers.Add(name, new[] { value });
            }
            catch {
                _headers[name] = new[] { value };
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