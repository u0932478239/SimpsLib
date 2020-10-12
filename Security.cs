using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SimpsLib
{
    public class Security
    {
        [DllImport("kernel32.dll")]
        private static extern unsafe bool VirtualProtect(byte* lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);

        public static unsafe void Initialize()
        {
            string location = Path.Combine(Directory.GetCurrentDirectory(), Process.GetCurrentProcess().ProcessName + ".exe");

            Process[] pname = Process.GetProcessesByName("Fiddler");

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Programs\\Fiddler\\App.ico"))
            {
                MessageBox.Show("An HTTP Debugger has been detected on your computer, please remove it before using this app!", "SimpsLib", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + location + "\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                }).Dispose(); Environment.Exit(0);
            }
            else if (pname.Length != 0)
            {
                MessageBox.Show("An HTTP Debugger has been detected on your computer, please remove it before using this app!", "SimpsLib", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + location + "\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                }).Dispose();
                Environment.Exit(0);
            }

            if (File.Exists("C:\\Program Files (x86)\\HTTPDebuggerPro\\HTTPDebuggerBrowser.dll"))
            {
                MessageBox.Show("An HTTP Debugger has been detected on your computer, please remove it before using this app!", "SimpsLib", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + location + "\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                }).Dispose();
                Environment.Exit(0);
            }
            else if (File.Exists("D:\\Program Files (x86)\\HTTPDebuggerPro\\HTTPDebuggerBrowser.dll"))
            {
                MessageBox.Show("An HTTP Debugger has been detected on your computer, please remove it before using this app!", "SimpsLib", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + location + "\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                }).Dispose(); Environment.Exit(0);
            }
            else if (File.Exists("F:\\Program Files (x86)\\HTTPDebuggerPro\\HTTPDebuggerBrowser.dll"))
            {
                MessageBox.Show("An HTTP Debugger has been detected on your computer, please remove it before using this app!", "SimpsLib", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + location + "\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                }).Dispose(); Environment.Exit(0);
            }
        }

        public static string getMD5()
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), Process.GetCurrentProcess().ProcessName + ".exe")))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static bool checkMD5(string url)
        {
            WebClient wc = new WebClient();
            wc.Proxy = null;
            string websitemd5 = wc.DownloadString(url);
            string programmd5 = getMD5();
            if (websitemd5 == programmd5)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Hash check failed, exiting...", "SimpsLib/", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return false;
            }
        }

        public static string encryptString(string input)
        {
            string salt = "R4VDM0lCbkkyQDM3V09Lbg==";
            string keys = "UTY5UVd1RWJicTBfQzFtMGxxLk9KSm9PUWJXdWFDSUg=";
            string password = Encoding.Default.GetString(Convert.FromBase64String(keys));
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));
            byte[] iv = Encoding.ASCII.GetBytes(Encoding.Default.GetString(Convert.FromBase64String(salt)));
            string decrypted = b(key, input, iv);
            return decrypted;
        }

        public static string decryptString(string input)
        {
            string salt = "R4VDM0lCbkkyQDM3V09Lbg==";
            string keys = "UTY5UVd1RWJicTBfQzFtMGxxLk9KSm9PUWJXdWFDSUg=";
            string password = Encoding.Default.GetString(Convert.FromBase64String(keys));
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));
            byte[] iv = Encoding.ASCII.GetBytes(Encoding.Default.GetString(Convert.FromBase64String(salt)));
            string decrypted = a(key, input, iv);
            return decrypted;
        }

        private static string a(byte[] a, string b, byte[] c)
        {
            Aes encryptor = Aes.Create();
            encryptor.Mode = CipherMode.CBC;
            encryptor.Key = a;
            encryptor.IV = c;
            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);
            string plainText = String.Empty;
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(b);
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] plainBytes = memoryStream.ToArray();
                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }
            return plainText;
        }

        private static string b(byte[] a, string b, byte[] c)
        {
            Aes encryptor = Aes.Create();
            encryptor.Mode = CipherMode.CBC;
            encryptor.Key = a;
            encryptor.IV = c;
            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);
            byte[] plainBytes = Encoding.ASCII.GetBytes(b);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
            return cipherText;
        }
    }
}
