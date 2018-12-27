using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dlinktester
{
    public partial class settings : Form
    {
        public settings()
        {
            InitializeComponent();
        }

        public string appdata;
        public string[] settingsdata;

        private void button2_Click(object sender, EventArgs e)
        {
            Form settingshelp = new settingshelp();
            settingshelp.Show();
        }

        private void settings_Load(object sender, EventArgs e)
        {
            appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\";
            if(!File.Exists(appdata + "bruitsettings.dat"))
            {
                File.Create(appdata + "bruitsettings.dat").Close(); ;
            }
            string setting = File.ReadAllText(appdata + "bruitsettings.dat");
            settingsdata = setting.Split(':');
            try
            {
                textBox1.Text = settingsdata[0];
                textBox2.Text = settingsdata[1];
            }
            catch
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string final = textBox1.Text + ":" + textBox2.Text;
            File.WriteAllText(appdata + "bruitsettings.dat", final);
            this.Close();
        }
    }
}
