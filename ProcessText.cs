using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NESPTI
{
    public partial class ConvertToIcal
    {
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

    }
}
