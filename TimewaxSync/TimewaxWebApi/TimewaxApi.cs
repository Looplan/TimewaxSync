using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using TimewaxSync.TimewaxWebApi;

namespace TimewaxSync.TimewaxWebApi
{
    public class TimewaxApi
    {

        private static TimewaxApi instance = null;

        public TokenObject Token { get; set; }

        public Authenticator Authenticator { get; set; }

        private TimewaxApi()
        {

        }

        public async Task Authenticate()
        {
            HttpClient client = new HttpClient();

            var requestContent = new XElement("request", 
                new XElement("client", Authenticator.Client), 
                new XElement("username", Authenticator.Username),
                new XElement("password", Authenticator.Password));
            var response = await client.PostAsync("https://api.timewax.com/authentication/token/get/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);
            if(root.Element("valid").Value == "yes")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TokenObject));
                Token = serializer.Deserialize(new StringReader(root.ToString())) as TokenObject;
            }
        }

        public async Task<List<CalendarEntry>> GetCalendarEntries(string DateFrom, string DateTo)
        {
            HttpClient client = new HttpClient();
            var requestContent = new XElement("request",
                new XElement("token", Token.Token),
                new XElement("dateFrom", DateFrom),
                new XElement("dateTo", DateTo));
            var response = await client.PostAsync("https://api.timewax.com/calendar/entries/list/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);
            var xmlEntries = root.Element("entries").Elements();

            XmlSerializer serializer = new XmlSerializer(typeof(CalendarEntry));

            List<CalendarEntry> calendarEntries = new List<CalendarEntry>();

            foreach (var xmlEntry in xmlEntries)
            {
                CalendarEntry calendarEntry = serializer.Deserialize(new StringReader(xmlEntry.ToString())) as CalendarEntry;
                calendarEntries.Add(calendarEntry);
            }
            return calendarEntries;
        }

        public async Task<List<Breakdown>> GetProjectActivities(string Project)
        {
            HttpClient client = new HttpClient();
            var requestContent = new XElement("request",
                new XElement("token", Token.Token),
                new XElement("project", Project));
            var response = await client.PostAsync("https://api.timewax.com/project/breakdown/list/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);
            var xmlEntries = root.Element("breakdowns").Elements();

            XmlSerializer serializer = new XmlSerializer(typeof(Breakdown));

            List<Breakdown> breakdownEntries = new List<Breakdown>();

            foreach (var xmlEntry in xmlEntries)
            {
                Breakdown calendarEntry = serializer.Deserialize(new StringReader(xmlEntry.ToString())) as Breakdown; 
                breakdownEntries.Add(calendarEntry);
            }
            return breakdownEntries;

        }

        public async Task ListProjects()
        {
            HttpClient client = new HttpClient();
            var requestContent = new XElement("request",
                new XElement("token", Token.Token));
            var response = await client.PostAsync("https://api.timewax.com/project/list/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);
        }
    
        public static TimewaxApi Instance {
            get {
                if (instance == null) { instance = new TimewaxApi(); }
                return instance;
            }
        }



    }
}
