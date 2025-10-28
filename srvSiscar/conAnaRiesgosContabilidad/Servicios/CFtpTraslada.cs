using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace conAnaRiesgosContabilidad
{
    class CFtpTraslada
    {
        private string host = null;
        private string user = null;
        private string pass = null;
        private FtpWebRequest ftpRequest = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;

        /* Construct Object */
        public CFtpTraslada(string hostIP, string userName, string password)
        {
            host = hostIP; user = userName; pass = password;
        }

        public string upload(string remoteFile, string localFile)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpRequest.GetRequestStream();
                /* Open a File Stream to Read the File for Upload */
                FileStream localFileStream = new FileStream(localFile, FileMode.Open);
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                try
                {
                    /*
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    }
                    */
                }
                catch (Exception ex)
                {
                    //EventLog.WriteEntry("wsSISCAR", string.Format("Error en CFtpTraslada.upload while {0}", ex.Message), //EventLogEntryType.Error, 234);
                    //Console.WriteLine(ex.ToString());
                }
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpRequest = null;
                return "0";
            }
            catch (Exception ex)
            {
                //EventLog.WriteEntry("wsSISCAR", string.Format("Error en CFtpTraslada.upload {0}", ex.Message), //EventLogEntryType.Error, 234);
                //Console.WriteLine(ex.ToString());
            }
            return "1";
        }
    }
}
