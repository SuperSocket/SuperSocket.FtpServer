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

            if (!session.Context.User.IncreaseConnection())
            {
                session.Send(Resource.ReachedLoginLimit_421);
                return;
            }

            DataConnection dataConn = session.CreateDataConnection();

            try
            {
                if (dataConn.RunDataConnection())
                {
                    session.Send(Resource.DataConnectionAccepted_150);
                    if (session.AppServer.FtpServiceProvider.ReadFile(session.Context, filename, dataConn.GetStream(session.Context)))
                        session.Send(Resource.DataTransferComplete_226);
                    else
                        session.Send(session.Context.Message);
                }
                else
                {
                    session.Send(Resource.DataConnectionCannotOpen_420);
                }
            }
            catch (SocketException)
            {
                session.Send(Resource.DataConnectionError_426);
            }
            catch (Exception e)
            {
                session.Logger.Error(e);
                session.Send(Resource.InputFileError_551, filename);
            }
            finally
            {
                session.Context.User.DecreaseConnection();
                session.CloseDataConnection(dataConn);
            }
        }

        #endregion
    }
}
