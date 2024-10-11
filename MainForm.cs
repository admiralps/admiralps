using System;
using System.Windows.Forms;
using PortSIP; // Ensure this is the correct namespace for the SDK

namespace admiralps
{
    public partial class MainForm : Form
    {
        

        public MainForm()
        {
            InitializeComponent();
            InitializePortSIP();
        }

        private void InitializePortSIP()
        {

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Show the registration form
            RegisterForm registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }

        // Additional methods to handle other functionalities
    }
}