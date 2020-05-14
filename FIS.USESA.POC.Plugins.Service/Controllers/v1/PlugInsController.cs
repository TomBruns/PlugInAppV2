using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using FIS.USESA.POC.Plugins.Service.PlugInSupport;
using FIS.USESA.POC.Plugins.Service.Entities;

namespace FIS.USESA.POC.Plugins.Service.Controllers.v1
{
    /// <summary>
    /// This class implements the API controller for PlugIns
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PlugInsController : ControllerBase
    {
        private readonly ILogger<PlugInsController> _logger;
        private PlugInsManager _plugIsManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plugIsManager"></param>
        /// <param name="logger"></param>
        public PlugInsController(PlugInsManager plugIsManager, ILogger<PlugInsController> logger)
        {
            _logger = logger;
            _plugIsManager = plugIsManager;
        }

        /// <summary>
        /// Returns a list of the currently loaded plugins
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<PlugInBE>), (int)HttpStatusCode.OK)]
        public ActionResult<List<PlugInBE>> GetPlugIns()
        {
            var loadedPlugIns = _plugIsManager.LoadedPlugIns;

            return Ok(loadedPlugIns);
        }

        /// <summary>
        /// Returns a list of the current assembly load contexts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("alcs")]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        public ActionResult<List<string>> GetAssemblyLoadContexts()
        {
            var loadedPlugIns = _plugIsManager.AssemblyLoadContexts;

            return Ok(loadedPlugIns);
        }

        /// <summary>
        /// Returns a list of the assemblies loaded into an AssemblyLoadContext
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("alc/{alcName}/assemblies")]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        public ActionResult<List<string>> GetAssemblysLoadedInALC(string alcName)
        {
            var loadedAssemblies = _plugIsManager.GetAssembliesLoadedInALC(alcName);

            return Ok(loadedAssemblies);
        }

        /// <summary>
        /// Returns a list of the plugin folders
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("folders")]
        [ProducesResponseType(typeof(List<PlugInBE>), (int)HttpStatusCode.OK)]
        public ActionResult<List<PlugInBE>> GetPlugInFolders()
        {
            var loadedPlugIns = _plugIsManager.PlugInFolders;

            return Ok(loadedPlugIns);
        }

        /// <summary>
        /// Unload a plugin
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is not workng yet!
        /// </remarks>
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult UnloadPlugin(string name, decimal version)
        {
            if (_plugIsManager.UnloadPlugIn(name, version))
            {
                return Ok();
            }
            else
            {
                return NotFound($"Could not unload plugin matching name: [{name}], version: [{version}]");
            }
        }

        /// <summary>
        /// Load a new plugin
        /// </summary>
        /// <param name="plugInFolder"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult UnloadPlugin(string plugInFolder)
        {
            var newPlugIn = _plugIsManager.ForceLoad(plugInFolder);

            if(newPlugIn != null)
            {
                return Ok($"Loaded plugin: [{newPlugIn.PlugInImpl.GetPlugInInfo()}] in ALC: [{newPlugIn.AssemblyLoadContextName}]");
            }
            else
            {
                return NotFound($"Could not load plugin from folder: [{plugInFolder}]");
            }
        }
    }
}
