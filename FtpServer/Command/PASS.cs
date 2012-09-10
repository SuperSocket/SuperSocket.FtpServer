using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Ftp.FtpService.Membership;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class PASS : FtpCommandBase
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (session.Logged)
            {
                session.Send(FtpCoreResource.AlreadyLoggedIn_230);
                return;
            }

            string password = requestInfo.Body;

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
                    session.Send(FtpCoreResource.LoggedIn_230);
                    session.Logged = true;
                }
                else
                {
                    session.Send(FtpCoreResource.ReachedLoginLimit_421);
                    session.Close();
                }
            }
            else
            {
                session.Send(FtpCoreResource.AuthenticationFailed_530);
            }
        }

        #endregion
    }
}
