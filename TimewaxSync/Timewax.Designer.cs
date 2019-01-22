namespace TimewaxSync
{
    partial class Timewax : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Timewax()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.timewaxTab = this.Factory.CreateRibbonTab();
            this.syncGroup = this.Factory.CreateRibbonGroup();
            this.uploadButton = this.Factory.CreateRibbonButton();
            this.downloadButton = this.Factory.CreateRibbonButton();
            this.timewaxTab.SuspendLayout();
            this.syncGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // timewaxTab
            // 
            this.timewaxTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.timewaxTab.Groups.Add(this.syncGroup);
            this.timewaxTab.Label = "Timewax";
            this.timewaxTab.Name = "timewaxTab";
            // 
            // syncGroup
            // 
            this.syncGroup.Items.Add(this.uploadButton);
            this.syncGroup.Items.Add(this.downloadButton);
            this.syncGroup.Label = "Sync";
            this.syncGroup.Name = "syncGroup";
            // 
            // uploadButton
            // 
            this.uploadButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.uploadButton.Label = "Upload";
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.OfficeImageId = "FileCheckIn";
            this.uploadButton.ShowImage = true;
            this.uploadButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.uploadButton_Click);
            // 
            // downloadButton
            // 
            this.downloadButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.downloadButton.Label = "Download";
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.OfficeImageId = "FileCheckOut";
            this.downloadButton.ShowImage = true;
            this.downloadButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.downloadButton_Click);
            // 
            // Timewax
            // 
            this.Name = "Timewax";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.timewaxTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Timewax_Load);
            this.timewaxTab.ResumeLayout(false);
            this.timewaxTab.PerformLayout();
            this.syncGroup.ResumeLayout(false);
            this.syncGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab timewaxTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup syncGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton uploadButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton downloadButton;
    }

    partial class ThisRibbonCollection
    {
        internal Timewax Timewax
        {
            get { return this.GetRibbon<Timewax>(); }
        }
    }
}
