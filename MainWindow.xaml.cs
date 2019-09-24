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
        // -- Misc                  -- Settings are currently defined here.
        // -- App.xaml.css          -- Create system tray icon. Depending on research, tie the file listeners to processes 1-3 here.


        // Todo:    1. Run via command line with flags.
        // Todo:    2. Handle calendar updates.
        // Todo:    3. Generate log file

        public MainWindow()
        {
            InitializeComponent();
            outputLbl.Content = Properties.Settings.Default.outputPath;
            inputLbl.Content = Properties.Settings.Default.inputPath;
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
