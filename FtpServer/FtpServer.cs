using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Raccent.Ftp.FtpService.Console;
using Raccent.Ftp.FtpService.Membership;
using Raccent.Ftp.FtpService.Storage;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System.IO;
using System.Reflection;
using SuperSocket.SocketBase.Protocol;
using System.Resources;

namespace Raccent.Ftp.FtpService
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

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, IRequestFilterFactory<StringRequestInfo> requestFilterFactory)
        {
            return base.Setup(rootConfig, config, socketServerFactory, requestFilterFactory);
        }

        public FtpServiceProviderBase FtpServiceProvider { get; private set; }

        public override bool IsReady
        {
            get { return (FtpServiceProvider != null && FtpServiceProvider != null); }
        }

        //protected override ConsoleHostInfo ConsoleHostInfo
        //{
        //    get
        //    {
        //        return new ConsoleHostInfo
        //        {
        //            ServiceInstance = new FtpManager(this),
        //            ServiceContracts = new Type[]
        //            {
        //                typeof(IUserManager),
        //                typeof(IServerManager),
        //                typeof(IStatusReporter)
        //            }
        //        };
        //    }
        //}

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

        protected override void OnAppSessionClosed(object sender, AppSessionClosedEventArgs<FtpSession> e)
        {
            var session = e.Session;
            this.FtpServiceProvider.ClearTempDirectory(session.Context);
            this.RemoveOnlineUser(session, session.Context.User);
            base.OnAppSessionClosed(sender, e);
        }
    }
}
