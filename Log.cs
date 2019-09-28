using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace NESPTI
{
    public partial class ConvertToIcal
    {
        public static void EventLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Properties.Settings.Default.outputPath + Path.DirectorySeparatorChar + "NESPTI_LOG.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

    }
}
