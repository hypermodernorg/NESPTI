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
using Calendar = Ical.Net.Calendar;

namespace NESPTI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
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

 

        public void CreateIcalEvent(string startTime, string endTime, string theDate, string raceTrack, string theEvent, string theSeries)
        {
     
            //nesTextBox.AppendText(theDate + startTime + endTime + theEvent + theSeries + raceTrack + "\n");


            // --  Begin create dateTime: 
            //     Research: DateTime.Parse()

            // 1.  First, lets get the YEAR
            Regex theYearRegex = new Regex(@"(\d{4,4})");
            Match theYearMatch = theYearRegex.Match(raceTrack); 

            // 2.   Next, get the MONTH and covert it to a number. Example: April to 4.
            Regex theMonthRegex = new Regex(@"\w*, (\w*) (\d*)"); // get the month and day in groups
            Match theMonthMatch = theMonthRegex.Match(theDate); // convert month
            var theMonthString = theMonthMatch.Groups[1];

            // 3.   Get the DAY. Example: 15 in 15th of July.
            var theDay = theMonthMatch.Groups[2];

            // 4.   Get the TIME and convert it to military format. Example: 2.00 PM   to 1400


            // 5.   Put it all together.

            //--------------------------------------------------------------------//
            //var now = DateTime.Now;
            var now = DateTime.Parse(theDate + " " + theYearMatch + " " + startTime);

            var e = new CalendarEvent
            {
                Start = new CalDateTime(now),
  
            };

            if (endTime != "")
            {
                var later = DateTime.Parse(theDate + " " + theYearMatch + " " + endTime);
                e.End = new CalDateTime(later);
            }
            
            //e.Name = raceTrack + " | " + theSeries + " | " + theEvent;

            //e.Name = "Test";
            e.Description = raceTrack + " | " + theSeries + " | " + theEvent;

            var calendar = new Calendar();
      
            calendar.Events.Add(e);

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);

            nesTextBox.AppendText(serializedCalendar + "\n");
            //-------------------------------------------------------------------//
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

                string raceTrack = lessLines[0]; // The Race Track/Series

                List<List<string>> allDays = DailySchedule(lessLines); // all events separated by day

                // append each line to the textBox.



           
                Regex filter1 = new Regex(@"((\d{1,2}:\d{2,2} \w{2,2}) \(*(\d{1,2}:\d{1,2} \w{2,2}))\)? (\S*) (.*)"); // matches events with open and close times -- 20190827 - Now accounts for parentheses
                Regex filter2 = new Regex(@"((^\d{1,2}:\d{2,2} \w{2,2}) (?!\(?\d{1,2}:\d{2,2} \w{2,2})(\S*)(.*))"); // matches events with just an open time

                //$input = '06/10/2011 19:00:02'; 
                //$date = strtotime($input);
                //echo date('d/M/Y h:i:s', $date);

                foreach (List<string> oneDay in allDays)
                {
                    
                    var theDate = oneDay[0]; 
                    //nesTextBox.AppendText("\n" + theDate + "\n");

                    foreach (string oneDayLine in oneDay)
                    {
                        string startTime;
                        string endTime;
                        string theEvent;
                        string theSeries;

                        if (oneDayLine.Contains("ARCA") || oneDayLine.Contains("NGOTS") || oneDayLine.Contains("MENCS") || oneDayLine.Contains("NXS") || oneDayLine.Contains("MKNPS"))
                        {

                            if (oneDayLine.Contains("PRACTICE") || oneDayLine.Contains("GARAGE") || oneDayLine.Contains("RACE") || oneDayLine.Contains("QUALIFYING"))
                            {
                                Match openAndClose = filter1.Match(oneDayLine);
                                Match openOnly = filter2.Match(oneDayLine);

                                if (openOnly.Success)
                                {
                                    startTime = openOnly.Groups[2].ToString();
                                    endTime = "";
                                    theSeries = openOnly.Groups[3].ToString();
                                    theEvent = openOnly.Groups[4].ToString();
                                    //nesTextBox.AppendText(startTime + theSeries + theEvent + "\n");
                                    
                                    CreateIcalEvent(startTime, endTime, theDate, raceTrack, theEvent, theSeries);
                                }
                                if (openAndClose.Success)
                                {
                                    startTime = openAndClose.Groups[2].ToString();
                                    endTime = openAndClose.Groups[3].ToString();
                                    theSeries = openAndClose.Groups[4].ToString();
                                    theEvent = openAndClose.Groups[5].ToString();
                                    //nesTextBox.AppendText(startTime + endTime + theSeries + theEvent + "\n");

                                    CreateIcalEvent(startTime, endTime, theDate, raceTrack, theEvent, theSeries);
                                }
                            }
                        }
                    }
                }
            }
        }



        public List<List<string>> DailySchedule(List<string> lessLines)
        {

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


                if (!match1.Success && !match2.Success)
                {
                    lessLines.Add(line);
                }
            }

            return lessLines;
        }
    }
}
