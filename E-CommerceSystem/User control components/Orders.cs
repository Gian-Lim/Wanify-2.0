using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace E_CommerceSystem
{
    public partial class Orders : UserControl
    {
        Config dbConfig;
        MySqlConnection conn;

        public Orders()
        {
            InitializeComponent();
        }

        private void OrdersControlcs_Load(object sender, EventArgs e)
        {

        }

        #region Properties

        public string _fullNAme;
        public string _email;
        public string _region;
        public string _province;
        public string _city;
        public string _barangay;
        public string _postal;
        public string _street;

        [Category("Custom Props")]
        public string FullName
        {
            get { return _fullNAme; }
            set { _fullNAme = value; fullName.Text = value; }
        }

        [Category("Custom Props")]
        public string Emails { get { return _email; } set { _email = value; emailAddress.Text = value; } }

        [Category("Custom Props")]
        public string Regions { get { return _region; } set { _region = value; region_label.Text = value; } }

        [Category("Custom Props")]
        public string Province { get { return _province; } set { _province = value; province_label.Text = value; } }

        [Category("Custom Props")]
        public string City { get { return _city; } set { _city = value; city_label.Text = value; } }

        [Category("Custom Props")]
        public string Barangay { get { return _barangay; } set { _barangay = value; barangay_label.Text = value; } }

        [Category("Custom Props")]
        public string Postal { get { return _postal; } set { _postal = value; postal_label.Text = value; } }

        [Category("Custom Props")]
        public string Street { get { return _street; } set { _street = value; street_name.Text = value; } }

        #endregion

        private void open_order_Click(object sender, EventArgs e)
        {
            AdminPrompt.showOrdPanel = true; // Gian888

            dbConfig = new Config();
            conn = dbConfig.getConnection();
            MySqlCommand selectUserIDBasedOnEmail = new MySqlCommand("SELECT * FROM user_address WHERE Email = ('" + Emails + "')", conn);
            
            try
            {
                conn.Open();
                MySqlDataReader fetchID = selectUserIDBasedOnEmail.ExecuteReader();
                fetchID.Read();

                new ViewOrder(fetchID.GetString("userID")).Show();

                fetchID.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
