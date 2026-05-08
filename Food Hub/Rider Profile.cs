using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Rider_Profile : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";
        private int currentRiderId;
        private bool hasDependent = false;

        public Rider_Profile()
        {
            InitializeComponent();
            currentRiderId = UserSession.UserId;
            SetupEventHandlers();
        }

        //Rider Profile Load events
        private void Rider_Profile_Load(object sender, EventArgs e)
        {
            LoadRiderProfile();
            LoadDependentData();
        }

        private void SetupEventHandlers()
        {
            Dobriderpicker.ValueChanged += Dobriderpicker_ValueChanged;

            updatebtn.Click += Updatebtn_Click;
            adddependentbtn.Click += Adddependentbtn_Click;

            checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
        }

        // Load rider profile data from the database
        private void LoadRiderProfile()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT First_name, Middle_Name, Last_Name, NIC, DOB, Age, 
                                   Contact_No, Address, Licence_No, Username, Password 
                                   FROM Rider WHERE Rider_ID = @riderId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@riderId", currentRiderId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Fill rider personal information
                                Firstnametxt.Text = reader["First_name"]?.ToString() ?? "";
                                middlenametxt.Text = reader["Middle_Name"]?.ToString() ?? "";
                                lastnametxt.Text = reader["Last_Name"]?.ToString() ?? "";
                                nictxt.Text = reader["NIC"]?.ToString() ?? "";

                                // Handle DOB
                                if (reader["DOB"] != DBNull.Value)
                                {
                                    Dobriderpicker.Value = Convert.ToDateTime(reader["DOB"]);
                                }

                                agetxt.Text = reader["Age"]?.ToString() ?? "";
                                contactnotxt.Text = reader["Contact_No"]?.ToString() ?? "";
                                addresstxt.Text = reader["Address"]?.ToString() ?? "";
                                licencenotxt.Text = reader["Licence_No"]?.ToString() ?? "";
                                usernametxt.Text = reader["Username"]?.ToString() ?? "";
                                passwordtxt.Text = reader["Password"]?.ToString() ?? "";
                                passwordtxt.UseSystemPasswordChar = true;
                            }
                            else
                            {
                                MessageBox.Show("Rider profile not found.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rider profile: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load dependent data if exists
        private void LoadDependentData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT Dep_Name, DOB, Relationship FROM Dependent 
                                   WHERE Rider_ID = @riderId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@riderId", currentRiderId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hasDependent = true;
                                depnametxt.Text = reader["Dep_Name"]?.ToString() ?? "";
                                relationshiotxt.Text = reader["Relationship"]?.ToString() ?? "";

                                if (reader["DOB"] != DBNull.Value)
                                {
                                    dateofbirthpickerdependent.Value = Convert.ToDateTime(reader["DOB"]);
                                }

                                adddependentbtn.Text = "Update Dependent";
                            }
                            else
                            {
                                hasDependent = false;
                                ClearDependentFields();
                                adddependentbtn.Text = "Add Dependent";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dependent data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearDependentFields()
        {
            depnametxt.Clear();
            relationshiotxt.Clear();
            dateofbirthpickerdependent.Value = DateTime.Now;
        }

        private void Dobriderpicker_ValueChanged(object sender, EventArgs e)
        {
            int age = CalculateAge(Dobriderpicker.Value);
            agetxt.Text = age.ToString();
        }


        private int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age < 0 ? 0 : age;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            passwordtxt.UseSystemPasswordChar = !checkBox1.Checked;
        }

        //Update rider profile information button click event
        private void Updatebtn_Click(object sender, EventArgs e)
        {
            if (!ValidateRiderInput())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"UPDATE Rider SET 
                                   First_name = @firstName,
                                   Middle_Name = @middleName,
                                   Last_Name = @lastName,
                                   NIC = @nic,
                                   DOB = @dob,
                                   Age = @age,
                                   Contact_No = @contactNo,
                                   Address = @address,
                                   Licence_No = @licenceNo,
                                   Username = @username,
                                   Password = @password
                                   WHERE Rider_ID = @riderId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@firstName", Firstnametxt.Text.Trim());
                        command.Parameters.AddWithValue("@middleName", middlenametxt.Text.Trim());
                        command.Parameters.AddWithValue("@lastName", lastnametxt.Text.Trim());
                        command.Parameters.AddWithValue("@nic", nictxt.Text.Trim());
                        command.Parameters.AddWithValue("@dob", Dobriderpicker.Value.Date);
                        command.Parameters.AddWithValue("@age", int.Parse(agetxt.Text));
                        command.Parameters.AddWithValue("@contactNo", contactnotxt.Text.Trim());
                        command.Parameters.AddWithValue("@address", addresstxt.Text.Trim());
                        command.Parameters.AddWithValue("@licenceNo", licencenotxt.Text.Trim());
                        command.Parameters.AddWithValue("@username", usernametxt.Text.Trim());
                        command.Parameters.AddWithValue("@password", passwordtxt.Text.Trim());
                        command.Parameters.AddWithValue("@riderId", currentRiderId);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Profile updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update profile.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add dependent information button click event
        private void Adddependentbtn_Click(object sender, EventArgs e)
        {
            if (!ValidateDependentInput())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query;
                    if (hasDependent)
                    {
                        // Update existing dependent
                        query = @"UPDATE Dependent SET 
                                Dep_Name = @depName,
                                DOB = @depDOB,
                                Relationship = @relationship
                                WHERE Rider_ID = @riderId";
                    }
                    else
                    {
                        // Add new dependent
                        query = @"INSERT INTO Dependent (Rider_ID, Dep_Name, DOB, Relationship)
                                VALUES (@riderId, @depName, @depDOB, @relationship)";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@riderId", currentRiderId);
                        command.Parameters.AddWithValue("@depName", depnametxt.Text.Trim());
                        command.Parameters.AddWithValue("@depDOB", dateofbirthpickerdependent.Value.Date);
                        command.Parameters.AddWithValue("@relationship", relationshiotxt.Text.Trim());

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            string message = hasDependent ? "Dependent updated successfully!" : "Dependent added successfully!";
                            MessageBox.Show(message, "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            hasDependent = true;
                            adddependentbtn.Text = "Update Dependent";
                        }
                        else
                        {
                            MessageBox.Show("Failed to save dependent information.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving dependent: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //validate rider input fields
        private bool ValidateRiderInput()
        {
            if (string.IsNullOrWhiteSpace(Firstnametxt.Text))
            {
                MessageBox.Show("First Name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Firstnametxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(lastnametxt.Text))
            {
                MessageBox.Show("Last Name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lastnametxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(nictxt.Text))
            {
                MessageBox.Show("NIC is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nictxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(contactnotxt.Text))
            {
                MessageBox.Show("Contact Number is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                contactnotxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(usernametxt.Text))
            {
                MessageBox.Show("Username is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                usernametxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(passwordtxt.Text))
            {
                MessageBox.Show("Password is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                passwordtxt.Focus();
                return false;
            }

            if (Dobriderpicker.Value > DateTime.Today)
            {
                MessageBox.Show("Date of birth cannot be in the future.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Dobriderpicker.Focus();
                return false;
            }

            return true;
        }

        //validate dependent input fields
        private bool ValidateDependentInput()
        {
            if (string.IsNullOrWhiteSpace(depnametxt.Text))
            {
                MessageBox.Show("Dependent Name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                depnametxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(relationshiotxt.Text))
            {
                MessageBox.Show("Relationship is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                relationshiotxt.Focus();
                return false;
            }

            if (dateofbirthpickerdependent.Value > DateTime.Today)
            {
                MessageBox.Show("Dependent's date of birth cannot be in the future.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dateofbirthpickerdependent.Focus();
                return false;
            }

            return true;
        }

        // Back button - return to Rider dashboard
        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to go back to the dashboard?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Rider riderForm = new Rider();
                riderForm.Show();
                this.Hide();
            }
        }
    }
}