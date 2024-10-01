using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartStartDeliveryForm.Classes;

namespace SmartStartDeliveryForm
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        static int AttemptCounter;
        private void IntializeCounter(int Attempts)
        {
            AttemptCounter = Attempts;
        }
        private void Login_Load(object sender, EventArgs e)
        {
            IntializeCounter(3);
        }

        private void LoginBTN_Click(object sender, EventArgs e)
        {
            string Username = txtUsername.Text;
            string Password = txtPassword.Text;

            bool IsAuthenticated = User.Login(Username, Password);

            if (IsAuthenticated)
            {
                DriverManagement driverManagementForm = new DriverManagement();
                this.Hide();
                driverManagementForm.Show();
            }
            else
            {
                if (AttemptCounter <= 0)
                {
                    this.Close();
                }
                MessageBox.Show($"Invalid Username Or Password. You have {AttemptCounter} more attempt(s)", "Incorrect Credentials");
                AttemptCounter--;
            }
        }
    }
}
