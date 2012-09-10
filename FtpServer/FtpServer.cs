using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Linq;
using System.Resources;
using SuperSocket.Common;
using SuperSocket.Ftp.FtpService.Membership;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService
{
    public class FtpServer : AppServer<FtpSession>
    {
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

        private bool SetupFtpProvider(IRootConfig rootConfig, IServerConfig config)
        {
            var providerType = config.Options.GetValue("ftpProviderType");
            var providerTypeName = config.Options.GetValue("ftpProviderName");

            if (string.IsNullOrEmpty(providerType) && string.IsNullOrEmpty(providerTypeName))
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("Either ftpProviderType or ftpProviderName is required!");

                return false;
            }

            if (!string.IsNullOrEmpty(providerType) && !string.IsNullOrEmpty(providerTypeName))
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("You cannot have both ftpProviderType and ftpProviderName configured!");

                return false;
            }

            if (!string.IsNullOrEmpty(providerTypeName))
            {
                var providers = rootConfig.GetChildConfig<TypeProviderCollection>("ftpProviders");

                if (providers == null || providers.Count == 0)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error("Configuration node ftpProviders is required!");

                    return false;
                }

                var typeProvider = providers.OfType<TypeProvider>().FirstOrDefault(p =>
                        providerTypeName.Equals(p.Name, StringComparison.OrdinalIgnoreCase));

                if (typeProvider == null)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.ErrorFormat("The ftp provider {0} was not found!", providerTypeName);

                    return false;
                }

                providerType = typeProvider.Type;
            }


            Type realProviderType;

            try
            {
                realProviderType = Type.GetType(providerType, true);
                FtpServiceProvider = (FtpServiceProviderBase)Activator.CreateInstance(realProviderType);
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(e);

                return false;
            }

            return FtpServiceProvider.Init(this, config);
        }

        private bool SetupResource(IServerConfig config)
        {
            return true;
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            if (!SetupFtpProvider(rootConfig, config))
                return false;

            if (!SetupResource(config))
                return false;

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
