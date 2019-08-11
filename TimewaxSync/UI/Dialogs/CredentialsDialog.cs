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
    public partial class CredentialsDialog : Form
    {

        private bool DisableClose = false;

        public CredentialsDialog(bool disableClose)
        {
            InitializeComponent();
            this.DisableClose = disableClose;
        }


        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                if(DisableClose)
                {
                    myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                }

                return myCp;
            }
        }

        private void CrededentialsDialog_Load(object sender, EventArgs e)
        {
            clientTextbox.Text = Properties.Settings.Default.Client;
            userTextbox.Text = Properties.Settings.Default.Username;
            passwordTextbox.Text = Properties.Settings.Default.Password;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Client = clientTextbox.Text;
            Properties.Settings.Default.Username = userTextbox.Text;
            Properties.Settings.Default.Password = passwordTextbox.Text;
            Properties.Settings.Default.Save();
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
