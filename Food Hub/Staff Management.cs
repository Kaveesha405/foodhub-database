using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Staff_Management : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";

        public Staff_Management()
        {
            InitializeComponent();
            InitializeFormEvents();
            ClearFields();
            SetPasswordMasking();
        }

        private void InitializeFormEvents()
        {
            guna2GradientTileButton1.Click += SearchButton_Click;
            guna2GradientTileButton2.Click += AddButton_Click;
            guna2GradientTileButton3.Click += UpdateButton_Click;
            guna2GradientTileButton4.Click += DeleteButton_Click;
            guna2GradientTileButton5.Click += ClearButton_Click;

            DateofBirthpicker.ValueChanged += DateofBirthpicker_ValueChanged;
            checkBox1.CheckedChanged += ShowPasswordCheckBox_CheckedChanged;
        }

        private void SetPasswordMasking()
        {
            Passwordtxtbox.UseSystemPasswordChar = true;
        }

        //Data grid refresh method
        private void RefreshDataGridView()
        {
            try
            {
                if (this.staffTableAdapter != null && this.food_HubDataSet3 != null)
                {
                    this.staffTableAdapter.Fill(this.food_HubDataSet16.Staff);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Refresh Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //search button click event
        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StaffIDtxtbox.Text))
            {
                MessageBox.Show("Please enter a Staff ID to search.", "Search Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT Staff_ID, Name, DOB, Contact_No, NIC, Address, Age, Username, Password 
                                   FROM Staff WHERE Staff_ID = @staffId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@staffId", StaffIDtxtbox.Text.Trim());

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                StaffNametxtbox.Text = reader["Name"].ToString();
                                DateofBirthpicker.Value = Convert.ToDateTime(reader["DOB"]);
                                ContactNotxtbox.Text = reader["Contact_No"].ToString();
                                StaffNICtxtbox.Text = reader["NIC"].ToString();
                                Addresstxtbox.Text = reader["Address"].ToString();
                                Agetxtbox.Text = reader["Age"].ToString();
                                Usernametxtbox.Text = reader["Username"].ToString();
                                Passwordtxtbox.Text = reader["Password"].ToString();

                                MessageBox.Show("Staff record found and loaded.", "Search Successful",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No staff found with the provided ID.", "Staff Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearFields();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching for staff: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //add button click event
        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!ValidateFields())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Staff WHERE Staff_ID = @staffId";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@staffId", StaffIDtxtbox.Text.Trim());
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Staff ID already exists. Please use a different ID.", "Duplicate ID",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string insertQuery = @"INSERT INTO Staff (Staff_ID, Name, DOB, Contact_No, NIC, Address, Age, Username, Password) 
                                         VALUES (@staffId, @name, @dob, @contactNo, @nic, @address, @age, @username, @password)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        AddParametersToCommand(command);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Staff added successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            RefreshDataGridView();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding staff: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //update button click event
        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StaffIDtxtbox.Text))
            {
                MessageBox.Show("Please search for a staff member first or enter a Staff ID.", "Update Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateFields())
                return;

            DialogResult result = MessageBox.Show("Are you sure you want to update this staff record?",
                "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = @"UPDATE Staff SET Name = @name, DOB = @dob, Contact_No = @contactNo, 
                                         NIC = @nic, Address = @address, Age = @age, Username = @username, Password = @password 
                                         WHERE Staff_ID = @staffId";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        AddParametersToCommand(command);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Staff updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshDataGridView();
                        }
                        else
                        {
                            MessageBox.Show("No staff found with the provided ID.", "Update Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating staff: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //delete button click event
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StaffIDtxtbox.Text))
            {
                MessageBox.Show("Please search for a staff member first or enter a Staff ID.", "Delete Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to delete the staff record for ID: {StaffIDtxtbox.Text}?\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM Staff WHERE Staff_ID = @staffId";

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@staffId", StaffIDtxtbox.Text.Trim());

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Staff deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            RefreshDataGridView();
                        }
                        else
                        {
                            MessageBox.Show("No staff found with the provided ID.", "Delete Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting staff: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //clear button click event
        private void ClearButton_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            StaffIDtxtbox.Clear();
            StaffNametxtbox.Clear();
            DateofBirthpicker.Value = DateTime.Now;
            ContactNotxtbox.Clear();
            StaffNICtxtbox.Clear();
            Addresstxtbox.Clear();
            Agetxtbox.Clear();
            Usernametxtbox.Clear();
            Passwordtxtbox.Clear();
            checkBox1.Checked = false;
        }

        //date time picker value changed event to calculate age
        private void DateofBirthpicker_ValueChanged(object sender, EventArgs e)
        {
            DateTime birthDate = DateofBirthpicker.Value;
            DateTime today = DateTime.Today;

            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            if (age < 0)
            {
                age = 0;
            }

            Agetxtbox.Text = age.ToString();
        }

        private void ShowPasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Passwordtxtbox.UseSystemPasswordChar = !checkBox1.Checked;
        }

        //validate input fields method
        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(StaffIDtxtbox.Text))
            {
                MessageBox.Show("Please enter Staff ID.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                StaffIDtxtbox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(StaffNametxtbox.Text))
            {
                MessageBox.Show("Please enter Staff Name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                StaffNametxtbox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(StaffNICtxtbox.Text))
            {
                MessageBox.Show("Please enter NIC.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                StaffNICtxtbox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ContactNotxtbox.Text))
            {
                MessageBox.Show("Please enter Contact Number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ContactNotxtbox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Addresstxtbox.Text))
            {
                MessageBox.Show("Please enter Address.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Addresstxtbox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Usernametxtbox.Text))
            {
                MessageBox.Show("Please enter Username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Usernametxtbox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Passwordtxtbox.Text))
            {
                MessageBox.Show("Please enter Password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Passwordtxtbox.Focus();
                return false;
            }

            if (!ContactNotxtbox.Text.All(char.IsDigit) || ContactNotxtbox.Text.Length < 10)
            {
                MessageBox.Show("Please enter a valid contact number (at least 10 digits).", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ContactNotxtbox.Focus();
                return false;
            }

            return true;
        }

        private void AddParametersToCommand(SqlCommand command)
        {
            command.Parameters.AddWithValue("@staffId", StaffIDtxtbox.Text.Trim());
            command.Parameters.AddWithValue("@name", StaffNametxtbox.Text.Trim());
            command.Parameters.AddWithValue("@dob", DateofBirthpicker.Value.Date);
            command.Parameters.AddWithValue("@contactNo", ContactNotxtbox.Text.Trim());
            command.Parameters.AddWithValue("@nic", StaffNICtxtbox.Text.Trim());
            command.Parameters.AddWithValue("@address", Addresstxtbox.Text.Trim());
            command.Parameters.AddWithValue("@age", int.Parse(Agetxtbox.Text));
            command.Parameters.AddWithValue("@username", Usernametxtbox.Text.Trim());
            command.Parameters.AddWithValue("@password", Passwordtxtbox.Text.Trim());
        }

        //back button click event to go back to admin dashboard
        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            if (!UserSession.IsLoggedIn() || UserSession.UserType != "Admin")
            {
                MessageBox.Show("Session expired. Please log in again.", "Session Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                UserSession.ClearSession();
                LogIn loginForm = new LogIn();
                loginForm.Show();
                this.Close();
                return;
            }

            Admin admin = new Admin();
            admin.Show();
            this.Hide();
        }

        //staff management form load event to load data into datagridview
        private void Staff_Management_Load(object sender, EventArgs e)
        {
            this.staffTableAdapter.Fill(this.food_HubDataSet16.Staff);
            try
            {
                this.staffTableAdapter.Fill(this.food_HubDataSet16.Staff);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}