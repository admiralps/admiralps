using System;
using System.Windows.Forms;
using PortSIP; // Ensure this is the correct namespace for the SDK

namespace admiralps
{
    public partial class MainForm : Form
    {
        private PortSIPLib portsipLib;

        public MainForm()
        {
            InitializeComponent();
            InitializePortSIP();
        }

        private void InitializePortSIP()
        {
            // Create an instance of the PortSIP library
            portsipLib = new PortSIPLib();
            
            // Initialize or configure the PortSIP SDK here
            // Example: portsipLib.Initialize();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Show the registration form
            RegisterForm registerForm = new RegisterForm(portsipLib);
            registerForm.ShowDialog();
        }

        // Additional methods to handle other functionalities
    }
}