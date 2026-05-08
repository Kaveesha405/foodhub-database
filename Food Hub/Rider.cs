using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Rider : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";
        private string riderName;

        public Rider()
        {
            InitializeComponent();
            LoadRiderData();
            PersonalizeWelcomeMessage();
        }

        //method to load rider data from database
        private void LoadRiderData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT First_name FROM Rider WHERE Rider_ID = @riderId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@riderId", UserSession.UserId);

                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            riderName = result.ToString();
                        }
                        else
                        {
                            riderName = UserSession.Username;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rider data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                riderName = UserSession.Username ?? "Rider";
            }
        }

        //method for personalized welcome message
        private void PersonalizeWelcomeMessage()
        {
            if (string.IsNullOrEmpty(riderName))
            {
                riderName = "Rider";
            }

            string firstName = riderName.Contains(' ') ? riderName.Split(' ')[0] : riderName;
            label1.Text = $"Welcome {firstName}";
        }

        //back button click event to login page
        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to go back to the login screen?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                UserSession.ClearSession();
                LogIn loginForm = new LogIn();
                loginForm.Show();
                this.Hide();
            }
        }

        //profile button click event
        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {
            Rider_Profile rider_Profile = new Rider_Profile();
            rider_Profile.Show();
            this.Hide();
        }

        //accept Order button click event
        private void guna2GradientTileButton2_Click(object sender, EventArgs e)
        {
            Order_Accept order_Accept = new Order_Accept();
            order_Accept.Show();
            this.Hide();
        }

        //bike Management button click event
        private void guna2GradientTileButton3_Click(object sender, EventArgs e)
        {
            Bike bike = new Bike();
            bike.Show();
            this.Hide();
        }

        //log Out button click event
        private void guna2GradientTileButton5_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to log out, {riderName}?",
                "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                UserSession.ClearSession();
                MessageBox.Show("You have been logged out successfully.", "Logged Out",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LogIn loginForm = new LogIn();
                loginForm.Show();
                this.Close();
            }
        }

    }
}