using System;
using System.Windows.Forms;
using PortSIP; // Ensure this is the correct namespace for the SDK

namespace admiralps
{
    public partial class RegisterForm : Form
    {
        private PortSIPLib portsipLib;

        public RegisterForm(PortSIPLib portsipLib)
        {
            InitializeComponent();
            this.portsipLib = portsipLib;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Retrieve SIP account details from the form fields
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string domain = txtDomain.Text;

            // Call the PortSIP registration method
            bool success = portsipLib.Register(username, password, domain); // Replace with actual method
            if (success)
            {
                MessageBox.Show("Registration successful!");
            }
            else
            {
                MessageBox.Show("Registration failed!");
            }
        }
    }
}