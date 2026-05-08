using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Bike_Assign : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";

        public Bike_Assign()
        {
            InitializeComponent();
        }

        private void Bike_Assign_Load(object sender, EventArgs e)
        {
            this.riderTableAdapter.Fill(this.food_HubDataSet1.Rider);
            this.bikeTableAdapter.Fill(this.food_HubDataSet4.Bike);
            this.bike_AssignmentTableAdapter.Fill(this.food_HubDataSet5.Bike_Assignment);

            // Load orders for current rider only
            LoadRiderOrders();

            InitializeForm();
        }

        private void LoadRiderOrders()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Load orders that are assigned to current rider (Pending, Processing) 
                    // AND previously delivered orders by the currently logged-in rider
                    string query = @"SELECT Order_No, Order_Status, Order_Quantity, Cus_ID, Order_Date, 
                                           Total_Amount, Payment_Method, Food_Item_1, Food_Item_2, Food_Item_3, 
                                           Order_Time
                                           FROM [Order] 
                                           WHERE Rider_ID = @RiderID 
                                           AND (Order_Status IN ('Preparing', 'Processing') OR Order_Status = 'Delivered')
                                           ORDER BY 
                                           CASE 
                                           WHEN Order_Status IN ('Preparing', 'Processing') THEN 1
                                           WHEN Order_Status = 'Delivered' THEN 2
                                           END,
                                           Order_Date DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RiderID", UserSession.UserId);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable orderTable = new DataTable();
                        adapter.Fill(orderTable);

                        DataGridView ordersGrid = null;

                        foreach (Control control in this.Controls)
                        {
                            if (control is DataGridView dgv)
                            {
                                if (control.Name.ToLower().Contains("order") ||
                                    control.Location.X < 700) 
                                {
                                    ordersGrid = dgv;
                                    break;
                                }
                            }
                        }

                        if (ordersGrid == null)
                        {
                            ordersGrid = FindOrdersGrid(this);
                        }

                        if (ordersGrid != null)
                        {
                            ordersGrid.DataSource = orderTable;

                            if (ordersGrid.Columns.Count > 0)
                            {
                                if (ordersGrid.Columns["Order_No"] != null)
                                {
                                    ordersGrid.Columns["Order_No"].HeaderText = "Order_No";
                                    ordersGrid.Columns["Order_No"].Width = 100;
                                }
                                if (ordersGrid.Columns["Order_Status"] != null)
                                {
                                    ordersGrid.Columns["Order_Status"].HeaderText = "Order_Status";
                                    ordersGrid.Columns["Order_Status"].Width = 120;
                                }
                                if (ordersGrid.Columns["Order_Quantity"] != null)
                                {
                                    ordersGrid.Columns["Order_Quantity"].HeaderText = "Order_Quantity";
                                    ordersGrid.Columns["Order_Quantity"].Width = 120;
                                }
                                if (ordersGrid.Columns["Cus_ID"] != null)
                                {
                                    ordersGrid.Columns["Cus_ID"].HeaderText = "Cus_ID";
                                    ordersGrid.Columns["Cus_ID"].Width = 100;
                                }

                                // Hide less important columns to fit the layout
                                if (ordersGrid.Columns["Order_Date"] != null)
                                    ordersGrid.Columns["Order_Date"].Visible = false;
                                if (ordersGrid.Columns["Total_Amount"] != null)
                                    ordersGrid.Columns["Total_Amount"].Visible = false;
                                if (ordersGrid.Columns["Payment_Method"] != null)
                                    ordersGrid.Columns["Payment_Method"].Visible = false;
                                if (ordersGrid.Columns["Food_Item_1"] != null)
                                    ordersGrid.Columns["Food_Item_1"].Visible = false;
                                if (ordersGrid.Columns["Food_Item_2"] != null)
                                    ordersGrid.Columns["Food_Item_2"].Visible = false;
                                if (ordersGrid.Columns["Food_Item_3"] != null)
                                    ordersGrid.Columns["Food_Item_3"].Visible = false;
                                if (ordersGrid.Columns["Order_Time"] != null)
                                    ordersGrid.Columns["Order_Time"].Visible = false;
                            }
                        }
                        else
                        {
                            if (this.food_HubDataSet17?.Order != null)
                            {
                                // Clear and reload the dataset
                                this.food_HubDataSet17.Order.Clear();

                                // Fill with filtered data
                                foreach (DataRow row in orderTable.Rows)
                                {
                                    var newRow = this.food_HubDataSet17.Order.NewRow();
                                    newRow["Order_No"] = row["Order_No"];
                                    newRow["Order_Status"] = row["Order_Status"];
                                    newRow["Order_Quantity"] = row["Order_Quantity"];
                                    newRow["Cus_ID"] = row["Cus_ID"];
                                    newRow["Order_Date"] = row["Order_Date"];
                                    newRow["Total_Amount"] = row["Total_Amount"];
                                    this.food_HubDataSet17.Order.Rows.Add(newRow);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rider orders: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataGridView FindOrdersGrid(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is DataGridView dgv)
                {
                    if (control.Name.ToLower().Contains("order") || control.Location.X < 700)
                    {
                        return dgv;
                    }
                }
                else if (control.HasChildren)
                {
                    DataGridView result = FindOrdersGrid(control);
                    if (result != null) return result;
                }
            }
            return null;
        }

        private void InitializeForm()
        {
            // Auto-fill and lock Rider ID with current logged-in user
            Rideridcmb.Text = UserSession.UserId.ToString();
            Rideridcmb.Enabled = false; // Lock the rider ID

            LoadVehicleRegNumbers();

            datepicker.Value = DateTime.Now;
            Dispatchtimepicker.Value = DateTime.Now;

            GenerateAssignmentID();

            TotalMeterstxt.ReadOnly = true;

            meterstarttxt.TextChanged += MeterTextChanged;
            meterendtxt.TextChanged += MeterTextChanged;

            Confirmbtn.Click += Confirmbtn_Click;
        }

        // Load available vehicle registration numbers
        private void LoadVehicleRegNumbers()
        {
            try
            {
                VehRegNocmb.Items.Clear();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                   
                    string query = "SELECT Veh_Reg_No FROM Bike WHERE Veh_Reg_No IS NOT NULL ORDER BY Veh_Reg_No";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                VehRegNocmb.Items.Add(reader["Veh_Reg_No"].ToString());
                            }
                        }
                    }
                }

                // Set properties for the combo box
                VehRegNocmb.DropDownStyle = ComboBoxStyle.DropDownList;

                if (VehRegNocmb.Items.Count == 0)
                {
                    MessageBox.Show("No vehicles available in the system.",
                        "No Vehicles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicle registration numbers: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Generate new assignment ID
        private void GenerateAssignmentID()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ISNULL(MAX(Assignment_ID), 0) + 1 FROM Bike_Assignment";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        Assignmentidtxt.Text = result.ToString();
                        Assignmentidtxt.ReadOnly = true; 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating assignment ID: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MeterTextChanged(object sender, EventArgs e)
        {
            TotalMeterstxt.Text = "";

            string startText = meterstarttxt.Text.Trim();
            string endText = meterendtxt.Text.Trim();

            if (!string.IsNullOrEmpty(startText) && !string.IsNullOrEmpty(endText))
            {
                if (startText.Length == endText.Length)
                {
                    CalculateTotalMeters();
                }
            }
        }
        //Calculate the total meters
        private void CalculateTotalMeters()
        {
            try
            {
                string startText = meterstarttxt.Text.Trim();
                string endText = meterendtxt.Text.Trim();

                if (int.TryParse(startText, out int startMeter) &&
                    int.TryParse(endText, out int endMeter))
                {
                    if (endMeter > startMeter)
                    {
                        int totalMeters = endMeter - startMeter;
                        TotalMeterstxt.Text = totalMeters.ToString();
                    }
                    else if (endMeter <= startMeter && endText.Length == startText.Length)
                    {
                        MessageBox.Show("End meter reading must be greater than start meter reading.",
                            "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        meterendtxt.Focus();
                        meterendtxt.SelectAll();
                        TotalMeterstxt.Text = "";
                    }
                }
                else
                {
                    //Check number format
                    if (startText.Length == endText.Length)
                    {
                        MessageBox.Show("Please enter valid numbers for meter readings.",
                            "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        TotalMeterstxt.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating meters: {ex.Message}",
                    "Calculation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                TotalMeterstxt.Text = "";
            }
        }

        private void Confirmbtn_Click(object sender, EventArgs e)
        {
            // Validate input
            if (ValidateInput())
            {
                if (SaveBikeAssignment())
                {
                    UpdateOrderStatusToDelivered();
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Assignmentidtxt.Text))
            {
                MessageBox.Show("Assignment ID is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (VehRegNocmb.SelectedItem == null || string.IsNullOrWhiteSpace(VehRegNocmb.Text))
            {
                MessageBox.Show("Please select a vehicle registration number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                VehRegNocmb.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(meterstarttxt.Text))
            {
                MessageBox.Show("Meter start reading is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                meterstarttxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(meterendtxt.Text))
            {
                MessageBox.Show("Meter end reading is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                meterendtxt.Focus();
                return false;
            }

            if (!int.TryParse(meterstarttxt.Text, out int startMeter))
            {
                MessageBox.Show("Please enter a valid number for meter start reading.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                meterstarttxt.Focus();
                return false;
            }

            if (!int.TryParse(meterendtxt.Text, out int endMeter))
            {
                MessageBox.Show("Please enter a valid number for meter end reading.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                meterendtxt.Focus();
                return false;
            }

            if (endMeter <= startMeter)
            {
                MessageBox.Show("End meter reading must be greater than start meter reading.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                meterendtxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TotalMeterstxt.Text))
            {
                MessageBox.Show("Total meters calculation is missing. Please check your meter readings.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check if there are any orders to deliver
            if (!HasPendingOrders())
            {
                MessageBox.Show("No preparing or processing orders found for delivery.",
                    "No Orders", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        private bool HasPendingOrders()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM [Order] 
                                   WHERE Rider_ID = @RiderID 
                                   AND Order_Status IN ('Preparing', 'Processing')";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RiderID", UserSession.UserId);
                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private bool SaveBikeAssignment()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO Bike_Assignment 
                                    (Assignment_ID, Date, Dispatch_Time, Meter_Start, Meter_End, Total_Meters, Veh_Reg_No, Rider_ID) 
                                    VALUES 
                                    (@AssignmentID, @Date, @DispatchTime, @MeterStart, @MeterEnd, @TotalMeters, @VehRegNo, @RiderID)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AssignmentID", int.Parse(Assignmentidtxt.Text));
                        command.Parameters.AddWithValue("@Date", datepicker.Value.Date);
                        command.Parameters.AddWithValue("@DispatchTime", Dispatchtimepicker.Value.ToString("HH:mm:ss"));
                        command.Parameters.AddWithValue("@MeterStart", int.Parse(meterstarttxt.Text));
                        command.Parameters.AddWithValue("@MeterEnd", int.Parse(meterendtxt.Text));
                        command.Parameters.AddWithValue("@TotalMeters", int.Parse(TotalMeterstxt.Text));
                        command.Parameters.AddWithValue("@VehRegNo", VehRegNocmb.Text);
                        command.Parameters.AddWithValue("@RiderID", UserSession.UserId);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Bike assignment saved successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh the assignment details grid
                            this.bike_AssignmentTableAdapter.Fill(this.food_HubDataSet5.Bike_Assignment);

                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Failed to save bike assignment.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving bike assignment: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void UpdateOrderStatusToDelivered()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"UPDATE [Order] 
                                   SET Order_Status = 'Delivered' 
                                   WHERE Rider_ID = @RiderID 
                                   AND Order_Status IN ('Processing', 'Preparing')";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RiderID", UserSession.UserId);

                        int updatedRows = command.ExecuteNonQuery();

                        if (updatedRows > 0)
                        {
                            MessageBox.Show("order status updated to 'Delivered'.",
                                "Order Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh the order grid to show updated status
                            LoadRiderOrders();
                        }
                        else
                        {
                            MessageBox.Show("No orders found to update. All assigned orders may already be delivered.",
                                "No Orders Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }

                // Reset form for new entry after successful save and order update
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating order status: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetForm()
        {
            // Generate new assignment ID
            GenerateAssignmentID();

            VehRegNocmb.SelectedIndex = -1;
            meterstarttxt.Text = "";
            meterendtxt.Text = "";
            TotalMeterstxt.Text = "";

            datepicker.Value = DateTime.Now;
            Dispatchtimepicker.Value = DateTime.Now;
        }

        // Back button click event
        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to go back?",
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