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
using System.Xml.Serialization;
using ComponentFactory.Krypton.Toolkit;

using MySql.Data.MySqlClient;

namespace E_CommerceSystem
{
    public partial class Profile : KryptonForm
    {
        Config dbConfig;
        MySqlConnection conn;
        UserPrompt refreshForm;
        AdminPrompt adminPrompt;

        public Profile()
        {
            InitializeComponent();
            this.FormClosing += Profile_FormClosing;
        }

        public void CheckUserRole() // Gian888
        {
            dbConfig = new Config();
            conn = dbConfig.getConnection();
            MySqlCommand checkUser = new MySqlCommand("SELECT * FROM users WHERE userID = ('" + UserID + "')", conn);
            try
            {
                conn.Open();
                MySqlDataReader fetchUser = checkUser.ExecuteReader();
                fetchUser.Read();

                if (fetchUser.HasRows)
                {

                    if (fetchUser.GetString("role").Equals("admin"))
                    {
                        UserID = fetchUser.GetString("userID");
                        new AdminPrompt(UserID).Show();
                        fetchUser.Close();
                        this.Hide();
                    }
                    else
                    {
                        UserID = fetchUser.GetString("userID");
                        new UserPrompt(UserID).Show();
                        fetchUser.Close();
                        this.Hide();
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                conn.Close();
            }

        }

        public Profile(string user_id)
        {
            InitializeComponent();
            this.UserID = user_id;
            this.FormClosing += Profile_FormClosing;


        }
        public string UserID { get; set; }


        private void Profile_FormClosed(object sender, FormClosedEventArgs e)
        {
            refreshForm = new UserPrompt(UserID);
            refreshForm.UserPrompt_Load(sender, e);
        }

        private void Profile_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshForm = new UserPrompt(UserID);
            refreshForm.UserPrompt_Load(sender, e);
        }

        private void EditProfile_Load(object sender, EventArgs e)
        {

            //update state
            int days, months, years;
            user_updateProfile.Visible = false;
            user_Name.Enabled = false;
            user_Gender.Enabled = false;
            user_Gmail.Enabled = false;
            user_phoneNumber.Enabled = false;
            update_username.Enabled = false;
            cmb_days.Enabled = false;
            cmb_months.Enabled = false;
            cmb_years.Enabled = false;

            for (days = 1; days <= 31; days++)
            {
                cmb_days.Items.Add(days);
            }

            for (months = 1; months <= 12; months++)
            {
                cmb_months.Items.Add(months);
            }

            for (years = 1980; years <= 2024; years++)
            {
                cmb_years.Items.Add(years);
            }

            user_Gender.Items.Add("Male");
            user_Gender.Items.Add("Female");
            user_Gender.Items.Add("Others");

            required_FieldBdate.Visible = false;
            required_FieldGender.Visible = false;
            required_FieldGmail.Visible = false;
            required_FieldName.Visible = false;
            required_FieldPhone.Visible = false;
            required_FieldRecEmail.Visible = false;

            fetchData();

        }

        private void fetchData()
        {
            dbConfig = new Config();
            conn = dbConfig.getConnection();

            MySqlCommand fetchUserData = new MySqlCommand("SELECT picture_directory, userName, Name, Gender, Email, Phone, Birth FROM users WHERE userID = ('" + UserID + "')", conn);

            try
            {
                conn.Open();
                MySqlDataReader fetchResult = fetchUserData.ExecuteReader();
                fetchResult.Read();

                update_username.Text = fetchResult.GetString("userName");
                user_Name.Text = fetchResult.GetString("Name");
                user_Gender.Text = fetchResult.GetString("Gender");
                user_Gmail.Text = fetchResult.GetString("Email");
                user_phoneNumber.Text = fetchResult.GetString("Phone");
                DateTime selectedDate = fetchResult.GetDateTime("Birth");


                if (selectedDate.Day == 1 && selectedDate.Month == 1)
                {

                    cmb_days.Text = "";
                    cmb_months.Text = "";
                }
                else
                {
                    cmb_days.SelectedItem = selectedDate.Day;
                    cmb_months.SelectedItem = selectedDate.Month;
                    cmb_years.SelectedItem = selectedDate.Year;
                }

                //display the image based on the fetched id
                long len = fetchResult.GetBytes(0, 0, null, 0, 0);
                byte[] array = new byte[System.Convert.ToInt32(len + 1)];
                fetchResult.GetBytes(0, 0, array, 0, System.Convert.ToInt32(len));

                MemoryStream ms = new MemoryStream(array);
                Bitmap bitmpap = new Bitmap(ms);
                user_uploadPhoto.Image = bitmpap;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
        }

        private void kryptonTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (user_phoneNumber.Text == string.Empty)
            {
                required_FieldPhone.Visible = true;
                required_FieldPhone.Text = "Required field";
                user_updateProfile.Enabled = false;
            }

            else
            {
                required_FieldPhone.Text = "";
                user_updateProfile.Enabled = true;
            }
        }

        private void user_gender_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (user_Gender.Text == string.Empty)
            {
                required_FieldGender.Visible = true;
                required_FieldGender.Text = "Required field";
                required_FieldGender.Enabled = false;
            }

            else
            {
                required_FieldGender.Text = "";
                user_updateProfile.Enabled = true;
            }
        }

