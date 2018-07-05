using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Huygens.Internal
{
    /// <summary>
    /// Tool to help load assemblies directly into AppDomains
    /// </summary>
    public class AssemblyReflectionProxy : MarshalByRefObject
    {
        private string _assemblyPath;

        /// <summary>
        /// Load an assembly
        /// </summary>
        public void LoadAssembly(string assemblyPath)
        {
            try
            {
                _assemblyPath = assemblyPath;
                Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            }
            catch (FileNotFoundException)
            {
                // Continue loading assemblies even if an assembly can not be loaded in the new AppDomain.
            }
        }

        /// <summary>
        /// Run a function in an app domain
        /// </summary>
        public TResult Reflect<TResult>(Func<Assembly, TResult> func)
        {
            DirectoryInfo directory = new FileInfo(_assemblyPath).Directory;
            ResolveEventHandler resolveEventHandler = (s, e) => OnReflectionOnlyResolve( e, directory);
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveEventHandler;

            var assembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().FirstOrDefault(a => string.Compare(a.Location, _assemblyPath, StringComparison.Ordinal) == 0);

            var result = func(assembly);

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;

            return result;
        }

        private Assembly OnReflectionOnlyResolve(ResolveEventArgs args, DirectoryInfo directory)
        {
            Assembly loadedAssembly =
                AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                    .FirstOrDefault(
                      asm => string.Equals(asm.FullName, args.Name,
                          StringComparison.OrdinalIgnoreCase));

            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            AssemblyName assemblyName =
                new AssemblyName(args.Name);
            string dependentAssemblyFilename =
                Path.Combine(directory.FullName,
                assemblyName.Name + ".dll");

            if (File.Exists(dependentAssemblyFilename))
            {
                return Assembly.ReflectionOnlyLoadFrom(
                    dependentAssemblyFilename);
            }
            return Assembly.ReflectionOnlyLoad(args.Name);
        }
    }
}