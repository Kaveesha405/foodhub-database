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
    public partial class Rider_Management : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";

        public Rider_Management()
        {
            InitializeComponent();
            InitializeFormEvents();
            LoadRidersToComboBox();
            ClearRiderFields();
            ClearDependentFields();
            SetPasswordMasking();
            LoadDataGrids();
        }

        private void InitializeFormEvents()
        {
            guna2GradientTileButton1.Click += SearchRiderButton_Click;
            guna2GradientTileButton2.Click += AddRiderButton_Click;
            guna2GradientTileButton3.Click += UpdateRiderButton_Click;
            guna2GradientTileButton4.Click += DeleteRiderButton_Click;
            guna2GradientTileButton5.Click += ClearRiderButton_Click;

            guna2GradientTileButton6.Click += SearchDependentButton_Click;
            guna2GradientTileButton10.Click += AddDependentButton_Click;
            guna2GradientTileButton9.Click += UpdateDependentButton_Click;
            guna2GradientTileButton8.Click += DeleteDependentButton_Click;
            guna2GradientTileButton7.Click += ClearDependentButton_Click;

            riderdobpicker.ValueChanged += RiderDobPicker_ValueChanged;
            checkBox1.CheckedChanged += ShowPasswordCheckBox_CheckedChanged;
            ridercmb.SelectedIndexChanged += RiderComboBox_SelectedIndexChanged;
        }

        private void SetPasswordMasking()
        {
            passwordtxt.UseSystemPasswordChar = true;
        }

        private void LoadDataGrids()
        {
            LoadRiderDataGrid();
            LoadDependentDataGrid();
        }

        private void RefreshDataGridViews()
        {
            try
            {
                LoadRiderDataGrid();
                LoadDependentDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing data grids: {ex.Message}", "Refresh Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        //Load Rider details in the datagrid view

        private void LoadRiderDataGrid()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT Rider_ID, First_Name, Middle_Name, Last_Name, NIC, Contact_No, 
                                   DOB, Age, Address, Licence_No, Username FROM Rider ORDER BY Rider_ID";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;

                        dataGridView1.AutoResizeColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rider data: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Load Dependent details in the datagrid view
        private void LoadDependentDataGrid()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT d.Rider_ID, d.Dep_Name, d.DOB, d.Relationship, 
                                   r.First_Name + ' ' + r.Last_Name as Rider_Name 
                                   FROM Dependent d 
                                   INNER JOIN Rider r ON d.Rider_ID = r.Rider_ID
                                   ORDER BY d.Rider_ID, d.Dep_Name";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView2.DataSource = dt;

                        dataGridView2.AutoResizeColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dependent data: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Rider ComboBox loading function
        private void LoadRidersToComboBox()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Rider_ID FROM Rider ORDER BY Rider_ID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ridercmb.Items.Clear();
                        while (reader.Read())
                        {
                            ridercmb.Items.Add(reader["Rider_ID"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading riders: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //rider management functions
        private void SearchRiderButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Rideridtxtbox.Text))
            {
                MessageBox.Show("Please enter a Rider ID to search.", "Search Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    //Search for rider using RiderID
                    string riderQuery = @"SELECT Rider_ID, First_Name, Middle_Name, Last_Name, NIC, Contact_No, 
                                        DOB, Age, Address, Licence_No, Username, Password 
                                        FROM Rider WHERE Rider_ID = @riderId";

                    using (SqlCommand command = new SqlCommand(riderQuery, connection))
                    {
                        command.Parameters.AddWithValue("@riderId", int.Parse(Rideridtxtbox.Text.Trim()));

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate rider fields
                                Firsttxt.Text = reader["First_Name"].ToString();
                                middletxt.Text = reader["Middle_Name"].ToString();
                                lasttxt.Text = reader["Last_Name"].ToString();
                                nictxt.Text = reader["NIC"].ToString();
                                txtbox.Text = reader["Contact_No"].ToString();
                                riderdobpicker.Value = Convert.ToDateTime(reader["DOB"]);
                                agetxt.Text = reader["Age"].ToString();
                                addresstxt.Text = reader["Address"].ToString();
                                licencetxt.Text = reader["Licence_No"].ToString();
                                usernametxt.Text = reader["Username"].ToString();
                                passwordtxt.Text = reader["Password"].ToString();

                                reader.Close();

                                SearchDependentsForRider(int.Parse(Rideridtxtbox.Text.Trim()));

                                MessageBox.Show("Rider record found and loaded.", "Search Successful",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No rider found with the provided ID.", "Rider Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearRiderFields();
                                ClearDependentFields();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching for rider: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Search Dependent details for a rider
        private void SearchDependentsForRider(int riderId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string dependentQuery = @"SELECT Dep_Name, DOB, Relationship FROM Dependent WHERE Rider_ID = @riderId";

                    using (SqlCommand command = new SqlCommand(dependentQuery, connection))
                    {
                        command.Parameters.AddWithValue("@riderId", riderId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                //Populate dependent fields
                                Depnametxt.Text = reader["Dep_Name"].ToString();
                                depdob.Value = Convert.ToDateTime(reader["DOB"]);
                                relationtxt.Text = reader["Relationship"].ToString();
                                ridercmb.Text = riderId.ToString();
                            }
                            else
                            {
                                MessageBox.Show("No dependents found for this rider.", "No Dependents",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearDependentFields();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching for dependents: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Rider add button click event
        private void AddRiderButton_Click(object sender, EventArgs e)
        {
            if (!ValidateRiderFields())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    //check if RiderID already exists
                    string checkQuery = "SELECT COUNT(*) FROM Rider WHERE Rider_ID = @riderId";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@riderId", int.Parse(Rideridtxtbox.Text.Trim()));
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Rider ID already exists. Please use a different ID.", "Duplicate ID",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string insertQuery = @"INSERT INTO Rider (Rider_ID, First_Name, Middle_Name, Last_Name, NIC, Contact_No, 
                                         DOB, Age, Address, Licence_No, Username, Password) 
                                         VALUES (@riderId, @firstName, @middleName, @lastName, @nic, @contactNo, 
                                         @dob, @age, @address, @licenceNo, @username, @password)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        AddRiderParametersToCommand(command);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Rider added successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearRiderFields();
                            LoadRidersToComboBox();
                            RefreshDataGridViews(); // Live refresh
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding rider: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Rider Update Button click event
        private void UpdateRiderButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Rideridtxtbox.Text))
            {
                MessageBox.Show("Please search for a rider first or enter a Rider ID.", "Update Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateRiderFields())
                return;

            DialogResult result = MessageBox.Show("Are you sure you want to update this rider record?",
                "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = @"UPDATE Rider SET First_Name = @firstName, Middle_Name = @middleName, 
                                         Last_Name = @lastName, NIC = @nic, Contact_No = @contactNo, DOB = @dob, 
                                         Age = @age, Address = @address, Licence_No = @licenceNo, Username = @username, 
                                         Password = @password WHERE Rider_ID = @riderId";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        AddRiderParametersToCommand(command);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Rider updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshDataGridViews(); // Live refresh
                        }
                        else
                        {
                            MessageBox.Show("No rider found with the provided ID.", "Update Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating rider: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Rider Delete button click event
        private void DeleteRiderButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Rideridtxtbox.Text))
            {
                MessageBox.Show("Please search for a rider first or enter a Rider ID.", "Delete Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to delete the rider record for ID: {Rideridtxtbox.Text}?\n\nThis will also delete all associated dependents.\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete dependent
                            string deleteDependentsQuery = "DELETE FROM Dependent WHERE Rider_ID = @riderId";
                            using (SqlCommand command = new SqlCommand(deleteDependentsQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@riderId", int.Parse(Rideridtxtbox.Text.Trim()));
                                command.ExecuteNonQuery();
                            }

                            // Delete rider
                            string deleteRiderQuery = "DELETE FROM Rider WHERE Rider_ID = @riderId";
                            using (SqlCommand command = new SqlCommand(deleteRiderQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@riderId", int.Parse(Rideridtxtbox.Text.Trim()));
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    transaction.Commit();
                                    MessageBox.Show("Rider and associated dependents deleted successfully!", "Success",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    ClearRiderFields();
                                    ClearDependentFields();
                                    LoadRidersToComboBox();
                                    RefreshDataGridViews();
                                }
                                else
                                {
                                    transaction.Rollback();
                                    MessageBox.Show("No rider found with the provided ID.", "Delete Failed",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting rider: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Rider clear button click event
        private void ClearRiderButton_Click(object sender, EventArgs e)
        {
            ClearRiderFields();
        }

        //dependent search button click event
        private void SearchDependentButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Depnametxt.Text))
            {
                MessageBox.Show("Please enter a dependent name to search.", "Search Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT Rider_ID, Dep_Name, DOB, Relationship FROM Dependent 
                                   WHERE Dep_Name LIKE @depName";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@depName", "%" + Depnametxt.Text.Trim() + "%");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ridercmb.Text = reader["Rider_ID"].ToString();
                                depdob.Value = Convert.ToDateTime(reader["DOB"]);
                                relationtxt.Text = reader["Relationship"].ToString();

                                MessageBox.Show("Dependent record found and loaded.", "Search Successful",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No dependent found with the provided name.", "Dependent Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearDependentFields();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching for dependent: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //dependent add button click event
        private void AddDependentButton_Click(object sender, EventArgs e)
        {
            if (!ValidateDependentFields())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    //check if dependent with same name and rider already exists
                    string checkQuery = "SELECT COUNT(*) FROM Dependent WHERE Rider_ID = @riderId AND Dep_Name = @depName";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@riderId", int.Parse(ridercmb.Text));
                        checkCommand.Parameters.AddWithValue("@depName", Depnametxt.Text.Trim());
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("A dependent with this name already exists for this rider.", "Duplicate Dependent",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string insertQuery = @"INSERT INTO Dependent (Rider_ID, Dep_Name, DOB, Relationship) 
                                         VALUES (@riderId, @depName, @dob, @relationship)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        AddDependentParametersToCommand(command);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Dependent added successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearDependentFields();
                            RefreshDataGridViews(); // Live refresh
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding dependent: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //dependent update button click event
        private void UpdateDependentButton_Click(object sender, EventArgs e)
        {
            if (!ValidateDependentFields())
                return;

            DialogResult result = MessageBox.Show("Are you sure you want to update this dependent record?",
                "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = @"UPDATE Dependent SET DOB = @dob, Relationship = @relationship 
                                         WHERE Rider_ID = @riderId AND Dep_Name = @depName";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        AddDependentParametersToCommand(command);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Dependent updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshDataGridViews();
                        }
                        else
                        {
                            MessageBox.Show("No dependent found with the provided details.", "Update Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating dependent: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //dependent delete button click event
        private void DeleteDependentButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Depnametxt.Text) || string.IsNullOrWhiteSpace(ridercmb.Text))
            {
                MessageBox.Show("Please search for a dependent first.", "Delete Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to delete the dependent: {Depnametxt.Text}?\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM Dependent WHERE Rider_ID = @riderId AND Dep_Name = @depName";

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@riderId", int.Parse(ridercmb.Text));
                        command.Parameters.AddWithValue("@depName", Depnametxt.Text.Trim());

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Dependent deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearDependentFields();
                                RefreshDataGridViews(); 
                        }
                        else
                        {
                            MessageBox.Show("No dependent found with the provided details.", "Delete Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting dependent: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearDependentButton_Click(object sender, EventArgs e)
        {
            ClearDependentFields();
        }
        //Helper Methods
        private void ClearRiderFields()
        {
            Rideridtxtbox.Clear();
            Firsttxt.Clear();
            middletxt.Clear();
            lasttxt.Clear();
            nictxt.Clear();
            txtbox.Clear();
            riderdobpicker.Value = DateTime.Now;
            agetxt.Clear();
            addresstxt.Clear();
            licencetxt.Clear();
            usernametxt.Clear();
            passwordtxt.Clear();
            checkBox1.Checked = false;
        }

        private void ClearDependentFields()
        {
            Depnametxt.Clear();
            ridercmb.SelectedIndex = -1;
            depdob.Value = DateTime.Now;
            relationtxt.Clear();
        }

        private void RiderDobPicker_ValueChanged(object sender, EventArgs e)
        {
            DateTime birthDate = riderdobpicker.Value;
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

            agetxt.Text = age.ToString();
        }

        private void ShowPasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            passwordtxt.UseSystemPasswordChar = !checkBox1.Checked;
        }

        private void RiderComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        //Validate rider fields
        private bool ValidateRiderFields()
        {
            if (string.IsNullOrWhiteSpace(Rideridtxtbox.Text))
            {
                MessageBox.Show("Please enter Rider ID.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Rideridtxtbox.Focus();
                return false;
            }

            if (!int.TryParse(Rideridtxtbox.Text.Trim(), out _))
            {
                MessageBox.Show("Please enter a valid numeric Rider ID.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Rideridtxtbox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Firsttxt.Text))
            {
                MessageBox.Show("Please enter First Name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Firsttxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(lasttxt.Text))
            {
                MessageBox.Show("Please enter Last Name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lasttxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(nictxt.Text))
            {
                MessageBox.Show("Please enter NIC.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nictxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtbox.Text))
            {
                MessageBox.Show("Please enter Contact Number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtbox.Focus();
                return false;
            }

            if (!txtbox.Text.All(char.IsDigit) || txtbox.Text.Length < 10)
            {
                MessageBox.Show("Please enter a valid contact number (at least 10 digits).", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtbox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(addresstxt.Text))
            {
                MessageBox.Show("Please enter Address.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                addresstxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(licencetxt.Text))
            {
                MessageBox.Show("Please enter Licence Number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                licencetxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(usernametxt.Text))
            {
                MessageBox.Show("Please enter Username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                usernametxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(passwordtxt.Text))
            {
                MessageBox.Show("Please enter Password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                passwordtxt.Focus();
                return false;
            }

            return true;
        }

        //Validate dependent fields
        private bool ValidateDependentFields()
        {
            if (string.IsNullOrWhiteSpace(Depnametxt.Text))
            {
                MessageBox.Show("Please enter Dependent Name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Depnametxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ridercmb.Text))
            {
                MessageBox.Show("Please select a Rider ID.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ridercmb.Focus();
                return false;
            }

            if (!int.TryParse(ridercmb.Text, out _))
            {
                MessageBox.Show("Please select a valid Rider ID.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ridercmb.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(relationtxt.Text))
            {
                MessageBox.Show("Please enter Relationship.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                relationtxt.Focus();
                return false;
            }

            return true;
        }

        private void AddRiderParametersToCommand(SqlCommand command)
        {
            command.Parameters.AddWithValue("@riderId", int.Parse(Rideridtxtbox.Text.Trim()));
            command.Parameters.AddWithValue("@firstName", Firsttxt.Text.Trim());
            command.Parameters.AddWithValue("@middleName", middletxt.Text.Trim());
            command.Parameters.AddWithValue("@lastName", lasttxt.Text.Trim());
            command.Parameters.AddWithValue("@nic", nictxt.Text.Trim());
            command.Parameters.AddWithValue("@contactNo", txtbox.Text.Trim());
            command.Parameters.AddWithValue("@dob", riderdobpicker.Value.Date);
            command.Parameters.AddWithValue("@age", int.Parse(agetxt.Text));
            command.Parameters.AddWithValue("@address", addresstxt.Text.Trim());
            command.Parameters.AddWithValue("@licenceNo", licencetxt.Text.Trim());
            command.Parameters.AddWithValue("@username", usernametxt.Text.Trim());
            command.Parameters.AddWithValue("@password", passwordtxt.Text.Trim());
        }

        private void AddDependentParametersToCommand(SqlCommand command)
        {
            command.Parameters.AddWithValue("@riderId", int.Parse(ridercmb.Text));
            command.Parameters.AddWithValue("@depName", Depnametxt.Text.Trim());
            command.Parameters.AddWithValue("@dob", depdob.Value.Date);
            command.Parameters.AddWithValue("@relationship", relationtxt.Text.Trim());
        }

        //back to admin dashboard button click event
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

    }
}