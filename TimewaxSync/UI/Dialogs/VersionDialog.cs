using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimewaxSync.UI.Dialogs
{
    public partial class VersionDialog : Form
    {
        public VersionDialog()
        {
            InitializeComponent();
        }

        private void VersionDialog_Load(object sender, EventArgs e)
        {
            label1.Text = "1.1.0.4";
        }
    }
}
