using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Serilog;

namespace NESPTI
{
    public partial class ConvertToIcal
    {

        public void CreateIcalEvent(string startTime, string endTime, string theDate, string raceTrack, string theEvent, string theSeries)
        {
            Log.Information("CreateIcalEvent Begin: StartTime:" + startTime + " endTime: " + endTime + " theDate:" + theDate + " raceTrack: " + raceTrack + " theEvent: " + theEvent + " theSeries: " + theSeries + " TimeZone: " + _timeZone);
            Regex theYearRegex = new Regex(@"(\d{4,4})");
            Match theYearMatch = theYearRegex.Match(raceTrack);

            var now = DateTime.Parse(theDate + " " + theYearMatch + " " + startTime);
            var later = new DateTime(); // 20191002 check if breaks - delete declaration if breaks and initialize below.
            var laterStr = "";

            var e = new CalendarEvent
            {
                Start = new CalDateTime(now, _timeZone),
                Summary = ChangeSeriesSymbols(theSeries) + " | " + ChangeSeriesSymbols(theEvent),
                Description = raceTrack + " | " + ChangeSeriesSymbols(theSeries) + " | " + ChangeSeriesSymbols(theEvent),
            };

            if (endTime != "")
            {
                later = DateTime.Parse(theDate + " " + theYearMatch + " " + endTime); // 20191002 check if breaks  - initialize here if breaks
                e.End = new CalDateTime(later, _timeZone);
                laterStr = later.ToString();
            }
            else
            {
                laterStr = "";
            }

            if (theEvent.Contains("GARAGE OPEN") || theEvent.Contains("GARAGE CLOSE"))
            {
                e.End = new CalDateTime(now.AddMinutes(15), _timeZone);
            }

            //FILENAME, DATE, START, END, TRACK, EVENT, SERIES, TIMEZONE, YEAR
            string fileName = FileName();
            AddEvents(fileName, theDate, now.ToString(), laterStr, raceTrack, theEvent, theSeries, theYearMatch.ToString());
            _calendar.Events.Add(e);

        }

        


    }
}
