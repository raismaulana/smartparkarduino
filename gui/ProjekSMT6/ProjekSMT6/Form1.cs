using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data;
using MySql.Data.MySqlClient;
using Data;
using System.Text.RegularExpressions;

namespace ProjekSMT6
{
    public partial class Form1 : Form
    {
        string dataKirim = null;
        string dataTerima = null;
        delegate void stringInvoke(string text, string lbl);
        public void TampilKTP()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
            var dbCon = DBConnection.Instance();
            dbCon.DatabaseName = "smartparking";
            if (dbCon.IsConnect() == true)
            {
                string jumlah = null;
                string query = "SELECT COUNT(no_ktp) FROM ktp";
                var cmd = new MySqlCommand(query, dbCon.Connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    jumlah = reader.GetString(0);
                }
                reader.Close();
                int jml = int.Parse(jumlah);
                query = "SELECT no_ktp,nama_ktp FROM ktp";
                cmd = new MySqlCommand(query, dbCon.Connection);
                reader = cmd.ExecuteReader();
                int i = 0;
                string[,] data = new string[jml,2];
                while (reader.Read())
                {
                    data[i, 0] = reader.GetString(0);
                    data[i, 1] = reader.GetString(1);
                    i++;
                }
                reader.Close();
                for (i = 0; i < jml; i++)
                {
                    dataGridView1.Rows.Add(data[i,0],data[i,1]);
                }
                dbCon.Close();
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
             
            if (!serialPort1.IsOpen)
            {
                int baudrate = 9600;
                if (comboBox1.SelectedIndex == -1){}
                else
                {
                    serialPort1.PortName = comboBox1.SelectedItem.ToString();
                }
                serialPort1.BaudRate = baudrate;
                try
                {             
                    serialPort1.Open();
                    TampilKTP();
                    checkBox1.Visible = true;
                    checkBox2.Visible = true;
                    textBox2.Visible = true;
                    button4.Visible = true;
                    label13.Visible = true;
                }
                catch
                {
                    MessageBox.Show("There was an error. Please make sure that the correct port was selected, and the device, plugged in.");
                }

            }
            else
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
                serialPort1.Close();
                timer2.Enabled = false;
                timer3.Enabled = false;
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox1.Visible = false;
                checkBox2.Visible = false;
                textBox2.Visible = false;
                button4.Visible = false;
                label13.Visible = false;
            }

        }

