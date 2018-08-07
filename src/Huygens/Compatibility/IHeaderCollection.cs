using System.Collections.Generic;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Interface for http headers
    /// </summary>
    public interface IHeaderCollection
    {
        /// <summary>
        /// Get value by key name
        /// </summary>
        string Get(string name);

        /// <summary>
        /// Remove a header by key name
        /// </summary>
        void Remove(string name);

        /// <summary>
        /// Add a new value
        /// </summary>
        void Add(string name, string value);

        /// <summary>
        /// Add a new value, removing any existing values
        /// </summary>
        void Set(string name, string value);
        
        /// <summary>
        /// Get value by key name
        /// </summary>
        string this[string name] { get; set; }

        /// <summary>
        /// List all keys
        /// </summary>
        IEnumerable<string> AllKeys { get; }
    }
}