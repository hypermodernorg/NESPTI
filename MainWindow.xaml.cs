using System;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Serilog;
using Application = System.Windows.Application;


namespace NESPTI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        // The program's main components are divided into five main parts:
        // 1. GetText.cs            -- Extract the text from the pdf using the iText library.
        // 2. ProcessText.cs        -- Process the extracted text and prepare to pass to the calendar using program logic and regex.
        // 3. CreateEvent.cs        -- Create an event from the variables collected from ProcessText.cs using the iCal.net library. 
        // 4. NewFileDetector.cs    -- Detect new files and process.
        // 5. DB.cs                 -- Database read, write, and delete methods. 
        // -- Misc                  -- Settings are currently defined here.
        // -- App.xaml.css          -- Create system tray icon. Depending on research, tie the file listeners to processes 1-3 here.
        // -- Log.cs                -- Serilog logging system.

        // Todo     1. Timezone information invalidates without vtimezone definitions. If possible, fix it.
        // Todo     -- Research: https://github.com/rianjs/ical.net/issues/266
        // Todo     -- Research: https://github.com/rianjs/ical.net/blob/master/v2/ical.NET/Components/VTimeZoneInfo.cs
        // Todo     -- Research: https://icalendar.org/iCalendar-RFC-5545/3-6-5-time-zone-component.html
        // Todo:    2. Potentially add options to export calendar by series.
        // Todo:    3. Add user setting to choose year to export master calendar.

        public MainWindow()
        {
            InitializeComponent();
            outputLbl.Content = Properties.Settings.Default.outputPath;
            inputLbl.Content = Properties.Settings.Default.inputPath;
            ConvertToIcal.EventLogger();
            
            // Check if db exists. If not, copy and paste it.
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "NESPTI" + Path.DirectorySeparatorChar + "NESPTI.db"  ))
            {
                DirectorySecurity securityRules = new DirectorySecurity();
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                          Path.DirectorySeparatorChar + "NESPTI");

                string sourceFile = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "database" + Path.DirectorySeparatorChar + "NESPTI.db";
                string destFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "NESPTI" + Path.DirectorySeparatorChar + "NESPTI.db";
                File.Copy(sourceFile, destFile, true);
            }
            Log.Information("DB Location:" +  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)); 
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            var x = new ConvertToIcal();


            // If the user elects to cancel the file dialogue, try catch below will handle it without causing NESPTI to crash.
            try
            {
                x.ConverterMain();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing ConverterMain from OpenNES");
            }
          
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

        private void InputButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fldrDlg = new FolderBrowserDialog())
            {

                fldrDlg.SelectedPath = Properties.Settings.Default.inputPath;

                if (fldrDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    Properties.Settings.Default.inputPath = fldrDlg.SelectedPath;
                    Properties.Settings.Default.Save();
                    inputLbl.Content = fldrDlg.SelectedPath.ToString();
                }
            }
        }

        private void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fldrDlg = new FolderBrowserDialog())
            {

                fldrDlg.SelectedPath = Properties.Settings.Default.outputPath;

                if (fldrDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    Properties.Settings.Default.outputPath = fldrDlg.SelectedPath;
                    Properties.Settings.Default.Save();
                    outputLbl.Content = fldrDlg.SelectedPath.ToString();
                }
            }
        }

        // Get year from user input and detect when the input is 4 digits, and is a number.
        private void YearTxt_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            String yearStr = yearTxt.Text;
            if (yearStr.Length == 4 && Regex.IsMatch(yearStr, @"^\d+$"))
            {
                var x = new ConvertToIcal();
                x.ExportMasterCalender(yearStr);
            }
        }
    }
}