        private void upload_photo_Click(object sender, EventArgs e)
        {


            openFileDialog1.Filter = "Images Files (*.jpg; *.jpeg; *.png)| *.jpg; *.jpeg; *.png;";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                user_uploadPhoto.Image = new Bitmap(openFileDialog1.FileName);
            }
            else
            {
                // do nothing
            }

            user_updateProfile.Visible = true;
            editProfile.Visible = false;
            user_Name.Enabled = true;
            user_Gender.Enabled = true;
            user_Gmail.Enabled = true;
            user_phoneNumber.Enabled = true;
            update_username.Enabled = true;
            cmb_days.Enabled = true;
            cmb_months.Enabled = true;
            cmb_years.Enabled = true;

        }

        private bool isFulfilled()
        {
            if (update_username.Text.Length == 0 || user_Gmail.Text.Length == 0 || user_Name.Text.Length == 0 || user_phoneNumber.Text.Length == 0 || user_Gender.SelectedIndex < 0 || cmb_months.SelectedIndex < 0 || cmb_days.SelectedIndex < 0 || cmb_years.SelectedIndex < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void signup_phone_TextChanged(object sender, EventArgs e)
        {
            if (user_Name.Text == string.Empty)
            {
                required_FieldName.Visible = true;
                required_FieldName.Text = "Required field";
                user_updateProfile.Enabled = false;
            }

            else
            {
                required_FieldName.Text = "";
                user_updateProfile.Enabled = true;
            }
        }

        private void user_updateProfile_Click(object sender, EventArgs e)
        {
            dbConfig = new Config();
            conn = dbConfig.getConnection();

            if (isFulfilled() == false)
            {
                required_FieldRecEmail.Visible = true;
                required_FieldName.Visible = true;
                required_FieldGmail.Visible = true;
                required_FieldGender.Visible = true;
                required_FieldBdate.Visible = true;
                required_FieldPhone.Visible = true;
            }

            else
            {
                required_FieldRecEmail.Visible = false;
                required_FieldName.Visible = false;
                required_FieldGmail.Visible = false;
                required_FieldGender.Visible = false;
                required_FieldBdate.Visible = false;
                required_FieldPhone.Visible = false;

                user_Name.Enabled = false;
                user_Gender.Enabled = false;
                user_Gmail.Enabled = false;
                user_phoneNumber.Enabled = false;
                update_username.Enabled = false;
                cmb_days.Enabled = false;
                cmb_months.Enabled = false;
                cmb_years.Enabled = false;

                //query dito na update sa db kung ano yung info
                MemoryStream ms = new MemoryStream();
                user_uploadPhoto.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arrImage = ms.GetBuffer();

                string updatedBirth = $"{cmb_years.SelectedItem}-{cmb_months.SelectedItem}-{cmb_days.SelectedItem}";


                MySqlCommand updateUserPhoto = new MySqlCommand("UPDATE users SET picture_directory = @picture_directory WHERE userID = ('" + UserID + "')", conn);
                updateUserPhoto.Parameters.AddWithValue("@picture_directory", arrImage);
                MySqlCommand updateUserData = new MySqlCommand("UPDATE users SET userName = ('" + update_username.Text + "'), Name = ('" + user_Name.Text + "'), Email = ('" + user_Gmail.Text + "'), Birth = ('" + updatedBirth + "'), Gender = ('" + user_Gender.Text + "') WHERE userID = ('" + UserID + "')", conn);

                try
                {
                    conn.Open();
                    updateUserData.ExecuteNonQuery();
                    updateUserPhoto.ExecuteNonQuery();


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                user_updateProfile.Visible = false;
                editProfile.Visible = true;
            }
        }

        private void editProfile_Click(object sender, EventArgs e)
        {
            user_updateProfile.Visible = true;
            editProfile.Visible = false;
            user_Name.Enabled = true;
            user_Gender.Enabled = true;
            user_Gmail.Enabled = true;
            user_phoneNumber.Enabled = true;
            update_username.Enabled = true;
            cmb_days.Enabled = true;
            cmb_months.Enabled = true;
            cmb_years.Enabled = true;
        }

        private void user_Gmail_TextChanged(object sender, EventArgs e)
        {
            if (user_Gmail.Text == string.Empty)
            {
                required_FieldGmail.Visible = true;
                required_FieldGmail.Text = "Required field";
                user_updateProfile.Enabled = false;
            }

            else
            {
                required_FieldGmail.Text = "";
                user_updateProfile.Enabled = true;
            }
        }

        private void user_phoneNumber_TextChanged(object sender, EventArgs e)
        {
            if (user_phoneNumber.Text == string.Empty)
            {
                required_FieldPhone.Visible = true;
                required_FieldPhone.Text = "Required field";
                user_updateProfile.Enabled = false;
            }

            else
            {
                required_FieldPhone.Text = "";
                user_updateProfile.Enabled = true;
            }
        }

        private void cmb_days_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void user_changePassword_Click(object sender, EventArgs e)
        {
            this.Hide();
            new PassRecovery().Show();
        }


        private void Profile_Activated(object sender, EventArgs e)
        {
            address_flowlayout.Controls.Clear();

            dbConfig = new Config();
            conn = dbConfig.getConnection();

            MySqlCommand selectAddress = new MySqlCommand("SELECT * FROM user_address WHERE userID = ('" + UserID + "')", conn);

            try
            {
                conn.Open();
                MySqlDataReader fetchAddress = selectAddress.ExecuteReader();

                while (fetchAddress.Read())
                {
                    AddressControl addressList = new AddressControl(UserID, "Edit");
                    addressList.FullName = fetchAddress.GetString("FullName");
                    addressList.Email = fetchAddress.GetString("Email");
                    addressList.Regions = fetchAddress.GetString("Region");
                    addressList.Province = fetchAddress.GetString("Province");
                    addressList.City = fetchAddress.GetString("City");
                    addressList.Barangay = fetchAddress.GetString("Barangay");
                    addressList.Postal = fetchAddress.GetString("PostalCode");
                    addressList.Street = fetchAddress.GetString("StreetName");
                    addressList.addressID = fetchAddress.GetInt32("AddressID");
                    addressList.Default = fetchAddress.GetString("isDefault");
                    address_flowlayout.Controls.Add(addressList);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Profile_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            CheckUserRole(); // Gian888
        }

        private void add_address_Click(object sender, EventArgs e)
        {
            new AddressEdit(UserID).Show();
        }

        private void update_username_TextChanged_1(object sender, EventArgs e)
        {
            if (update_username.Text == string.Empty)
            {
                required_FieldRecEmail.Visible = true;
                required_FieldRecEmail.Text = "Required field";
                user_updateProfile.Enabled = false;
            }

            else
            {
                required_FieldRecEmail.Text = "";
                user_updateProfile.Enabled = true;
            }
        }
    }
}