        private void ComboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            //menghapus nilai yang ada pada combobox
            comboBox1.Items.Clear();
            //mengambil ulang port yang tersedia
            string[] ports = SerialPort.GetPortNames();
            //memasukan kembali nilai port yang telah diambil
            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                label1.Text = "Status: Terputus";
                button1.Text = "Hubungkan";
            } else
            {
                label1.Text = "Status: Terhubung";
                button1.Text = "Putuskan";
            }
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //timer3.Enabled = false;
            //timer2.Enabled = true;
        }
        private void Timer2_Tick(object sender, EventArgs e)
        {
            dataKirim = "daftar";
            if (serialPort1.IsOpen == true)
            {
                serialPort1.WriteLine(dataKirim);
            }
        
            dataKirim = "";

        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            dataTerima = serialPort1.ReadLine();
            string[] data = dataTerima.Split('~');
            if (data[0] == "ir")
            {
                SetText(data[1], "label2");
            }
            else if (data[0] == "kartu")
            {
                SetText(data[1], "label7");
            }
            else if(data[0] == "gerbang")
            {
                SetText(data[2], "label8");
            }
            
            dataTerima = "";
        }

        private void SetText(string text, string lbl)
        {
            if (lbl == "label2")
            {
                if (this.label2.InvokeRequired)
                {
                    stringInvoke d = new stringInvoke(SetText);
                    this.Invoke(d, new object[] { text, lbl });
                }
                else
                {
                    this.label2.Text = text;
                }
            }
            else if (lbl == "label7")
            {
                if (this.label7.InvokeRequired)
                {
                    stringInvoke d = new stringInvoke(SetText);
                    this.Invoke(d, new object[] { text, lbl });
                }
                else
                {
                    this.label7.Text = text;
                }
            }
            else if (lbl == "label8")
            {
                if (this.label8.InvokeRequired)
                {
                    stringInvoke d = new stringInvoke(SetText);
                    this.Invoke(d, new object[] { text, lbl });
                }
                else
                {
                    this.label8.Text = text;
                }
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            string no_ktp = label7.Text;
            no_ktp = Regex.Replace(no_ktp, @"\t|\n|\r", "");
            string nama_ktp = textBox1.Text;
            nama_ktp = Regex.Replace(nama_ktp, @"\t|\n|\r", "");
            if (no_ktp != "" && nama_ktp != "")
            {
                string pesan = string.Empty;
                foreach (DataGridViewRow Datarow in dataGridView1.Rows)
                {
                    pesan = Datarow.Cells[0].Value.ToString();
                    if (pesan.Equals(no_ktp))
                    {
                        pesan = "true";
                        break;
                    }
                    pesan = "";
                }
                if (string.IsNullOrEmpty(pesan))
                {
                    var dbCon = DBConnection.Instance();
                    dbCon.DatabaseName = "smartparking";
                    if (dbCon.IsConnect())
                    {
                        string query = "INSERT INTO ktp (no_ktp, nama_ktp) VALUES ('" + no_ktp + "','" + nama_ktp + "')";
                        var cmd = new MySqlCommand(query, dbCon.Connection);
                        cmd.ExecuteNonQuery();
                    }
                    dbCon.Close();
                    TampilKTP();
                } else
                {
                    MessageBox.Show("KTP SUDAH TERDAFTAR");
                }
            }
            else
            {
                MessageBox.Show("data tidak boleh kosong");
            }
            label7.Text = "";
            textBox1.Text = "";
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            string no_ktp = textBox2.Text;
            if (no_ktp != "")
            {
                
                var dbCon = DBConnection.Instance();
                dbCon.DatabaseName = "smartparking";
                if (dbCon.IsConnect())
                {
                    string query = "DELETE FROM ktp WHERE no_ktp='"+no_ktp+"'";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.ExecuteNonQuery();
                }
                dbCon.Close();
                TampilKTP();
            }
            else
            {
                MessageBox.Show("data tidak boleh kosong");
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox2.Checked = false;
            if (checkBox1.Checked == true)
            {
                checkBox1.Text = "MONITORING AKTIF";
                timer3.Enabled = true;
                

            }
            else
            {
                checkBox1.Text = "MONITORING TIDAK AKTIF";
                label2.Text = "";
                timer3.Enabled = false;
            }
        }

        private void Timer3_Tick(object sender, EventArgs e)
        {
            dataKirim = "1";
            if (serialPort1.IsOpen == true)
            {
            serialPort1.WriteLine(dataKirim);
            }
            dataKirim = "";
            string info = label2.Text;
            info = Regex.Replace(info, @"\t|\n|\r", "");
            string[] datas = info.Split(':');
            string[] a = new string[4];
            int i = 0;
            foreach (string data in datas)
            { 
                a[i] = data;
                i++;
            }
            
            if (a[0] == "isi")
            {
                label3.BackColor = Color.Red;
            }
            else
            {
                label3.BackColor = Color.LimeGreen;
            }

            if (a[1] == "isi")
            {
                label4.BackColor = Color.Red;
            }
            else
            {
                label4.BackColor = Color.LimeGreen;
            }

            if (a[2] == "isi")
            {
                label5.BackColor = Color.Red;
            }
            else
            {
                label5.BackColor = Color.LimeGreen;
            }

            if (a[3] == "isi")
            {
                label6.BackColor = Color.Red;
            }
            else
            {
                label6.BackColor = Color.LimeGreen;
            }
            i = 0;
            int j = 0;
            while (i < 4)
            {
                if (a[i] == "kosong")
                {
                    j = j+1;
                }
                i++;
            }
            label10.Text = j.ToString();
            
            string lbl7 = label7.Text.ToString();
            if (string.IsNullOrEmpty(lbl7))
            {
                
            } else
            {
                lbl7 = Regex.Replace(lbl7, @"\t|\n|\r", "");
                
                string pesan = string.Empty;
                foreach (DataGridViewRow Datarow in dataGridView1.Rows)
                {
                    pesan = Datarow.Cells[0].Value.ToString();
                    if (pesan.Equals(lbl7))
                    {
                        pesan = "true";
                        break;
                    }
                    pesan = "";
                }
                
                if (pesan == "true")
                {
                    serialPort1.WriteLine("true");
                    string lbl8 = label8.Text.ToString();
                    string keterangan = null;
                    keterangan = Regex.Replace(lbl8, @"\t|\n|\r", "");
                    if (keterangan == "i")
                    {
                        var dbCon = DBConnection.Instance();
                        dbCon.DatabaseName = "smartparking";
                        if (dbCon.IsConnect())
                        {
                            string query = "INSERT INTO `log` (`ktp_no_ktp`, `waktu_log`, `status_log`) VALUES ('"+lbl7+"', CURRENT_TIMESTAMP, 'masuk');";
                            var cmd = new MySqlCommand(query, dbCon.Connection);
                            cmd.ExecuteNonQuery();
                        }
                        dbCon.Close();
                    }
                    else if(keterangan == "o")
                    {
                        var dbCon = DBConnection.Instance();
                        dbCon.DatabaseName = "smartparking";
                        if (dbCon.IsConnect())
                        {
                            string query = "INSERT INTO `log` (`ktp_no_ktp`, `waktu_log`, `status_log`) VALUES ('" +lbl7 + "', CURRENT_TIMESTAMP, 'keluar');";
                            var cmd = new MySqlCommand(query, dbCon.Connection);
                            cmd.ExecuteNonQuery();
                        }
                        dbCon.Close();
                    } else
                    {

                    }
                    keterangan = string.Empty;
                }
                else if (string.IsNullOrEmpty(pesan))
                {
                    serialPort1.WriteLine("false");
                }
                pesan = "";
                while (!string.IsNullOrEmpty(label7.Text.ToString()) && !string.IsNullOrEmpty(label8.Text.ToString()))
                {
                    label8.Text = string.Empty;
                    label7.Text = string.Empty;
                }
            }
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            label7.Text = string.Empty;
            checkBox1.Checked = false;
            if (checkBox2.Checked == true)
            {
                timer2.Enabled = true;
                textBox1.Visible = true;
                button3.Visible = true;
                checkBox1.Visible = false;
            }
            else if (checkBox2.Checked == false)
            {
                timer2.Enabled = false;
                textBox1.Visible = false;
                button3.Visible = false;
                checkBox1.Visible = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

namespace Data { 
    public class DBConnection
    {
        private DBConnection()
        {
        }

        private string databaseName = string.Empty;
        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }

        public string Password { get; set; }
        private MySqlConnection connection = null;
        public MySqlConnection Connection
        {
            get { return connection; }
        }

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(databaseName))
                    return false;
                string connstring = string.Format("Server=localhost; database={0}; UID=root; password=", databaseName);
                connection = new MySqlConnection(connstring);
                connection.Open();
            } else
            {
                string connstring1 = string.Format("Server=localhost; database={0}; UID=root; password=", databaseName);
                connection = new MySqlConnection(connstring1);
                connection.Open();
            }
            

            return true;
            
        }

        public void Close()
        {
            connection.Close();
        }
    }
}