using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Food_Hub
{
    public partial class Order_Accept : Form
    {
        private string connectionString = "Data Source=LAPTOP-UJ535S4S\\SQLEXPRESS;Initial Catalog = Food Hub; Integrated Security=True; Encrypt=True;TrustServerCertificate=True";

        public Order_Accept()
        {
            InitializeComponent();
        }

        //Form Load event
        private void Order_Accept_Load(object sender, EventArgs e)
        {
            this.orderTableAdapter1.Fill(this.food_HubDataSet27.Order);
            this.customerTableAdapter.Fill(this.food_HubDataSet9.Customer);
            LoadRiderOrders();
        }

        //Load the currently logged in rider's orders
        private void LoadRiderOrders()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Rider_ID, Order_No, Cus_ID, Order_Quantity, Order_Status FROM [Order] WHERE Rider_ID = @riderId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@riderId", UserSession.UserId);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable orderTable = new DataTable();
                    adapter.Fill(orderTable);
                    orderdetailsgrid.DataSource = orderTable;
                }
            }
        }

        //back button click event back to login page
        private void guna2GradientCircleButton1_Click(object sender, EventArgs e)
        {
            Rider riderForm = new Rider();
            riderForm.Show();
            this.Hide();
        }

        //Assign button click event to go to bike assign form
        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {
            Bike_Assign bike_Assign = new Bike_Assign();
            bike_Assign.Show();
            this.Hide();
        }
    }
}