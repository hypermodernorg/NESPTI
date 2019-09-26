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
            }

      
        }
        public static void WatchDirectory()
        {
            if (_watcher.EnableRaisingEvents)
            {
                MessageBox.Show("Already Monitoring New Events");
            }

            if (_watcher.EnableRaisingEvents == false)
            {
                //FileSystemWatcher watcher = new FileSystemWatcher();
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
            }
 

        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Task.Delay(1000).Wait();

            var x = new ConvertToIcal();
            x.ConverterMain(e);
            
        }
    }
}
