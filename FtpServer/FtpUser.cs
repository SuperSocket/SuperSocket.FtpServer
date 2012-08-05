using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using SuperSocket.SocketBase;


namespace Raccent.Ftp.FtpService
{
    public class FtpUser
    {
        private object m_UserLock = new object();

        public int UserID { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        private long m_MaxSpace;

        public long MaxSpace { get; set; }

        private long m_UsedSpace;

        public long UsedSpace { get; set; }

        private int m_MaxConnections;

        public int MaxConnections
        {
            get { return m_MaxConnections; }
            set { m_MaxConnections = value; }
        }

        private int m_MaxUploadSpeed;

        public int MaxUploadSpeed { get; set; }

        private int m_MaxDownloadSpeed;

        public int MaxDownloadSpeed { get; set; }


        public string Root { get; set; }

        private int m_ConnectionCount = 0;

        public int ConnectionCount
        {
            get { return m_ConnectionCount; }
            set { m_ConnectionCount = value; }
        }

        public bool IncreaseConnection()
        {
            if (m_MaxConnections > 0)
            {
                lock (m_UserLock)
                {
                    if (m_ConnectionCount >= m_MaxConnections)
                        return false;
                    else
                    {
                        m_ConnectionCount++;
                        return true;
                    }
                }
            }
            else
            {
                return true;
            }
        }

        public int DecreaseConnection()
        {
            lock (m_UserLock)
            {
                m_ConnectionCount--;
                return m_ConnectionCount;
            }
        }

        public void ChangeUsedSpace(long space)
        {
            lock (m_UserLock)
            {
                m_UsedSpace += space;
            }
        }
    }
}
