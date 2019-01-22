using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Tools.Ribbon;
using TimewaxSync.TimewaxWebApi;
using TimewaxSync.Tools;

namespace TimewaxSync
{
    public partial class Timewax
    {
        TimewaxApi api = TimewaxApi.Instance;

        List<int> rows = new List<int>();

        private async void Timewax_Load(object sender, RibbonUIEventArgs e)
        {
            // Set the credentials.
            api.Authenticator = new Authenticator() {
                Client = "LooPlan20170324152805",
                Username = "MSINKE",
                Password = "Fongers5!!"
            };

            // Authenticate with the Timewax API.
            await api.Authenticate();

            // Setup listener to changes on the Planning sheet.
            Globals.Planning.Change += (range) => {
                foreach (Range row in range.Rows)
                {
                    if (!rows.Contains(row.Row))
                    {
                        rows.Add(row.Row);
                    }
                }
            };

        }

        private async void downloadButton_Click(object sender, RibbonControlEventArgs e)
        {
            if(rows.Count > 0)
            {
                DialogResult result = MessageBox.Show("Je hebt veranderingen gemaakt, wil je die overschrijven?", 
                    "Download",
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    return;
                }
                else
                {
                    foreach (int row in rows)
                    {
                        Range range = Globals.Planning.Rows[row];
                        range.Interior.Color = System.Drawing.Color.Transparent;
                    }
                }
            }

            //
            List<CalendarEntry> calendarEntries = await api.GetCalendarEntries("20190101", "20191201");

            int index = 2;
            foreach(var calendarEntry in calendarEntries)
            {
                Globals.Planning.Cells[index, 1] = calendarEntry.ID;
                Globals.Planning.Cells[index, 2] = calendarEntry.Date;
                Globals.Planning.Cells[index, 3] = calendarEntry.TimeFrom;
                Globals.Planning.Cells[index, 4] = calendarEntry.TimeTo;
                Globals.Planning.Cells[index, 5] = calendarEntry.Project;
                Globals.Planning.Cells[index, 6] = calendarEntry.BreakdownParent;
                Globals.Planning.Cells[index, 7] = calendarEntry.BreakdownCode;
                index++;
            }
            rows.Clear();
        }

        private void uploadButton_Click(object sender, RibbonControlEventArgs e)
        {
            foreach(int row in rows)
            {
                Range Cells = Globals.Planning.Cells;
                Range range = Globals.Planning.Rows[row];
                range.Interior.Color = ColorTranslator.FromHtml("#f79f2c");

                double timeFrom = Cells[row, 3].Value;

                XElement entry = new XElement("entry");
                entry.Add(new XElement("id", Cells[row, 1].Value));
                entry.Add(new XElement("date", Cells[row, 2].Value));
                entry.Add(new XElement("timeFrom", ExcelDate.ParseExcelDate(timeFrom.ToString()).ToShortTimeString()));
                entry.Add(new XElement("project", Cells[row, 5].Value));
                entry.Add(new XElement("breakdown", Cells[row, 6].Value));
            }
        }
    }
}
