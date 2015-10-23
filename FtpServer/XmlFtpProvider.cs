using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.Ftp.FtpCommon;
using SuperSocket.Ftp.FtpService;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.Ftp.FtpService
{
    public class XmlFtpProvider : FtpServiceProviderBase
    {
        private Dictionary<string, FtpUser> m_UserDict = new Dictionary<string, FtpUser>(StringComparer.OrdinalIgnoreCase);

        private Timer m_UserConfigReadTimer;

        private const int c_UserFileReadInterval = 60 * 1000 * 5; // 5 minutes

        private DateTime m_LastUserFileLoadTime = DateTime.MinValue;

        private string m_UserConfigFile;
        
        public override bool Init(FtpServer server, IServerConfig config)
        {
            if (!base.Init(server, config))
                return false;

            var userSettingFile = config.Options.GetValue("userSetting");            

            if (string.IsNullOrEmpty(userSettingFile))
            {
                server.Logger.Error("No user setting file was not defined!");
                return false;
            }

            if (!Path.IsPathRooted(userSettingFile))
                userSettingFile = server.GetFilePath(userSettingFile);

            if (!File.Exists(userSettingFile))
            {
                AppServer.Logger.Error("The userSetting file cannot be found!");
                return false;
            }

            m_UserConfigFile = userSettingFile;

            ReadUserConfigFile();

            m_UserConfigReadTimer = new Timer(OnUserConfigReadTimerCallback, null, Timeout.Infinite, Timeout.Infinite);

            return true;
        }

        public override void OnStarted()
        {
            m_UserConfigReadTimer.Change(c_UserFileReadInterval, c_UserFileReadInterval);
            base.OnStarted();
        }

        public override void OnStopped()
        {
            m_UserConfigReadTimer.Change(Timeout.Infinite, Timeout.Infinite);
            base.OnStopped();
        }

        private void OnUserConfigReadTimerCallback(object state)
        {
            m_UserConfigReadTimer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                ReadUserConfigFile();
            }
            catch (Exception e)
            {
                AppServer.Logger.Error("Failed to ReadUserConfigFile.", e);
            }

            m_UserConfigReadTimer.Change(c_UserFileReadInterval, c_UserFileReadInterval);
        }

        private void ReadUserConfigFile()
        {
            if (!File.Exists(m_UserConfigFile))
                return;

            var lastFileWriteTime = File.GetLastWriteTime(m_UserConfigFile);

            if (lastFileWriteTime <= m_LastUserFileLoadTime)
                return;

            var users = XmlSerializerUtil.Deserialize<List<FtpUser>>(m_UserConfigFile);

            m_UserDict = users.ToDictionary(u => u.UserName, u => u, StringComparer.OrdinalIgnoreCase);

            m_LastUserFileLoadTime = lastFileWriteTime;
        }

        public override AuthenticationResult Authenticate(string username, string password, out FtpUser user)
        {
            lock (SyncRoot)
            {
                if (!m_UserDict.TryGetValue(username, out user))
                    return AuthenticationResult.NotExist;

                if ("*".Equals(user.Password))
                    return AuthenticationResult.Success;

                if (string.CompareOrdinal(password, user.Password) != 0)
                    return AuthenticationResult.PasswordError;

                return AuthenticationResult.Success;
            }
        }

        public override void UpdateUsedSpaceForUser(FtpUser user)
        {
            //Do nothing
        }

        public override List<FtpUser> GetAllUsers()
        {
            lock (SyncRoot)
            {
                return m_UserDict.Values.ToList();
            }
        }

        public override FtpUser GetFtpUserByID(long userID)
        {
            throw new NotImplementedException();
        }

        public override FtpUser GetFtpUserByUserName(string username)
        {
            lock (SyncRoot)
            {
                FtpUser user;
                m_UserDict.TryGetValue(username, out user);
                return user;
            }
        }

        public override bool UpdateFtpUser(FtpUser user)
        {
            //Do nothing
            return true;
        }

        public override CreateUserResult CreateFtpUser(FtpUser user)
        {
            //Do nothing
            return CreateUserResult.UnknownError;
        }

        public override bool ChangePassword(long userID, string password)
        {
            throw new NotImplementedException();
        }

        public override long GetUserRealUsedSpace(FtpContext context, string username)
        {
            string userStorageRoot = context.User.Root;
            return GetDirectorySize(userStorageRoot);
        }

        public override string GetTempDirectory(string sessionID)
        {
            return string.Empty;
        }

        public override void ClearTempDirectory(FtpContext context)
        {
            //Do nothing
        }
    }
}
