using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
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
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly SemaphoreSlim throttleSemaphore = new SemaphoreSlim(1, 1);
        private static DateTime lastRequestTime = DateTime.MinValue;
        private static readonly TimeSpan minRequestInterval = TimeSpan.FromMilliseconds(200);
        private const int maxRetries = 3;

        static TimewaxApi()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 10;
        }

        public TokenObject Token { get; set; }

        public Authenticator Authenticator { get; set; }

        public bool ErrorDisplay { get; set; }

        private TimewaxApi()
        {

        }

        private static async Task<HttpResponseMessage> ThrottledPostAsync(string url, HttpContent content)
        {
            await throttleSemaphore.WaitAsync();
            try
            {
                for (int attempt = 0; attempt <= maxRetries; attempt++)
                {
                    var elapsed = DateTime.UtcNow - lastRequestTime;
                    if (elapsed < minRequestInterval)
                    {
                        await Task.Delay(minRequestInterval - elapsed);
                    }

                    lastRequestTime = DateTime.UtcNow;
                    var response = await httpClient.PostAsync(url, content);

                    if (response.StatusCode == (HttpStatusCode)429)
                    {
                        if (attempt == maxRetries)
                            throw new HttpRequestException("Timewax API rate limit exceeded after " + (maxRetries + 1) + " attempts.");

                        var retryAfter = response.Headers.RetryAfter?.Delta;
                        var delay = retryAfter ?? TimeSpan.FromSeconds(Math.Pow(2, attempt + 1));
                        await Task.Delay(delay);
                        continue;
                    }

                    return response;
                }

                throw new HttpRequestException("Timewax API request failed after retries.");
            }
            finally
            {
                throttleSemaphore.Release();
            }
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
            ErrorDisplay = true;
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
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.AppendAllText(path, error.ToString() + "\n");

            return error.ToString();
        }

        public async Task Authenticate()
        {
            var requestContent = new XElement("request", 
                new XElement("client", Authenticator.Client), 
                new XElement("username", Authenticator.Username),
                new XElement("password", Authenticator.Password));


            HttpResponseMessage httpResponseMessage = null;
            string responseContent = string.Empty;
            try {
                httpResponseMessage = await ThrottledPostAsync("https://api.timewax.com/authentication/token/get/", new StringContent(requestContent.ToString()));
                responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                throw;
            }
                

            XElement root = XElement.Parse(responseContent);
            if(root.Element("valid").Value == "yes")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TokenObject));
                Token = serializer.Deserialize(new StringReader(root.ToString())) as TokenObject;
            } else
            {
                if(root.Element("errors").Value == "invalid_password")
                {
                    logApiError(requestContent, root, false);
                    throw new Exception("Invalid credentials, please change them under settings at the Timewax Tab to the most recent one.");
                }
                return;
            }

        }

        public async Task<List<CalendarEntry>> GetCalendarEntries(string DateFrom, string DateTo)
        {
            var requestContent = new XElement("request",
                new XElement("token", Token.Token),
                new XElement("dateFrom", DateFrom),
                new XElement("dateTo", DateTo));
            var response = await ThrottledPostAsync("https://api.timewax.com/calendar/entries/list/", new StringContent(requestContent.ToString()));
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
            var requestContent = new XElement("request",
                new XElement("token", Token.Token),
                new XElement("project", Project));

            HttpResponseMessage response;
            try
            {
                response = await ThrottledPostAsync("https://api.timewax.com/project/breakdown/list/", new StringContent(requestContent.ToString()));
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send request to Timewax API.", ex);
            }

            string responseContent;
            try
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to read response content from Timewax API.", ex);
            }

            XElement root;
            try
            {
                root = XElement.Parse(responseContent);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse XML response from Timewax API: " + responseContent, ex);
            }

            if (root.Element("valid").Value != "yes")
            {
                string error = logApiError(requestContent, root);
                throw new Exception(error);
            }

            try
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
            catch (Exception ex)
            {
                throw new Exception("Failed to deserialize breakdown entries from Timewax API response.", ex);
            }
        }

        public async Task<List<ProjectInfo>> ListProjects(bool isActive = false)
        {
            var requestContent = new XElement("request",
                new XElement("token", Token.Token),
                new XElement("isActive", isActive ? "yes" : "no"));
            var response = await ThrottledPostAsync("https://api.timewax.com/project/list/", new StringContent(requestContent.ToString()));
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
            var requestContent = new XElement("request",
                new XElement("token", Token.Token), new XElement("project", identifier));
            var response = await ThrottledPostAsync("https://api.timewax.com/project/get/", new StringContent(requestContent.ToString()));
            var responseContent = await response.Content.ReadAsStringAsync();
            XElement root = XElement.Parse(responseContent);


            XmlSerializer serializer = new XmlSerializer(typeof(Project));

            return null;
        }

        public async Task<XElement> EditCalendarEntry(XElement content)
        {
            var requestContent = new XElement("request", content, new XElement("token", Token.Token));

            var response = await ThrottledPostAsync("https://api.timewax.com/calendar/entries/edit/", new StringContent(requestContent.ToString()));
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
            var requestContent = new XElement("request", entry, new XElement("token", Token.Token));

            var response = await ThrottledPostAsync("https://api.timewax.com/calendar/entries/add/", new StringContent(requestContent.ToString()));
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
            var requestContent = new XElement("request", new XElement("entry", new XElement("id", id)), new XElement("token", Token.Token));

            var response = await ThrottledPostAsync("https://api.timewax.com/calendar/entries/delete/", new StringContent(requestContent.ToString()));
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
