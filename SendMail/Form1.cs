using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Net.Mail;
using System.Configuration;
using System.Net.Configuration;
using System.Net;
using System.IO;
using System.Net.Mime;

namespace SendMail
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        string[] MailAddress;
        int interval;
        private void frmMain_Load(object sender, EventArgs e)
        {
            string val = ConfigurationManager.AppSettings["TimerInterval"];
            interval = Convert.ToInt32(val);
            tmr.Interval = interval;

            string path = "MailList.txt";
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            MailAddress = File.ReadAllLines(path);
            lblCout.Text = $"{MailAddress.Length} adet mail adresi algılandı";
            File.Delete(path);
            pBar.Maximum = MailAddress.Length;
        }

        string attachmentFilename;

        private void SendMail(string ToMailAdress)
        {
            Configuration oConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var mailSettings = oConfig.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;

            if (mailSettings != null)
            {
                string result = "Başarılı";
                try
                {
                    int port = mailSettings.Smtp.Network.Port;
                    string from = mailSettings.Smtp.From;
                    string host = mailSettings.Smtp.Network.Host;
                    string pwd = mailSettings.Smtp.Network.Password;
                    string uid = mailSettings.Smtp.Network.UserName;

                    var message = new MailMessage
                    {
                        From = new MailAddress(from)
                    };
                    message.To.Add(new MailAddress(ToMailAdress));
                    // message.CC.Add(new MailAddress(from));
                    message.Subject = txtSubject.Text;
                    message.IsBodyHtml = true;
                    message.Body = rtxtBody.Text;


                    // Attachment attachment = new Attachment(attachmentPath);
                    //message.Attachments.Add(attachment);

                    if (attachmentFilename != null)
                    {
                        Attachment attachment = new Attachment(attachmentFilename, MediaTypeNames.Application.Octet);
                        ContentDisposition disposition = attachment.ContentDisposition;
                        disposition.CreationDate = File.GetCreationTime(attachmentFilename);
                        disposition.ModificationDate = File.GetLastWriteTime(attachmentFilename);
                        disposition.ReadDate = File.GetLastAccessTime(attachmentFilename);
                        disposition.FileName = Path.GetFileName(attachmentFilename);
                        disposition.Size = new FileInfo(attachmentFilename).Length;
                        disposition.DispositionType = DispositionTypeNames.Attachment;
                        message.Attachments.Add(attachment);
                    }

                    var client = new SmtpClient
                    {
                        Host = host,
                        Port = port,
                        Credentials = new NetworkCredential(uid, pwd),
                        EnableSsl = true
                    };


                    client.Send(message);
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
                pBar.Value++;
                lblCout.Text = $"{MailAddress.Length - 1} adet mail adresi kaldı";
                Result(ToMailAdress, result);
                tmr.Enabled = true;
            }

        }

        private void Result(string ToMailAdress, string result)
        {
            string ResultPath = "Result.txt";
            if (!File.Exists(ResultPath))
            {
                File.Create(ResultPath).Close();
            }
            using (StreamWriter sw = File.AppendText(ResultPath))
            {
                sw.WriteLine($"{ToMailAdress} {result}");
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            txtSubject.Enabled = false;
            rtxtBody.Enabled = false;
            timer1.Start();
        }

        private void tmr_Tick(object sender, EventArgs e)
        {
            tmr.Enabled = false;
            if (MailAddress.Length > 0)
            {
                string address = MailAddress[0];
                lblToAddress.Text = address;
                SendMail(address);
                MailAddress = MailAddress.Where(x => x != MailAddress[0]).ToArray();
            }
            else
            {
                btnStart.Enabled = true;
                timer1.Enabled = false;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            string path = "MailList.txt";
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }

            foreach (string item in MailAddress)
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(item);
                }
            }
        }

        int countdown = 1;
        private void timer1_Tick(object sender, EventArgs e)
        {
            int result = ((interval / 1000) - countdown++);
            if (result > 0)
            {
                lblCountDown.Text = ((interval / 1000) - countdown++).ToString();
            }
            else
            {
                countdown = 1;
                tmr_Tick(sender, e);
            }
        }

        private void btnSelectFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                attachmentFilename = file.FileName;
            }
        }
    }
}
