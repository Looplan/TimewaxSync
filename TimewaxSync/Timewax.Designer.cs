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
            this.settingsGroup = this.Factory.CreateRibbonGroup();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.uploadButton = this.Factory.CreateRibbonButton();
            this.downloadButton = this.Factory.CreateRibbonButton();
            this.credentialsButton = this.Factory.CreateRibbonButton();
            this.versionButton = this.Factory.CreateRibbonButton();
            this.helpButton = this.Factory.CreateRibbonButton();
            this.timewaxTab.SuspendLayout();
            this.syncGroup.SuspendLayout();
            this.settingsGroup.SuspendLayout();
            this.group1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timewaxTab
            // 
            this.timewaxTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.timewaxTab.Groups.Add(this.syncGroup);
            this.timewaxTab.Groups.Add(this.settingsGroup);
            this.timewaxTab.Groups.Add(this.group1);
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
            // settingsGroup
            // 
            this.settingsGroup.Items.Add(this.credentialsButton);
            this.settingsGroup.Label = "Settings";
            this.settingsGroup.Name = "settingsGroup";
            // 
            // group1
            // 
            this.group1.Items.Add(this.versionButton);
            this.group1.Items.Add(this.helpButton);
            this.group1.Label = "Info";
            this.group1.Name = "group1";
            // 
            // uploadButton
            // 
            this.uploadButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.uploadButton.Image = global::TimewaxSync.Properties.Resources.upload80;
            this.uploadButton.Label = "Upload";
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.OfficeImageId = "FileCheckIn";
            this.uploadButton.ShowImage = true;
            this.uploadButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.uploadButton_Click);
            // 
            // downloadButton
            // 
            this.downloadButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.downloadButton.Image = global::TimewaxSync.Properties.Resources.download80;
            this.downloadButton.Label = "Download";
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.OfficeImageId = "FileCheckOut";
            this.downloadButton.ShowImage = true;
            this.downloadButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.downloadButton_Click);
            // 
            // credentialsButton
            // 
            this.credentialsButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.credentialsButton.Label = "Inloggegevens";
            this.credentialsButton.Name = "credentialsButton";
            this.credentialsButton.OfficeImageId = "AdpPrimaryKey";
            this.credentialsButton.ShowImage = true;
            this.credentialsButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.credentialsButton_Click);
            // 
            // versionButton
            // 
            this.versionButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.versionButton.Label = "Version";
            this.versionButton.Name = "versionButton";
            this.versionButton.OfficeImageId = "Info";
            this.versionButton.ShowImage = true;
            this.versionButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.versionButton_Click);
            // 
            // helpButton
            // 
            this.helpButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.helpButton.Label = "Hulp";
            this.helpButton.Name = "helpButton";
            this.helpButton.OfficeImageId = "Help";
            this.helpButton.ShowImage = true;
            this.helpButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.helpButton_Click);
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
            this.settingsGroup.ResumeLayout(false);
            this.settingsGroup.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab timewaxTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup syncGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton uploadButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton downloadButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup settingsGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton credentialsButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton versionButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton helpButton;
    }

    partial class ThisRibbonCollection
    {
        internal Timewax Timewax
        {
            get { return this.GetRibbon<Timewax>(); }
        }
    }
}
