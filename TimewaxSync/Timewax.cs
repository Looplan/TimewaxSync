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
using TimewaxSync.TimewaxWebApi.Data;
using System.Threading.Tasks;
using System.Threading;
using TimewaxSync.UI;
using TimewaxSync.UI.Dialogs;
using System.Text.RegularExpressions;

namespace TimewaxSync
{
    public partial class Timewax
    {
        TimewaxApi api = TimewaxApi.Instance;

        TimewaxExcelSync TimewaxExcelSync { get; set; }

        StatusDialog dialog = new StatusDialog();

        // List of row numbers, where the row's cell(s) have changed.
        private List<int> changedRows = new List<int>();

        private void Timewax_Load(object sender, RibbonUIEventArgs e)
        {
            bool hasCredentials = HasCredentials();

            if(!hasCredentials)
            {
                CredentialsDialog dialog = new CredentialsDialog(true);
                dialog.Controls["MainLabel"].Text = "Voordat je kan werken met de Timewax intergratie, \n moet je toegangsgegevens geven voor Timewax.";
                DialogResult result = dialog.ShowDialog();
            }

            Setup();

            SetupChangedRowListener();

            Globals.ThisWorkbook.Application.ScreenUpdating = true;
        }

        private bool HasCredentials()
        {
            return !(string.IsNullOrEmpty(Properties.Settings.Default.Client)
                 || string.IsNullOrEmpty(Properties.Settings.Default.Username)
                || string.IsNullOrEmpty(Properties.Settings.Default.Password));
        }

