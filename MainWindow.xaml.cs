using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Win32;
using System.IO;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using MahApps.Metro;
using Calendar = Ical.Net.Calendar;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace NESPTI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        // Todo:    1. Add timezone support.
        // Todo:    -- 20190910 - Progress made for four timezones. Need more research for other tracks and their respective timezones. 
        // Todo:    2. Correctly handle event end times that are in parentheses, which should be discarded.
        // Todo:    3. Play with the garage open and close events end times. i.e... 15 minutes.
        // Todo:    -- 20190910 - Completed, but needs testing.
        // Todo:    --  Added, however doesn't display properly (text not fitting in small 15 minute space) in all views. Might add this as a user parameter instead of hardcoded time.
        // Todo:    4. System Tray and hide form function to simulate background process.
        // Todo:    -- 20190912 - Completed
        // Todo:    5. Run via command line with flags.
        // Todo:    6. While running in the background, check for new files and convert.
        // Todo:    7. Handle calendar updates.

        public MainWindow()
        {
            InitializeComponent();
        }

        // The program's main components are divided into four main parts:
        // 1. GetText.cs            -- Extract the text from the pdf using the iText library.
        // 2. ProcessText.cs        -- Process the extracted text and prepare to pass to the calendar using program logic and regex.
        // 3. CreateEvent.cs        -- Create an event from the variables collected from ProcessText.cs using the iCal.net library.
        // 4. NewFileDetector.cs    -- Detect new files and process.
        // -- Misc                  -- Static variables, user events, and settings are currently defined here.
        // -- App.xaml.css          -- Create system tray icon. Depending on research, tie the file listeners to processes 1-3 here.
        



        public void Button_Click(object sender, RoutedEventArgs e)
        {
            var x = new ConvertToIcal();
            x.ConverterMain();
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            using (var fldrDlg = new FolderBrowserDialog())
            {

                fldrDlg.SelectedPath = Properties.Settings.Default.sourcePath;

                if (fldrDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                 
                    Properties.Settings.Default.sourcePath = fldrDlg.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }
    }
}
