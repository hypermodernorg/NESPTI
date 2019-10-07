using System;
using System.Windows;
using System.Windows.Forms;
using Serilog;


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


        // Todo:    1. Run via command line with flags. -- May never happen. Doesn't seem to be needed.
        // Todo:    2. Handle calendar database updates. -- Completed - needs testing.
        // Todo:    3. Export master calendar. -- In progress.
        // Todo:    4. Potentially add options to export calendar by series.
        // Todo:    5. Handle the canceled event.... event. - Todo added 20191003 after Aaron noticed a cancelled event in the pdf. -- Completed
        // Todo:    6. Check to see if the "many from one" events are still correct, such as: -- 20191003 - Completed but needs testing.
        // Todo:        -- Garage open and close. -- still seems to work.
        // Todo:        -- Two series listed on the same event. -- Non issue? 20191003 -- Fixed, multiple series per event now create an event per series.


        public MainWindow()
        {
            InitializeComponent();
            outputLbl.Content = Properties.Settings.Default.outputPath;
            inputLbl.Content = Properties.Settings.Default.inputPath;
            ConvertToIcal.EventLogger();
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
    }
}