        private async void Setup()
        {
            // Set the credentials.
            api.Authenticator = new Authenticator()
            {
                Client = Properties.Settings.Default.Client,
                Username = Properties.Settings.Default.Username,
                Password = Properties.Settings.Default.Password
            };

            try
            {
                // Authenticate with the Timewax API.
                await api.Authenticate();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            TimewaxExcelSync = new TimewaxExcelSync(api);
        }

        private void SetupChangedRowListener()
        {
            Globals.Breakdowns.Change += (range) =>
            {
                foreach (Range row in range.Rows)
                {
                    if (!changedRows.Contains(row.Row))
                    {
                        changedRows.Add(row.Row);
                    }
                }
            };
        }

        private void ClearColorZone()
        {
            // Reset color
            Breakdowns sheet = Globals.Breakdowns;
            Range usedRange = sheet.UsedRange;
            Range colorZone = usedRange.Columns["A:J", Type.Missing];
            colorZone.Interior.ColorIndex = 0;
        }

        private void ResetStatusDialog()
        {
            dialog.progressBar.Style = ProgressBarStyle.Marquee;
            dialog.progressBar.Value = 0;
            dialog.StatusLabel.Text = "Crunching some cookies...";
        }
        private async void downloadButton_Click(object sender, RibbonControlEventArgs e)
        {
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            ResetStatusDialog();
            dialog.Center();
            dialog.Show();

            if (changedRows.Count > 0)
            {
                DialogResult result = MessageBox.Show(dialog, "Je hebt veranderingen gemaakt, wil je die overschrijven?",
                    "Download",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    return;
                }  
            }

            await Download();

            dialog.Hide();

            changedRows.Clear();

        }

        private async Task Download()
        {
            try
            {
                dialog.StatusLabel.Text = "Clearing color codes..";
                ClearColorZone();

                dialog.StatusLabel.Text = "Clearing previous data...";
                Range allRowsExceptHeaderRow = Globals.Breakdowns.UsedRange.Offset[1, 0];
                Range allRowsTillTimeIDExceptHeaderRow = allRowsExceptHeaderRow.Resize[allRowsExceptHeaderRow.Height, 10];
                allRowsTillTimeIDExceptHeaderRow.Value = "";

                // Disable automatic calculation and screen updating.
                Globals.ThisWorkbook.Application.ScreenUpdating = false;
                Globals.ThisWorkbook.Application.Calculation = XlCalculation.xlCalculationManual;

                dialog.StatusLabel.Text = "Downloading info...";

                List<ExcelEntry> entries = await Task.Run(async () => await TimewaxExcelSync.GetExcelEntries());

                dialog.StatusLabel.Text = "Writing information (Excel might freeze)";

                int index = 2;

                foreach (ExcelEntry entry in entries)
                {
                    entry.WriteToRow(index);
                    index++;
                }

                // Re-enable automatic calculation and screen updating.
                Globals.ThisWorkbook.Application.ScreenUpdating = true;
                Globals.ThisWorkbook.Application.Calculation = XlCalculation.xlCalculationAutomatic;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Validate()
        {
            // Get all rows that are in use.
            List<Range> rows = GetUsedRows();

            // Find all rows that don't have a Time-ID.
            List<Range> rowsWithoutTimeID = GetUsedRowsWithoutTimeID(rows);

            // Find all rows with a Time-ID.
            List<Range> rowsWithTimeID = GetUsedRowsWithTimeID(rows);

            // Find all rows that are changed and have a Time-ID.
            List<Range> changedRowsWithTimeID = rowsWithTimeID.Where((row) => changedRows.Contains(row.Row)).ToList();

            List<ExcelEntry> addEntries = GetAddEntries(rowsWithoutTimeID);

            List<ExcelEntry> updateEntries = GetUpdateEntries(changedRowsWithTimeID);

            List<ExcelEntry> removeEntries = GetRemoveEntries(changedRowsWithTimeID);
        }

        private async void uploadButton_Click(object sender, RibbonControlEventArgs e)
        {
            await Upload();
        }

        private List<ExcelEntry> GetAddEntries(List<Range>rowsWithoutTimeID)
        {
            List<ExcelEntry> addEntries = new List<ExcelEntry>();

            // Loop through each row with no Time-ID.
            foreach (Range row in rowsWithoutTimeID)
            {

                ExcelEntry buffer = ExcelEntry.FromRow(row.Row);

                // Check if the entry is ignorable.
                bool ignorable = IsIgnorableEntry(buffer);

                if (!ignorable)
                {
                    // Check if the entry is valid.
                    bool valid = ValidateExcelEntry(buffer);

                    if (valid)
                    {
                        addEntries.Add(buffer);
                    }
                }
            }
            return addEntries;
        }

        private List<ExcelEntry> GetUpdateEntries(List<Range> changedRowsWithTimeID)
        {
            List<ExcelEntry> updateEntries = new List<ExcelEntry>();

            // Loop trough each row with a Time-ID.
            foreach (Range row in changedRowsWithTimeID)
            {

                ExcelEntry buffer = ExcelEntry.FromRow(row.Row);

                // Check if the entry is ignorable.
                bool ignorable = IsIgnorableEntry(buffer);

                if (!ignorable)
                {
                    // Check if the entry is valid.
                    bool valid = ValidateExcelEntryWithTimeID(buffer);

                    if (valid)
                    {
                        updateEntries.Add(buffer);
                    }
                }
            }
            return updateEntries;
        }

        private List<ExcelEntry> GetRemoveEntries(List<Range> changedRowsWithTimeID)
        {
            List<ExcelEntry> removeEntries = new List<ExcelEntry>();

            // Loop trough each row with a Time-ID.
            foreach (Range row in changedRowsWithTimeID)
            {

                ExcelEntry buffer = ExcelEntry.FromRow(row.Row);

                // Check if the entry is ignorable.
                bool ignorable = IsIgnorableEntry(buffer);

                if (ignorable)
                {
                    removeEntries.Add(buffer);
                }
            }
            return removeEntries;
        }

        private async Task Upload()
        {
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            ClearColorZone();

            ResetStatusDialog();
            dialog.Center();
            dialog.Show();

            // Get all rows that are in use.
            List<Range> rows = GetUsedRows();

            // Find all rows that don't have a Time-ID.
            List<Range> rowsWithoutTimeID = GetUsedRowsWithoutTimeID(rows);

            // Find all rows with a Time-ID.
            List<Range> rowsWithTimeID = GetUsedRowsWithTimeID(rows);

            // Find all rows that are changed and have a Time-ID.
            List<Range> changedRowsWithTimeID = rowsWithTimeID.Where((row) => changedRows.Contains(row.Row)).ToList();

            List<ExcelEntry> addEntries = GetAddEntries(rowsWithoutTimeID);

            List<ExcelEntry> updateEntries = GetUpdateEntries(changedRowsWithTimeID);

            List<ExcelEntry> removeEntries = GetRemoveEntries(changedRowsWithTimeID);

            // List to store rows, where the details are not complete.
            List<Range> errorRows = new List<Range>();

            bool upload = true;

            if (errorRows.Count > 0)
            {
                dialog.StatusLabel.Text = "Errors gevonden";

                DialogResult result = MessageBox.Show(dialog, "Niet alle velden zijn overal correct ingevuld, doorgaan?",
                   "Upload",
                   MessageBoxButtons.OKCancel,
                   MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {
                    upload = true;
                }
                else
                {
                    upload = false;
                }
            }
            else if (addEntries.Count == 0 && updateEntries.Count == 0 && removeEntries.Count == 0)
            {
                upload = false;

                dialog.progressBar.Style = ProgressBarStyle.Continuous;
                dialog.progressBar.Value = 100;
                dialog.StatusLabel.Text = "Niks om up te daten!";

                await Task.Run(() => { Thread.Sleep(1500); });

            }

            if (upload)
            {
                dialog.StatusLabel.Text = "Uploading......";

                List<TimewaxExcelSync.ExcelEntryResult> addResults = await Task.Run(async () => await TimewaxExcelSync.AddExcelEntries(addEntries));
                ProcessAddEntriesResults(addResults);

                List<TimewaxExcelSync.ExcelEntryResult> updateResults = await Task.Run(async () => await TimewaxExcelSync.UpdateExcelEntries(updateEntries));
                ProcessUpdateEntriesResults(updateResults);

                List<TimewaxExcelSync.ExcelEntryResult> removeResults = await Task.Run(async () => await TimewaxExcelSync.RemoveExcelEntries(removeEntries));
                ProcessRemoveEntriesResults(removeResults);
            }

            dialog.Hide();
        }

        private List<Range> GetUsedRows()
        {
            List<Range> rows = new List<Range>();

            // Loop through each row in the used range.
            foreach (Range row in Globals.Breakdowns.UsedRange.Rows)
            {

                // Make sure we don't add the header row.
                if (row.Row != 1)
                {
                    rows.Add(row);
                }
            }
            return rows;
        }

        /// <summary>
        /// Find all rows which have no Time-ID.
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private List<Range> GetUsedRowsWithoutTimeID(List<Range> rows)
        {
           return rows.Where((range) => {
                string timeId = range.Cells[1, Globals.Breakdowns.TimeIDs.Column].Value?.ToString();
                return string.IsNullOrEmpty(timeId);
            }).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private List<Range> GetUsedRowsWithTimeID(List<Range> rows)
        {
            return rows.Where((range) => {
                string timeId = range.Cells[1, Globals.Breakdowns.TimeIDs.Column].Value?.ToString();
                return !string.IsNullOrEmpty(timeId);
            }).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private bool IsIgnorableEntry(ExcelEntry entry) =>
            entry.Date == null &&
            entry.TimeFrom == null &&
            entry.TimeTo == null &&
            string.IsNullOrEmpty(entry.EmployeeCode);

        private bool ValidateExcelEntry(ExcelEntry entry)
        {
            bool validated = true;

            if (string.IsNullOrEmpty(entry.ID))
            {
                validated = false;

                Globals.Breakdowns.IDs.Cells[entry.Row, 1].Interior.Color = ColorTranslator.FromHtml("#d32836");
            }

            if(entry.Date == null || (DateTime.Now - entry.Date.Value).Days > (365 * 3))
            {
                validated = false;

                Globals.Breakdowns.Dates.Cells[entry.Row, 1].Interior.Color = ColorTranslator.FromHtml("#d32836");
            }

            if(entry.TimeFrom == null)
            {
                validated = false;

                Globals.Breakdowns.TimeBegin.Cells[entry.Row, 1].Interior.Color = ColorTranslator.FromHtml("#d32836");
            }

            if (entry.TimeTo == null)
            {
                validated = false;

                Globals.Breakdowns.TimeEnd.Cells[entry.Row, 1].Interior.Color = ColorTranslator.FromHtml("#d32836");
            }

            if (string.IsNullOrEmpty(entry.EmployeeCode) || !Regex.IsMatch(entry.EmployeeCode, @"^[a-zA-Z]+$"))
            {
                validated = false;

                // Set the row to red, indicating failure or incomplete.
                Globals.Breakdowns.Cells[entry.Row, 5].Interior.Color = ColorTranslator.FromHtml("#d32836");
            }

            if (string.IsNullOrEmpty(entry.Project))
            {
                validated = false;

                Globals.Breakdowns.Projects.Cells[entry.Row, 1].Interior.Color = ColorTranslator.FromHtml("#d32836");
            }

            if (string.IsNullOrEmpty(entry.Phase))
            {
                validated = false;

                Globals.Breakdowns.Phases.Cells[entry.Row, 1].Interior.Color = ColorTranslator.FromHtml("#d32836");
            }

            if (string.IsNullOrEmpty(entry.Code))
            {
                validated = false;

                Globals.Breakdowns.Codes.Cells[entry.Row, 1].Interior.Color = ColorTranslator.FromHtml("#d32836");
            }

            return validated;
        }

        private bool ValidateExcelEntryWithTimeID(ExcelEntry entry)
        {
            bool validated = true;
            validated = ValidateExcelEntry(entry);

            if(string.IsNullOrEmpty(entry.TimeID) || !Regex.IsMatch(entry.TimeID, @"^\d+$"))
            {
                validated = false;
                Globals.Breakdowns.TimeIDs.Cells[entry.Row, 1].Interior.Color = ColorTranslator.FromHtml("#d32836");
            }
            return validated;
        }

        private void ProcessAddEntriesResults(List<TimewaxExcelSync.ExcelEntryResult> results)
        {
            foreach (TimewaxExcelSync.ExcelEntryResult entry in results)
            {
                if(entry.Valid)
                {
                    try
                    {
                        Range row = Globals.Breakdowns.Cells[entry.Row, 1].Resize[1, 10];

                        // Write the ID
                        Globals.Breakdowns.TimeIDs.Cells[row.Row, 1].Value = entry.TimeID;

                        // Set color to added.
                        Range colorZone = row.Columns["B:D", Type.Missing];
                        colorZone.Interior.Color = ColorTranslator.FromHtml("#88c103");

                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private void ProcessUpdateEntriesResults(List<TimewaxExcelSync.ExcelEntryResult> results)
        {
            foreach (TimewaxExcelSync.ExcelEntryResult entry in results)
            {
                if (entry.Valid)
                {
                    try
                    {
                        Range row = Globals.Breakdowns.Cells[entry.Row, 1].Resize[1, 10];

                        // Set color to updated.
                        Range colorZone = row.Columns["B:D", Type.Missing];
                        colorZone.Interior.Color = ColorTranslator.FromHtml("#fc8337");

                    }
                    catch (Exception ex)
                    {

                    }
                }                
            }
        }

        private void ProcessRemoveEntriesResults(List<TimewaxExcelSync.ExcelEntryResult> results)
        {
            foreach (TimewaxExcelSync.ExcelEntryResult entry in results)
            {
                if (entry.Valid)
                {
                    try
                    {
                        Range row = Globals.Breakdowns.Cells[entry.Row, 1].Resize[1, 10];
                        row.EntireRow.Delete();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private void credentialsButton_Click(object sender, RibbonControlEventArgs e)
        {
            new CredentialsDialog(false).Show();
        }

        private void versionButton_Click(object sender, RibbonControlEventArgs e)
        {
            new VersionDialog().Show();
        }

        private void helpButton_Click(object sender, RibbonControlEventArgs e)
        {
            new HelpDialog().Show();
        }
    }
}
