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
        // Todo:    2. Correctly handle event end times that are in parentheses, which should be discarded.
        // Todo:    3. Play with the garage open and close events end times. i.e... 15 minutes.
        // Todo:    --  Added, however doesn't display properly (text not fitting in small 15 minute space) in all views. Might add this as a user parameter instead of hardcoded time.
        // Todo:    4. Testing and bug fixes.
        // Todo:    5. Polish it up.
        // Todo:    6. The future... do multiple files at once.

        public MainWindow()
        {
            InitializeComponent();
     
        }

        // get the number of pages

        static Calendar _calendar = new Calendar();
        static string _timeZone = "Eastern Standard Time";
        
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
            Regex theYearRegex = new Regex(@"(\d{4,4})");
            Match theYearMatch = theYearRegex.Match(raceTrack); 

            Regex theMonthRegex = new Regex(@"\w*, (\w*) (\d*)"); // get the month and day in groups
            Match theMonthMatch = theMonthRegex.Match(theDate); // convert month
            var theMonthString = theMonthMatch.Groups[1];

            var theDay = theMonthMatch.Groups[2];
            var now = DateTime.Parse(theDate + " " + theYearMatch + " " + startTime);

            var e = new CalendarEvent
            {
                Start = new CalDateTime(now, _timeZone),
                Summary = ChangeSeriesSymbols(theSeries) + " | " + ChangeSeriesSymbols(theEvent),
                Description = raceTrack + " | " + ChangeSeriesSymbols(theSeries) + " | " + ChangeSeriesSymbols(theEvent),
            };


            if (endTime != "")
            {
                var later = DateTime.Parse(theDate + " " + theYearMatch + " " + endTime);
                e.End = new CalDateTime(later, _timeZone);
            }

            if (theEvent.Contains("GARAGE OPEN") || theEvent.Contains("GARAGE CLOSE"))
            {
                e.End = new CalDateTime(now.AddMinutes(15), _timeZone);
            }
            
            //e.Description = raceTrack + " | " + ChangeSeriesSymbols(theSeries) + " | " + ChangeSeriesSymbols(theEvent);

            //nesTextBox.AppendText(ChangeSeriesSymbols(theSeries) + "\n\n");
            _calendar.Events.Add(e);

        }

        public string ChangeSeriesSymbols(string toChange)
        {
            var seriesDictionary = new Dictionary<string, string>
            {
                { "NKNPS-E", "K & N Series" },
                { "ARCA", "Arca Series" },
                { "NGOTS", "Truck Series" },
                { "NXS", "Xfinity Series" },
                { "MENCS", "Cup Series" }
            };
            string theChanged = toChange;
            foreach (KeyValuePair<string, string> entry in seriesDictionary)
            {
               
                if (toChange.Contains(entry.Key))
                {
                    theChanged = theChanged.Replace(entry.Key, entry.Value);
                    //nesTextBox.AppendText("change detected");
                }
            }

            return theChanged;

        }

        public string TimeZone(string theText)
        {
            string nodaTimeZone = "";
            var tzDictionary = new Dictionary<string, string>
            {
                { "Eastern Standard Time", "America/Indiana/Indianapolis" },
                { "Central Standard Time", "US/Central" },
                { "Pacific Standard Time", "US/Pacific" },
                { "Mountain Standard Time", "America/Denver" }
           
            };

            foreach (KeyValuePair<string, string> entry in tzDictionary)
            {
                if (theText.Contains(entry.Key))
                {
                    nodaTimeZone = entry.Value;
                }

            }

            _timeZone = nodaTimeZone;
            return nodaTimeZone;
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            
            string theText = "";
            nesTextBox.Text = "";
            string raceTrack = "";
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                InitialDirectory = Properties.Settings.Default.sourcePath,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Get the number of pages.
                var numberOfPages = NumberOfPages(openFileDialog.FileName.ToString());

                // Get the text, line by line
                for (int i = 1; i <= numberOfPages; i++)
                {
                    theText += PageText(i, openFileDialog.FileName.ToString()) + "\n";
                }

                var nadaTomeTimeZone = TimeZone(theText);
                

                List<string> lines = theText.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                ).ToList();

                List<string> lessLines = LessLines(lines); // remove some of the unneeded lines.

                raceTrack = lessLines[0]; // The Race Track/Series

                List<List<string>> allDays = DailySchedule(lessLines); // all events separated by day

                //Regex filter1 = new Regex(@"((\d{1,2}:\d{2,2} \w{2,2}) \(*(\d{1,2}:\d{1,2} \w{2,2}))\)? (\S*) (.*)"); // matches events with open and close times -- 20190827 - Now accounts for parentheses
                //Regex filter2 = new Regex(@"((^\d{1,2}:\d{2,2} \w{2,2}) (?!\(?\d{1,2}:\d{2,2} \w{2,2})(\S*)(.*))"); // matches events with just an open time
                // ((\d{1,2}:\d{2,2} \w{2,2})( \(*(\d{1,2}:\d{1,2} \w{2,2}))*)\)? (\S*(, \S*)*) (.*)
                Regex filter3 = new Regex(@"((\d{1,2}:\d{2,2} \w{2,2})( \(*(\d{1,2}:\d{1,2} \w{2,2}))*)\)? ([\w-]+(, [\w-]+)*) (.*)");
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

                            if (oneDayLine.Contains("PRACTICE") || oneDayLine.Contains("GARAGE OPEN") || oneDayLine.Contains("RACE") || oneDayLine.Contains("QUALIFYING"))
                            {
                                //Match openAndClose = filter1.Match(oneDayLine);
                                //Match openOnly = filter2.Match(oneDayLine);
                                Match masterMatch = filter3.Match(oneDayLine);

                                // attempt to unite the two filters into one, while accounting for multiple series per event.     
                                if (masterMatch.Success)
                                {
                                    startTime = masterMatch.Groups[2].ToString();
                                    endTime = masterMatch.Groups[4].ToString(); // Not group 3 because it may contain a "(".
                                    theSeries = masterMatch.Groups[5].ToString();
                                    theEvent = masterMatch.Groups[7].ToString();

                                    //nesTextBox.AppendText(startTime + endTime + "\t\t" + theSeries + "\t\t" + theEvent + "\n");
                                    if (oneDayLine.Contains("GARAGE OPEN"))
                                    {
                                     
                                        CreateIcalEvent(startTime, "", theDate, raceTrack, theEvent, theSeries);

                                        if (endTime != "") // If the GARAGE OPEN even contains an endTime;
                                        {
                                            CreateIcalEvent(endTime, "", theDate, raceTrack, "GARAGE CLOSES", theSeries);
                                        }
                                        
                                    }
                                    else
                                    {
                                        CreateIcalEvent(startTime, endTime, theDate, raceTrack, theEvent, theSeries);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            _calendar.AddProperty("X-WR-CALNAME", raceTrack);
            //_calendar.AddTimeZone(new VTimeZone("America/New_York"));


          

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(_calendar);

            nesTextBox.AppendText(serializedCalendar);
            //myString = myString.Substring(0, myString.Length-3);
            string saveFileName= openFileDialog.FileName.ToString()
                .Substring(0, openFileDialog.FileName.ToString().Length - 4);
            File.WriteAllText(@saveFileName + ".ics", serializedCalendar);
            _calendar.Dispose();
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

        //sourcePath


        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            using (var fldrDlg = new FolderBrowserDialog())
            {
                //fldrDlg.Filter = "Png Files (*.png)|*.png";
                //fldrDlg.Filter = "Excel Files (*.xls, *.xlsx)|*.xls;*.xlsx|CSV Files (*.csv)|*.csv"
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
