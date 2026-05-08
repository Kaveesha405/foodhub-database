using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Staff : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";
        private string staffName;

        public Staff()
        {
            InitializeComponent();
            LoadStaffData();
            PersonalizeWelcomeMessage();
        }

        //load staff data from database
        private void LoadStaffData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT Name FROM Staff WHERE Staff_ID = @staffId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@staffId", UserSession.UserId);

                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            staffName = result.ToString();
                        }
                        else
                        {
                            staffName = UserSession.Username;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading staff data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                staffName = UserSession.Username ?? "Staff";
            }
        }

        //method for personalized welcome message
        private void PersonalizeWelcomeMessage()
        {
            if (string.IsNullOrEmpty(staffName))
            {
                staffName = "Staff";
            }

            string firstName = staffName.Contains(' ') ? staffName.Split(' ')[0] : staffName;
            label1.Text = $"Welcome {firstName}";
        }

        //Back button - return to login
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
            Staff_Profile staff_Profile = new Staff_Profile();
            staff_Profile.Show();
            this.Hide();
        }

        //place Order button click event
        private void guna2GradientTileButton2_Click(object sender, EventArgs e)
        {
            Customer_Details customer_Details = new Customer_Details();
            customer_Details.Show();
            this.Hide();
        }

        //add Food Items/Ingredients button click event
        private void guna2GradientTileButton3_Click(object sender, EventArgs e)
        {
            Food_item food_Item = new Food_item();
            food_Item.Show();
            this.Hide();
        }

        //log Out button click event
        private void guna2GradientTileButton5_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to log out, {staffName}?",
                "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                UserSession.ClearSession();
                MessageBox.Show("You have been logged out successfully.", "Logged Out",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LogIn loginForm = new LogIn();
                loginForm.Show();
                this.Hide();
            }
        }

    }
}