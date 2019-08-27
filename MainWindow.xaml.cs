using System;
using System.Collections.Generic;
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

namespace NESPTI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        // bugs:    20190824 - When there are more then one page, results are duplicated.
        //          -- Notes - The issue occurs before any filters.
        //          -- 201926 - Fixed: Solved in two steps. 1. Processed each page separately. 2. Added new line after each page
        // bugs:    20190824 - Last line is missing.
        //          -- 20190824 - Fixed: Issue due to vaguely defined regEx - filter1   
        // bugs:    20190824 - Every day after the first day, is appended at the end of the previous string list.
        //          -- 20191824 - Fixed: Issue due to inverted logic.

        // get the number of pages
        public int NumberOfPages(string filename)
        {
            int numberOfPages = 0;
            ITextExtractionStrategy its = new SimpleTextExtractionStrategy(); 
            PdfDocument pdf = new PdfDocument(new PdfReader(filename)); 
            numberOfPages = pdf.GetNumberOfPages(); 
            pdf.Close();
            return numberOfPages;
        }
        
        // Get the page text. Called from a loop to get pages one by one.
        public string PageText(int pageNumber, string filename)
        {
            string pageText = "";
            ITextExtractionStrategy its = new SimpleTextExtractionStrategy(); 
            PdfDocument pdf = new PdfDocument(new PdfReader(filename));
            
            PdfPage page = pdf.GetPage(pageNumber);
      
            pageText = PdfTextExtractor.GetTextFromPage(page, its);
          
            pdf.Close();
            return pageText;
        }



        public void Button_Click(object sender, RoutedEventArgs e)
        {
            string theText = "";
            nesTextBox.Text = "";
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                InitialDirectory = @"c:\NESPTI\"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Get the number of pages.
                var numberOfPages = NumberOfPages(openFileDialog.FileName.ToString());

                // Get the text, line by line
                for (int i = 1; i <= numberOfPages; i++)
                {
                    theText += PageText(i, openFileDialog.FileName.ToString()) + "\n";
                    //nesTextBox.Text = theText;
                }

                List<string> lines = theText.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                ).ToList();

                List<string> lessLines = LessLines(lines); // remove some of the unneeded lines.

                List<List<string>> allDays = DailySchedule(lessLines); // all events separated by day

                // append each line to the textBox.
                foreach (List<string> oneDay in allDays)
                {
                    //nesTextBox.AppendText(space);
                    foreach (string oneDayLine in oneDay)
                    {


                        nesTextBox.AppendText(oneDayLine + "\n");
                    }
                }
            }
        }



        public List<List<string>> DailySchedule(List<string> lessLines)
        {

            string trackStr = lessLines[0];
            List<List<string>> allDays = new List<List<string>>(); // outer list containing all days
            List<string> oneDay = new List<string>(); // inner list containing one day

            int i = 0;

            Regex filter1 = new Regex(@"\w*, \w* \d{1,2}"); // check for the "Monday, August 13" date format

            foreach (string line in lessLines)
            {

                Match match1 = filter1.Match(line);

                if (match1.Success)
                {

                    List<string> savedOneDay = new List<string>();

                    if (i > 0)
                    {
                        savedOneDay.AddRange(oneDay);
                        allDays.Add(savedOneDay);
                        oneDay.Clear();
                    }
                    oneDay.Add(line);
                    i++;
                }

                else
                {
                    if (i > 0)
                    {
                        oneDay.Add(line);
                    }
                }
            }
            allDays.Add(oneDay);

            return allDays;


        } // END DailySchedule


        // Function to remove a few unneeded lines.
        // May not be needed in the future, but its nice to exclude them for now.
        public List<string> LessLines(List<string> lines)
        {
            List<string> lessLines = new List<string>();
            //\d{1,2}\/\d{1,2}\/\d{4} (?=\()
            //Regex filter1 = new Regex(@"\d*/\d*/\d*"); // get rid of that one date in the footer.
            Regex filter1 = new Regex(@"\d{1,2}\/\d{1,2}\/\d{4} (?=\()");
            Regex filter2 = new Regex(@"When vehicles are not moving"); // remove vehicles not moving line

            foreach (string line in lines)
            {
                Match match1 = filter1.Match(line);
                Match match2 = filter2.Match(line);

                if (match1.Success)
                {
                    //MessageBox.Show(line);

                }
                if (!match1.Success && !match2.Success)
                {
                    lessLines.Add(line);
                }
            }

            return lessLines;
        }
    }
}
