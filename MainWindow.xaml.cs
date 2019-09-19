using System.Windows;
using System.Windows.Forms;


namespace NESPTI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        // The program's main components are divided into four main parts:
        // 1. GetText.cs            -- Extract the text from the pdf using the iText library.
        // 2. ProcessText.cs        -- Process the extracted text and prepare to pass to the calendar using program logic and regex.
        // 3. CreateEvent.cs        -- Create an event from the variables collected from ProcessText.cs using the iCal.net library.
        // 4. NewFileDetector.cs    -- Detect new files and process.
        // -- Misc                  -- Static variables, user events, and settings are currently defined here.
        // -- App.xaml.css          -- Create system tray icon. Depending on research, tie the file listeners to processes 1-3 here.

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
        // Todo:    -- 20190917 - Progress. Functional but can be polished.
        // Todo:    7. Handle calendar updates.

        public MainWindow()
        {
            InitializeComponent();
        }

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
