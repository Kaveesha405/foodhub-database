using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class LogIn : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";

        public LogIn()
        {
            InitializeComponent();
        }

        //show/hide password function
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            passwordTextBox.PasswordChar = checkBox1.Checked ? '\0' : '*';
        }

        //clear button click event
        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            usernameTextBox.Clear();
            passwordTextBox.Clear();
            checkBox1.Checked = false;
            usernameTextBox.Focus();
        }

        //login button click event
        private void guna2GradientButton2_Click_1(object sender, EventArgs e)
        {
            LoginUser();
        }

        //back button click event
        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //validate login user method
        private void LoginUser()
        {
            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text;

            //Validate the username and password
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter your username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                usernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                passwordTextBox.Focus();
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT ReferenceID, UserType FROM Users WHERE Username = @username AND Password = @password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {//initiate the user session
                                int userId = Convert.ToInt32(reader["ReferenceID"]);
                                string userType = reader["UserType"].ToString();

                                UserSession.UserId = userId;
                                UserSession.Username = username;
                                UserSession.UserType = userType;

                                MessageBox.Show($"Welcome {username}!", "Login Successful",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                if (userType == "Admin")
                                {
                                    Admin adminForm = new Admin();
                                    adminForm.Show();
                                }
                                else if (userType == "Staff")
                                {
                                    Staff staffForm = new Staff();
                                    staffForm.Show();
                                }
                                else if (userType == "Rider")
                                {
                                    Rider riderForm = new Rider();
                                    riderForm.Show();
                                }

                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Invalid username or password.", "Login Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                passwordTextBox.Clear();
                                passwordTextBox.Focus();
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

    public static class UserSession
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }
        public static string UserType { get; set; }

        public static void ClearSession()
        {
            UserId = 0;
            Username = string.Empty;
            UserType = string.Empty;
        }

        public static bool IsLoggedIn()
        {
            return UserId > 0 && !string.IsNullOrEmpty(Username);
        }
    }
}