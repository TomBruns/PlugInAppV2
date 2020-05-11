using System;
using System.Collections.Generic;
using System.Text;

using static FIS.USESA.POC.Plugins.Shared.Constants.SchedulerConstants;

namespace FIS.USESA.POC.Plugins.Shared.Entities
{
    public class StdTaskReturnValueBE
    {
        public STD_STEP_STATUS StepStatus { get; set; }

        public string ReturnMessage { get; set; }
    }
}
