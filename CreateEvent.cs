using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace NESPTI
{
    public partial class ConvertToIcal
    {

        public void CreateIcalEvent(string startTime, string endTime, string theDate, string raceTrack, string theEvent, string theSeries)
        {
            Regex theYearRegex = new Regex(@"(\d{4,4})");
            Match theYearMatch = theYearRegex.Match(raceTrack);
            Regex theMonthRegex = new Regex(@"\w*, (\w*) (\d*)"); // get the month and day in groups
            Match theMonthMatch = theMonthRegex.Match(theDate); // convert month
            //var theMonthString = theMonthMatch.Groups[1];
            //var theDay = theMonthMatch.Groups[2];
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

            _calendar.Events.Add(e);

        }


    }
}
