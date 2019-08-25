using System;
using System.Collections.Generic;
using System.IO;
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
using MahApps.Metro.Controls;
using Microsoft.Win32;
using iText;
using iText.IO;
using com.itextpdf;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

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
     
        public void Button_Click(object sender, RoutedEventArgs e)
        {

            // bugs:    20190824 - When there are more then one page, results are duplicated.
            //          -- Notes - The issue occurs before any filters.
            // bugs:    20190824 - Last line is missing.
            //          -- 20190824 - Fixed: Issue due to vaguely defined regEx - filter1   
            // bugs:    20190824 - Every day after the first day, is appended at the end of the previous string list.
            //          -- 20191824 - Fixed: Issue due to inverted logic.
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                InitialDirectory = @"c:\NESPTI\"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ITextExtractionStrategy its = new SimpleTextExtractionStrategy(); // important
                string filename = openFileDialog.FileName;
                PdfDocument pdf = new PdfDocument(new PdfReader(filename)); //new PdfWriter(@"c:\NESPTI\test.pdf")); // needs administrator permissions
                var pageCount = pdf.GetNumberOfPages();
                string space = "\n\n\n";

                string theText = "";

                // count the pages of the pdf, and append each page to theText.
                // RESEARCH: text extraction strategy for itext documentation
                // https://api.itextpdf.com/iText7/7.0.5/com/itextpdf/kernel/pdf/canvas/parser/PdfTextExtractor.html
         
                for (int i = 1; i <= pageCount ; i++) {


                    //  New Plan: Workaround for pdfTextExtractor limitations/Issues
                    //  1.get number of pages then close pdf
                    //  2.get each page from 1 to number of pages in a class that returns string.
                    //  3. build string from each iteration



                    PdfPage page = pdf.GetPage(i);
                    theText += PdfTextExtractor.GetTextFromPage(page, its); // Keep eye on extraction "its" strategy as this might be problematic.

                }

                pdf.Close(); // close the pdf -


                //nesTextBox.Text = ""; // clear the text box of initial value.
                //nesTextBox.Text = theText;
                // Split the string into an array by newline
                List<string> lines = theText.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                ).ToList();

                List<string> lessLines = LessLines(lines); // remove some of the unneeded lines.


                // Parse the rest of the data
                // Once i figure out how to store the parsed data, assign to variable of decided data type.
                // testing currently

                List<List<string>> allDays = DailySchedule(lessLines); // all events separated by day


                //MessageBox.Show(allDays[0][0] + "   " + allDays[1][0]);



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

//So the events we care about are the Garage Open/Close times, Practice sessions, Qualifying sessions, and Race start times for each series.

//Also, we do not want the Garage Open to Garage Close to be a single event, as that creates a big block on the calendar.  We just want the open and close each to be it's own event.

//NKNPS-E is the K&N Series
//ARCA is the Arca Series
//NGOTS is the Truck Series
//NXS is the Xfinity Series
//MENCS is the Cup Series.
