using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Permissions;
using System.Windows;

namespace NESPTI
{
    class NewFileDetector
    {
        static FileSystemWatcher _watcher = new FileSystemWatcher();

        public static void StopWatchDirectory()
        {
            _watcher.EnableRaisingEvents = false;
            MessageBox.Show("Stopped Monitoring New Events");
            _watcher.Dispose();
        }
        public static void WatchDirectory()
        {
            //FileSystemWatcher watcher = new FileSystemWatcher();
            _watcher.Path = @"C:\NESPTI\watch";
            _watcher.NotifyFilter = NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName;

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

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            var x = new ConvertToIcal();
            x.ConverterMain(e);
            
        }
    }
}
