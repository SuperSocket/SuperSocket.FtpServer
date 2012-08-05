using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class PROT : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            if (session.SecureProtocol == SslProtocols.None)
            {
                session.SendResponse(Resource.ProtDisabled_431);
                return;
            }

            string level = requestInfo.Data;

            if (string.IsNullOrEmpty(level) || level.Length > 1)
            {
                session.SendParameterError();
                return;
            }

            switch (level[0])
            {
                case ('C'):
                case ('c'):
                    session.Context.DataSecureProtocol = SslProtocols.None;
                    break;
                case ('P'):
                case ('p'):
                    session.Context.DataSecureProtocol = session.SecureProtocol;
                    break;
                default:
                    session.SendResponse(Resource.ProtectionLevelUnknow_504);
                    return;
            }

            session.Context.ResetState();
            session.SendResponse(Resource.ProtOk_200);
        }

        #endregion
    }
}
