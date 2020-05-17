using System;
using System.Reflection;
using System.Runtime.Loader;

namespace FIS.USESA.POC.Plugins.Service.PlugInSupport
{
    // https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
    // https://github.com/dotnet/samples/tree/master/core/extensions/AppWithPlugin
    class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string name, string pluginPath, bool isUnloadable) : base(name, isUnloadable)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            System.Diagnostics.Debug.WriteLine($"Name: [{assemblyName}] => Path: [{assemblyPath ?? "<null>"}]");

            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
