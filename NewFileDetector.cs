using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Permissions;

namespace NESPTI
{
    class NewFileDetector
    {

        public static void WatchDirectory()
        {
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = @"C:\NESPTI\watch";
                watcher.NotifyFilter = NotifyFilters.LastWrite
                                       | NotifyFilters.LastWrite
                                       | NotifyFilters.FileName
                                       | NotifyFilters.DirectoryName;

                // Only watch pdf files.
                watcher.Filter = "*.pdf";


                // Add event handlers.
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                watcher.Deleted += OnChanged;
              

                // Begin watching.
                watcher.EnableRaisingEvents = true;
            }


        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
        }
    }
}
