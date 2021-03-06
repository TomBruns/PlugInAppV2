﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

using FIS.USESA.POC.Plugins.Service.Entities;
using FIS.USESA.POC.Plugins.Shared.Attributes;
using FIS.USESA.POC.Plugins.Shared.Entities;
using FIS.USESA.POC.Plugins.Shared.Interfaces;

namespace FIS.USESA.POC.Plugins.Service.PlugInSupport
{
    /// <summary>
    /// This class abstracts the interaction with the Plugins
    /// </summary>
    public class PlugInsManager
    {
        PlugInsConfigBE _plugInsConfig;
        KafkaServiceConfigBE _kafkaConfig;
        List<PlugInBE> _loadedPlugIns;

        internal PlugInsManager(PlugInsConfigBE plugInsConfig, KafkaServiceConfigBE kafkaConfig)
        {
            _plugInsConfig = plugInsConfig;
            _kafkaConfig = kafkaConfig;

            Compose(_plugInsConfig);
        }

        /// <summary>
        /// Returns a list of the loaded plugins
        /// </summary>
        public List<PlugInBE> LoadedPlugIns
        {
            get => _loadedPlugIns ?? new List<PlugInBE>();
        }

        /// <summary>
        /// Returns a list of the application load contexts
        /// </summary>
        public List<string> AssemblyLoadContexts
        {
            // AssemblyLoadContext.All seems to return inconsistent results
            //get => AssemblyLoadContext.All.Select(c => c.Name).ToList();
            get
            {
                List<string> alcs = new List<string>();

                foreach(var xx in _loadedPlugIns)
                {
                    var alc = AssemblyLoadContext.GetLoadContext(xx.PlugInImpl.GetType().Assembly);
                    alcs.Add(alc.Name);
                }

                return alcs; 
            }
        }

        /// <summary>
        /// Returns a list of plugin folders
        /// </summary>
        public List<string> PlugInFolders
        {
            get => new DirectoryInfo(_plugInsConfig.PlugInsParentFolder).GetDirectories().Select(di => $".\\{di.Name}").ToList();
        }

        /// <summary>Returns a list of assemblies loaded into the specified assembly load context</summary>
        /// <param name="alcName">Name of the alc.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        public List<string> GetAssembliesLoadedInALC(string alcName)
        {
            var loadedAssys = new List<string>();

            var xx = AssemblyLoadContext.All.Where(alc => alc.Name == alcName).FirstOrDefault();

            if (xx != null)
            {
                loadedAssys = xx.Assemblies.Select(a => a.FullName ).ToList<string>();
            }

            return loadedAssys;
        }

