using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class RETR : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            string filename = requestInfo.Data;

            if (string.IsNullOrEmpty(filename))
            {
                session.SendParameterError();
                return;
            }

            if (!session.Context.User.IncreaseConnection())
            {
                session.SendResponse(Resource.ReachedLoginLimit_421);
                return;
            }

            DataConnection dataConn = session.CreateDataConnection();

            try
            {
                if (dataConn.RunDataConnection())
                {
                    session.SendResponse(Resource.DataConnectionAccepted_150);
                    if (session.AppServer.FtpServiceProvider.ReadFile(session.Context, filename, dataConn.GetStream(session.Context)))
                        session.SendResponse(Resource.DataTransferComplete_226);
                    else
                        session.SendResponse(session.Context.Message);
                }
                else
                {
                    session.SendResponse(Resource.DataConnectionCannotOpen_420);
                }
            }
            catch (SocketException)
            {
                session.SendResponse(Resource.DataConnectionError_426);
            }
            catch (Exception e)
            {
                session.Logger.Error(e);
                session.SendResponse(Resource.InputFileError_551, filename);
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
