using System;
using System.Collections.Generic;
using System.Text;

namespace FIS.USESA.POC.Plugins.Shared.Constants
{
    public static class SchedulerConstants
    {
		public enum STD_STEP_STATUS
		{
			FAILURE = -1,
			UNKNOWN = 0,
			SUCCESS = 1,
		}

		public enum LOG_LEVEL
        {
			INFO = 1,
			WARNING = 2,
			ERROR = 3
        }
	}
}
