using ComponentFactory.Krypton.Toolkit;
using MySql.Data.MySqlClient;
using System;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Speech.Synthesis;
using System.Linq;

namespace E_CommerceSystem
{
    public partial class SignUp : KryptonForm
    {
        Config dbConfig;
        MySqlConnection conn;
        SendEmailVerification emailVerify;

        public SignUp()
        {

            InitializeComponent();
        }

        private void SignUp_Load(object sender, EventArgs e)
        {
            //required field prompts
            error_username.Visible = true;
            error_password.Visible = true;
            error_gmail.Visible = true;
            error_confirmpassword.Visible = true;
            error_phone.Visible = true;
            address_panel.Visible = false;
            terms_panel.Enabled = false;
            sign_up.Visible = false;
            next_btn.Enabled = false;
            back_prev_panel.Visible = false;

            sign_up.Enabled = false;
            signup_confirmpassword.UseSystemPasswordChar = true;
            signup_password.UseSystemPasswordChar = true;
            defaultGmail.Enabled = false;

            defaultGmail.Enabled = false;
            cmbProvince.Enabled = false;
            cmbCity.Enabled = false;
            cmbBarangay.Enabled = false;
            postal_code.Enabled = false;
            street_name.Enabled = false;

            if (full_name.Text != string.Empty)
            {
                required_fullname.Visible = false;
            }
            else if (required_postal.Text != string.Empty)
            {
                required_postal.Visible = false;
            }
            else if (required_street.Text != string.Empty)
            {
                required_street.Visible = false;
            }


            cmbRegion.Items.Add("METRO MANILA");
            cmbProvince.Items.Add("METRO MANILA");

            string[] cities = {
            "Binondo", "Caloocan City", "Ermita", "Intramuros", "Las Pinas City", "Makati City",
            "Malabon City", "Malate", "Mandaluyong City", "Marikina City", "Muntinlupa City", "Navotas City",
            "Paco", "Pandacan", "Paranaque City", "Pasay City", "Pasig City", "Pateros", "Port Area", "Quezon City",
            "Quiapo", "Sampaloc", "San Juan City", "San Miguel", "San Nicolas", "Santa Ana", "Santa Cruz", "Taguig City",
            "Tondo I / Ii", "Valenzuela City"
            };


            foreach (string city in cities)
            {
                cmbCity.Items.Add(city);
                cmbCity.AutoCompleteMode = AutoCompleteMode.Suggest;
                cmbCity.AutoCompleteSource = AutoCompleteSource.ListItems;

            }
        }

        private void SignUp_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Do not add application exit here
        }

