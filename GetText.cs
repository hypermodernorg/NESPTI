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
using Ical.Net;
using Ical.Net.Serialization;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Win32;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Timers;
using Common.Logging.Configuration;
using Serilog;

namespace NESPTI
{
    public partial class ConvertToIcal
    {
        // variables shared between all methods.
        static Calendar _calendar = new Calendar();
        private static string _timeZone = "";
        private static string _theText = "";
        private static string _fileName = ""; // added to send to db.
        private string _currentYear = DateTime.Now.Year.ToString();


        // Get all the text.
        public string[] GetAllText(FileSystemEventArgs e = null)
        {
            _theText = ""; // Clear the existing text before use.
            string saveFileName = "";


            // Check if the window is visible, if so, get the text this way through user dialogue.
            if (e == null)
            {
                Log.Information("Begin User Selected File Process");
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    InitialDirectory = Properties.Settings.Default.sourcePath,
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    // Get the number of pages.
                    Log.Information("File opened: " + openFileDialog.FileName.ToString());
                    var numberOfPages = NumberOfPages(openFileDialog.FileName.ToString());
                    Log.Information("Number of pages: " + numberOfPages);


                    // Get the text, page by page
                    Log.Information("Begin page by page text extraction.");
                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        _theText += PageText(i, openFileDialog.FileName.ToString()) + "\n";
                        Log.Information("Extracted page: " + i);
                    }

                    Log.Information("End page by page text extraction.");
                }

