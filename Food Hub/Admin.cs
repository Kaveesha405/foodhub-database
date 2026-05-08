using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Admin : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";
        private string adminName;

        public Admin()
        {
            InitializeComponent();
            LoadAdminData();
            PersonalizeWelcomeMessage();
        }

        private void LoadAdminData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get admin details from the Admin table using ReferenceID from UserSession
                    string query = "SELECT Admin_Name FROM Admin WHERE AdminID = @adminId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@adminId", UserSession.UserId);

                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            adminName = result.ToString();
                        }
                        else
                        {
                            adminName = UserSession.Username;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading admin data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                adminName = UserSession.Username ?? "Admin";
            }
        }

        //Method for personalized welcome message
        private void PersonalizeWelcomeMessage()
        {
            if (string.IsNullOrEmpty(adminName))
            {
                adminName = "Admin";
            }

            string firstName = adminName.Contains(' ') ? adminName.Split(' ')[0] : adminName;

            label1.Text = $"Welcome {firstName}";

            try
            {
                using (Graphics g = this.CreateGraphics())
                {
                    SizeF textSize = g.MeasureString(label1.Text, label1.Font);

                    int newWidth = Math.Min((int)textSize.Width + 40, 500);
                    label1.Size = new Size(newWidth, label1.Height);

                    label1.Location = new Point((this.Width - newWidth) / 2, label1.Location.Y);
                }
            }
            catch
            {
           
            }
        }

        //return to login page button
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

        //staff Management button
        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!UserSession.IsLoggedIn() || UserSession.UserType != "Admin")
                {
                    MessageBox.Show("Access denied. Please log in as admin.", "Access Denied",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ReturnToLogin();
                    return;
                }

                Staff_Management staff_Management = new Staff_Management();
                staff_Management.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Staff Management: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //rider Management button
        private void guna2GradientTileButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!UserSession.IsLoggedIn() || UserSession.UserType != "Admin")
                {
                    MessageBox.Show("Access denied. Please log in as admin.", "Access Denied",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ReturnToLogin();
                    return;
                }

                Rider_Management rider_Management = new Rider_Management();
                rider_Management.Show(); 
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Rider Management: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //log Out button
        private void guna2GradientTileButton5_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to log out, {adminName}?",
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

        private void ReturnToLogin()
        {
            UserSession.ClearSession();
            LogIn loginForm = new LogIn();
            loginForm.Show();
            this.Hide();
        }

        private void Admin_Load(object sender, EventArgs e)
        {

        }
    }
}