using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class MDTM : FtpCommandBase
    {
        public override bool IsExtCommand
        {
            get
            {
                return true;
            }
        }

        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            string filename = requestInfo.Body;

            if (string.IsNullOrEmpty(filename))
            {
                session.SendParameterError();
                return;
            }

            DateTime mdfTime = session.AppServer.FtpServiceProvider.GetModifyTime(session.Context, filename);

            if (session.Context.Status == FtpStatus.Error)
            {
                session.Send(session.Context.Message);
            }
            else
            {
                session.Send(string.Format(FtpCoreResource.FileOk_213, mdfTime.ToString("yyyyMMddhhmmss")));
            }
        }

        #endregion
    }
}
