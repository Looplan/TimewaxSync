using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using TimewaxSync.TimewaxWebApi;
using TimewaxSync.TimewaxWebApi.Data;
using static TimewaxSync.Tools.TimewaxExcelSync;

namespace TimewaxSync.TimewaxWebApi
{
    public class TimewaxApi
    {

        private static TimewaxApi instance = null;

        public TokenObject Token { get; set; }

        public Authenticator Authenticator { get; set; }
       
        public bool ErrorDisplay { get; set; }

        private TimewaxApi()
        {

        }

        public static XElement ToXElement<T>(object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter streamWriter = new StreamWriter(memoryStream))
                {
                    var xmlSerializer = new XmlSerializer(typeof(T));
                    xmlSerializer.Serialize(streamWriter, obj);
                    return XElement.Parse(Encoding.ASCII.GetString(memoryStream.ToArray()));
                }
            }
        }

        private void displayErrorIfNeeded()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TimewaxSync\log.xml";
            if (!ErrorDisplay)
            {
                ErrorDisplay = true;
                DialogResult result = MessageBox.Show("Error occured, please check the logs at " + path);
                ErrorDisplay = false;
            }
        }
     
        private string logApiError(XElement requestContent, XElement responseContent, bool messageBox = true)
        {
            
            XElement error = new XElement("error",
                new XElement("id", Guid.NewGuid().ToString()),
                new XElement("time", DateTime.Now.ToLongTimeString()),
                new XElement("token", ToXElement<TokenObject>(Token)),
                new XElement(ToXElement<Authenticator>(Authenticator)),
                requestContent, 
                responseContent
                );

            if(messageBox)
            {
                displayErrorIfNeeded();
            }

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TimewaxSync\log.xml";
            File.AppendAllText(path, error.ToString() + "\n");

            return error.ToString();
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
            } else
            {
                if(root.Element("errors").Value == "invalid_password")
                {
                    MessageBox.Show("Invalid credentials, please change them under settings at the Timewax Tab to the most recent one.");
                    logApiError(requestContent, root, false);
                }
                return;
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

            if (root.Element("valid").Value == "yes")
            {
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
            else
            {
                string error = logApiError(requestContent, root);
                throw new Exception(error);
            }         
        }

        public async Task<List<Breakdown>> GetProjectActivities(string Project)
        {
            try
            {
                HttpClient client = new HttpClient();
                var requestContent = new XElement("request",
                    new XElement("token", Token.Token),
                    new XElement("project", Project));
                var response = await client.PostAsync("https://api.timewax.com/project/breakdown/list/", new StringContent(requestContent.ToString()));
                var responseContent = await response.Content.ReadAsStringAsync();
                XElement root = XElement.Parse(responseContent);

                if (root.Element("valid").Value == "yes")
                {
                    var xmlEntries = root.Element("breakdowns").Elements();

                    XmlSerializer serializer = new XmlSerializer(typeof(Breakdown));

                    List<Breakdown> breakdownEntries = new List<Breakdown>();

                    foreach (var xmlEntry in xmlEntries)
                    {
                        Breakdown breakdown = serializer.Deserialize(new StringReader(xmlEntry.ToString())) as Breakdown;
                        breakdownEntries.Add(breakdown);
                    }
                    return breakdownEntries;
                }
                else
                {
                    string error = logApiError(requestContent, root);
                    throw new Exception(error);
                }               
            } catch (Exception exception)
            {
                throw exception;
            }           
        }

        public async Task<List<ProjectInfo>> ListProjects(bool isActive = false)
        {
            HttpClient client = new HttpClient();
            var requestContent = new XElement("request",
                new XElement("token", Token.Token),
                new XElement("isActive", isActive ? "yes" : "no"));
            var response = await client.PostAsync("https://api.timewax.com/project/list/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);

            if (root.Element("valid").Value == "yes")
            {
                var xmlEntries = root.Element("projects").Elements();

                XmlSerializer serializer = new XmlSerializer(typeof(ProjectInfo));
                List<ProjectInfo> projectEntries = new List<ProjectInfo>();

                foreach (var xmlEntry in xmlEntries)
                {
                    ProjectInfo calendarEntry = serializer.Deserialize(new StringReader(xmlEntry.ToString())) as ProjectInfo;
                    projectEntries.Add(calendarEntry);
                }
                return projectEntries;
            }
            else
            {
                string error = logApiError(requestContent, root);
                throw new Exception(error);
            }         
        }
    
        public async Task<Project> GetProject(string identifier)
        {
            HttpClient client = new HttpClient();
            var requestContent = new XElement("request",
                new XElement("token", Token.Token), new XElement("project", identifier));
            var response = await client.PostAsync("https://api.timewax.com/project/get/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);


            XmlSerializer serializer = new XmlSerializer(typeof(Project));

            return null;
        }

        public async Task<XElement> EditCalendarEntry(XElement content)
        {
            HttpClient client = new HttpClient();
            var requestContent = new XElement("request", content, new XElement("token", Token.Token));

            var response = await client.PostAsync("https://api.timewax.com/calendar/entries/edit/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);

            if (root.Element("valid").Value == "no")
            {
                logApiError(requestContent, root);
            }
            return root;
        }

        public async Task<XElement> AddCalendarEntry(XElement entry)
        {
            HttpClient client = new HttpClient();
            var requestContent = new XElement("request", entry, new XElement("token", Token.Token));

            var response = await client.PostAsync("https://api.timewax.com/calendar/entries/add/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);

            if (root.Element("valid").Value == "no")
            {
                string error = logApiError(requestContent, root);
            }

            return root;
        }

        public async Task<XElement> RemoveCalendarEntry(string id)
        {
            HttpClient client = new HttpClient();
            var requestContent = new XElement("request", new XElement("entry", new XElement("id", id)), new XElement("token", Token.Token));

            var response = await client.PostAsync("https://api.timewax.com/calendar/entries/delete/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);

            if (root.Element("valid").Value == "no")
            {
                logApiError(requestContent, root);
            }
            return root;
        }

        public static TimewaxApi Instance {
            get {
                if (instance == null) { instance = new TimewaxApi(); }
                return instance;
            }
        }

    }
}
