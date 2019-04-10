using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MD5Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_openFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.ShowDialog();
            if (!File.Exists(openFile.FileName))
                return;
            txt_fileName.Text = openFile.FileName.Split('\\')[openFile.FileName.Split('\\').Length - 1];
            txt_Md5.Text= GetMD5HashFromFile(openFile.FileName);
        }

        public string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
               
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("文件处理异常,程序即将退出。");
                return "";
                //throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        /// <summary>
        /// 从ftp服务器下载文件的功能
        /// </summary>
        /// <returns></returns>
        public string Download()
        {
            try
            {
                string FTPCONSTR = txt_ip.Text;
                string FTPUSERNAME = txt_user.Text;
                string FTPPASSWORD = txt_pwd.Text;
                string ftpfilepath = txt_remote.Text;

                var filePath = AppDomain.CurrentDomain.BaseDirectory;
                filePath = filePath.Replace("我的电脑\\", "");
                string newFileName = filePath + ftpfilepath.Split('/')[ftpfilepath.Split('/').Length - 1];
                if (File.Exists(newFileName))
                {
                    File.Delete(newFileName);
                }
                
                ftpfilepath = ftpfilepath.Replace("\\", "/");
                string url = FTPCONSTR + (ftpfilepath.StartsWith("/")?ftpfilepath:"/"+ftpfilepath);
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
               
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                FileStream outputStream = new FileStream(newFileName, FileMode.Create);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
                return newFileName;
            }
            catch (Exception ex)
            {
                //errorinfo = string.Format("因{0},无法下载", ex.Message);
                MessageBox.Show(ex.Message);
                return "";
            }
        }

        private void btn_ftp_Click(object sender, EventArgs e)
        {
            if (!CheckData())
                return;
            var fileName = Download();
            var md5 = GetMD5HashFromFile(fileName);
            if (md5.Equals(txt_Md5.Text)) {
                MessageBox.Show(string.Format(@"本地文件{0}与FTP上的文件具有相同的MD5值({1}),程序将不做任何处理并结束！", txt_fileName.Text, md5));
                return;
            }
            MessageBox.Show("程序处理");
        }

        private bool CheckData() {
            if (string.IsNullOrEmpty(txt_ip.Text)) {
                MessageBox.Show("请输入FTP IP地址");
                txt_ip.Focus();
                return false;
            }
            
            if (string.IsNullOrEmpty(txt_user.Text))
            {
                MessageBox.Show("请输入FTP账号");
                txt_user.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txt_pwd.Text))
            {
                MessageBox.Show("请输入FTP密码");
                txt_pwd.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txt_remote.Text))
            {
                MessageBox.Show("请输入FTP远程文件路径");
                txt_remote.Focus();
                return false;
            }
            return true;
        }
    }
}
