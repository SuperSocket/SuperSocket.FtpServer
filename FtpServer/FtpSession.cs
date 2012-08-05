using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Text;
using SuperSocket.Ftp.FtpService.Membership;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Ftp.FtpService
{
    public class FtpSession : AppSession<FtpSession>
    {
        public new FtpServer AppServer
        {
            get { return (FtpServer)base.AppServer; }
        }

        public FtpContext Context { get; private set; }

        public bool Logged { get; internal set; }

        protected override void OnInit()
        {
            base.OnInit();
            Context = new FtpContext();
            Context.TempDirectory = this.AppServer.FtpServiceProvider.GetTempDirectory(this.SessionID);
        }

        public override void Close()
        {
            CloseAllDataConnection();
            base.Close();
        }

        public int CurrentDataConnectionPort { get; set; }

        private void CloseAllDataConnection()
        {
            for (int i = m_DataConnections.Count - 1; i >= 0; i--)
            {
                var c = m_DataConnections[i];

                c.Close();

                lock (m_DataConnsSyncRoot)
                {
                    m_DataConnections.RemoveAt(i);
                }
            }
        }

        internal void CloseDataConnection(DataConnection conn)
        {
            if (conn == null)
                return;

            conn.Close();

            lock (m_DataConnsSyncRoot)
            {
                m_DataConnections.Remove(conn);
            }
        }

        protected override void OnSessionStarted()
        {
            Send(Resource.FTP_Welcome);
            base.OnSessionStarted();
        }

        public void SendParameterError()
        {
            Send(Resource.InvalidArguments_501);
        }

        private List<DataConnection> m_DataConnections = new List<DataConnection>();

        private object m_DataConnsSyncRoot = new object();

        internal IEnumerable<DataConnection> DataConnections
        {
            get { return m_DataConnections; }
        }

        internal DataConnection CurrentDataConnection { get; private set; }

        internal DataConnection CreateDataConnection()
        {
            DataConnection dataConn = new DataConnection(this, CurrentDataConnectionPort);

            lock (m_DataConnsSyncRoot)
            {
                CurrentDataConnection = dataConn;
                m_DataConnections.Add(dataConn);
            }

            return dataConn;
        }

        public override void HandleException(Exception e)
        {
            Send(Resource.UnknownError_450);
        }
    }
}
