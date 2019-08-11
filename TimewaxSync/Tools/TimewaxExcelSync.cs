using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TimewaxSync.TimewaxWebApi.Data;

namespace TimewaxSync.Tools
{
    public class TimewaxExcelSync
    {

        public struct ExcelEntryResult
        {
            public int ID { get; set; }

            public int TimeID { get; set; }

            public int Row { get; set; }

            public bool Valid { get; set; }
        }

        public TimewaxWebApi.TimewaxApi TimewaxApi { get; set; }

        public TimewaxExcelSync(TimewaxWebApi.TimewaxApi timewaxApi)
        {
            this.TimewaxApi = timewaxApi;
        }

        public async Task<List<ExcelEntry>> GetExcelEntries()
        {
            try
            {
                List<CalendarEntry> calendarEntries = await TimewaxApi.GetCalendarEntries("20160101", DateTime.Now.AddYears(1).ToString("yyyyMMdd"));

                List<ProjectInfo> projects = await TimewaxApi.ListProjects(true);

                List<Task> downloadTasks = new List<Task>();

                Dictionary<ProjectInfo, List<Breakdown>> pairs = new Dictionary<ProjectInfo, List<Breakdown>>();

                foreach (var project in projects)
                {
                    Task downloadTask = Task.Run(async () =>
                    {
                        List<Breakdown> breakdowns = await TimewaxApi.GetProjectActivities(project.Code);
                        pairs.Add(project, breakdowns);
                    });
                    Thread.Sleep(100);
                    downloadTasks.Add(downloadTask);
                }

                await Task.WhenAll(downloadTasks);

                List<ExcelEntry> result = new List<ExcelEntry>();

                foreach (var pair in pairs)
                {
                    ProjectInfo project = pair.Key;
                    foreach (var breakdown in pair.Value)
                    {
                        // Check if a CalendarEntry existed for the breakdown.
                        var associatedEntries = calendarEntries.Where((entry) =>
                        {
                            return (entry.Breakdown == breakdown.Name &&
                            (entry.BreakdownParent == breakdown.ParentName || string.IsNullOrEmpty(breakdown.ParentName)) &&
                            entry.BreakdownCode == breakdown.Code &&
                            entry.Project == project.Name);
                        }).ToList();

                        if(associatedEntries.Count == 0)
                        {
                            result.Add(new ExcelEntry()
                            {
                                ID = breakdown.ID,
                                Date = null,
                                TimeFrom = null,
                                TimeTo = null,
                                EmployeeCode = "",
                                Project = project.Name,
                                Phase = breakdown.Name,
                                Code = breakdown.Code,
                                Employee = "",
                                TimeID = "",
                                Status = breakdown.Status
                            });
                        } else
                        {
                            foreach (CalendarEntry associatedEntry in associatedEntries)
                            {
                                DateTime date;
                                bool hasDate = DateTime.TryParse(associatedEntry?.Date, out date);

                                TimeSpan timeFrom;
                                bool hasTimeFrom = TimeSpan.TryParse(associatedEntry?.TimeFrom, out timeFrom);

                                TimeSpan timeTo;
                                bool hasTimeTo = TimeSpan.TryParse(associatedEntry?.TimeTo, out timeTo);

                                result.Add(new ExcelEntry()
                                {
                                    ID = breakdown.ID,
                                    Date = hasDate ? date : (DateTime?)null,
                                    TimeFrom = hasTimeFrom ? timeFrom : (TimeSpan?)null,
                                    TimeTo = hasTimeTo ? timeTo : (TimeSpan?)null,
                                    EmployeeCode = associatedEntry?.ResourceCode ?? "",
                                    Project = project.Name,
                                    Phase = breakdown.Name,
                                    Code = breakdown.Code,
                                    Employee = associatedEntry?.ResourceName ?? "",
                                    TimeID = associatedEntry?.ID ?? "",
                                    Status = breakdown.Status
                                });
                            }
                        }                       
                    }
                }
                return result;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<List<ExcelEntryResult>> AddExcelEntries(List<ExcelEntry> entries)
        {
            List<KeyValuePair<ExcelEntryResult, Task<XElement>>> storage = new List<KeyValuePair<ExcelEntryResult, Task<XElement>>>();
            List<Task> tasks = new List<Task>();
            foreach (ExcelEntry entry in entries)
            {
                TimeSpan? hours = entry.TimeTo - entry.TimeFrom;

                XElement content = new XElement("entry");
                content.Add(new XElement("type", "entry"));
                content.Add(new XElement("resource", entry.EmployeeCode));
                content.Add(new XElement("status", "Soft"));
                content.Add(new XElement("date", entry.Date?.ToString("yyyyMMdd")));
                content.Add(new XElement("timeFrom", entry.TimeFrom?.ToString(@"hh\:mm")));
                content.Add(new XElement("hours", hours?.TotalHours));
                content.Add(new XElement("project", entry.Project));
                content.Add(new XElement("breakdown", entry.Phase));

                Task<XElement> uploadTask = Task.Run(async () => await TimewaxApi.AddCalendarEntry(content));

                int.TryParse(entry.ID, out int id);

                storage.Add(new KeyValuePair<ExcelEntryResult, Task<XElement>>(
                    new ExcelEntryResult() { Row = entry.Row, ID = id }, 
                    uploadTask));

                tasks.Add(uploadTask);
                Thread.Sleep(50);
            }
            await Task.WhenAll(tasks);

            List<ExcelEntryResult> list = new List<ExcelEntryResult>();
            foreach(KeyValuePair<ExcelEntryResult, Task<XElement>> pair in storage)
            {
                XElement responseContent = pair.Value.Result;

                ExcelEntryResult result = pair.Key;
                int.TryParse(responseContent.Element("id")?.Value, out int timeID);

                result.TimeID = timeID;
                result.Valid = (responseContent.Element("valid").Value == "yes");
                list.Add(result);
            }
            return list;
        }

        public async Task<List<ExcelEntryResult>> UpdateExcelEntries(List<ExcelEntry> entries)
        {
            List<KeyValuePair<ExcelEntryResult, Task<XElement>>> storage = new List<KeyValuePair<ExcelEntryResult, Task<XElement>>>();
            List<Task> tasks = new List<Task>();
            foreach (ExcelEntry entry in entries)
            {
                TimeSpan? hours = entry.TimeTo - entry.TimeFrom;

                XElement content = new XElement("entry");
                content.Add(new XElement("id", entry.TimeID));
                content.Add(new XElement("type", "entry"));
                content.Add(new XElement("resource", entry.EmployeeCode));
                content.Add(new XElement("project", entry.Project));
                content.Add(new XElement("breakdown", entry.Phase));
                content.Add(new XElement("status", "Soft"));
                content.Add(new XElement("date", entry.Date?.ToString("yyyyMMdd")));
                content.Add(new XElement("timeFrom", entry.TimeFrom?.ToString(@"hh\:mm")));
                content.Add(new XElement("hours", hours?.TotalHours));

                Task<XElement> uploadTask = Task.Run(async () => await TimewaxApi.EditCalendarEntry(content));

                int.TryParse(entry.ID, out int id);
                int.TryParse(entry.TimeID, out int timeID);

                storage.Add(new KeyValuePair<ExcelEntryResult, Task<XElement>>(
                    new ExcelEntryResult() { Row = entry.Row, ID = id, TimeID = timeID },
                    uploadTask));

                tasks.Add(uploadTask);
                Thread.Sleep(50);
            }
            await Task.WhenAll(tasks);

            List<ExcelEntryResult> list = new List<ExcelEntryResult>();
            foreach (KeyValuePair<ExcelEntryResult, Task<XElement>> pair in storage)
            {
                XElement responseContent = pair.Value.Result;

                ExcelEntryResult result = pair.Key;
                result.Valid = (responseContent.Element("valid").Value == "yes");
                list.Add(result);
            }
            return list;
        }

        public async Task<List<ExcelEntryResult>> RemoveExcelEntries(List<ExcelEntry> entries)
        {
            List<KeyValuePair<ExcelEntryResult, Task<XElement>>> storage = new List<KeyValuePair<ExcelEntryResult, Task<XElement>>>();
            List<Task> tasks = new List<Task>();
            foreach (ExcelEntry entry in entries)
            {
                Task<XElement> uploadTask = Task.Run(async () => await TimewaxApi.RemoveCalendarEntry(entry.TimeID));

                int.TryParse(entry.ID, out int id);
                int.TryParse(entry.TimeID, out int timeID);

                storage.Add(new KeyValuePair<ExcelEntryResult, Task<XElement>>(
                    new ExcelEntryResult() { Row = entry.Row, ID = id, TimeID = timeID },
                    uploadTask));

                tasks.Add(uploadTask);
                Thread.Sleep(50);
            }
            await Task.WhenAll(tasks);


            List<ExcelEntryResult> list = new List<ExcelEntryResult>();
            foreach (KeyValuePair<ExcelEntryResult, Task<XElement>> pair in storage)
            {
                XElement responseContent = pair.Value.Result;

                ExcelEntryResult result = pair.Key;
                result.Valid = (responseContent.Element("valid").Value == "yes");
                list.Add(result);
            }
            return list;
        }
    }
}
