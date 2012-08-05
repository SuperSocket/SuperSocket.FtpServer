using System;
using System.Collections.Generic;
using System.Text;
using Raccent.Ftp.FtpService.Membership;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class PASS : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (session.Logged)
            {
                session.SendResponse(Resource.AlreadyLoggedIn_230);
                return;
            }

            string password = requestInfo.Data;

            if (string.IsNullOrEmpty(password))
            {
                session.SendParameterError();
                return;
            }

            FtpUser user = null;

            AuthenticationResult result = AuthenticationResult.Success;

            //if (session.Context.IsAnonymous)
            //    user = new Anonymous();
            //else

            result = session.AppServer.FtpServiceProvider.Authenticate(session.Context.UserName, password, out user);

            if (result == AuthenticationResult.Success)
            {
                if (session.AppServer.Logon(session.Context, user))
                {
                    session.SendResponse(Resource.LoggedIn_230);
                    session.Logged = true;
                }
                else
                {
                    session.SendResponse(Resource.ReachedLoginLimit_421);
                    session.Close();
                }
            }
            else
            {
                session.SendResponse(Resource.AuthenticationFailed_530);
            }
        }

        #endregion
    }
}
