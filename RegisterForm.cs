using System;
using System.Windows.Forms;

namespace admiralps
{
    public partial class RegisterForm : Form
    {
        private MainForm mainForm;

        public RegisterForm(MainForm form)
        {
            InitializeComponent();
            mainForm = form;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string domain = txtDomain.Text;

            mainForm.RegisterAccount(username, password, domain);
            this.Close();
        }
    }
}
