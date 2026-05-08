using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Bike : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";
        private int currentRiderId;
        private bool isEditMode = false;
        private string selectedVehRegNo = "";

        public Bike()
        {
            InitializeComponent();
            currentRiderId = UserSession.UserId; 
            SetupEventHandlers();
        }

        //Load the Bike Details datagrid when the form loads
        private void Bike_Load(object sender, EventArgs e)
        {
            this.bikeTableAdapter1.Fill(this.food_HubDataSet26.Bike);
            try
            {
                this.bikeTableAdapter.Fill(this.food_HubDataSet4.Bike);

                LoadAllBikes(); 
                SetupDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupEventHandlers()
        {
            searchbtn.Click += Searchbtn_Click;
            addbtn.Click += Addbtn_Click;
            updatebtn.Click += Updatebtn_Click;
            deletebtn.Click += Deletebtn_Click;
            clearbtn.Click += Clearbtn_Click;

        }

        private void SetupDataGridView()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;

            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns["vehRegNoDataGridViewTextBoxColumn"].HeaderText = "Vehicle Reg No";
                dataGridView1.Columns["engineNumDataGridViewTextBoxColumn"].HeaderText = "Engine Number";
                dataGridView1.Columns["regDateDataGridViewTextBoxColumn"].HeaderText = "Registration Date";
                dataGridView1.Columns["modelDataGridViewTextBoxColumn"].HeaderText = "Model";

                if (dataGridView1.Columns["brandDataGridViewTextBoxColumn"] != null)
                    dataGridView1.Columns["brandDataGridViewTextBoxColumn"].HeaderText = "Brand";
                if (dataGridView1.Columns["colourDataGridViewTextBoxColumn"] != null)
                    dataGridView1.Columns["colourDataGridViewTextBoxColumn"].HeaderText = "Colour";
            }
        }

        private void LoadAllBikes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Load ALL bikes from the database
                    string query = @"SELECT Veh_Reg_No, Brand, Reg_Date, Colour, Engine_Num, Model
                                   FROM Bike ORDER BY Veh_Reg_No";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        bikeBindingSource.DataSource = dt;
                        dataGridView1.DataSource = bikeBindingSource;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bikes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Search button click event
        private void Searchbtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(vehregnotxt.Text))
            {
                MessageBox.Show("Please enter a Vehicle Registration Number to search.", "Input Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                vehregnotxt.Focus();
                return;
            }

            SearchBike(vehregnotxt.Text.Trim());
        }

        private void SearchBike(string vehRegNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Search for bike by registration number
                    string query = @"SELECT Veh_Reg_No, Brand, Reg_Date, Colour, Engine_Num, Model
                                   FROM Bike WHERE Veh_Reg_No = @vehRegNo";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@vehRegNo", vehRegNo);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Fill form fields with bike data
                                vehregnotxt.Text = reader["Veh_Reg_No"].ToString();
                                brandtxt.Text = reader["Brand"].ToString();
                                colourtxt.Text = reader["Colour"].ToString();
                                enginenotxt.Text = reader["Engine_Num"].ToString();

                                if (reader["Reg_Date"] != DBNull.Value)
                                {
                                    dateTimePicker2.Value = Convert.ToDateTime(reader["Reg_Date"]);
                                }

                                isEditMode = true;
                                selectedVehRegNo = vehRegNo;

                                updatebtn.Enabled = true;
                                deletebtn.Enabled = true;
                                addbtn.Text = "Add New";

                                MessageBox.Show($"Bike details loaded successfully!",
                                    "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No bike found with this registration number.",
                                    "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearForm();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching bike: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Add button click event
        private void Addbtn_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if vehicle registration number already exists
                    string checkQuery = "SELECT COUNT(*) FROM Bike WHERE Veh_Reg_No = @vehRegNo";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@vehRegNo", vehregnotxt.Text.Trim());
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("A bike with this registration number already exists.",
                                "Duplicate Registration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            vehregnotxt.Focus();
                            return;
                        }
                    }
                    //Add new bike
                    string insertQuery = @"INSERT INTO Bike (Veh_Reg_No, Brand, Reg_Date, Colour, Engine_Num, Model)
                                         VALUES (@vehRegNo, @brand, @regDate, @colour, @engineNum, @model)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@vehRegNo", vehregnotxt.Text.Trim());
                        command.Parameters.AddWithValue("@brand", brandtxt.Text.Trim());
                        command.Parameters.AddWithValue("@regDate", dateTimePicker2.Value.Date);
                        command.Parameters.AddWithValue("@colour", colourtxt.Text.Trim());
                        command.Parameters.AddWithValue("@engineNum", enginenotxt.Text.Trim());
                        command.Parameters.AddWithValue("@model", brandtxt.Text.Trim());

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Bike added successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearForm();
                            LoadAllBikes(); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding bike: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Update button click event
        private void Updatebtn_Click(object sender, EventArgs e)
        {
            if (!isEditMode || string.IsNullOrEmpty(selectedVehRegNo))
            {
                MessageBox.Show("Please search and select a bike to update.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Update bike 
                    string updateQuery = @"UPDATE Bike SET 
                                         Brand = @brand,
                                         Reg_Date = @regDate,
                                         Colour = @colour,
                                         Engine_Num = @engineNum,
                                         Model = @model
                                         WHERE Veh_Reg_No = @vehRegNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@brand", brandtxt.Text.Trim());
                        command.Parameters.AddWithValue("@regDate", dateTimePicker2.Value.Date);
                        command.Parameters.AddWithValue("@colour", colourtxt.Text.Trim());
                        command.Parameters.AddWithValue("@engineNum", enginenotxt.Text.Trim());
                        command.Parameters.AddWithValue("@model", brandtxt.Text.Trim());
                        command.Parameters.AddWithValue("@vehRegNo", selectedVehRegNo);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Bike updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadAllBikes(); 
                        }
                        else
                        {
                            MessageBox.Show("Failed to update bike. Please try again.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating bike: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Delete button click event
        private void Deletebtn_Click(object sender, EventArgs e)
        {
            if (!isEditMode || string.IsNullOrEmpty(selectedVehRegNo))
            {
                MessageBox.Show("Please search and select a bike to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to delete the bike with registration number: {selectedVehRegNo}?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Delete bike
                        string deleteQuery = "DELETE FROM Bike WHERE Veh_Reg_No = @vehRegNo";

                        using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@vehRegNo", selectedVehRegNo);

                            int deleteResult = command.ExecuteNonQuery();
                            if (deleteResult > 0)
                            {
                                MessageBox.Show("Bike deleted successfully!", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                ClearForm();
                                LoadAllBikes(); 
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete bike. Please try again.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting bike: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //Clear button click event
        private void Clearbtn_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            vehregnotxt.Clear();
            brandtxt.Clear();
            colourtxt.Clear();
            enginenotxt.Clear();
            dateTimePicker2.Value = DateTime.Now;

            isEditMode = false;
            selectedVehRegNo = "";

            updatebtn.Enabled = false;
            deletebtn.Enabled = false;
            addbtn.Text = "Add";

            vehregnotxt.Focus();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(vehregnotxt.Text))
            {
                MessageBox.Show("Vehicle Registration Number is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                vehregnotxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(brandtxt.Text))
            {
                MessageBox.Show("Brand is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                brandtxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(colourtxt.Text))
            {
                MessageBox.Show("Colour is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                colourtxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(enginenotxt.Text))
            {
                MessageBox.Show("Engine Number is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                enginenotxt.Focus();
                return false;
            }

            if (dateTimePicker2.Value > DateTime.Today)
            {
                MessageBox.Show("Registration date cannot be in the future.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dateTimePicker2.Focus();
                return false;
            }

            return true;
        }

        //Back button to go to Rider Dashboard
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