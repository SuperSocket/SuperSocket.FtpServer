using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class RETR : FtpCommandBase
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            string filename = requestInfo.Body;

            if (string.IsNullOrEmpty(filename))
            {
                session.SendParameterError();
                return;
            }

            DataConnection dataConn = session.DataConnection;

            try
            {
                if (dataConn.RunDataConnection().Wait(60 * 1000))
                {
                    session.Send(FtpCoreResource.DataConnectionAccepted_150);
                    if (session.AppServer.FtpServiceProvider.ReadFile(session.Context, filename, dataConn.GetStream(session.Context)))
                        session.Send(FtpCoreResource.DataTransferComplete_226);
                    else
                        session.Send(session.Context.Message);
                }
                else
                {
                    session.Send(FtpCoreResource.DataConnectionCannotOpen_420);
                }
            }
            catch (SocketException)
            {
                session.Send(FtpCoreResource.DataConnectionError_426);
            }
            catch (Exception e)
            {
                session.Logger.Error(e);
                session.Send(FtpCoreResource.InputFileError_551, filename);
            }
            finally
            {
                session.CloseDataConnection();
            }
        }

        #endregion
    }
}