                try
                {
                    saveFileName = openFileDialog.FileName.ToString()
                        .Substring(0, openFileDialog.FileName.ToString().Length - 4);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "There was most likely no file selected.");
                }

            
                _fileName = Path.GetFileName(openFileDialog.FileName.ToString());
                string fileName = FileName();
                DeleteEvents(fileName); // delete any existing records with this filename and prepare to replace.
            }

            // If the window is not visible, then automatically check for new files.
            if (e != null)
            {
                // Get the number of pages
                Log.Information("File opened: " + e.FullPath.ToString());
                var numberOfPages = NumberOfPages(e.FullPath);
                Log.Information("Number of pages: " + numberOfPages);


                Log.Information("Begin page by page text extraction.");
                for (int i = 1; i <= numberOfPages; i++)
                {
                    _theText += PageText(i, e.FullPath) + "\n";
                    Log.Information("Extracted page: " + i);
                }

                Log.Information("End page by page text extraction.");

                saveFileName = e.FullPath.Substring(0, e.FullPath.Length - 4);
                _fileName = e.Name;
                string fileName = FileName();
                DeleteEvents(fileName); // delete any existing records with this filename and prepare to replace.
            }

            TimeZone(_theText); // Scan the text for a timezone.
            string[] getAllText = new string[] {_theText, saveFileName};
            return getAllText;
        }

        public void ConverterMain(FileSystemEventArgs e = null)
        {
            string[] getAllText = GetAllText(e);
            _theText = getAllText[0];
            string saveFileName = getAllText[1];
            string raceTrack = "";
            string fileName = FileName();

            // Split the string by lines into a list of string.
            List<string> lines = _theText.Split(
                new[] {"\r\n", "\r", "\n"},
                StringSplitOptions.None
            ).ToList();
            Log.Information("Text to List of strings.");

            List<string> lessLines = LessLines(lines); // Filter some unneeded lines.

            raceTrack = lessLines[0]; // The Race Track/Series

            List<List<string>> allDays = DailySchedule(lessLines); // all events separated by day


            Regex filter3 =
                new Regex(@"((\d{1,2}:\d{2,2} \w{2,2})( \(*(\d{1,2}:\d{1,2} \w{2,2}))*)\)? ([\w-]+(, [\w-]+)*) (.*)");
            foreach (List<string> oneDay in allDays)
            {
                var theDate = oneDay[0];

                foreach (string oneDayLine in oneDay)
                {
                    string startTime;
                    string endTime;
                    string theEvent;
                    string theSeries;

                    if (oneDayLine.Contains("ARCA") || oneDayLine.Contains("NGOTS") || oneDayLine.Contains("NCS") ||
                        oneDayLine.Contains("NXS") || oneDayLine.Contains("MKNPS"))
                    {
                        if (oneDayLine.Contains("PRACTICE") || oneDayLine.Contains("GARAGE OPEN") ||
                            oneDayLine.Contains("RACE") || oneDayLine.Contains("QUALIFYING"))
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

                                // Begin attempt to account for multiple events

                                Array theSeriesArr = theSeries.Split(',');

                                foreach (string individualSeries in theSeriesArr)
                                {
                                    theSeries = individualSeries.Trim();
                                    // This handles the case where the endtime is enclosed in parentheses. 
                                    // Parentheses in the endtime simply indicates the start time in eastern standard time, and can be discarded.
                                    if (masterMatch.Groups[3].ToString().Contains("("))
                                    {
                                        endTime = "";
                                    }

                                    //nesTextBox.AppendText(startTime + endTime + "\t\t" + theSeries + "\t\t" + theEvent + "\n");
                                    if (oneDayLine.Contains("GARAGE OPEN"))
                                    {
                                        CreateIcalEvent(startTime, "", theDate, raceTrack, theEvent, theSeries);

                                        if (endTime != "") // If the GARAGE OPEN even contains an endTime;
                                        {
                                            CreateIcalEvent(endTime, "", theDate, raceTrack, "GARAGE CLOSES",
                                                theSeries);
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


            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(_calendar);


            if (e == null)
            {
                File.WriteAllText(@saveFileName + ".ics", serializedCalendar);
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).openNesLbl.Content = "Created: " + saveFileName + ".ics";
                    }
                }

                try
                {
                    string sourceFile = saveFileName;

                    string destinationFile = Properties.Settings.Default.outputPath + Path.DirectorySeparatorChar +
                                             "processed" + Path.DirectorySeparatorChar + Path.GetFileName(saveFileName + ".pdf").ToUpper();
                    var i = 0;
                    while (File.Exists(destinationFile))
                    {
                        i++;
                        var destFileString = destinationFile.Substring(0, destinationFile.Length - 4) + "-" + i;
                        destinationFile = destFileString + ".pdf";
                    }

                    if (!File.Exists(destinationFile))
                    {
                        System.IO.File.Move(saveFileName + ".pdf", destinationFile);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error moving file to processed directory:");
                }


            }

            if (e != null)
            {
                Directory.CreateDirectory(Properties.Settings.Default.outputPath + Path.DirectorySeparatorChar +
                                          "ics_individual");
                string outputFileName = Properties.Settings.Default.outputPath + Path.DirectorySeparatorChar +
                                        "ics_individual" + Path.DirectorySeparatorChar +
                                        e.Name.Substring(0, e.Name.Length - 4);
                File.WriteAllText(outputFileName + ".ics", serializedCalendar);

                // Move the processed files to the processed folder
                System.IO.Directory.CreateDirectory(Properties.Settings.Default.outputPath +
                                                    Path.DirectorySeparatorChar + "processed");
                string sourceFile = Properties.Settings.Default.outputPath + Path.DirectorySeparatorChar + e.Name;
                string destinationFile = Properties.Settings.Default.outputPath + Path.DirectorySeparatorChar +
                                         "processed" + Path.DirectorySeparatorChar + fileName.ToUpper();

                // If processed file already exist in processed folder, append an indicator to the number of updates to the filename.
                var i = 0;
                while (File.Exists(destinationFile))
                {
                    i++;
                    var destFileString = destinationFile.Substring(0, destinationFile.Length - 4) + "-" + i;
                    destinationFile = destFileString + ".pdf";
                }

                if (!File.Exists(destinationFile))
                {
                    System.IO.File.Move(sourceFile, destinationFile);
                }
            }

            _calendar.Dispose();
            ExportMasterCalender();
        }

        // get the number of pages
        public int NumberOfPages(string filename)
        {
            int numberOfPages = 0;
            PdfDocument
                pdf = new PdfDocument(
                    new PdfReader(
                        filename)); // file access error here -- Solved by adding time between file detection and file open.
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
    }
}