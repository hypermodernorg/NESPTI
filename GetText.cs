using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using Ical.Net.Serialization;
using Microsoft.Win32;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace NESPTI
{
    public partial class MainWindow
    {

    // Get all the text.
    public string[] GetAllText()
        {
            string theText = ""; // Clear the existing text before use.
            nesTextBox.Text = ""; // Clear the textbox before use.
            string saveFileName = "";



            // Check if the window is visible, if so, get the text this way through user dialogue.
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
            {
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
                }
                saveFileName = openFileDialog.FileName.ToString()
                    .Substring(0, openFileDialog.FileName.ToString().Length - 4);
            }

            // If the window is not visible, then automatically check for new files.
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible == false)
            {
                // Check for new files
            }

            string[] getAllText = new string[] {theText, saveFileName};
            return getAllText;
        }

        public void ConverterMain()
        {
            
            string [] getAllText = GetAllText();
            string theText = getAllText[0];
            string saveFileName = getAllText[1];
            string raceTrack = "";
            
            var nadaTomeTimeZone = TimeZone(theText); // Get the pdf timezone, and translate it to nada format.

            // Split the string by lines into a list of string.
            List<string> lines = theText.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            ).ToList();

            List<string> lessLines = LessLines(lines); // Filter some unneeded lines.

            raceTrack = lessLines[0]; // The Race Track/Series

            List<List<string>> allDays = DailySchedule(lessLines); // all events separated by day


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
            
            _calendar.AddProperty("X-WR-CALNAME", raceTrack);
            //_calendar.AddTimeZone(new VTimeZone("America/New_York"));

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(_calendar);

            nesTextBox.AppendText(serializedCalendar);
        
            //string saveFileName = openFileDialog.FileName.ToString()
            //    .Substring(0, openFileDialog.FileName.ToString().Length - 4);
            File.WriteAllText(@saveFileName + ".ics", serializedCalendar);
            _calendar.Dispose();
        }
    }
    
}
