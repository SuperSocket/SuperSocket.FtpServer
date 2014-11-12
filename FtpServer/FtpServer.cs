using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using SuperSocket.Common;
using SuperSocket.Ftp.FtpService.Command;
using SuperSocket.Ftp.FtpService.Membership;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService
{
    public class FtpServer : AppServer<FtpSession>
    {
        internal string FeaturesResponse { get; private set; }

        public FtpServer()
        {

        }

        public override void Stop()
        {
            base.Stop();

            lock (m_SyncRoot)
                m_DictOnlineUser.Clear();
        }

        public FtpServiceProviderBase FtpServiceProvider { get; private set; }

        internal int MaxFailedLogInTimes { get; private set; }

        private bool SetupFtpProvider(IRootConfig rootConfig, IServerConfig config)
        {
            var providerType = config.Options.GetValue("ftpProviderType");
            var providerTypeName = config.Options.GetValue("ftpProviderName");

            if (string.IsNullOrEmpty(providerType) && string.IsNullOrEmpty(providerTypeName))
            {
                try
                {
                    var ftpProvider = new XmlFtpProvider();

                    if (!ftpProvider.Init(this, config))
                        return false;

                    FtpServiceProvider = ftpProvider;
                    return true;
                }
                catch (Exception e)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error(e);

                    return false;
                }
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
            var featureList = TextEncoding.CodePage == Encoding.UTF8.CodePage
                    ? m_ExtCommands.Select(c => " " + c).Union(new string[] { " UTF-8" }).ToArray()
                    : m_ExtCommands.Select(c => " " + c).ToArray();

            FeaturesResponse = string.Format(FtpCoreResource.FeaturesOk_221, string.Join("\r\n", featureList));
            return true;
        }

        private string[] m_ExtCommands;

        protected override bool SetupCommands(Dictionary<string, ICommand<FtpSession, StringRequestInfo>> commandContainer)
        {
            if (!base.SetupCommands(commandContainer))
                return false;

            m_ExtCommands = commandContainer.Values.OfType<FtpCommandBase>().Where(c =>
                    c.IsExtCommand).Select(c => c.Name).ToArray();

            return true;
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            int maxFailedLogInTimes;

            if (!int.TryParse(config.Options.GetValue("maxFailedLogInTimes", "5"), out maxFailedLogInTimes))
            {
                Logger.ErrorFormat("Invalid configuration attribute 'maxFailedLogInTimes'.");
                return false;
            }

            MaxFailedLogInTimes = maxFailedLogInTimes;

            if (!SetupFtpProvider(rootConfig, config))
                return false;

            if (!SetupResource(config))
                return false;

            return true;
        }

        protected override void OnStarted()
        {
            var ftpProvider = FtpServiceProvider;

            if (ftpProvider != null)
                ftpProvider.OnStarted();
        }

        protected override void OnStopped()
        {
            var ftpProvider = FtpServiceProvider;

            if (ftpProvider != null)
                ftpProvider.OnStopped();
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
