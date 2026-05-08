using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Order : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";
        private int loggedInStaffId;
        private decimal foodItem1Price = 0;
        private decimal foodItem2Price = 0;
        private decimal foodItem3Price = 0;

        public Order(int staffId = 1)
        {
            InitializeComponent();
            loggedInStaffId = staffId;
            InitializeForm();
        }

        //Order Form Load event
        private void Order_Load(object sender, EventArgs e)
        {
            this.foodItemTableAdapter2.Fill(this.food_HubDataSet28.FoodItem);
            try
            {
                this.customerTableAdapter.Fill(this.food_HubDataSet6.Customer);
                this.ingredientsTableAdapter.Fill(this.food_HubDataSet8.Ingredients);
                this.riderTableAdapter.Fill(this.food_HubDataSet1.Rider);
                this.orderTableAdapter2.Fill(this.food_HubDataSet13.Order);

                LoadFormData();
                SetupEventHandlers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeForm()
        {
            // Set Staff ID and lock it
            StaffIdtxt.Text = loggedInStaffId.ToString();
            StaffIdtxt.ReadOnly = true;
            StaffIdtxt.BackColor = Color.LightGray;

            // Set current date and time
            OrdatePicker.Value = DateTime.Now;
            Ordertimepicker.Value = DateTime.Now;

            totalamountbtn.ReadOnly = true;
            totalamountbtn.BackColor = Color.LightGray;

            // Set payment method items
            payementcmb.Items.Clear();
            payementcmb.Items.AddRange(new string[] { "Cash", "Card" });

            // Set order status items
            Orstatuscmb.Items.Clear();
            Orstatuscmb.Items.AddRange(new string[] { "Processing", "Preparing" });
            Orstatuscmb.SelectedIndex = 0;
        }

        private void LoadFormData()
        {
            LoadCustomerIDs();
            LoadFoodItems();
            LoadRiderIDs();
            LoadOrderHistory();
        }

        private void LoadCustomerIDs()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Cus_ID FROM Customer ORDER BY Cus_ID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        CusIDcmb.Items.Clear();
                        while (reader.Read())
                        {
                            CusIDcmb.Items.Add(reader["Cus_ID"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customer IDs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFoodItems()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Item_Name FROM FoodItem ORDER BY Item_Name";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Clear all food item combo boxes
                        fooditem1btn.Items.Clear();
                        fooditembtn2.Items.Clear();
                        fooditem3btn.Items.Clear();

                        while (reader.Read())
                        {
                            string itemName = reader["Item_Name"].ToString();
                            fooditem1btn.Items.Add(itemName);
                            fooditembtn2.Items.Add(itemName);
                            fooditem3btn.Items.Add(itemName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading food items: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRiderIDs()
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
                        riderIDcmb.Items.Clear();
                        while (reader.Read())
                        {
                            riderIDcmb.Items.Add(reader["Rider_ID"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rider IDs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrderHistory()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Order_No, Cus_ID, Rider_ID FROM [Order] ORDER BY Order_No DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        ordergrid.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupEventHandlers()
        {
            // Add event handlers for buttons and controls
            calculatebtn.Click += calculatebtn_Click;
            clearbtn.Click += clearbtn_Click;
            Sendtoriderbtn.Click += Sendtoriderbtn_Click;

            // Add event handlers for food item selection changes
            fooditem1btn.SelectedIndexChanged += FoodItem_SelectedIndexChanged;
            fooditembtn2.SelectedIndexChanged += FoodItem_SelectedIndexChanged;
            fooditem3btn.SelectedIndexChanged += FoodItem_SelectedIndexChanged;
        }

        private void FoodItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reset total when food items change
            totalamountbtn.Text = "";
        }

        private decimal GetFoodItemPriceFromDatabase(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                return 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Price FROM FoodItem WHERE Item_Name = @itemName";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@itemName", itemName);
                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting price for {itemName}: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return 0;
        }

        private void calculatebtn_Click(object sender, EventArgs e)
        {
            try
            {
                decimal totalAmount = 0;

                // Calculate price for each selected food item from database
                if (fooditem1btn.SelectedItem != null)
                {
                    foodItem1Price = GetFoodItemPriceFromDatabase(fooditem1btn.SelectedItem.ToString());
                    totalAmount += foodItem1Price;
                }

                if (fooditembtn2.SelectedItem != null)
                {
                    foodItem2Price = GetFoodItemPriceFromDatabase(fooditembtn2.SelectedItem.ToString());
                    totalAmount += foodItem2Price;
                }

                if (fooditem3btn.SelectedItem != null)
                {
                    foodItem3Price = GetFoodItemPriceFromDatabase(fooditem3btn.SelectedItem.ToString());
                    totalAmount += foodItem3Price;
                }

                if (totalAmount == 0)
                {
                    MessageBox.Show("Please select at least one food item.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Display total amount
                totalamountbtn.Text = $"Rs. {totalAmount:F2}";

                MessageBox.Show($"Total calculated: Rs. {totalAmount:F2}", "Calculation Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating total: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Sendtoriderbtn_Click(object sender, EventArgs e)
        {
            if (!ValidateOrderInput())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if Order No already exists
                    if (!string.IsNullOrEmpty(orderNotxt.Text) && int.TryParse(orderNotxt.Text, out int checkOrderNo))
                    {
                        string checkQuery = "SELECT COUNT(*) FROM [Order] WHERE Order_No = @orderNo";
                        using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@orderNo", checkOrderNo);
                            int count = (int)checkCommand.ExecuteScalar();
                            if (count > 0)
                            {
                                MessageBox.Show("Order Number already exists. Please use a different order number.",
                                    "Duplicate Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    // Insert order into database
                    string insertQuery = @"INSERT INTO [Order] (Order_No, Order_Date, Order_Time, Payment_Method, 
                                         Order_Status, Food_Item_1, Food_Item_2, Food_Item_3, Order_Quantity, 
                                         Total_Amount, Staff_ID, Cus_ID, Rider_ID) 
                                         VALUES (@orderNo, @orderDate, @orderTime, @paymentMethod, @orderStatus, 
                                         @foodItem1, @foodItem2, @foodItem3, @quantity, @totalAmount, 
                                         @staffId, @cusId, @riderId)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@orderNo", int.Parse(orderNotxt.Text));
                        command.Parameters.AddWithValue("@orderDate", OrdatePicker.Value.Date);
                        command.Parameters.AddWithValue("@orderTime", Ordertimepicker.Value.ToString("HH:mm:ss"));
                        command.Parameters.AddWithValue("@paymentMethod", payementcmb.SelectedItem?.ToString() ?? "");
                        command.Parameters.AddWithValue("@orderStatus", Orstatuscmb.SelectedItem?.ToString() ?? "Pending");

                        command.Parameters.AddWithValue("@foodItem1",
                        fooditem1btn.SelectedItem?.ToString() ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@foodItem2",
                        fooditembtn2.SelectedItem?.ToString() ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@foodItem3",
                        fooditem3btn.SelectedItem?.ToString() ?? (object)DBNull.Value);

                        command.Parameters.AddWithValue("@quantity", int.Parse(orderquantitybtn.Text));

                        // Extract numeric value from total amount
                        string totalText = totalamountbtn.Text.Replace("Rs. ", "");
                        command.Parameters.AddWithValue("@totalAmount", decimal.Parse(totalText));

                        command.Parameters.AddWithValue("@staffId", loggedInStaffId);
                        command.Parameters.AddWithValue("@cusId", int.Parse(CusIDcmb.SelectedItem.ToString()));
                        command.Parameters.AddWithValue("@riderId",
                            riderIDcmb.SelectedItem != null ? int.Parse(riderIDcmb.SelectedItem.ToString()) : (object)DBNull.Value);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Order sent to rider successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh order history grid
                            LoadOrderHistory();

                            // Clear form but keep staff ID
                            ClearOrderForm();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateOrderInput()
        {
            if (string.IsNullOrWhiteSpace(orderNotxt.Text))
            {
                MessageBox.Show("Order Number is required.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                orderNotxt.Focus();
                return false;
            }

            if (!int.TryParse(orderNotxt.Text, out int orderNo) || orderNo <= 0)
            {
                MessageBox.Show("Order Number must be a positive number.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                orderNotxt.Focus();
                return false;
            }

            if (CusIDcmb.SelectedItem == null)
            {
                MessageBox.Show("Please select a Customer ID.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CusIDcmb.Focus();
                return false;
            }

            if (payementcmb.SelectedItem == null)
            {
                MessageBox.Show("Please select a payment method.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                payementcmb.Focus();
                return false;
            }

            // Validate order quantity
            if (string.IsNullOrWhiteSpace(orderquantitybtn.Text) ||
                !int.TryParse(orderquantitybtn.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid order quantity.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                orderquantitybtn.Focus();
                return false;
            }

            // Check maximum quantity is 3
            if (quantity > 3)
            {
                MessageBox.Show("Maximum order quantity is 3.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                orderquantitybtn.Focus();
                return false;
            }

            // Count selected food items
            int selectedItemsCount = 0;
            if (fooditem1btn.SelectedItem != null) selectedItemsCount++;
            if (fooditembtn2.SelectedItem != null) selectedItemsCount++;
            if (fooditem3btn.SelectedItem != null) selectedItemsCount++;

            if (selectedItemsCount == 0)
            {
                MessageBox.Show("Please select at least one food item.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check if order quantity matches the number of selected items
            if (quantity != selectedItemsCount)
            {
                MessageBox.Show($"Order quantity ({quantity}) must match the number of selected food items ({selectedItemsCount}).",
                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                orderquantitybtn.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(totalamountbtn.Text))
            {
                MessageBox.Show("Please calculate the total amount first.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                calculatebtn.Focus();
                return false;
            }

            return true;
        }

        private void clearbtn_Click(object sender, EventArgs e)
        {
            ClearOrderForm();
        }

        private void ClearOrderForm()
        {
            // Clear all fields except Staff ID
            orderNotxt.Clear();
            CusIDcmb.SelectedIndex = -1;
            OrdatePicker.Value = DateTime.Now;
            Ordertimepicker.Value = DateTime.Now;
            payementcmb.SelectedIndex = -1;
            Orstatuscmb.SelectedIndex = 0; // Reset to "Pending"

            fooditem1btn.SelectedIndex = -1;
            fooditembtn2.SelectedIndex = -1;
            fooditem3btn.SelectedIndex = -1;

            orderquantitybtn.Clear();
            totalamountbtn.Clear();
            riderIDcmb.SelectedIndex = -1;

            // Reset prices
            foodItem1Price = 0;
            foodItem2Price = 0;
            foodItem3Price = 0;

            // Keep Staff ID locked
            StaffIdtxt.Text = loggedInStaffId.ToString();
            StaffIdtxt.ReadOnly = true;
            StaffIdtxt.BackColor = Color.LightGray;
        }

        // Back button - return to Staff dashboard
        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to go back to the Staff dashboard?",
            "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Customer_Details customer_Details = new Customer_Details();
                customer_Details.Show();
                this.Hide();
            }
        }

    }
}