using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimewaxSync.UI
{
    public partial class StatusDialog : Form
    {
        public StatusDialog()
        {
            InitializeComponent();
        }

        public void Center()
        {
            CenterToScreen();
        }

        public void ShowError(string message)
        {
            if (!ErrorLabel.Visible)
            {
                ErrorLabel.Visible = true;
                ErrorListBox.Visible = true;
                CloseButton.Visible = true;
                this.ClientSize = new System.Drawing.Size(393, 470);
                CenterToScreen();
            }

            ErrorListBox.Items.Add(message);
            ErrorListBox.TopIndex = ErrorListBox.Items.Count - 1;

            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 100;
        }

        public void ClearErrors()
        {
            ErrorListBox.Items.Clear();
            ErrorLabel.Visible = false;
            ErrorListBox.Visible = false;
            CloseButton.Visible = false;
            this.ClientSize = new System.Drawing.Size(393, 324);
        }

        public bool HasErrors => ErrorListBox.Items.Count > 0;

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }


    }
}
