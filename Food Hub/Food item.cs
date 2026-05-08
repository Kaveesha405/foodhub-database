using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Food_item : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";

        public Food_item()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void Food_item_Load(object sender, EventArgs e)
        {
            try
            {
                LoadFoodItems();
                LoadIngredients();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //load food items into datagridview
        private void LoadFoodItems()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM FoodItem ORDER BY Item_No";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading food items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //load ingredients into datagridview
        private void LoadIngredients()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Ingredients ORDER BY Ingredient_ID";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataGridView2 != null)
                    {
                        dataGridView2.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ingredients: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //load categories into combobox
        private void LoadCategories()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    categorycmb.Items.Clear();
                    categorycmb.Items.Add("Burger");
                    categorycmb.Items.Add("Sides");
                    categorycmb.Items.Add("Pizza");
                    categorycmb.Items.Add("Chicken");
                    categorycmb.Items.Add("Seafood");
                    categorycmb.Items.Add("Wrap");
                    categorycmb.Items.Add("Mexican");

                    string query = "SELECT DISTINCT Item_Category FROM FoodItem WHERE Item_Category IS NOT NULL AND Item_Category != ''";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string itemCategory = reader["Item_Category"].ToString();
                                if (!categorycmb.Items.Contains(itemCategory))
                                {
                                    categorycmb.Items.Add(itemCategory);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshDataGrids()
        {
            LoadFoodItems();
            LoadIngredients();
        }

        //Food Item Search Button click event
        private void Itemsearchbtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNOtxt.Text))
            {
                MessageBox.Show("Please enter an Item Number to search.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(ItemNOtxt.Text, out int itemNo))
            {
                MessageBox.Show("Please enter a valid Item Number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM FoodItem WHERE Item_No = @itemNo";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@itemNo", itemNo);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Itemnametxt.Text = reader["Item_Name"].ToString();
                                categorycmb.Text = reader["Item_Category"].ToString();
                                Pricetxt.Text = reader["Price"].ToString();
                                ingredintidstxt.Text = reader["Ingredients"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Item not found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearItemFields();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Ingredient Search Button click event
        private void Ingredientsearchbutton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Ingredientidtxt.Text))
            {
                MessageBox.Show("Please enter an Ingredient ID to search.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(Ingredientidtxt.Text, out int ingredientId))
            {
                MessageBox.Show("Please enter a valid Ingredient ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Ingredients WHERE Ingredient_ID = @ingredientId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ingredientId", ingredientId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ingredientnametxt.Text = reader["Ing_Name"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Ingredient not found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearIngredientFields();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching ingredient: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Item add button click event
        private void itemaddbtn_Click(object sender, EventArgs e)
        {
            if (!ValidateItemFields())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO FoodItem (Item_Name, Item_Category, Price, Ingredients) VALUES (@itemName, @itemCategory, @price, @ingredients)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@itemName", Itemnametxt.Text.Trim());
                        command.Parameters.AddWithValue("@itemCategory", categorycmb.Text.Trim());
                        command.Parameters.AddWithValue("@price", decimal.Parse(Pricetxt.Text));
                        command.Parameters.AddWithValue("@ingredients", ingredintidstxt.Text.Trim());

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Item added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearItemFields();
                            RefreshDataGrids();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //item update button click event
        private void itemupdatebtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNOtxt.Text))
            {
                MessageBox.Show("Please enter an Item Number to update.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(ItemNOtxt.Text, out int itemNo))
            {
                MessageBox.Show("Please enter a valid Item Number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateItemFields())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE FoodItem SET Item_Name = @itemName, Item_Category = @itemCategory, Price = @price, Ingredients = @ingredients WHERE Item_No = @itemNo";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@itemNo", itemNo);
                        command.Parameters.AddWithValue("@itemName", Itemnametxt.Text.Trim());
                        command.Parameters.AddWithValue("@itemCategory", categorycmb.Text.Trim());
                        command.Parameters.AddWithValue("@price", decimal.Parse(Pricetxt.Text));
                        command.Parameters.AddWithValue("@ingredients", ingredintidstxt.Text.Trim());

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Item updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshDataGrids();
                        }
                        else
                        {
                            MessageBox.Show("Item not found or no changes made.", "Update Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //FoodItem delete button click event
        private void itemdeletebtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNOtxt.Text))
            {
                MessageBox.Show("Please enter an Item Number to delete.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(ItemNOtxt.Text, out int itemNo))
            {
                MessageBox.Show("Please enter a valid Item Number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM FoodItem WHERE Item_No = @itemNo";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@itemNo", itemNo);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Item deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearItemFields();
                                RefreshDataGrids();
                            }
                            else
                            {
                                MessageBox.Show("Item not found.", "Delete Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //Food Item clear button click event
        private void itemclearbutton_Click(object sender, EventArgs e)
        {
            ClearItemFields();
        }


        //Ingredient add button click event
        private void ingredientaddbtn_Click(object sender, EventArgs e)
        {
            if (!ValidateIngredientFields())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Ingredients (Ing_Name) VALUES (@ingName)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ingName", ingredientnametxt.Text.Trim());

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Ingredient added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearIngredientFields();
                            RefreshDataGrids();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding ingredient: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Ingredient update button click event
        private void ingredientupdatebtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Ingredientidtxt.Text))
            {
                MessageBox.Show("Please enter an Ingredient ID to update.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(Ingredientidtxt.Text, out int ingredientId))
            {
                MessageBox.Show("Please enter a valid Ingredient ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateIngredientFields())
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Ingredients SET Ing_Name = @ingName WHERE Ingredient_ID = @ingredientId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ingredientId", ingredientId);
                        command.Parameters.AddWithValue("@ingName", ingredientnametxt.Text.Trim());

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Ingredient updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshDataGrids();
                        }
                        else
                        {
                            MessageBox.Show("Ingredient not found or no changes made.", "Update Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating ingredient: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //ingredient delete button click event
        private void ingredientdeletebtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Ingredientidtxt.Text))
            {
                MessageBox.Show("Please enter an Ingredient ID to delete.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(Ingredientidtxt.Text, out int ingredientId))
            {
                MessageBox.Show("Please enter a valid Ingredient ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this ingredient?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Ingredients WHERE Ingredient_ID = @ingredientId";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ingredientId", ingredientId);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Ingredient deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearIngredientFields();
                                RefreshDataGrids();
                            }
                            else
                            {
                                MessageBox.Show("Ingredient not found.", "Delete Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting ingredient: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //Ingredient clear button click event
        private void ingredientclearbtn_Click(object sender, EventArgs e)
        {
            ClearIngredientFields();
        }

        //Validate FoodItem input fields
        private bool ValidateItemFields()
        {
            if (string.IsNullOrWhiteSpace(Itemnametxt.Text))
            {
                MessageBox.Show("Please enter an item name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Itemnametxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(categorycmb.Text))
            {
                MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                categorycmb.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Pricetxt.Text))
            {
                MessageBox.Show("Please enter a price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Pricetxt.Focus();
                return false;
            }

            if (!decimal.TryParse(Pricetxt.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Pricetxt.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ingredintidstxt.Text))
            {
                MessageBox.Show("Please enter ingredient IDs.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ingredintidstxt.Focus();
                return false;
            }

            return true;
        }

        //Validate Ingredient input fields

        private bool ValidateIngredientFields()
        {
            if (string.IsNullOrWhiteSpace(ingredientnametxt.Text))
            {
                MessageBox.Show("Please enter an ingredient name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ingredientnametxt.Focus();
                return false;
            }

            return true;
        }


        //Clear FoodItem input fields methods
        private void ClearItemFields()
        {
            ItemNOtxt.Clear();
            Itemnametxt.Clear();
            categorycmb.Text = "";
            Pricetxt.Clear();
            ingredintidstxt.Clear();
        }

        //Clear Ingredient input fields methods
        private void ClearIngredientFields()
        {
            Ingredientidtxt.Clear();
            ingredientnametxt.Clear();
        }

        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to go back to the staff dashboard?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Staff staffForm = new Staff();
                staffForm.Show();
                this.Hide();
            }
        }
    }
}