        public static bool isValidPhone(string phone)
        {
            string phonePattern = "((^(\\+)(\\d){12}$)|(^\\d{11}$))";
            var regex = new Regex(phonePattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(phone);
        }

        public static bool isValidEmail(string email) //function of checking if the email is proper format
        {
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }

        private void sign_up_Click(object sender, EventArgs e)
        {
            dbConfig = new Config();
            emailVerify = new SendEmailVerification();
            conn = dbConfig.getConnection();
            //avoid duplicates
            MySqlCommand checkIfDataExists = new MySqlCommand("SELECT * FROM temporary_signups WHERE userName = ('" + signup_username.Text + "') OR phoneNumber = ('" + signup_phone.Text + "') OR Email = ('" + signup_gmail.Text + "')", conn);
            try
            {
                conn.Open();
                MySqlDataReader checkData = checkIfDataExists.ExecuteReader();   //execute the command on line 83
                checkData.Read(); //fetch once

                if (checkData.HasRows) //check if exists using HasRows
                {
                    MBPopup("User Already Exists!");
                    checkData.Close(); //closing to avoid unclosed reader error
                }
                else
                {
                    checkData.Close();

                    string validatedEmail = signup_gmail.Text + defaultGmail.Text;

                    if (isValidPhone(signup_phone.Text) && isValidEmail(validatedEmail))
                    {
                        string code = emailVerify.EmailVerificationCode(validatedEmail, signup_username.Text);

                        MySqlCommand insertTempSignUp = new MySqlCommand("INSERT INTO temporary_signups (userName, password, phoneNumber, Email, verificationCode) VALUES (@userName, @password, @phoneNumber, @Email, @verificationCode)", conn);
                        insertTempSignUp.Parameters.AddWithValue("@userName", signup_username.Text);
                        insertTempSignUp.Parameters.AddWithValue("@password", signup_password.Text);
                        insertTempSignUp.Parameters.AddWithValue("@phoneNumber", signup_phone.Text);
                        insertTempSignUp.Parameters.AddWithValue("@Email", validatedEmail);
                        insertTempSignUp.Parameters.AddWithValue("@verificationCode", code);

                        insertTempSignUp.ExecuteNonQuery();

                        new Verification(validatedEmail, signup_username.Text, full_name.Text, cmbRegion.Text, cmbProvince.Text, cmbCity.Text, cmbBarangay.Text, postal_code.Text, street_name.Text).Show();
                        this.Close();

                    }
                    else
                    {
                        MBPopup("Invalid Inputs Detected!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void back_btn_Click(object sender, EventArgs e)
        {
            this.Hide();
            new Login().Show();
        }
        public void MBPopup(string message)
        {
            Form form = new Form();
            using (CustomMessageBox mb = new CustomMessageBox())
            {
                form.StartPosition = FormStartPosition.Manual;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Opacity = .50d;
                form.BackColor = Color.Black;
                form.Size = this.Size;
                form.Location = this.Location;
                form.ShowInTaskbar = false;
                form.Show();
                mb.Owner = form;
                mb.Title = message;
                mb.ShowDialog();
                form.Dispose();
            }
        }

        private void next_btn_Click(object sender, EventArgs e)
        {
            if (signup_password.Text != signup_confirmpassword.Text)
            {
                MBPopup("UNMATCHED PASSWORD!");
            }

            else
            {
                back_btn.Visible = false;
                back_prev_panel.Visible = true;
                address_panel.Visible = true;
                next_btn.Visible = false;
                sign_up.Visible = true;
                terms_panel.Enabled = true;
            }
        }

        private void full_name_TextChanged(object sender, EventArgs e)
        {
            if (full_name.Text.Any(char.IsDigit))
            {
                if (full_name.Text == string.Empty)
                {

                    sign_up.Enabled = false;
                    eula.Checked = false;
                    required_fullname.Text = "Required Field";
                    required_fullname.Visible = true;

                }
                else
                {
                    sign_up.Enabled = false;
                    eula.Checked = false;
                    required_fullname.Text = "Full Name cannot contain a number!";
                    required_fullname.Visible = true;
                }
            }
            else
            {
                required_fullname.Visible = false;
                if (full_name.Text != string.Empty && cmbRegion.SelectedIndex >= 0 && cmbProvince.SelectedIndex >= 0 && cmbCity.SelectedIndex >= 0 && cmbBarangay.SelectedIndex >= 0 && postal_code.Text != string.Empty && street_name.Text != string.Empty && eula.Checked)
                {
                    sign_up.Enabled = true;
                }
                else
                {
                    sign_up.Enabled = false;

                }
            }
        }

        private void cmbRegion_SelectedIndexChanged(object sender, EventArgs e) { cmbProvince.Enabled = true; }

        private void cmbProvince_SelectedIndexChanged(object sender, EventArgs e) { cmbCity.Enabled = true; }


        private void cmbCity_SelectedIndexChanged(object sender, EventArgs e)
        {

            cmbBarangay.Enabled = true;
            cmbBarangay.AutoCompleteMode = AutoCompleteMode.Suggest;
            cmbBarangay.AutoCompleteSource = AutoCompleteSource.ListItems;

            if (cmbCity.Text.Equals("Binondo"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 287", "Barangay 288", "Barangay 289", "Barangay 290", "Barangay 291",
                "Barangay 292", "Barangay 293", "Barangay 294", "Barangay 295", "Barangay 296"
                };

                foreach (string BrgyBinondo in Barangays)
                {
                    BrgyBinondo.ToUpper();
                    cmbBarangay.Items.Add(BrgyBinondo);

                }
            }

            else if (cmbCity.Text.Equals("Caloocan City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 100", "Barangay 101", "Barangay 102", "Barangay 103", "Barangay 104",
                "Barangay 105", "Barangay 106", "Barangay 107", "Barangay 108", "Barangay 109",
                "Barangay 11", "Barangay 110", "Barangay 111", "Barangay 112", "Barangay 113",
                "Barangay 114", "Barangay 115", "Barangay 116", "Barangay 117", "Barangay 118",
                "Barangay 119", "Barangay 12", "Barangay 120", "Barangay 121", "Barangay 122",
                "Barangay 123", "Barangay 124", "Barangay 125", "Barangay 126", "Barangay 127",
                "Barangay 128", "Barangay 129", "Barangay 13", "Barangay 130", "Barangay 131",
                "Barangay 132", "Barangay 133", "Barangay 134", "Barangay 135", "Barangay 136",
                "Barangay 137", "Barangay 138", "Barangay 139", "Barangay 14", "Barangay 140",
                "Barangay 141", "Barangay 142", "Barangay 143", "Barangay 144", "Barangay 145",
                "Barangay 146", "Barangay 147", "Barangay 148", "Barangay 149", "Barangay 15",
                "Barangay 150", "Barangay 151", "Barangay 152", "Barangay 153", "Barangay 154",
                "Barangay 155", "Barangay 156", "Barangay 157", "Barangay 158", "Barangay 159",
                "Barangay 16", "Barangay 160", "Barangay 161", "Barangay 162", "Barangay 163",
                "Barangay 164", "Barangay 165", "Barangay 166", "Barangay 167", "Barangay 168",
                "Barangay 169", "Barangay 17", "Barangay 170", "Barangay 171", "Barangay 172",
                "Barangay 173", "Barangay 174", "Barangay 175", "Barangay 176", "Barangay 177",
                "Barangay 178", "Barangay 179", "Barangay 18", "Barangay 180", "Barangay 181",
                "Barangay 182", "Barangay 183", "Barangay 184", "Barangay 185", "Barangay 186",
                "Barangay 187", "Barangay 188", "Barangay 19", "Barangay 2", "Barangay 20",
                "Barangay 21", "Barangay 22", "Barangay 23", "Barangay 24", "Barangay 25",
                "Barangay 26", "Barangay 27", "Barangay 28", "Barangay 29", "Barangay 3",
                "Barangay 30", "Barangay 31", "Barangay 32", "Barangay 33", "Barangay 34",
                "Barangay 35", "Barangay 36", "Barangay 37", "Barangay 38", "Barangay 39",
                "Barangay 4", "Barangay 40", "Barangay 41", "Barangay 42", "Barangay 43",
                "Barangay 44", "Barangay 45", "Barangay 46", "Barangay 47", "Barangay 48",
                "Barangay 49", "Barangay 5", "Barangay 50", "Barangay 51", "Barangay 52",
                "Barangay 53", "Barangay 54", "Barangay 55", "Barangay 56", "Barangay 57",
                "Barangay 58", "Barangay 59", "Barangay 6", "Barangay 60", "Barangay 61",
                "Barangay 62", "Barangay 63", "Barangay 64", "Barangay 65", "Barangay 66",
                "Barangay 67", "Barangay 68", "Barangay 69", "Barangay 7", "Barangay 70",
                "Barangay 71", "Barangay 72", "Barangay 73", "Barangay 74", "Barangay 75",
                "Barangay 76", "Barangay 77", "Barangay 78", "Barangay 79", "Barangay 8",
                "Barangay 80", "Barangay 81", "Barangay 82", "Barangay 83", "Barangay 84",
                "Barangay 85", "Barangay 86", "Barangay 87", "Barangay 88", "Barangay 89",
                "Barangay 9", "Barangay 90", "Barangay 91", "Barangay 92", "Barangay 93",
                "Barangay 94", "Barangay 95", "Barangay 96", "Barangay 97", "Barangay 98",
                "Barangay 99"
            };


                foreach (string BrgyCaloocan in Barangays)
                {
                    BrgyCaloocan.ToUpper();
                    cmbBarangay.Items.Add(BrgyCaloocan);


                }
            }

            else if (cmbCity.Text.Equals("Ermita"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 659", "Barangay 659-A", "Barangay 660", "Barangay 660-A", "Barangay 661",
                "Barangay 663", "Barangay 663-A", "Barangay 664", "Barangay 666", "Barangay 667",
                "Barangay 668", "Barangay 669", "Barangay 670"
            };

                foreach (string BrgyErmita in Barangays)
                {
                    BrgyErmita.ToUpper();
                    cmbBarangay.Items.Add(BrgyErmita);

                }
            }


            else if (cmbCity.Text.Equals("Intramuros"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 654", "Barangay 655", "Barangay 656", "Barangay 657", "Barangay 658"
            };

                foreach (string BrgyIntramuros in Barangays)
                {
                    BrgyIntramuros.ToUpper();
                    cmbBarangay.Items.Add(BrgyIntramuros);

                }
            }


            else if (cmbCity.Text.Equals("Las Pinas City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Almanza Dos", "Almanza Uno", "B.F. International Village", "Daniel Fajardo", "Elias Aldana",
                "Ilaya", "Manuyo Dos", "Manuyo Uno", "Pamplona Dos", "Pamplona Uno", "Pilar",
                "Pulang Lupa Dos", "Pulang Lupa Uno", "Talon Dos", "Talon Kuatro", "Talon Singko",
                "Talon Tres", "Talon Uno", "Zapote"
            };

                foreach (string BrgyLasPinas in Barangays)
                {
                    BrgyLasPinas.ToUpper();
                    cmbBarangay.Items.Add(BrgyLasPinas);

                }
            }


            else if (cmbCity.Text.Equals("Makati City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Bangkal", "Bel-air", "Carmona", "Cembo", "Comembo", "Dasmarinas", "East Rembo",
                "Forbes Park", "Guadalupe Nuevo", "Guadalupe Viejo", "Kasilawan", "La Paz", "Magallanes",
                "Olympia", "Palanan", "Pembo", "Pinagkaisahan", "Pio Del Pilar", "Pitogo", "Poblacion",
                "Post Proper Northside", "Post Proper Southside", "Rizal", "San Antonio", "San Isidro",
                "San Lorenzo", "Santa Cruz", "Singkamas", "South Cembo", "Tejeros", "Urdaneta",
                "Valenzuela", "West Rembo"
            };

                foreach (string BrgyMakati in Barangays)
                {

                    BrgyMakati.ToUpper();
                    cmbBarangay.Items.Add(BrgyMakati);

                }
            }


            else if (cmbCity.Text.Equals("Malabon City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Acacia", "Baritan", "Bayan-Bayanan", "Catmon", "Concepcion", "Dampalit", "Flores",
                "Hulong Duhat", "Ibaba", "Longos", "Maysilo", "Muzon", "Niugan", "Panghulo", "Potrero",
                "San Agustin", "Santolan", "Tanong", "Tinajeros", "Tonsuya", "Tugatog"
            };

                foreach (string BrgyMalabon in Barangays)
                {
                    BrgyMalabon.ToUpper();
                    cmbBarangay.Items.Add(BrgyMalabon);
                }
            }


            else if (cmbCity.Text.Equals("Malate"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 688", "Barangay 689", "Barangay 690", "Barangay 691", "Barangay 692",
                "Barangay 693", "Barangay 694", "Barangay 695", "Barangay 696", "Barangay 697",
                "Barangay 698", "Barangay 699", "Barangay 700", "Barangay 701", "Barangay 702",
                "Barangay 703", "Barangay 704", "Barangay 705", "Barangay 706", "Barangay 707",
                "Barangay 708", "Barangay 709", "Barangay 710", "Barangay 711", "Barangay 712",
                "Barangay 713", "Barangay 714", "Barangay 715", "Barangay 716", "Barangay 717",
                "Barangay 718", "Barangay 719", "Barangay 720", "Barangay 721", "Barangay 722",
                "Barangay 723", "Barangay 724", "Barangay 725", "Barangay 726", "Barangay 727",
                "Barangay 728", "Barangay 729", "Barangay 730", "Barangay 731", "Barangay 732",
                "Barangay 733", "Barangay 734", "Barangay 735", "Barangay 736", "Barangay 737",
                "Barangay 738", "Barangay 739", "Barangay 740", "Barangay 741", "Barangay 742",
                "Barangay 743", "Barangay 744"
            };


                foreach (string BrgyMalate in Barangays)
                {
                    BrgyMalate.ToUpper();
                    cmbBarangay.Items.Add(BrgyMalate);
                }
            }

            else if (cmbCity.Text.Equals("Mandaluyong City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Addition Hills", "Bagong Silang", "Barangka Drive", "Barangka Ibaba", "Barangka Ilaya",
                "Barangka Itaas", "Buayang Bato", "Burol", "Daang Bakal", "Hagdang Bato Itaas",
                "Hagdang Bato Libis", "Harapin Ang Bukas", "Highway Hills", "Hulo", "Mabini-J. Rizal",
                "Malamig", "Mauway", "Namayan", "New Zaniga", "Old Zaniga", "Pag-asa", "Plainview",
                "Pleasant Hills", "Poblacion", "San Jose", "Vergara", "Wack-Wack Greenhills"
            };

                foreach (string BrgyMandaluyong in Barangays)
                {
                    BrgyMandaluyong.ToUpper();
                    cmbBarangay.Items.Add(BrgyMandaluyong);
                }
            }

            else if (cmbCity.Text.Equals("Marikina City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangka", "Calumpang", "Concepcion Dos", "Concepcion Uno", "Fortune",
                "Industrial Valley", "Jesus De La Pena", "Malanday", "Marikina Heights (Concepcion)",
                "Nangka", "Parang", "San Roque", "Santa Elena", "Santo Nino", "Tanong", "Tumana"
            };


                foreach (string BrgyMarikina in Barangays)
                {
                    BrgyMarikina.ToUpper();
                    cmbBarangay.Items.Add(BrgyMarikina);
                }
            }


            else if (cmbCity.Text.Equals("Muntinlupa City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Alabang", "Ayala Alabang", "Bayanan", "Buli", "Cupang",
                "Poblacion", "Putatan", "Sucat", "Tunasan"
            };

                foreach (string BrgyMuntinlupa in Barangays)
                {
                    BrgyMuntinlupa.ToUpper();
                    cmbBarangay.Items.Add(BrgyMuntinlupa);
                }
            }


            else if (cmbCity.Text.Equals("Navotas City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Bagumbayan North", "Bagumbayan South", "Bangculasi", "Daanghari", "Navotas East",
                "Navotas West", "North Bay Blvd.", "San Jose", "San Rafael Village", "San Roque",
                "Sipac-Almacen", "Tangos", "Tanza"
            };


                foreach (string BrgyNavotas in Barangays)
                {
                    BrgyNavotas.ToUpper();
                    cmbBarangay.Items.Add(BrgyNavotas);
                }
            }

            else if (cmbCity.Text.Equals("Paco"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 662", "Barangay 664-A", "Barangay 671", "Barangay 672", "Barangay 673",
                "Barangay 674", "Barangay 675", "Barangay 676", "Barangay 677", "Barangay 678",
                "Barangay 679", "Barangay 680", "Barangay 681", "Barangay 682", "Barangay 683",
                "Barangay 684", "Barangay 685", "Barangay 686", "Barangay 687", "Barangay 809",
                "Barangay 810", "Barangay 811", "Barangay 812", "Barangay 813", "Barangay 814",
                "Barangay 815", "Barangay 816", "Barangay 817", "Barangay 818", "Barangay 819",
                "Barangay 820", "Barangay 821", "Barangay 822", "Barangay 823", "Barangay 824",
                "Barangay 825", "Barangay 826", "Barangay 827", "Barangay 828", "Barangay 829",
                "Barangay 830", "Barangay 831", "Barangay 832"
            };

                foreach (string BrgyPaco in Barangays)
                {
                    BrgyPaco.ToUpper();
                    cmbBarangay.Items.Add(BrgyPaco);
                }
            }

            else if (cmbCity.Text.Equals("Pandacan"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 833", "Barangay 834", "Barangay 835", "Barangay 836", "Barangay 837",
                "Barangay 838", "Barangay 839", "Barangay 840", "Barangay 841", "Barangay 842",
                "Barangay 843", "Barangay 844", "Barangay 845", "Barangay 846", "Barangay 847",
                "Barangay 848", "Barangay 849", "Barangay 850", "Barangay 851", "Barangay 852",
                "Barangay 853", "Barangay 855", "Barangay 856", "Barangay 857", "Barangay 858",
                "Barangay 859", "Barangay 860", "Barangay 861", "Barangay 862", "Barangay 863",
                "Barangay 864", "Barangay 865", "Barangay 866", "Barangay 867", "Barangay 868",
                "Barangay 869", "Barangay 870", "Barangay 871", "Barangay 872"
            };

                foreach (string BrgyPandacan in Barangays)
                {
                    BrgyPandacan.ToUpper();
                    cmbBarangay.Items.Add(BrgyPandacan);
                }
            }

            else if (cmbCity.Text.Equals("Paranaque City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "B.F. Homes", "Baclaran", "Don Bosco", "Don Galo", "La Huerta",
                "Marcelo Green Village", "Merville", "Moonwalk", "San Antonio", "San Dionisio",
                "San Isidro", "San Martin De Porres", "Santo Nino", "Sun Valley", "Tambo", "Vitalez"
            };

                foreach (string BrgyParanaque in Barangays)
                {
                    BrgyParanaque.ToUpper();
                    cmbBarangay.Items.Add(BrgyParanaque);
                }
            }


            else if (cmbCity.Text.Equals("Pasay City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 1", "Barangay 10", "Barangay 100", "Barangay 101", "Barangay 102",
                "Barangay 103", "Barangay 104", "Barangay 105", "Barangay 106", "Barangay 107",
                "Barangay 108", "Barangay 109", "Barangay 11", "Barangay 110", "Barangay 111",
                "Barangay 112", "Barangay 113", "Barangay 114", "Barangay 115", "Barangay 116",
                "Barangay 117", "Barangay 118", "Barangay 119", "Barangay 12", "Barangay 120",
                "Barangay 121", "Barangay 122", "Barangay 123", "Barangay 124", "Barangay 125",
                "Barangay 126", "Barangay 127", "Barangay 128", "Barangay 129", "Barangay 13",
                "Barangay 130", "Barangay 131", "Barangay 132", "Barangay 133", "Barangay 134",
                "Barangay 135", "Barangay 136", "Barangay 137", "Barangay 138", "Barangay 139",
                "Barangay 14", "Barangay 140", "Barangay 141", "Barangay 142", "Barangay 143",
                "Barangay 144", "Barangay 145", "Barangay 146", "Barangay 147", "Barangay 148",
                "Barangay 149", "Barangay 15", "Barangay 150", "Barangay 151", "Barangay 152",
                "Barangay 153", "Barangay 154", "Barangay 155", "Barangay 156", "Barangay 157",
                "Barangay 158", "Barangay 159", "Barangay 16", "Barangay 160", "Barangay 161",
                "Barangay 162", "Barangay 163", "Barangay 164", "Barangay 165", "Barangay 166",
                "Barangay 167", "Barangay 168", "Barangay 169", "Barangay 17", "Barangay 170",
                "Barangay 171", "Barangay 172", "Barangay 173", "Barangay 174", "Barangay 175",
                "Barangay 176", "Barangay 177", "Barangay 178", "Barangay 179", "Barangay 18",
                "Barangay 180", "Barangay 181", "Barangay 182", "Barangay 183", "Barangay 184",
                "Barangay 185", "Barangay 186", "Barangay 187", "Barangay 188", "Barangay 189",
                "Barangay 19", "Barangay 190", "Barangay 191", "Barangay 192", "Barangay 193",
                "Barangay 194", "Barangay 195", "Barangay 196", "Barangay 197", "Barangay 198",
                "Barangay 199", "Barangay 2", "Barangay 20", "Barangay 200", "Barangay 201",
                "Barangay 21", "Barangay 22", "Barangay 23", "Barangay 24", "Barangay 25",
                "Barangay 26", "Barangay 27", "Barangay 28", "Barangay 29", "Barangay 3",
                "Barangay 30", "Barangay 31", "Barangay 32", "Barangay 33", "Barangay 34",
                "Barangay 35", "Barangay 36", "Barangay 37", "Barangay 38", "Barangay 39",
                "Barangay 4", "Barangay 40", "Barangay 41", "Barangay 42", "Barangay 43",
                "Barangay 44", "Barangay 45", "Barangay 46", "Barangay 47", "Barangay 48",
                "Barangay 49", "Barangay 5", "Barangay 50", "Barangay 51", "Barangay 52",
                "Barangay 53", "Barangay 54", "Barangay 55", "Barangay 56", "Barangay 57",
                "Barangay 58", "Barangay 59", "Barangay 6", "Barangay 60", "Barangay 61",
                "Barangay 62", "Barangay 63", "Barangay 64", "Barangay 65", "Barangay 66",
                "Barangay 67", "Barangay 68", "Barangay 69", "Barangay 7", "Barangay 70",
                "Barangay 71", "Barangay 72", "Barangay 73", "Barangay 74", "Barangay 75",
                "Barangay 76", "Barangay 77", "Barangay 78", "Barangay 79", "Barangay 8",
                "Barangay 80", "Barangay 81", "Barangay 82", "Barangay 83", "Barangay 84",
                "Barangay 85", "Barangay 86", "Barangay 87", "Barangay 88", "Barangay 89",
                "Barangay 9", "Barangay 90", "Barangay 91", "Barangay 92", "Barangay 93",
                "Barangay 94", "Barangay 95", "Barangay 96", "Barangay 97", "Barangay 98",
                "Barangay 99"
            };

                foreach (string BrgyPasay in Barangays)
                {
                    BrgyPasay.ToUpper();
                    cmbBarangay.Items.Add(BrgyPasay);
                }
            }


            else if (cmbCity.Text.Equals("Pasig City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Bagong Ilog", "Bagong Katipunan", "Bambang", "Buting", "Caniogan",
                "Dela Paz", "Kalawaan", "Kapasigan", "Kapitolyo", "Malinao",
                "Manggahan", "Maybunga", "Oranbo", "Palatiw", "Pinagbuhatan",
                "Pineda", "Rosario", "Sagad", "San Antonio", "San Joaquin",
                "San Jose", "San Miguel", "San Nicolas", "Santa Cruz", "Santa Lucia",
                "Santa Rosa", "Santo Tomas", "Santolan", "Sumilang", "Ugong"
            };


                foreach (string BrgyPasig in Barangays)
                {
                    BrgyPasig.ToUpper();
                    cmbBarangay.Items.Add(BrgyPasig);
                }
            }

            else if (cmbCity.Text.Equals("Pateros"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Aguho", "Magtanggol", "Martires Del 96", "Poblacion", "San Pedro",
                "San Roque", "Santa Ana", "Santo Rosario-Kanluran", "Santo Rosario-Silangan", "Tabacalera"
            };

                foreach (string BrgyPateros in Barangays)
                {
                    BrgyPateros.ToUpper();
                    cmbBarangay.Items.Add(BrgyPateros);
                }
            }


            else if (cmbCity.Text.Equals("Port Area"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 649", "Barangay 650", "Barangay 651", "Barangay 652", "Barangay 653"
            };


                foreach (string BrgyPort in Barangays)
                {
                    BrgyPort.ToUpper();
                    cmbBarangay.Items.Add(BrgyPort);
                }
            }


            else if (cmbCity.Text.Equals("Quezon City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Alicia", "Amihan", "Apolonio Samson", "Aurora", "Baesa", "Bagbag",
                "Bagong Lipunan Ng Crame", "Bagong Pag-Asa", "Bagong Silangan", "Bagumbayan", "Bagumbuhay",
                "Bahay Toro", "Balingasa", "Balong Bato", "Batasan Hills", "Bayanihan", "Blue Ridge A",
                "Blue Ridge B", "Botocan", "Bungad", "Camp Aguinaldo", "Capri", "Central",
                "Claro", "Commonwealth", "Culiat", "Damar", "Damayan", "Damayang Lagi",
                "Del Monte", "Dioquino Zobel", "Don Manuel", "Dona Imelda", "Dona Josefa", "Duyan-Duyan",
                "E. Rodriguez", "East Kamias", "Escopa I", "Escopa Ii", "Escopa Iii", "Escopa Iv",
                "Fairview", "Greater Lagro", "Gulod", "Holy Spirit", "Horseshoe", "Immaculate Concepcion",
                "Kaligayahan", "Kalusugan", "Kamuning", "Katipunan", "Kaunlaran", "Kristong Hari",
                "Krus Na Ligas", "Laging Handa", "Libis", "Lourdes", "Loyola Heights", "Maharlika",
                "Malaya", "Mangga", "Manresa", "Mariana", "Mariblo", "Marilag",
                "Masagana", "Masambong", "Matandang Balara", "Milagrosa", "N.s. Amoranto (Gintong Silahis)", "Nagkaisang Nayon",
                "Nayong Kanluran", "New Era (Constitution Hills)", "North Fairview", "Novaliches Proper", "Obrero", "Old Capitol Site",
                "Paang Bundok", "Pag-ibig Sa Nayon", "Paligsahan", "Paltok", "Pansol", "Paraiso",
                "Pasong Putik Proper", "Pasong Tamo", "Payatas", "Phil-Am", "Pinagkaisahan", "Pinyahan",
                "Project 6", "Quirino 2A", "Quirino 2B", "Quirino 2C", "Quirino 3A", "Ramon Magsaysay",
                "Roxas", "Sacred Heart", "Saint Ignatius", "Saint Peter", "Salvacion", "San Agustin",
                "San Antonio", "San Bartolome", "San Isidro", "San Isidro Labrador", "San Jose", "San Martin De Porres",
                "San Roque", "San Vicente", "Sangandaan", "Santa Cruz", "Santa Lucia", "Santa Monica",
                "Santa Teresita", "Santo Cristo", "Santo Domingo (Matalahib)", "Santo Nino", "Santol", "Sauyo",
                "Sienna", "Sikatuna Village", "Silangan", "Socorro", "South Triangle", "Tagumpay",
                "Talayan", "Talipapa", "Tandang Sora", "Tatalon", "Teachers Village East", "Teachers Village West",
                "U.p. Campus", "U.p. Village", "Ugong Norte", "Unang Sigaw", "Valencia", "Vasra",
                "Veterans Village", "Villa Maria Clara", "West Kamias", "West Triangle", "White Plains"
            };

                foreach (string BrgyQuezon in Barangays)
                {

                    BrgyQuezon.ToUpper();
                    cmbBarangay.Items.Add(BrgyQuezon);
                }
            }


            else if (cmbCity.Text.Equals("Quiapo"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 306", "Barangay 307", "Barangay 308", "Barangay 309", "Barangay 383",
                "Barangay 384", "Barangay 385", "Barangay 386", "Barangay 387", "Barangay 388",
                "Barangay 389", "Barangay 390", "Barangay 391", "Barangay 392", "Barangay 393",
                "Barangay 394"
            };

                foreach (string BrgyQuiapo in Barangays)
                {

                    BrgyQuiapo.ToUpper();
                    cmbBarangay.Items.Add(BrgyQuiapo);
                }
            }

            else if (cmbCity.Text.Equals("Sampaloc"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 395", "Barangay 396", "Barangay 397", "Barangay 398", "Barangay 399",
                "Barangay 400", "Barangay 401", "Barangay 402", "Barangay 403", "Barangay 404",
                "Barangay 405", "Barangay 406", "Barangay 407", "Barangay 408", "Barangay 409",
                "Barangay 410", "Barangay 411", "Barangay 412", "Barangay 413", "Barangay 414",
                "Barangay 415", "Barangay 416", "Barangay 417", "Barangay 418", "Barangay 419",
                "Barangay 420", "Barangay 421", "Barangay 422", "Barangay 423", "Barangay 424",
                "Barangay 425", "Barangay 426", "Barangay 427", "Barangay 428", "Barangay 429",
                "Barangay 430", "Barangay 431", "Barangay 432", "Barangay 433", "Barangay 434",
                "Barangay 435", "Barangay 436", "Barangay 437", "Barangay 438", "Barangay 439",
                "Barangay 440", "Barangay 441", "Barangay 442", "Barangay 443", "Barangay 444",
                "Barangay 445", "Barangay 446", "Barangay 447", "Barangay 448", "Barangay 449",
                "Barangay 450", "Barangay 451", "Barangay 452", "Barangay 453", "Barangay 454",
                "Barangay 455", "Barangay 456", "Barangay 457", "Barangay 458", "Barangay 459",
                "Barangay 460", "Barangay 461", "Barangay 462", "Barangay 463", "Barangay 464",
                "Barangay 465", "Barangay 466", "Barangay 467", "Barangay 468", "Barangay 469",
                "Barangay 470", "Barangay 471", "Barangay 472", "Barangay 473", "Barangay 474",
                "Barangay 475", "Barangay 476", "Barangay 477", "Barangay 478", "Barangay 479",
                "Barangay 480", "Barangay 481", "Barangay 482", "Barangay 483", "Barangay 484",
                "Barangay 485", "Barangay 486", "Barangay 487", "Barangay 488", "Barangay 489",
                "Barangay 490", "Barangay 491", "Barangay 492", "Barangay 493", "Barangay 494",
                "Barangay 495", "Barangay 496", "Barangay 497", "Barangay 498", "Barangay 499",
                "Barangay 500", "Barangay 501", "Barangay 502", "Barangay 503", "Barangay 504",
                "Barangay 505", "Barangay 506", "Barangay 507", "Barangay 508", "Barangay 509",
                "Barangay 510", "Barangay 511", "Barangay 512", "Barangay 513", "Barangay 514",
                "Barangay 515", "Barangay 516", "Barangay 517", "Barangay 518", "Barangay 519",
                "Barangay 520", "Barangay 521", "Barangay 522", "Barangay 523", "Barangay 524",
                "Barangay 525", "Barangay 526", "Barangay 527", "Barangay 528", "Barangay 529",
                "Barangay 530", "Barangay 531", "Barangay 532", "Barangay 533", "Barangay 534",
                "Barangay 535", "Barangay 536", "Barangay 537", "Barangay 538", "Barangay 539",
                "Barangay 540", "Barangay 541", "Barangay 542", "Barangay 543", "Barangay 544",
                "Barangay 545", "Barangay 546", "Barangay 547", "Barangay 548", "Barangay 549",
                "Barangay 550", "Barangay 551", "Barangay 552", "Barangay 553", "Barangay 554",
                "Barangay 555", "Barangay 556", "Barangay 557", "Barangay 558", "Barangay 559",
                "Barangay 560", "Barangay 561", "Barangay 562", "Barangay 563", "Barangay 564",
                "Barangay 565", "Barangay 566", "Barangay 567", "Barangay 568", "Barangay 569",
                "Barangay 570", "Barangay 571", "Barangay 572", "Barangay 573", "Barangay 574",
                "Barangay 575", "Barangay 576", "Barangay 577", "Barangay 578", "Barangay 579",
                "Barangay 580", "Barangay 581", "Barangay 582", "Barangay 583", "Barangay 584",
                "Barangay 585", "Barangay 586", "Barangay 587", "Barangay 587-A", "Barangay 588",
                "Barangay 589", "Barangay 590", "Barangay 591", "Barangay 592", "Barangay 593",
                "Barangay 594", "Barangay 595", "Barangay 596", "Barangay 597", "Barangay 598",
                "Barangay 599", "Barangay 600", "Barangay 601", "Barangay 602", "Barangay 603",
                "Barangay 604", "Barangay 605", "Barangay 606", "Barangay 607", "Barangay 608",
                "Barangay 609", "Barangay 610", "Barangay 611", "Barangay 612", "Barangay 613",
                "Barangay 614", "Barangay 615", "Barangay 616", "Barangay 617", "Barangay 618",
                "Barangay 619", "Barangay 620", "Barangay 621", "Barangay 622", "Barangay 623",
                "Barangay 624", "Barangay 625", "Barangay 626", "Barangay 627", "Barangay 628",
                "Barangay 629", "Barangay 630", "Barangay 631", "Barangay 632", "Barangay 633",
                "Barangay 634", "Barangay 635", "Barangay 636"
            };


                foreach (string BrgySampaloc in Barangays)
                {
                    BrgySampaloc.ToUpper();
                    cmbBarangay.Items.Add(BrgySampaloc);
                }
            }


            else if (cmbCity.Text.Equals("San Juan City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Addition Hills", "Balong-Bato", "Batis", "Corazon De Jesus", "Ermitano",
                "Greenhills", "Halo-Halo (St. Joseph)", "Isabelita", "Kabayanan", "Little Baguio",
                "Maytunas", "Onse", "Pasadena", "Pedro Cruz", "Progreso", "Rivera",
                "Salapan", "San Perfecto", "Santa Lucia", "Tibagan", "West Crame"
            };

                foreach (string BrgySanJuan in Barangays)
                {
                    BrgySanJuan.ToUpper();
                    cmbBarangay.Items.Add(BrgySanJuan);
                }
            }


            else if (cmbCity.Text.Equals("San Miguel"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 637", "Barangay 638", "Barangay 639", "Barangay 640", "Barangay 641",
                "Barangay 642", "Barangay 643", "Barangay 644", "Barangay 645", "Barangay 646",
                "Barangay 647", "Barangay 648"
            };


                foreach (string BrgySanMiguel in Barangays)
                {
                    BrgySanMiguel.ToUpper();
                    cmbBarangay.Items.Add(BrgySanMiguel);
                }
            }


            else if (cmbCity.Text.Equals("San Nicolas"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 268", "Barangay 269", "Barangay 270", "Barangay 271", "Barangay 272",
                "Barangay 273", "Barangay 274", "Barangay 275", "Barangay 276", "Barangay 281",
                "Barangay 282", "Barangay 283", "Barangay 284", "Barangay 285", "Barangay 286"
            };



                foreach (string BrgySanNicolas in Barangays)
                {
                    BrgySanNicolas.ToUpper();
                    cmbBarangay.Items.Add(BrgySanNicolas);
                }
            }


            else if (cmbCity.Text.Equals("Santa Ana"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 745", "Barangay 746", "Barangay 747", "Barangay 748", "Barangay 749",
                "Barangay 750", "Barangay 751", "Barangay 752", "Barangay 753", "Barangay 754",
                "Barangay 755", "Barangay 756", "Barangay 757", "Barangay 758", "Barangay 759",
                "Barangay 760", "Barangay 761", "Barangay 762", "Barangay 763", "Barangay 764",
                "Barangay 765", "Barangay 766", "Barangay 767", "Barangay 768", "Barangay 769",
                "Barangay 770", "Barangay 771", "Barangay 772", "Barangay 773", "Barangay 774",
                "Barangay 775", "Barangay 776", "Barangay 777", "Barangay 778", "Barangay 779",
                "Barangay 780", "Barangay 781", "Barangay 782", "Barangay 783", "Barangay 784",
                "Barangay 785", "Barangay 786", "Barangay 787", "Barangay 788", "Barangay 789",
                "Barangay 790", "Barangay 791", "Barangay 792", "Barangay 793", "Barangay 794",
                "Barangay 795", "Barangay 796", "Barangay 797", "Barangay 798", "Barangay 799",
                "Barangay 800", "Barangay 801", "Barangay 802", "Barangay 803", "Barangay 804",
                "Barangay 805", "Barangay 806", "Barangay 807", "Barangay 808", "Barangay 818-A",
                "Barangay 866", "Barangay 873", "Barangay 874", "Barangay 875", "Barangay 876",
                "Barangay 877", "Barangay 878", "Barangay 879", "Barangay 880", "Barangay 881",
                "Barangay 882", "Barangay 883", "Barangay 884", "Barangay 885", "Barangay 886",
                "Barangay 887", "Barangay 888", "Barangay 889", "Barangay 890", "Barangay 891",
                "Barangay 892", "Barangay 893", "Barangay 894", "Barangay 895", "Barangay 896",
                "Barangay 897", "Barangay 898", "Barangay 899", "Barangay 900", "Barangay 901",
                "Barangay 902", "Barangay 903", "Barangay 904", "Barangay 905"
            };


                foreach (string BrgySantaAna in Barangays)
                {
                    BrgySantaAna.ToUpper();
                    cmbBarangay.Items.Add(BrgySantaAna);
                }
            }


            else if (cmbCity.Text.Equals("Santa Cruz"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 297", "Barangay 298", "Barangay 299", "Barangay 300", "Barangay 301",
                "Barangay 302", "Barangay 303", "Barangay 304", "Barangay 305", "Barangay 306",
                "Barangay 307", "Barangay 308", "Barangay 309", "Barangay 310", "Barangay 311",
                "Barangay 312", "Barangay 313", "Barangay 314", "Barangay 315", "Barangay 316",
                "Barangay 317", "Barangay 318", "Barangay 319", "Barangay 320", "Barangay 321",
                "Barangay 322", "Barangay 323", "Barangay 324", "Barangay 325", "Barangay 326",
                "Barangay 327", "Barangay 328", "Barangay 329", "Barangay 330", "Barangay 331",
                "Barangay 332", "Barangay 333", "Barangay 334", "Barangay 335", "Barangay 336",
                "Barangay 337", "Barangay 338", "Barangay 339", "Barangay 340", "Barangay 341",
                "Barangay 342", "Barangay 343", "Barangay 344", "Barangay 345", "Barangay 346",
                "Barangay 347", "Barangay 348", "Barangay 349", "Barangay 350", "Barangay 351",
                "Barangay 352", "Barangay 353", "Barangay 354", "Barangay 355", "Barangay 356",
                "Barangay 357", "Barangay 358", "Barangay 359", "Barangay 360", "Barangay 361",
                "Barangay 362", "Barangay 363", "Barangay 364", "Barangay 365", "Barangay 366",
                "Barangay 367", "Barangay 368", "Barangay 369", "Barangay 370", "Barangay 371",
                "Barangay 372", "Barangay 373", "Barangay 374", "Barangay 375", "Barangay 376",
                "Barangay 377", "Barangay 378", "Barangay 379", "Barangay 380", "Barangay 381",
                "Barangay 382"
            };



                foreach (string BrgySantaCruz in Barangays)
                {
                    BrgySantaCruz.ToUpper();
                    cmbBarangay.Items.Add(BrgySantaCruz);
                }
            }

            else if (cmbCity.Text.Equals("Taguig City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Bagumbayan", "Bambang", "Calzada", "Central Bicutan", "Central Signal Village",
                "Fort Bonifacio", "Hagonoy", "Ibayo-Tipas", "Katuparan", "Ligid-Tipas",
                "Lower Bicutan", "Maharlika Village", "Napindan", "New Lower Bicutan", "North Daan Hari",
                "North Signal Village", "Palingon", "Pinagsama", "San Miguel", "Santa Ana",
                "South Daan Hari", "South Signal Village", "Tanyag", "Tuktukan", "Upper Bicutan",
                "Ususan", "Wawa", "Western Bicutan"
            };

                foreach (string BrgyTaguig in Barangays)
                {
                    BrgyTaguig.ToUpper();
                    cmbBarangay.Items.Add(BrgyTaguig);
                }
            }


            else if (cmbCity.Text.Equals("Tondo I / Ii"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Barangay 1", "Barangay 10", "Barangay 100", "Barangay 101", "Barangay 102",
                "Barangay 103", "Barangay 104", "Barangay 105", "Barangay 106", "Barangay 107",
                "Barangay 108", "Barangay 109", "Barangay 11", "Barangay 110", "Barangay 111",
                "Barangay 112", "Barangay 113", "Barangay 114", "Barangay 115", "Barangay 116",
                "Barangay 117", "Barangay 118", "Barangay 119", "Barangay 12", "Barangay 120",
                "Barangay 121", "Barangay 122", "Barangay 123", "Barangay 124", "Barangay 125",
                "Barangay 126", "Barangay 127", "Barangay 128", "Barangay 129", "Barangay 13",
                "Barangay 130", "Barangay 131", "Barangay 132", "Barangay 133", "Barangay 134",
                "Barangay 135", "Barangay 136", "Barangay 137", "Barangay 138", "Barangay 139",
                "Barangay 14", "Barangay 140", "Barangay 141", "Barangay 142", "Barangay 143",
                "Barangay 144", "Barangay 145", "Barangay 146", "Barangay 147", "Barangay 148",
                "Barangay 149", "Barangay 15", "Barangay 150", "Barangay 151", "Barangay 152",
                "Barangay 153", "Barangay 154", "Barangay 155", "Barangay 156", "Barangay 157",
                "Barangay 158", "Barangay 159", "Barangay 16", "Barangay 160", "Barangay 161",
                "Barangay 162", "Barangay 163", "Barangay 164", "Barangay 165", "Barangay 166",
                "Barangay 167", "Barangay 168", "Barangay 169", "Barangay 17", "Barangay 170",
                "Barangay 171", "Barangay 172", "Barangay 173", "Barangay 174", "Barangay 175",
                "Barangay 176", "Barangay 177", "Barangay 178", "Barangay 179", "Barangay 18",
                "Barangay 180", "Barangay 181", "Barangay 182", "Barangay 183", "Barangay 184",
                "Barangay 185", "Barangay 186", "Barangay 187", "Barangay 188", "Barangay 189",
                "Barangay 19", "Barangay 190", "Barangay 191", "Barangay 192", "Barangay 193",
                "Barangay 194", "Barangay 195", "Barangay 196", "Barangay 197", "Barangay 198",
                "Barangay 199", "Barangay 2", "Barangay 20", "Barangay 200", "Barangay 201",
                "Barangay 202", "Barangay 202-A", "Barangay 203", "Barangay 204", "Barangay 205",
                "Barangay 206", "Barangay 207", "Barangay 208", "Barangay 209", "Barangay 210",
                "Barangay 211", "Barangay 212", "Barangay 213", "Barangay 214", "Barangay 215",
                "Barangay 216", "Barangay 217", "Barangay 218", "Barangay 219", "Barangay 220",
                "Barangay 221", "Barangay 222", "Barangay 223", "Barangay 224", "Barangay 225",
                "Barangay 226", "Barangay 227", "Barangay 228", "Barangay 229", "Barangay 230",
                "Barangay 231", "Barangay 232", "Barangay 233", "Barangay 234", "Barangay 235",
                "Barangay 236", "Barangay 237", "Barangay 238", "Barangay 239", "Barangay 240",
                "Barangay 241", "Barangay 242", "Barangay 243", "Barangay 244", "Barangay 245",
                "Barangay 246", "Barangay 247", "Barangay 248", "Barangay 249", "Barangay 25",
                "Barangay 250", "Barangay 251", "Barangay 252", "Barangay 253", "Barangay 254",
                "Barangay 255", "Barangay 256", "Barangay 257", "Barangay 258", "Barangay 259",
                "Barangay 26", "Barangay 260", "Barangay 261", "Barangay 262", "Barangay 263",
                "Barangay 264", "Barangay 265", "Barangay 266", "Barangay 267", "Barangay 28",
                "Barangay 29", "Barangay 3", "Barangay 30", "Barangay 31", "Barangay 32",
                "Barangay 33", "Barangay 34", "Barangay 35", "Barangay 36", "Barangay 37",
                "Barangay 38", "Barangay 39", "Barangay 4", "Barangay 41", "Barangay 42",
                "Barangay 43", "Barangay 44", "Barangay 45", "Barangay 46", "Barangay 47",
                "Barangay 48", "Barangay 49", "Barangay 5", "Barangay 50", "Barangay 51",
                "Barangay 52", "Barangay 53", "Barangay 54", "Barangay 55", "Barangay 56",
                "Barangay 57", "Barangay 58", "Barangay 59", "Barangay 6", "Barangay 60",
                "Barangay 61", "Barangay 62", "Barangay 63", "Barangay 64", "Barangay 65",
                "Barangay 66", "Barangay 67", "Barangay 68", "Barangay 69", "Barangay 7",
                "Barangay 70", "Barangay 71", "Barangay 72", "Barangay 73", "Barangay 74",
                "Barangay 75", "Barangay 76", "Barangay 77", "Barangay 78", "Barangay 79",
                "Barangay 8", "Barangay 80", "Barangay 81", "Barangay 82", "Barangay 83",
                "Barangay 84", "Barangay 85", "Barangay 86", "Barangay 87", "Barangay 88",
                "Barangay 89", "Barangay 9", "Barangay 90", "Barangay 91", "Barangay 92",
                "Barangay 93", "Barangay 94", "Barangay 95", "Barangay 96", "Barangay 97",
                "Barangay 98", "Barangay 99"
            };


                foreach (string BrgyTondo in Barangays)
                {
                    BrgyTondo.ToUpper();
                    cmbBarangay.Items.Add(BrgyTondo);
                }
            }

            else if (cmbCity.Text.Equals("Valenzuela City"))
            {
                cmbBarangay.Items.Clear();
                cmbBarangay.Text = "";

                string[] Barangays = {
                "Arkong Bato", "Bagbaguin", "Balangkas", "Bignay", "Bisig", "Canumay East",
                "Canumay West", "Coloong", "Dalandanan", "Hen. T. De Leon", "Isla", "Karuhatan",
                "Lawang Bato", "Lingunan", "Mabolo", "Malanday", "Malinta", "Mapulang Lupa",
                "Marulas", "Maysan", "Palasan", "Parada", "Pariancillo Villa", "Paso De Blas",
                "Pasolo", "Poblacion", "Pulo", "Punturin", "Rincon", "Tagalag", "Ugong",
                "Viente Reales", "Wawang Pulo"
            };

                foreach (string BrgyValenzuela in Barangays)
                {
                    BrgyValenzuela.ToUpper();
                    cmbBarangay.Items.Add(BrgyValenzuela);
                }
            }
        }

        private void cmbBarangay_SelectedIndexChanged(object sender, EventArgs e) { postal_code.Enabled = true; }

        private void postal_code_TextChanged(object sender, EventArgs e)
        {
            if (postal_code.Text == string.Empty)
            {
                sign_up.Enabled = false;
                eula.Checked = false;
                required_postal.Text = "Required Field";
                required_postal.Visible = true;
                street_name.Enabled = false;
            }
            else
            {
                required_postal.Visible = false;
                street_name.Enabled = true;

                if (full_name.Text != string.Empty && cmbRegion.SelectedIndex >= 0 && cmbProvince.SelectedIndex >= 0 && cmbCity.SelectedIndex >= 0 && cmbBarangay.SelectedIndex >= 0 && postal_code.Text != string.Empty && street_name.Text != string.Empty && eula.Checked)
                {
                    sign_up.Enabled = true;
                }
                else
                {
                    sign_up.Enabled = false;

                }
            }
        }

        private void street_name_TextChanged(object sender, EventArgs e)
        {
            if (street_name.Text == string.Empty)
            {
                sign_up.Enabled = false;
                eula.Checked = false;
                required_street.Text = "Required Field";
                required_street.Visible = true;
            }
            else
            {
                required_street.Visible = false;
                if (full_name.Text != string.Empty && cmbRegion.SelectedIndex >= 0 && cmbProvince.SelectedIndex >= 0 && cmbCity.SelectedIndex >= 0 && cmbBarangay.SelectedIndex >= 0 && postal_code.Text != string.Empty && street_name.Text != string.Empty && eula.Checked)
                {
                    sign_up.Enabled = true;
                }
                else
                {
                    sign_up.Enabled = false;

                }
            }
        }

        private void postal_code_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
                required_postal.Text = "Not Accepting letters!";
                required_postal.Visible = true;
            }
            else
            {
                required_postal.Text = "Required Field";
            }
        }

        private void signup_username_TextChanged_1(object sender, EventArgs e)
        {
            if (signup_username.Text == string.Empty)
            {
                eula.Checked = false;
                error_username.Visible = true;
                error_username.Text = "Required field";
                next_btn.Enabled = false;

            }
            else
            {
                error_username.Text = "";
                string combinedEmail = signup_gmail.Text + defaultGmail.Text;
                if (isValidPhone(signup_phone.Text) && isValidEmail(combinedEmail))
                {
                    next_btn.Enabled = true;
                }

            }
        }

        private void signup_password_TextChanged_1(object sender, EventArgs e)
        {

            if (signup_password.Text.Length < 8 || signup_password.Text.Length > 14 || !signup_password.Text.Any(char.IsUpper) || !signup_password.Text.Any(char.IsLower) || signup_password.Text.Contains(" "))
            {
                if (signup_password.Text == string.Empty)
                {
                    error_password.Text = "Required field";
                    error_password.Visible = true;
                }
                else
                {
                    error_password.Text = "Password not strong enough!";
                    error_password.Visible = true;
                }

            }
            else
            {
                error_password.Text = "";

                string combinedEmail = signup_gmail.Text + defaultGmail.Text;
                if (isValidPhone(signup_phone.Text) && isValidEmail(combinedEmail))
                {
                    next_btn.Enabled = true;
                }
            }
        }

        private void signup_confirmpassword_TextChanged_1(object sender, EventArgs e)
        {

            if (signup_confirmpassword.Text != signup_password.Text)
            {
                if (signup_confirmpassword.Text == string.Empty)
                {
                    error_confirmpassword.Text = "Required Field";
                    error_confirmpassword.Visible = true;
                }
                else
                {
                    error_confirmpassword.Text = "Password not match!";
                    error_confirmpassword.Visible = true;
                }
            }
            else
            {
                error_confirmpassword.Visible = false;
                string combinedEmail = signup_gmail.Text + defaultGmail.Text;
                if (isValidPhone(signup_phone.Text) && isValidEmail(combinedEmail))
                {
                    next_btn.Enabled = true;
                }


            }
        }

        private void signup_gmail_TextChanged_1(object sender, EventArgs e)
        {
            string concatGmail = signup_gmail.Text + defaultGmail.Text;

            if (signup_gmail.Text == string.Empty || isValidEmail(concatGmail) == false)
            {


                if (isValidEmail(concatGmail) == false)
                {
                    error_gmail.Visible = true;
                    error_gmail.Text = "Invalid E-Mail format!";
                    next_btn.Enabled = false;
                }
                else
                {
                    error_gmail.Visible = true;
                    error_gmail.Text = "Required field";
                    next_btn.Enabled = false;
                }

            }

            else
            {
                error_gmail.Text = "";
                string combinedEmail = signup_gmail.Text + defaultGmail.Text;
                if (isValidPhone(signup_phone.Text) && isValidEmail(combinedEmail))
                {
                    next_btn.Enabled = true;
                }
            }
        }

        private void signup_phone_TextChanged_1(object sender, EventArgs e)
        {
            if (signup_phone.Text == string.Empty || isValidPhone(signup_phone.Text) == false)
            {
                eula.Checked = false;

                error_phone.Visible = true;
                error_phone.Text = "Invalid Phone Number!";
                next_btn.Enabled = false;

            }

            else
            {
                error_phone.Text = "";

                string combinedEmail = signup_gmail.Text + defaultGmail.Text;
                if (isValidPhone(signup_phone.Text) && isValidEmail(combinedEmail))
                {
                    next_btn.Enabled = true;
                }
            }
        }

        private void signup_show_password_Click_1(object sender, EventArgs e) { signup_password.UseSystemPasswordChar = signup_show_password.Checked ? false : true; }

        private void signup_show_confirm_password_Click(object sender, EventArgs e) { signup_confirmpassword.UseSystemPasswordChar = signup_show_confirm_password.Checked ? false : true; }

        private void back_prev_panel_Click(object sender, EventArgs e)
        {
            address_panel.Visible = false;
            next_btn.Visible = true;
            sign_up.Visible = false;
            terms_panel.Enabled = false;
            back_prev_panel.Visible = false;
            back_btn.Visible = true;
        }

        private void signup_phone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
                error_phone.Text = "Not Accepting letters!";
                error_phone.Visible = true;
            }
            else
            {
                error_phone.Text = "Invalid Phone Number!";

            }
        }

        private void eula_Click_1(object sender, EventArgs e)
        {
            sign_up.Enabled = full_name.Text != string.Empty && cmbRegion.SelectedIndex >= 0 && cmbProvince.SelectedIndex >= 0 && cmbCity.SelectedIndex >= 0 && cmbBarangay.SelectedIndex >= 0 && postal_code.Text != string.Empty && street_name.Text != string.Empty ? true : false;
        }
    }
}
