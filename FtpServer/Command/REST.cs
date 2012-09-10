using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Ftp.FtpCommon;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class REST : FtpCommandBase
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
            if (!session.Logged)
                return;

            long offset = requestInfo.Body.ToLong();

            session.Context.Offset = offset;

            session.Send(FtpCoreResource.RestartOk_350, offset);
        }

        #endregion
    }
}
