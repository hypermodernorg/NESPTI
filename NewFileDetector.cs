using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Permissions;
using System.Windows;
using System.Timers;
using System.Threading;
using System.Windows.Threading;
using Serilog;
using Timer = System.Timers.Timer;

namespace NESPTI
{
    class NewFileDetector
    {
        static FileSystemWatcher _watcher = new FileSystemWatcher();

        public static void StopWatchDirectory()
        {


            if (_watcher.EnableRaisingEvents)
            {
                _watcher.EnableRaisingEvents = false;
                MessageBox.Show("Stopped Monitoring New Events");
                Log.Information("User asked to stop the directory monitor.");
            }

      
        }
        public static void WatchDirectory()
        {
            if (_watcher.EnableRaisingEvents)
            {
                MessageBox.Show("Already Monitoring New Events");
                Log.Information("User asked to directory monitor, but the monitor is already active.");
            }

            if (_watcher.EnableRaisingEvents == false)
            {
                _watcher.Path = Properties.Settings.Default.inputPath;
                _watcher.NotifyFilter = NotifyFilters.FileName;
                //  | NotifyFilters.DirectoryName;

                //Only watch pdf files.
                _watcher.Filter = "*.pdf";

                //Add event handlers.
                //watcher.Changed += OnChanged;
                _watcher.Created += OnChanged;
                //watcher.Deleted += OnChanged;

                //Begin watching.
                _watcher.EnableRaisingEvents = true;
                MessageBox.Show("Now Monitoring New Events");
                Log.Information("User began the directory monitor.");
            }
 

        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Log.Information("Begin: Delay between file open and file detection.");
            Task.Delay(1000).Wait();
            Log.Information("End: Delay between file open and file detection.");
            var x = new ConvertToIcal();

            try
            {
                x.ConverterMain(e);
                Log.Information("Succeeded: Call to ConverterMain from the directory watcher.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed: Call to ConverterMain from the directory watcher.");
            }
            
            
        }
    }
}