        /// <summary>
        /// Gets the job plug in.
        /// </summary>
        /// <param name="jobPlugInName">Name of the job plugIn.</param>
        /// <param name="jobPlugInVersion">Version of the job plugIn.</param>
        /// <returns>PlugInBE.</returns>
        /// <exception cref="ApplicationException">No plug-in found for Event Type: [{jobPlugInType}]</exception>
        /// <exception cref="ApplicationException">Multiple plug-ins [{plugIn.Count()}] found for Event Type: [{scheduledTaskType}]</exception>
        public PlugInBE GetJobPlugIn(string jobPlugInName, Version jobPlugInVersion)
        {
            var plugIn = _loadedPlugIns
                          .Where(lpi => lpi.Name.ToUpper() == jobPlugInName.ToUpper()
                                            && lpi.Version == jobPlugInVersion)
                          .ToList();

            if (plugIn == null || plugIn.Count() == 0)
            {
                throw new ApplicationException($"No plug-in found for Job Name: [{jobPlugInName}], Version: [{jobPlugInVersion}]");
            }
            else if (plugIn.Count() != 1)
            {
                throw new ApplicationException($"Multiple plug-ins [{plugIn.Count()}] found for Job Name: [{jobPlugInName}], Version: [{jobPlugInVersion}]");
            }
            else
            {
                return plugIn.First();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobPlugInName"></param>
        /// <param name="jobPlugInVersion"></param>
        public bool UnloadPlugIn(string jobPlugInName, Version jobPlugInVersion)
        {
            // find the plug-in
            var plugIn = GetJobPlugIn(jobPlugInName, jobPlugInVersion);

            // remove the reference to the plug-in alc in this collection
            _loadedPlugIns.Remove(plugIn);

            // find the assemly load context that holds 
            var alc = AssemblyLoadContext.GetLoadContext(plugIn.GetType().Assembly);

            var alcs = AssemblyLoadContext.All;
            //var alc = alcs.Where(a => a.Name == plugIn.AssemblyLoadContextName).FirstOrDefault();

            // null out our reference to the assy
            plugIn.PlugInImpl = null;
            plugIn = null;

            if (alc != null)
            {
                // unload the assembly load context
                alc.Unload();
                alc = null;

                // cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>Forces plug-in in a specified folder to load.</summary>
        /// <param name="pluginSubdirectory">The plugin subdirectory.</param>
        /// <returns>PlugInBE.</returns>
        public PlugInBE ForceLoad(string pluginSubdirectory)
        {
            string newPlugInFolderPathName = System.IO.Path.Combine(_plugInsConfig.PlugInsParentFolder, pluginSubdirectory);

            string newPlugInAssyPathName = FindPlugInAssyInFolder(newPlugInFolderPathName);

            var newPlugInAssy = LoadPlugin(newPlugInAssyPathName);

            var newPlugIns =  CreateCommands(newPlugInAssy);

            foreach (var newPlugIn in newPlugIns)
            {
                newPlugIn.PlugInImpl.InjectConfig(_kafkaConfig);

                _loadedPlugIns.Add(newPlugIn);
            }

            return newPlugIns.First();
        }

        /// <summary>
        /// Probe the PlugIn subfolders and load the plugins
        /// </summary>
        /// <param name="plugInsConfig"></param>
        private void Compose(PlugInsConfigBE plugInsConfig)
        {
            // get a list of file pathnames to assys under the plugins parent folder that implement the IPlugIn interface
            List<string> plugInPaths = GetListOfPlugInAssys(plugInsConfig.PlugInsParentFolder);

            // compose a list of the plug-ins
            _loadedPlugIns = plugInPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPlugin(pluginPath);
                return CreateCommands(pluginAssembly);
            }).ToList();

            // inject the kafka config into each plugin
            foreach (var loadedPlugIn in _loadedPlugIns)
            {
                loadedPlugIn.PlugInImpl.InjectConfig(_kafkaConfig);
            }
        }

        #region ======= Helper Methods ======================================================================

        /// <summary>
        /// Find the full paths to the assys in all of the plug-in child folders that implement the IPlugIn interface
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <returns></returns>
        private static List<string> GetListOfPlugInAssys(string parentFolder)
        {
            // find the names of all the plug-in subfolders
            var plugInFolderPathNames = Directory.GetDirectories(parentFolder);

            List<string> plugInPaths = new List<string>();

            foreach (var plugInFolderPathName in plugInFolderPathNames)
            {
                string plugInAssyPath = FindPlugInAssyInFolder(plugInFolderPathName);
                plugInPaths.Add(plugInAssyPath);
            }

            return plugInPaths;
        }

        /// <summary>
        /// Find the full path to the assy on a folder that implements the IPlugIn interface
        /// </summary>
        /// <param name="plugInFolderPathName"></param>
        /// <returns></returns>
        private static string FindPlugInAssyInFolder(string plugInFolderPathName)
        {
            // Get the array of runtime assemblies.
            // This will allow us to at least inspect types depending only on BCL.
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            string[] currentExeAssemblies = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll");

            // Create the list of assembly paths consisting of runtime assemblies
            var paths = new List<string>(runtimeAssemblies);
            paths.Add(plugInFolderPathName);
            paths.AddRange(currentExeAssemblies);

            // Create MetadataLoadContext that can resolve assemblies using the created list.
            var resolver = new PathAssemblyResolver(paths);
            var mlc = new MetadataLoadContext(resolver);

            List<string> assysInPlugInFolder = Directory.GetFiles(plugInFolderPathName, "*.dll").ToList();

            var interfaceFilter = new TypeFilter(InterfaceFilter);
            string qualifiedInterfaceName = typeof(IPlugIn).FullName;

            using (mlc)
            {
                foreach (var assyPathName in assysInPlugInFolder)
                {
                    // Load assembly into MetadataLoadContext.
                    Assembly assembly = mlc.LoadFromAssemblyPath(assyPathName);
                    AssemblyName name = assembly.GetName();

                    foreach (var type in assembly.GetTypes())
                    {
                        var myInterfaces = type.FindInterfaces(interfaceFilter, qualifiedInterfaceName);
                        if (myInterfaces.Length > 0)
                        {
                            return assyPathName;
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Load a plugin into a custom AssemblyLoadContext
        /// </summary>
        /// <param name="pluginPath"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Assembly LoadPlugin(string pluginPath)
        {
            // build a unique name for the load context based of the plugin's parent folder
            var pathParts = pluginPath.Split(System.IO.Path.DirectorySeparatorChar);
            string loadContextName = pathParts[^2];

            // create a new unloadable AssemblyLoadContext on the plug-in's folder 
            Console.WriteLine($"Loading commands from: {pluginPath}");
            PluginLoadContext loadContext = new PluginLoadContext(loadContextName, pluginPath, true);

            // return the loaded plugin assembly
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));
        }

        /// <summary>
        /// Find the types in an assembly that implement the IPlugIn interface
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static IEnumerable<PlugInBE> CreateCommands(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugIn).IsAssignableFrom(type))
                {
                    IPlugIn result = Activator.CreateInstance(type) as IPlugIn;
                    if (result != null)
                    {
                        count++;

                        var plugIn = new PlugInBE()
                        {
                            AssemblyLoadContextName = AssemblyLoadContext.GetLoadContext(assembly).Name,
                            LoadedAtDT = DateTime.Now,
                            Name = type.GetCustomAttribute<JobPlugInAttribute>().Name,
                            PlugInInfo = result.GetPlugInInfo(),
                            PlugInImpl = result,
                            PlugInPath = assembly.CodeBase,
                            Version = type.Assembly.GetName().Version
                        };

                        yield return plugIn;
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }

        /// <summary>
        /// Type filter used to identify Assemblies with classes that implement a specific Interface
        /// </summary>
        /// <param name="typeObj"></param>
        /// <param name="criteriaObj"></param>
        /// <returns></returns>
        private static bool InterfaceFilter(Type typeObj, Object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }

        #endregion
    }
}
