using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
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
        
        public override bool Init(FtpServer server, IServerConfig config)
        {
            if (!base.Init(server, config))
                return false;

            var userSettingFile = config.Options.GetValue("userSetting");

            if (string.IsNullOrEmpty(userSettingFile))
            {
                server.Logger.Error("No user setting file defined!");
                return false;
            }

            List<FtpUser> users;

            try
            {
                users = XmlSerializerUtil.Deserialize<List<FtpUser>>(server.GetFilePath(userSettingFile));
            }
            catch (Exception e)
            {
                AppServer.Logger.Error("Failed to deserialize FtpUser list file!", e);
                return false;
            }

            m_UserDict = users.ToDictionary(u => u.UserName, u => u, StringComparer.OrdinalIgnoreCase);

            return true;
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
