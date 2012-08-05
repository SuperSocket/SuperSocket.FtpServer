using System;
using System.Collections.Generic;
using System.Net;
using SuperSocket.Ftp.FtpService.Membership;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System.IO;
using System.Reflection;
using SuperSocket.SocketBase.Protocol;
using System.Resources;

namespace SuperSocket.Ftp.FtpService
{
    public class FtpServer : AppServer<FtpSession>
    {
        public ResourceManager Resource { get; private set; }

        public FtpServer()
            : base()
        {

        }

        public override void Stop()
        {
            base.Stop();

            lock (m_SyncRoot)
                m_DictOnlineUser.Clear();
        }

        public FtpServiceProviderBase FtpServiceProvider { get; private set; }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            return true;
        }


        private object m_SyncRoot = new object();

        private Dictionary<long, FtpUser> m_DictOnlineUser = new Dictionary<long, FtpUser>();

        public bool Logon(FtpContext context, FtpUser user)
        {
            FtpUser onlineUser = null;

            lock (m_SyncRoot)
            {
                m_DictOnlineUser.TryGetValue(user.UserID, out onlineUser);
            }

            if (onlineUser != null)
            {
                context.User = onlineUser;
                return onlineUser.IncreaseConnection();
            }
            else
            {
                user.IncreaseConnection();
                context.User = user;

                lock (m_SyncRoot)
                {
                    m_DictOnlineUser[user.UserID] = user;
                }

                return true;
            }
        }

        public void RemoveOnlineUser(FtpSession session, FtpUser user)
        {
            FtpUser onlineUser = null;

            lock (m_SyncRoot)
            {
                m_DictOnlineUser.TryGetValue(user.UserID, out onlineUser);
            }

            if (onlineUser != null)
            {
                if (onlineUser.DecreaseConnection() <= 0)
                {
                    lock (m_SyncRoot)
                    {
                        m_DictOnlineUser.Remove(user.UserID);
                    }

                    FtpServiceProvider.UpdateUsedSpaceForUser(user);
                }
            }
        }

        protected override void OnSessionClosed(FtpSession session, CloseReason reason)
        {
            base.OnSessionClosed(session, reason);
            this.FtpServiceProvider.ClearTempDirectory(session.Context);
            this.RemoveOnlineUser(session, session.Context.User);
        }
    }
}
