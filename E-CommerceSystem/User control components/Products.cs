using ComponentFactory.Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace E_CommerceSystem
{
    public partial class Products : UserControl
    {
        Config dbConfig;
        MySqlConnection conn;
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLeft, int nRight, int nTop, int nBottom, int nWidth, int nHeight);

        public Products()
        {
            InitializeComponent();
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 40, 40));
        }
        public Products(string user_id, string requestName)
        {
            InitializeComponent();
            this.UserID = user_id;
            this.RequestName = requestName;
        }

        public string UserID { get; set; }

        public string RequestName { get; set; }

        private void UserControl1_Load(object sender, EventArgs e)
        {
            if (RequestName.Equals("user")) //user prompt
            {
                view_prodDetails.Visible = true;
                edit_product.Visible = false;
                delete_product.Visible = false;

            }
            else //admin prompt
            {
                view_prodDetails.Visible = false;
                edit_product.Visible = true;
                delete_product.Visible = true;
                if (_stock > 0)
                {
                    edit_product.Text = "EDIT";
                }
                else
                {
                    edit_product.Text = "RESTOCK";
                }
            }
        }

        #region Properties

        public string _productName;
        public string _price;
        public Image _icon;
        public int _stock;

        [Category("Custom Props")]
        public string ProdName
        {
            get { return _productName; }
            set { _productName = value; product_name.Text = value; }
        }

        [Category("Custom Props")]
        public string Price { get { return _price; } set { _price = value; product_price.Text = value; } }

        [Category("Custom Props")]
        public Image Photo { get { return _icon; } set { _icon = value; product_Photo.Image = value; } }

        [Category("Custom Props")]
        public int Stock { get { return _stock; } set { _stock = value; } }

        #endregion

        private void view_prodDetails_Click(object sender, EventArgs e)
        {
            new ProductDescription(getCurrentPrdID(), getCurrentUserID(), ProdName).Show();

        }

        private string getCurrentPrdID()
        {
            dbConfig = new Config();
            conn = dbConfig.getConnection();
            string currentID;

            MySqlCommand selectCurrentPrd = new MySqlCommand("SELECT * FROM products WHERE product_name = ('" + ProdName + "')", conn);

            try
            {
                conn.Open();
                MySqlDataReader fetchPrd = selectCurrentPrd.ExecuteReader();
                fetchPrd.Read();
                currentID = fetchPrd.GetString("productID");

                return currentID;
            }
            catch (Exception) { }

            return null;
        }

        private string getCurrentUserID()
        {
            dbConfig = new Config();
            conn = dbConfig.getConnection();

            MySqlCommand fetchUserData = new MySqlCommand("SELECT userID FROM users WHERE userID = ('" + UserID + "')", conn);

            try
            {
                conn.Open();
                MySqlDataReader fetchData = fetchUserData.ExecuteReader();
                fetchData.Read();

                return fetchData.GetString("userID");
            }
            catch (Exception)
            {        }
            return null;
        }

        private void edit_product_Click(object sender, EventArgs e)
        {
            dbConfig = new Config();
            conn = dbConfig.getConnection();

            MySqlCommand selectProdID = new MySqlCommand("SELECT * FROM products WHERE product_name = ('" + ProdName + "')", conn);

            try
            {
                conn.Open();
                MySqlDataReader fetchProdId = selectProdID.ExecuteReader();
                fetchProdId.Read();

                string productID = fetchProdId.GetString("productID");

                AdminPrompt.showProdPanel = true;


                new AddProduct(productID).Show();
                fetchProdId.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
                
        }


        bool checkProductsInOrders(int productID)
        {
            try
            {
                dbConfig = new Config();
                conn = dbConfig.getConnection();
                MySqlCommand checkOrdersExists = new MySqlCommand("SELECT * FROM orders WHERE productID = (" + productID + ")", conn);
                conn.Open();
                MySqlDataReader checkOrders = checkOrdersExists.ExecuteReader();

                if (checkOrders.HasRows)
                {
                    checkOrders.Close();

                    return false;
                }
                else
                {
                    checkOrders.Close();

                    return true;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return true;
        }

        private void delete_product_Click_1(object sender, EventArgs e)
        {
            dbConfig = new Config();
            conn = dbConfig.getConnection();

            AdminPrompt.showProdPanel = true;

            try
            {
                conn.Open();
                MySqlCommand fetchProducts = new MySqlCommand("SELECT productID FROM products WHERE product_name = ('" + ProdName + "')", conn);
                MySqlDataReader fetchID = fetchProducts.ExecuteReader();
                fetchID.Read();
                int productID = fetchID.GetInt32("productID");
                if (checkProductsInOrders(productID))
                {
                    fetchID.Close();

                    MySqlCommand deleteProductinStats = new MySqlCommand("DELETE FROM product_sales_statistics WHERE productID = ('" + productID + "')", conn);
                    deleteProductinStats.ExecuteNonQuery();

                    MySqlCommand deleteProductinChart = new MySqlCommand("DELETE FROM cart WHERE productName = ('" + ProdName + "')", conn);
                    deleteProductinChart.ExecuteNonQuery();

                    MySqlCommand deleteProduct = new MySqlCommand("DELETE FROM products WHERE product_name = ('" + ProdName + "')", conn);
                    deleteProduct.ExecuteNonQuery();

                    MessageBox.Show("Product Deleted Successfully!");
                    
                }
                else
                {
                    fetchID.Close();

                    MessageBox.Show("There's currently order for this product. Cannot be deleted!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
   }

