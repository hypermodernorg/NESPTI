using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace NESPTI
{
    public partial class ConvertToIcal
    {
        // connect to the database and return connection.
        public SQLiteConnection Connect()
        {
            SQLiteConnection conn;
            string dbDirectory = AppDomain.CurrentDomain.BaseDirectory + "Resources" + Path.DirectorySeparatorChar + "db" + Path.DirectorySeparatorChar + "NESPTI.db";
            conn = new SQLiteConnection("Data Source=" + dbDirectory + "; Version=3;New=True;Compress=True;");
            conn.Open();
            return conn;
        }

        // Get all the events in the database
        public DataTable GetEvents()
        {
            Task.Delay(500).Wait();
            SQLiteConnection conn = Connect(); // connect to the database
            SQLiteCommand sqliteCmd = conn.CreateCommand();
            sqliteCmd.CommandText = "SELECT ID, FILENAME, DATE, START, END, TRACK, EVENT, SERIES, TIMEZONE, YEAR FROM EVENTS";
            SQLiteDataAdapter dt = new SQLiteDataAdapter(sqliteCmd);
            DataTable calEvents = new DataTable();
            dt.Fill(calEvents);
            conn.Close(); // close database connection
            return calEvents;
        }

        // Method to check if filename already exist in the calendar.
        public DataTable CheckForFilename(string fileName)
        {
            SQLiteConnection conn = Connect(); // connect to the database
            SQLiteCommand sqliteCmd = conn.CreateCommand();
            sqliteCmd.CommandText = $"SELECT ID, FILENAME FROM EVENTS WHERE FILENAME = {fileName}"; 
            SQLiteDataAdapter dt = new SQLiteDataAdapter(sqliteCmd);
            DataTable calEvents = new DataTable();
            dt.Fill(calEvents);
            conn.Close(); // close database connection
            return calEvents;
        }

        // Method to delete the old events and replace with new.
        public void DeleteEvents(string fileName)
        {
            Task.Delay(500).Wait();
            SQLiteConnection conn = Connect();
            SQLiteCommand sqliteCmd = conn.CreateCommand();
            sqliteCmd.CommandText = $"DELETE FROM EVENTS WHERE FILENAME = '{fileName}'";
            sqliteCmd.ExecuteNonQuery();
            conn.Close();
        }

        // Method to write new or updated events
        public void AddEvents(string fileName, string theDate, string now, string later, string raceTrack, string theEvent, string theSeries, string theYearMatch) // all the data parameters here
        {
            Task.Delay(100).Wait();
            // Probably will be called in a loop to add events one by one
            SQLiteConnection conn = Connect();
            SQLiteCommand sqliteCmd = conn.CreateCommand();

            //AddEvents(fileName, theDate, now.ToString(), later, raceTrack, theEvent, theSeries,_timezone, theYearMatch);
            sqliteCmd.CommandText = $"INSERT INTO EVENTS (FILENAME, DATE, START, END, TRACK, EVENT, SERIES, TIMEZONE, YEAR) VALUES ('{fileName}', '{theDate}', '{now}', '{later}', '{raceTrack}', '{theEvent}', '{theSeries}', '{_timeZone}', '{theYearMatch}')";
            sqliteCmd.ExecuteNonQuery();
            conn.Close();
        }

    }
}
