using System;
using System.Windows.Forms;
using PortSIP;

namespace admiralps
{
    public partial class MainForm : Form
    {
        private PortSIPSDK sdk;

        public MainForm()
        {
            InitializeComponent();
            InitializeSIP();
        }

        private void InitializeSIP()
        {
            sdk = new PortSIPSDK();
            sdk.SetLogLevel(2); // Set log level (0: None, 1: Error, 2: Warning, 3: Info, 4: Debug)

            // Initialize SDK
            int result = sdk.Initialize("0.0.0.0", 5060, "user-agent");
            if (result == 0)
            {
                MessageBox.Show("SDK initialized successfully.");
            }
            else
            {
                MessageBox.Show($"Failed to initialize SDK: {result}");
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm(this);
            registerForm.ShowDialog();
        }

        public void RegisterAccount(string username, string password, string domain)
        {
            int result = sdk.Register(username, password, domain, 5060);
            if (result == 0)
            {
                MessageBox.Show("Registration successful!");
            }
            else
            {
                MessageBox.Show($"Registration failed: {result}");
            }
        }

        private void btnCall_Click(object sender, EventArgs e)
        {
            // Placeholder for call functionality
            MessageBox.Show("Call functionality not implemented yet.");
        }
    }
}