using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ComponentFactory.Krypton.Toolkit;
using Guna.Charts.Interfaces;
using Guna.Charts.WinForms;
using MySql.Data.MySqlClient;

namespace E_CommerceSystem
{
    public partial class AdminPrompt : KryptonForm
    {
        Config dbConfig;
        MySqlConnection conn;
        Products product;

        public static Boolean showProdPanel { get; set; } // Gian888
        public static Boolean showOrdPanel { get; set; } // Gian888

        GunaChart salesAnalyticsChart;
        GunaChart topSoldProduct;
        public AdminPrompt()
        {
            InitializeComponent();
        }

        public AdminPrompt(string user_id)
        {
            InitializeComponent();
            this.UserID = user_id;
        }

        public string UserID { get; set; }

        private void AdminPrompt_Load(object sender, EventArgs e)
        {

        }


        private void fetchProductsByCateg()
        {
            product_FlowLayout.Controls.Clear();
            dbConfig = new Config();
            conn = dbConfig.getConnection();


            MySqlCommand fetchAll = new MySqlCommand("SELECT image, product_name, price FROM products WHERE category = ('" + lblRef.Text + "')", conn);
            try
            {
                conn.Open();
                MySqlDataReader fetchAllPrd = fetchAll.ExecuteReader();
                if (!fetchAllPrd.Read())
                {
                    fetchAllPrd.Close();
                    fetchAllItems();
                }
                else
                {
                    fetchAllPrd.Close();
                    MySqlCommand fetchByCateg = new MySqlCommand("SELECT image, product_name, price FROM products WHERE category = ('" + lblRef.Text + "')", conn);
                    MySqlDataReader fetchCategResult = fetchByCateg.ExecuteReader();
                    while (fetchCategResult.Read())
                    {
                        long len = fetchCategResult.GetBytes(0, 0, null, 0, 0);
                        byte[] array = new byte[System.Convert.ToInt32(len + 1)];
                        fetchCategResult.GetBytes(0, 0, array, 0, System.Convert.ToInt32(len));

                        MemoryStream ms = new MemoryStream(array);
                        Bitmap bitmpap = new Bitmap(ms);
                        product = new Products(UserID, "admin");

                        product.ProdName = fetchCategResult.GetString("product_name");
                        product.Price = $"₱{fetchCategResult.GetString("price")}";
                        product.Photo = bitmpap;
                        product_FlowLayout.Controls.Add(product);
                    }

                    fetchCategResult.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void fetchAllItems()
        {
            product_FlowLayout.Controls.Clear();
            dbConfig = new Config();
            conn = dbConfig.getConnection();

            MySqlCommand fetchByCateg = new MySqlCommand("SELECT image, product_name, price, product_stock FROM products WHERE product_name  LIKE '" + searchField.Text + "%' ORDER BY product_name", conn);
            try
            {
                conn.Open();
                MySqlDataReader fetchCategResult = fetchByCateg.ExecuteReader();
                while (fetchCategResult.Read())
                {

                    long len = fetchCategResult.GetBytes(0, 0, null, 0, 0);
                    byte[] array = new byte[System.Convert.ToInt32(len + 1)];
                    fetchCategResult.GetBytes(0, 0, array, 0, System.Convert.ToInt32(len));

                    MemoryStream ms = new MemoryStream(array);
                    Bitmap bitmpap = new Bitmap(ms);
                    product = new Products(UserID, "admin");

                    product.ProdName = fetchCategResult.GetString("product_name");
                    product._stock = fetchCategResult.GetInt32("product_stock");
                    product.Price = $"₱{fetchCategResult.GetString("price")}";
                    product.Photo = bitmpap;

                    product_FlowLayout.Controls.Add(product);
                }
                /*   conn.Close();*/
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void admin_dashboard_Click(object sender, EventArgs e)
        {
            dbConfig = new Config();
            conn = dbConfig.getConnection();
            lblRef.Visible = false;

            //sales chart

            this.salesAnalyticsChart = salaes_chart;
            admin_dashboard_panel.Controls.Add(this.salesAnalyticsChart);
            this.topSoldProduct = new GunaChart();
            //selling chart

            topSoldProduct.Legend.Display = false;
            topSoldProduct.Title.Display = false;

            admin_dashboard_panel.Visible = true;
            admin_products_panel.Visible = false;
            admin_orders_panel.Visible = false;

            admin_products.Enabled = true;
            admin_orders.Enabled = true;
            admin_dashboard.Enabled = false;
            admin_header.Text = "DASHBOARD";

           
            best_selling_panel.Controls.Add(this.topSoldProduct);
            topSoldProduct.Width = best_selling_panel.Width;
            topSoldProduct.Height = best_selling_panel.Height;
            topSoldProduct.Animation.Easing = Easing.EaseOutQuart;
            topSoldProduct.Animation.Duration = 800;


            MySqlCommand selectBaseStats = new MySqlCommand("SELECT * FROM sales_statistics", conn);

            int numberOfOrders = 0, totalRevenue = 0;
            double saleAverage = 0.0;
            try
            {
                conn.Open();
                MySqlDataReader fetchAllStats = selectBaseStats.ExecuteReader();

                while (fetchAllStats.Read())
                {
                    numberOfOrders += fetchAllStats.GetInt32("total_orders");
                    totalRevenue += fetchAllStats.GetInt32("total_revenue");
                }

                if (totalRevenue > 0 && numberOfOrders > 0)
                {
                    saleAverage = totalRevenue / numberOfOrders;

                }

                lbl_revenue.Text = $"P{totalRevenue}";
                lbl_no_orders.Text = $"{numberOfOrders}";
                lbl_average_orders.Text = $"P{saleAverage}";

                fetchAllStats.Close();

                MySqlCommand selectMontlhyAnalytics = new MySqlCommand("SELECT * FROM sales_statistics", conn);
                MySqlDataReader fetchMontlyAnalytics = selectMontlhyAnalytics.ExecuteReader();

                salesAnalyticsChart.YAxes.GridLines.Display = false;
                GunaLineDataset dataset = new GunaLineDataset();


                while (fetchMontlyAnalytics.Read())
                {
                    dataset.DataPoints.Add(fetchMontlyAnalytics.GetString("Month"), fetchMontlyAnalytics.GetInt32("total_revenue"));
                }

                salesAnalyticsChart.Datasets.Add(dataset);
                salesAnalyticsChart.Update();

                fetchMontlyAnalytics.Close();

                MySqlCommand selectTop3BestSold = new MySqlCommand("SELECT number_of_orders, productName FROM product_sales_statistics ORDER BY number_of_orders DESC LIMIT 2", conn);
                MySqlDataReader fetchTop3 = selectTop3BestSold.ExecuteReader();

                GunaBarDataset prod_dataset = new GunaBarDataset();

                while (fetchTop3.Read())
                {
                    prod_dataset.DataPoints.Add(fetchTop3.GetString("productName"), fetchTop3.GetInt32("number_of_orders"));
                }

                topSoldProduct.Datasets.Add(prod_dataset);
                topSoldProduct.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void admin_dashboard_panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void logoutBtn_Click(object sender, EventArgs e)
        {
            this.Close();
            new ProductDescription().Close();
            new Login().Show();

        }

        private void searchField_TextChanged(object sender, EventArgs e)
        {
            fetchAllItems();
        }

        private void admin_products_Click(object sender, EventArgs e)
        {
            category_cmb.Items.Clear();

            string[] perfumeVariant = { "All", "Men Perfume ", "Women Perfume", "Unisex Perfume" };

            foreach (string variant in perfumeVariant)
            {

                category_cmb.Items.Add(variant);
            }

            category_cmb.SelectedItem = "All";

            dbConfig = new Config();
            conn = dbConfig.getConnection();

            showProductPanel(); // Gian888

            MySqlCommand getUserImage = new MySqlCommand("SELECT picture_directory FROM users WHERE userID = ('" + UserID + "')", conn);
            try
            {
                conn.Open();
                MySqlDataReader fetchUserImage = getUserImage.ExecuteReader();
                fetchUserImage.Read();

                if (fetchUserImage.HasRows)
                {
                    //display the image based on the fetched id
                    long len = fetchUserImage.GetBytes(0, 0, null, 0, 0);
                    byte[] array = new byte[System.Convert.ToInt32(len + 1)];
                    fetchUserImage.GetBytes(0, 0, array, 0, System.Convert.ToInt32(len));

                    MemoryStream ms = new MemoryStream(array);
                    Bitmap bitmpap = new Bitmap(ms);
                    user_pic.Image = bitmpap;

                    fetchProductsByCateg();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void searchAllOrders()
        {
            orders_flowlayout.Controls.Clear();
            dbConfig = new Config();
            conn = dbConfig.getConnection();

            showOrderPanel();

            MySqlCommand selectOrderedProducts = new MySqlCommand("SELECT Region, Province, City, Barangay, StreetName FROM orders WHERE orderStatus = 'PREPARING' OR orderStatus = 'SHIP' OR orderStatus = 'RECEIVE' LIMIT 1", conn);
            MySqlCommand selectFullName = new MySqlCommand("SELECT user_address.FullName, user_address.Email FROM user_address INNER JOIN orders ON user_address.userID = orders.userID WHERE FullName LIKE ('" + orders_searchField.Text + "%')", conn);

            try
            {
                conn.Open();
                MySqlDataReader fetchFullname = selectFullName.ExecuteReader();
                fetchFullname.Read();

                if (fetchFullname.HasRows)
                {
                    string fullName = fetchFullname.GetString("FullName");
                    string email = fetchFullname.GetString("Email");

                    fetchFullname.Close();

                    // --------------------------------------------------------------------------- //

                    MySqlDataReader fetchOrderedProducts = selectOrderedProducts.ExecuteReader();
                    while (fetchOrderedProducts.Read())
                    {
                        Orders orderLists = new Orders();

                        orderLists.FullName = fullName;
                        orderLists.Emails = email;
                        orderLists.Regions = fetchOrderedProducts.GetString("Region");
                        orderLists.Province = fetchOrderedProducts.GetString("Province");
                        orderLists.Barangay = fetchOrderedProducts.GetString("Barangay");
                        orderLists.City = fetchOrderedProducts.GetString("City");
                        orderLists.Street = fetchOrderedProducts.GetString("StreetName");

                        orders_flowlayout.Controls.Add(orderLists);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void admin_orders_Click(object sender, EventArgs e)
        {
            orders_flowlayout.Controls.Clear();
            dbConfig = new Config();
            conn = dbConfig.getConnection();

            showOrderPanel();

            MySqlCommand selectOrderedProducts = new MySqlCommand("SELECT Region, Province, City, Barangay, StreetName FROM orders WHERE orderStatus = 'PREPARING' OR orderStatus = 'SHIP' OR orderStatus = 'RECEIVE' LIMIT 1", conn);
            MySqlCommand selectFullName = new MySqlCommand("SELECT user_address.FullName, user_address.Email FROM user_address INNER JOIN orders ON user_address.userID = orders.userID", conn);

            try
            {
                conn.Open();
                MySqlDataReader fetchFullname = selectFullName.ExecuteReader();
                fetchFullname.Read();

                if (!fetchFullname.Read())
                {
                    fetchFullname.Close();
                    searchAllOrders();
                }
                else
                {
                    if (fetchFullname.HasRows)
                    {
                        string fullName = fetchFullname.GetString("FullName");
                        string email = fetchFullname.GetString("Email");

                        fetchFullname.Close();

                        // --------------------------------------------------------------------------- //

                        MySqlDataReader fetchOrderedProducts = selectOrderedProducts.ExecuteReader();
                        while (fetchOrderedProducts.Read())
                        {
                            Orders orderLists = new Orders();

                            orderLists.FullName = fullName;
                            orderLists.Emails = email;
                            orderLists.Regions = fetchOrderedProducts.GetString("Region");
                            orderLists.Province = fetchOrderedProducts.GetString("Province");
                            orderLists.Barangay = fetchOrderedProducts.GetString("Barangay");
                            orderLists.City = fetchOrderedProducts.GetString("City");
                            orderLists.Street = fetchOrderedProducts.GetString("StreetName");

                            orders_flowlayout.Controls.Add(orderLists);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void admin_pic_Click(object sender, EventArgs e)
        {
            if (admin_picPanel.Visible == false)
            {
                admin_pic.BackColor = SystemColors.Window;
                admin_user_pic.Visible = true;
                admin_picPanel.Visible = true;
            }

            else
            {
                admin_user_pic.Visible = false;
                admin_picPanel.Visible = false;
            }
        }

        private void adminProfile_Click(object sender, EventArgs e)
        {
            this.Close();
            new Profile(UserID).Show();
        }

        private void notification_Click(object sender, EventArgs e)
        {

        }

        private void add_product_Click(object sender, EventArgs e)
        {
            new AddProduct().Show();
            AdminPrompt.showProdPanel = true; // Gian888
        }

        private void AdminPrompt_Activated(object sender, EventArgs e)
        {
            admin_dashboard_Click(sender, e);

            if (showProdPanel) // Gian888
            {
                showProductPanel();
                showProdPanel = false;
            }

            else if (showOrdPanel) // Gian888
            {
                showOrderPanel();
                showOrdPanel = false;
            }
        }
        private void showProductPanel() // Gian888
        {
            admin_products_panel.Visible = true;
            admin_dashboard_panel.Visible = false;
            admin_orders_panel.Visible = false;
            admin_dashboard.Enabled = true;
            admin_orders.Enabled = true;
            admin_products.Enabled = false;
            admin_header.Text = "PRODUCTS";
        }
        private void showOrderPanel() // Gian888
        {
            admin_orders_panel.Visible = true;
            admin_products_panel.Visible = false;
            admin_dashboard_panel.Visible = false;

            admin_dashboard.Enabled = true;
            admin_orders.Enabled = false;
            admin_products.Enabled = true;
            admin_header.Text = "ORDERS";
        }

        private void category_cmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblRef.Text = category_cmb.Text;
            fetchProductsByCateg();
        }

        private void orders_searchField_TextChanged(object sender, EventArgs e)
        {
            searchAllOrders();
        }

        private void admin_products_panel_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}
