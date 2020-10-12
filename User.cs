using System;
using System.IO;
using System.Management;
using System.Net;
using System.Windows.Forms;

namespace SimpsLib
{
    public class User
    {
        public static string getIP()
        {
            if(!hasInternet())
            {
                MessageBox.Show("This application requires an internet connection, please verify your internet connection!", "SimpsLib/", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            string ip;
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            request.Proxy = null;
            WebResponse response = request.GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());
            ip = stream.ReadToEnd();
            stream.Close();
            response.Close();
            int first = ip.IndexOf("Address: ") + 9;
            int last = ip.LastIndexOf("");
            ip = ip.Substring(first, last - first);
            return ip.Replace("</body></html>", "");
        }

        public static bool hasInternet()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Proxy = null;
                    using (webClient.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static string getHWID()
        {
            string drive = "C";
            if (drive == string.Empty)
            {
                foreach (DriveInfo compDrive in DriveInfo.GetDrives())
                {
                    if (compDrive.IsReady)
                    {
                        drive = compDrive.RootDirectory.ToString();
                        break;
                    }
                }
            }
            if (drive.EndsWith(":\\"))
            {
                drive = drive.Substring(0, drive.Length - 2);
            }
            string volumeSerial = getVolumeSerial(drive);
            string cpuID = getCPUID();
            return cpuID.Substring(13) + cpuID.Substring(1, 4) + volumeSerial + cpuID.Substring(4, 4);
        }

        private static string getVolumeSerial(string drive)
        {
            ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            disk.Get();
            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();
            return volumeSerial;
        }

        private static string getCPUID()
        {
            string cpuInfo = "";
            ManagementClass managClass = new ManagementClass("win32_processor");
            ManagementObjectCollection managCollec = managClass.GetInstances();
            foreach (ManagementObject managObj in managCollec)
            {
                if (cpuInfo == "")
                {
                    cpuInfo = managObj.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return cpuInfo;
        }

        public static string getUSERNAME()
        {
            return Environment.UserName;
        }

        public static string getPCNAME()
        {
            return Environment.MachineName;
        }

        public static string getCurrentDirectory()
        {
            return Environment.CurrentDirectory;
        }

        public static void deleteFileSilent(string fileName)
        {
            if(File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }
}
