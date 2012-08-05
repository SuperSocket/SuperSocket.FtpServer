using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class PBSZ : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            session.Context.ResetState();
            session.SendResponse(Resource.PBSZ_200);
        }

        #endregion
    }
}
