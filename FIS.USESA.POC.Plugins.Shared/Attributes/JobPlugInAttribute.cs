using System;
using System.Collections.Generic;
using System.Text;

namespace FIS.USESA.POC.Plugins.Shared.Attributes
{
    /// <summary>
    /// This attribute is added to all Plug-In Classes
    /// </summary>
    public class JobPlugInAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
