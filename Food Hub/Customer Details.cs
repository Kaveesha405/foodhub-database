using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Customer_Details : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";
        private int selectedCustomerId = -1;

        public Customer_Details()
        {
            InitializeComponent();
            LoadCustomerData();
            SetupEventHandlers();
            ClearFields();
        }

        private void SetupEventHandlers()
        {
            Addbtn.Click += Addbtn_Click;
            Updatebtn.Click += Updatebtn_Click;
            Deletebtn.Click += Deletebtn_Click;
            Clearbtn.Click += Clearbtn_Click;
            Searchbtn.Click += Searchbtn_Click;
        }

        private void Customer_Details_Load(object sender, EventArgs e)
        {
            try
            {
                this.customerTableAdapter.Fill(this.food_HubDataSet6.Customer);
                LoadCustomerData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load all customer data into DataGridView
        private void LoadCustomerData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Cus_ID, Cus_Name, NIC, Contact_No FROM Customer ORDER BY Cus_ID";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customer data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Search button click event
        private void Searchbtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CusIDtxt.Text) && int.TryParse(CusIDtxt.Text, out int customerId))
            {
                SearchCustomerById(customerId);
            }
        }

        // Search customer by ID and populate fields
        private void SearchCustomerById(int customerId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT Cus_ID, Cus_Name, NIC, DOB, Contact_No, 
                                   Location_No, City, Street, Lane 
                                   FROM Customer WHERE Cus_ID = @customerId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@customerId", customerId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                selectedCustomerId = Convert.ToInt32(reader["Cus_ID"]);
                                CusNametxt.Text = reader["Cus_Name"].ToString();
                                NICtxt.Text = reader["NIC"].ToString();
                                Contatxt.Text = reader["Contact_No"].ToString();
                                LocationNotxt.Text = reader["Location_No"].ToString();
                                Citytxt.Text = reader["City"].ToString();
                                Streettxt.Text = reader["Street"].ToString();
                                Lanetxt.Text = reader["Lane"].ToString();

                                if (reader["DOB"] != DBNull.Value)
                                {
                                    dobpicker.Value = Convert.ToDateTime(reader["DOB"]);
                                }

                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching customer: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add new customer click event
        private void Addbtn_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if customer ID already exists
                    if (!string.IsNullOrEmpty(CusIDtxt.Text) && int.TryParse(CusIDtxt.Text, out int checkId))
                    {
                        string checkQuery = "SELECT COUNT(*) FROM Customer WHERE Cus_ID = @customerId";
                        using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@customerId", checkId);
                            int count = (int)checkCommand.ExecuteScalar();
                            if (count > 0)
                            {
                                MessageBox.Show("Customer ID already exists. Use Update to modify existing customer.",
                                    "Duplicate ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    string query = @"INSERT INTO Customer (Cus_ID, Cus_Name, NIC, DOB, Contact_No, 
                                   Location_No, City, Street, Lane) 
                                   VALUES (@cusId, @cusName, @nic, @dob, @contact, 
                                   @locationNo, @city, @street, @lane)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddParametersToCommand(command);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Customer added successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadCustomerData(); 
                            selectedCustomerId = int.Parse(CusIDtxt.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding customer: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update existing customer
        private void Updatebtn_Click(object sender, EventArgs e)
        {
            if (selectedCustomerId == -1)
            {
                MessageBox.Show("Please select a customer to update.", "Warning",
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
                    string query = @"UPDATE Customer SET Cus_Name = @cusName, NIC = @nic, 
                                   DOB = @dob, Contact_No = @contact, Location_No = @locationNo, 
                                   City = @city, Street = @street, Lane = @lane 
                                   WHERE Cus_ID = @cusId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddParametersToCommand(command);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Customer updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadCustomerData(); // Refresh DataGridView
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating customer: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete customer click event
        private void Deletebtn_Click(object sender, EventArgs e)
        {
            if (selectedCustomerId == -1)
            {
                MessageBox.Show("Please select a customer to delete.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to delete customer {CusNametxt.Text}?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Customer WHERE Cus_ID = @cusId";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@cusId", selectedCustomerId);

                            int deleteResult = command.ExecuteNonQuery();
                            if (deleteResult > 0)
                            {
                                MessageBox.Show("Customer deleted successfully!", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadCustomerData();
                                ClearFields();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Clear button click event
        private void Clearbtn_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        // Clear all input fields
        private void ClearFields()
        {
            CusIDtxt.Clear();
            CusNametxt.Clear();
            NICtxt.Clear();
            Contatxt.Clear();
            LocationNotxt.Clear();
            Citytxt.Clear();
            Streettxt.Clear();
            Lanetxt.Clear();
            dobpicker.Value = DateTime.Now;

        }

        private void AddParametersToCommand(SqlCommand command)
        {
            command.Parameters.AddWithValue("@cusId", int.Parse(CusIDtxt.Text));
            command.Parameters.AddWithValue("@cusName", CusNametxt.Text.Trim());
            command.Parameters.AddWithValue("@nic", NICtxt.Text.Trim());
            command.Parameters.AddWithValue("@dob", dobpicker.Value.Date);
            command.Parameters.AddWithValue("@contact", Contatxt.Text.Trim());
            command.Parameters.AddWithValue("@locationNo", LocationNotxt.Text.Trim());
            command.Parameters.AddWithValue("@city", Citytxt.Text.Trim());
            command.Parameters.AddWithValue("@street", Streettxt.Text.Trim());
            command.Parameters.AddWithValue("@lane", Lanetxt.Text.Trim());
        }

        // Validate input fields
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(CusIDtxt.Text))
            {
                MessageBox.Show("Customer ID is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CusIDtxt.Focus();
                return false;
            }

            if (!int.TryParse(CusIDtxt.Text, out int cusId) || cusId <= 0)
            {
                MessageBox.Show("Customer ID must be a positive number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CusIDtxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(CusNametxt.Text))
            {
                MessageBox.Show("Customer Name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CusNametxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(NICtxt.Text))
            {
                MessageBox.Show("NIC is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NICtxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Contatxt.Text))
            {
                MessageBox.Show("Contact Number is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Contatxt.Focus();
                return false;
            }

            // Validate contact number 
            if (Contatxt.Text.Length < 10)
            {
                MessageBox.Show("Contact Number should be at least 10 digits.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Contatxt.Focus();
                return false;
            }

            return true;
        }

        // Back button event return to Staff dashboard
        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to go back to the Staff dashboard?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Staff staffForm = new Staff();
                staffForm.Show();
                this.Hide();
            }
        }

        // Proceed to Order button event
        private void guna2GradientTileButton2_Click(object sender, EventArgs e)
        {
            Order order = new Order();
            order.Show();
            this.Hide();
        }

    }
}