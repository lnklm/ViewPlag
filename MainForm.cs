using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;

namespace Studrate
{
    public partial class MainForm : Form
    {

        //SqlConnection connection;
        string connectionString = @"Data Source=SONDER\SQLEXPRESS;Initial Catalog = Lstudent; Integrated Security = True";

        //Data Source=SONDER\SQLEXPRESS;Initial Catalog = Lstudent; Integrated Security = True

        public MainForm()
        {
            InitializeComponent();

            //connectionString = ConfigurationManager.ConnectionStrings[""].ConnectionString;
        }


        private void insertStudent(SqlCommand command, Match match)
        {
            command.Parameters["@surname"].Value = match.Groups[1].Value;
            command.Parameters["@class"].Value = match.Groups[2].Value;
            command.Parameters["@gradebook"].Value = match.Groups[3].Value;
            command.Parameters["@name"].Value = match.Groups[4].Value;
            command.Parameters["@patronymic"].Value = match.Groups[5].Value;
            command.ExecuteScalar();
        }

        private int seekId(SqlCommand command, Match match)
        {
            command.Parameters["@class"].Value = match.Groups[2].Value;
            command.Parameters["@gradebook"].Value = match.Groups[3].Value;
            var id = command.ExecuteScalar();
            if (id != null)
                return Convert.ToInt32(id);
            else
                return -1;
        }



        private string textFile;

        private void button1_Click(object sender, EventArgs e)
        {
            string str = textFile;

            //string pattern = @"([A-Z]{2}\d{1,2})_\S+\/";
            //Regex regex = new Regex(pattern);

            //string replacement = "$1";
            //str = Regex.Replace(str, pattern, replacement);

            //string patternStr = @"([a-z]+)-([a-z]{2}\d{2})(\d{2})-([a-z]+)-?([a-z]+)?_[a-z]{2}(\d)\s\((\d{1,2})%\)";
            string patternStr = @"\/([a-z]+)-([a-z]{2}\d{2})(\d{2})-([a-z]+)-?([a-z]+)?_[a-z]{2}(\d)_\S+\/\s\((\d{1,2})%\)";
            //regex = new Regex(patternStr, RegexOptions.IgnoreCase);
            Regex regex = new Regex(patternStr, RegexOptions.IgnoreCase);

            string[] rows = str.Split('\n');

            //-----------------------------------------------------------
            //1-surname, 2-Class, 3-gradebook, 4-name, 5-patronymic, 6-lab, 7-%
            string query = "INSERT INTO Student (Name, Surname, Patronymic, Class, Gradebook) "
                         + " VALUES(@name, @surname, @patronymic, @class, @gradebook);";


            string q_seekId = "SELECT Id_student FROM Student" 
                + " WHERE Class = @class AND Gradebook = @gradebook;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlCommand c_seekId = new SqlCommand(q_seekId, connection))
            //using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                connection.Open();

                command.Parameters.Add("@name", SqlDbType.NVarChar);
                command.Parameters.Add("@surname", SqlDbType.NVarChar);
                command.Parameters.Add("@patronymic", SqlDbType.NVarChar);
                command.Parameters.Add("@class", SqlDbType.NVarChar);
                command.Parameters.Add("@gradebook", SqlDbType.NVarChar);

                c_seekId.Parameters.Add("@class", SqlDbType.NVarChar);
                c_seekId.Parameters.Add("@gradebook", SqlDbType.NVarChar);

                foreach (var oneRow in rows)
                {
                    Match match1 = regex.Match(oneRow);
                    Match match2 = regex.Match(oneRow).NextMatch();
                    if (match1.Success && match2.Success)
                    {
                        int id1 = seekId(c_seekId, match1);
                        int id2 = seekId(c_seekId, match2);

                        if (id1 < 0)
                        {
                            insertStudent(command, match1);
                            id1 = seekId(c_seekId, match1);
                        }
                        if (id2 < 0)
                        {
                            insertStudent(command, match2);
                            id2 = seekId(c_seekId, match2);
                        }

                        //string outputId = "First = " + id1 + ", Second = " + id2;
                        //MessageBox.Show(outputId);
                        richTextBox1.Text += id1 + "-" + match1.Groups[1].Value + "-" +
                                id2 + "-" + match2.Groups[1].Value + "\n";


                        //richTextBox1.Text += match1.Groups[1].Value + "-" +
                        //        match1.Groups[2].Value + "-" +
                        //        match1.Groups[3].Value + "-" +
                        //        match1.Groups[4].Value + "-" +
                        //        match1.Groups[5].Value + "-" +
                        //        match1.Groups[6].Value + "-" +
                        //        match1.Groups[7].Value + "\n";
                    }
                    else
                    {
                        if (!oneRow.Contains("http") && oneRow.Trim().Length != 0)
                        MessageBox.Show("Error row: " + oneRow, "Error");
                    }
                }
            }
            //MessageBox.Show("Connection closed");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "e:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            StreamReader sr = new StreamReader(myStream);
                            textFile = sr.ReadToEnd();
                            //richTextBox1.Text += textFile;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
    }
}
