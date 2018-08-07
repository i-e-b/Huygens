using System.Collections.Generic;
using System.Collections.Specialized;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Wrapper for NVC headers
    /// </summary>
    public class NameValueCollectionHeaderWrapper : IHeaderCollection
    {
        private readonly NameValueCollection _nvc;

        /// <summary>
        /// Wrap headers
        /// </summary>
        public NameValueCollectionHeaderWrapper(NameValueCollection nvc)
        {
            _nvc = nvc;
        }

        /// <inheritdoc />
        public string Get(string name)
        {
            return _nvc.Get(name);
        }

        /// <inheritdoc />
        public void Remove(string name)
        {
            _nvc.Remove(name);
        }

        /// <inheritdoc />
        public void Add(string name, string value)
        {
            _nvc.Add(name, value);
        }

        /// <inheritdoc />
        public void Set(string name, string value)
        {
            _nvc.Remove(name);
            _nvc.Add(name, value);
        }

        /// <inheritdoc />
        public string this[string name]
        {
            get { return _nvc[name]; }
            set { _nvc[name] = value; }
        }

        /// <inheritdoc />
        public IEnumerable<string> AllKeys { get { return _nvc.AllKeys; } }
    }
}