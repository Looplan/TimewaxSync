using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimewaxSync.Tools;

namespace TimewaxSync
{
    public class ExcelEntry
    {

        public string ID;

        public DateTime? Date;

        public TimeSpan? TimeFrom;

        public TimeSpan? TimeTo;

        public string EmployeeCode;

        public string Project;

        public string Phase;

        public string Code;

        public string Employee;

        public string TimeID;

        public string Status;

        public int Row;

        public void WriteToRow(int index)
        {
            Globals.Breakdowns.Cells[index, 1].Resize[1, 11].Value = new object[] {
                ID,
                Date,
                TimeFrom?.ToString() ?? "",
                TimeTo?.ToString() ?? "",
                EmployeeCode ?? "",
                Project,
                Phase,
                Code,
                Employee,
                TimeID,
                Status
            };          
        }

        public static ExcelEntry FromRow(int index)
        {            
            DateTime? date = null;
            DateTime buffer;
            if(DateTime.TryParse(Globals.Breakdowns.Dates.Cells[index, 1].Value?.ToString() ?? "", out buffer))
            {
                date = buffer;
            }

            return new ExcelEntry()
            {
                Row = index,
                ID = Globals.Breakdowns.IDs.Cells[index, 1].Value?.ToString() ?? "",
                Date = date,
                TimeFrom = ExcelDate.ParseExcelDate(Globals.Breakdowns.TimeBegin.Cells[index, 1].Value?.ToString())?.TimeOfDay ?? null,
                TimeTo = ExcelDate.ParseExcelDate(Globals.Breakdowns.TimeEnd.Cells[index, 1].Value?.ToString())?.TimeOfDay ?? null,
                EmployeeCode = Globals.Breakdowns.Cells[index, 5].Value?.ToString() ?? "",
                Project = Globals.Breakdowns.Projects.Cells[index, 1].Value?.ToString() ?? "",
                Phase = Globals.Breakdowns.Phases.Cells[index, 1].Value?.ToString() ?? "",
                Code = Globals.Breakdowns.Codes.Cells[index, 1].Value?.ToString() ?? "",
                Employee = Globals.Breakdowns.Employees.Cells[index, 1].Value?.ToString() ?? "",
                TimeID = Globals.Breakdowns.TimeIDs.Cells[index, 1].Value?.ToString() ?? "",
            };
        }

    }
}
