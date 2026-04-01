using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<List<ExcelEntry>> GetExcelEntries(IProgress<string> progress = null, IProgress<string> errors = null)
        {
            try
            {
                progress?.Report("Downloading calendar entries...");
                List<CalendarEntry> calendarEntries = await TimewaxApi.GetCalendarEntries("20160101", DateTime.Now.AddYears(1).ToString("yyyyMMdd"));

                progress?.Report("Downloading project list...");
                List<ProjectInfo> projects = await TimewaxApi.ListProjects(true);

                Dictionary<ProjectInfo, List<Breakdown>> pairs = new Dictionary<ProjectInfo, List<Breakdown>>();

                for (int i = 0; i < projects.Count; i++)
                {
                    var project = projects[i];
                    progress?.Report($"Downloading activities for project {i + 1}/{projects.Count}: {project.Name}...");

                    List<Breakdown> breakdowns;
                    try
                    {
                        breakdowns = await TimewaxApi.GetProjectActivities(project.Name);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            progress?.Report($"Retrying with project code {project.Code}...");
                            breakdowns = await TimewaxApi.GetProjectActivities(project.Code);
                        }
                        catch (Exception ex)
                        {
                            errors?.Report($"Project '{project.Name}' ({project.Code}): {ex.Message}");
                            continue;
                        }
                    }

                    pairs.Add(project, breakdowns);
                }

                progress?.Report("Processing entries...");

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
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ExcelEntryResult>> AddExcelEntries(List<ExcelEntry> entries)
        {
            List<ExcelEntryResult> list = new List<ExcelEntryResult>();
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

                XElement responseContent = await TimewaxApi.AddCalendarEntry(content);

                int.TryParse(entry.ID, out int id);
                int.TryParse(responseContent.Element("id")?.Value, out int timeID);

                list.Add(new ExcelEntryResult()
                {
                    Row = entry.Row,
                    ID = id,
                    TimeID = timeID,
                    Valid = (responseContent.Element("valid").Value == "yes")
                });
            }
            return list;
        }

        public async Task<List<ExcelEntryResult>> UpdateExcelEntries(List<ExcelEntry> entries)
        {
            List<ExcelEntryResult> list = new List<ExcelEntryResult>();
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

                XElement responseContent = await TimewaxApi.EditCalendarEntry(content);

                int.TryParse(entry.ID, out int id);
                int.TryParse(entry.TimeID, out int timeID);

                list.Add(new ExcelEntryResult()
                {
                    Row = entry.Row,
                    ID = id,
                    TimeID = timeID,
                    Valid = (responseContent.Element("valid").Value == "yes")
                });
            }
            return list;
        }

        public async Task<List<ExcelEntryResult>> RemoveExcelEntries(List<ExcelEntry> entries)
        {
            List<ExcelEntryResult> list = new List<ExcelEntryResult>();
            foreach (ExcelEntry entry in entries)
            {
                XElement responseContent = await TimewaxApi.RemoveCalendarEntry(entry.TimeID);

                int.TryParse(entry.ID, out int id);
                int.TryParse(entry.TimeID, out int timeID);

                list.Add(new ExcelEntryResult()
                {
                    Row = entry.Row,
                    ID = id,
                    TimeID = timeID,
                    Valid = (responseContent.Element("valid").Value == "yes")
                });
            }
            return list;
        }
    }
}
