using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Text;
using SuperSocket.Ftp.FtpService.Membership;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

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

        internal int FailedLogInTimes { get; set; }

        protected override void OnInit()
        {
            base.OnInit();
            Context = new FtpContext();
            Context.TempDirectory = this.AppServer.FtpServiceProvider.GetTempDirectory(this.SessionID);
        }

        public override void Close()
        {
            if (DataConnection != null)
            {
                DataConnection.Close();
                DataConnection = null;
            }

            base.Close();
        }

        internal void CloseDataConnection()
        {
            var dataConnection = DataConnection;

            if (dataConnection == null)
                return;

            dataConnection.Close();

            DataConnection = null;
        }

        protected override void OnSessionStarted()
        {
            Send(FtpCoreResource.FTP_Welcome);
            base.OnSessionStarted();
        }

        public void SendParameterError()
        {
            Send(FtpCoreResource.InvalidArguments_501);
        }

        internal DataConnection DataConnection { get; set; }

        protected override void HandleException(Exception e)
        {
            Send(FtpCoreResource.UnknownError_450);
        }

        protected override void HandleUnknownRequest(StringRequestInfo requestInfo)
        {
            this.Send(FtpCoreResource.UnknownCommand_500);
        }
    }
}
