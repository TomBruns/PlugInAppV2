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
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult UnloadPlugin(string name, decimal version)
        {
            //var plugIn = _scheduleTaskPlugIns.Where(pi => pi.Metadata.Name == name && pi.Metadata.Version == (double)version).FirstOrDefault();

            //if(plugIn != null)
            //{
            //    _scheduleTaskPlugIns = _scheduleTaskPlugIns.Where(pi => pi.Metadata.Name != name && pi.Metadata.Version != (double)version);
            //    GC.Collect();
            //    return Ok();
            //}
            //else
            //{
            return NotFound($"No plugin found matching name: [{name}], version: [{version}]");
            //}
        }
    }
}
