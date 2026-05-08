using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Staff_Profile : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";
        private int currentStaffId;

        public Staff_Profile()
        {
            InitializeComponent();
            currentStaffId = UserSession.UserId;
            LoadStaffProfile();

            guna2GradientTileButton3.Click += UpdateProfileButton_Click;
            guna2GradientCircleButton1.Click += guna2GradientCircleButton1_Click;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            contactnotxt.KeyPress += contactnotxt_KeyPress;
            age.KeyPress += age_KeyPress;
            nictxt.Leave += nictxt_Leave;
            dobpicker.ValueChanged += dobpicker_ValueChanged;

            passwordtxt.PasswordChar = '*';
        }

        //staff profile load method
        private void LoadStaffProfile()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT Staff_ID, Name, DOB, Contact_No, NIC, Address, Age, Username, Password 
                                   FROM Staff WHERE Staff_ID = @staffId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@staffId", currentStaffId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate form controls with data
                                nametxt.Text = reader["Name"].ToString();
                                dobpicker.Value = Convert.ToDateTime(reader["DOB"]);
                                contactnotxt.Text = reader["Contact_No"].ToString();
                                nictxt.Text = reader["NIC"].ToString();
                                addresstxt.Text = reader["Address"].ToString();
                                age.Text = reader["Age"].ToString();
                                usernametxt.Text = reader["Username"].ToString();
                                passwordtxt.Text = reader["Password"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Staff profile not found.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profile: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update Profile Button Click Event
        private void UpdateProfileButton_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                UpdateStaffProfile();
            }
        }

        //validate use input fields method
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(nametxt.Text))
            {
                MessageBox.Show("Name is required.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nametxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(passwordtxt.Text))
            {
                MessageBox.Show("Password is required.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                passwordtxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(contactnotxt.Text))
            {
                MessageBox.Show("Contact number is required.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                contactnotxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(nictxt.Text))
            {
                MessageBox.Show("NIC is required.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nictxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(addresstxt.Text))
            {
                MessageBox.Show("Address is required.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                addresstxt.Focus();
                return false;
            }

            // Validate age
            if (!int.TryParse(age.Text, out int ageValue) || ageValue <= 0 || ageValue > 120)
            {
                MessageBox.Show("Please enter a valid age (1-120).", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                age.Focus();
                return false;
            }

            // Validate contact number
            if (!IsValidPhoneNumber(contactnotxt.Text))
            {
                MessageBox.Show("Please enter a valid 10-digit contact number starting with 0.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                contactnotxt.Focus();
                return false;
            }

            // Validate NIC
            if (!IsValidNIC(nictxt.Text))
            {
                MessageBox.Show("NIC must be exactly 12 digits (new format only).", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nictxt.Focus();
                return false;
            }


            return true;
        }

        //update staff profile method
        private void UpdateStaffProfile()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string updateQuery = @"UPDATE Staff 
                                         SET Name = @name, 
                                             DOB = @dob,
                                             Contact_No = @contact, 
                                             NIC = @nic, 
                                             Address = @address, 
                                             Age = @age,
                                             Username = @username,
                                             Password = @password 
                                         WHERE Staff_ID = @staffId";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@name", nametxt.Text.Trim());
                        command.Parameters.AddWithValue("@dob", dobpicker.Value.Date);
                        command.Parameters.AddWithValue("@contact", contactnotxt.Text.Trim());
                        command.Parameters.AddWithValue("@nic", nictxt.Text.Trim());
                        command.Parameters.AddWithValue("@address", addresstxt.Text.Trim());
                        command.Parameters.AddWithValue("@age", int.Parse(age.Text.Trim()));
                        command.Parameters.AddWithValue("@username", usernametxt.Text.Trim());
                        command.Parameters.AddWithValue("@password", passwordtxt.Text.Trim());
                        command.Parameters.AddWithValue("@staffId", currentStaffId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Profile updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No changes were made to the profile.", "Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    MessageBox.Show("This NIC is already registered with another staff member.",
                        "Duplicate NIC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Database Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Back button to return to Staff dashboard
        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            Staff staffForm = new Staff();
            staffForm.Show();
            this.Hide();
        }

        // Show/Hide password checkbox
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                passwordtxt.PasswordChar = '\0';
            }
            else
            {
                passwordtxt.PasswordChar = '*';
            }
        }

        private void contactnotxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void age_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // Validate NIC when user leaves the field
        private void nictxt_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(nictxt.Text))
            {
                if (!IsValidNIC(nictxt.Text.Trim()))
                {
                    MessageBox.Show("Invalid NIC format. Please enter a valid 12-digit NIC.",
                        "Invalid NIC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    nictxt.Focus();
                }
            }
        }

        // Auto-calculate age when DOB changes
        private void dobpicker_ValueChanged(object sender, EventArgs e)
        {
            int calculatedAge = DateTime.Now.Year - dobpicker.Value.Year;

            if (DateTime.Now.DayOfYear < dobpicker.Value.DayOfYear)
                calculatedAge--;

            age.Text = calculatedAge.ToString();
        }

        private bool IsValidNIC(string nic)
        {
            string cleanNic = nic.Trim();
            return cleanNic.Length == 12 && cleanNic.All(char.IsDigit);
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            string cleanNumber = phoneNumber.Replace("-", "").Replace(" ", "").Trim();
            return cleanNumber.Length == 10 && cleanNumber.StartsWith("0") && cleanNumber.All(char.IsDigit);
        }
    }
}