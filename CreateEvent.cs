using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
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

        public void ExportMasterCalender()
        {

            Calendar masterCalendar = new Calendar();

            Log.Information("ExportMasterCalendar Called");
            DataTable allEvents = GetEvents(); // From DB.cs
           
            string theYear = "";

            // Go through all the events in the database
            foreach (DataRow eventsRow in allEvents.Rows)
            {
                var e = new CalendarEvent();
                // store the row columns in variables
                string fileName = eventsRow["FILENAME"].ToString();
                string theDate = eventsRow["DATE"].ToString();
                DateTime now = DateTime.Parse(eventsRow["START"].ToString());
                string laterStr = eventsRow["END"].ToString();
                string raceTrack = eventsRow["TRACK"].ToString();
                string theEvent = eventsRow["EVENT"].ToString();
                string theSeries = eventsRow["SERIES"].ToString();
                string timeZone = eventsRow["TIMEZONE"].ToString();
                theYear = eventsRow["YEAR"].ToString();


                e.Start = new CalDateTime(now, timeZone);
                e.Summary = theSeries + " | " + ChangeSeriesSymbols(theSeries) + " | " + ChangeSeriesSymbols(theEvent);
                e.Description = raceTrack + " | " + theSeries + " | " + ChangeSeriesSymbols(theSeries) + " | " +
                                ChangeSeriesSymbols(theEvent);
          
            

                if (laterStr != "")
                {
                    DateTime later = DateTime.Parse(laterStr);
                    e.End = new CalDateTime(later, timeZone);

                }


                if (theEvent.Contains("GARAGE OPEN") || theEvent.Contains("GARAGE CLOSE"))
                {
                    e.End = new CalDateTime(now.AddMinutes(15), timeZone);
                }


                masterCalendar.Events.Add(e); // add event to the master calendar.



            }
            masterCalendar.AddProperty("X-WR-CALNAME", "Master NASCAR Event Schedule");
            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(masterCalendar);
            string outputFileName = Properties.Settings.Default.outputPath + Path.DirectorySeparatorChar + " Master NASCAR Event Schedule";
            try
            {
                File.WriteAllText(outputFileName + ".ics", serializedCalendar);
            }
            catch (Exception ex)
            {
                Log.Error("Error while writing master calendar file: " + ex);
            }
            masterCalendar.Dispose();
        }
    }
}
