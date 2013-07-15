using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.SocketBase;


namespace SuperSocket.Ftp.FtpService
{
    public class FtpContext
    {
        public FtpContext()
        {
            Charset = Encoding.ASCII;
        }

        private FtpUser m_User = new FtpUser();

        public FtpUser User
        {
            get { return m_User; }
            set { m_User = value; }
        }

        public string UserName { get; set; }

        public long UserID
        {
            get { return m_User.UserID; }
        }

        public Encoding Charset { get; set; }

        private long m_Offset = 0;

        public long Offset
        {
            get { return m_Offset; }
            set { m_Offset = value; }
        }

        private string m_CurrentPath = "/";

        public string CurrentPath
        {
            get { return m_CurrentPath; }
            set { m_CurrentPath = value; }
        }

        private long m_CurrentFolderID = 0;

        public long CurrentFolderID
        {
            get { return m_CurrentFolderID; }
            set { m_CurrentFolderID = value; }
        }

        private string m_RenameFor = string.Empty;

        public string RenameFor
        {
            get { return m_RenameFor; }
            set { m_RenameFor = value; }
        }

        private ItemType m_RenameItemType = ItemType.File;

        public ItemType RenameItemType
        {
            get { return m_RenameItemType; }
            set { m_RenameItemType = value; }
        }

        private string m_TempDirectory = string.Empty;

        public string TempDirectory
        {
            get { return m_TempDirectory; }
            set { m_TempDirectory = value; }
        }

        private TransferType m_TransferType = TransferType.I;

        public TransferType TransferType
        {
            get { return m_TransferType; }
            set { m_TransferType = value; }
        }

        private SslProtocols m_DataSecureProtocol = SslProtocols.None;

        public SslProtocols DataSecureProtocol
        {
            get { return m_DataSecureProtocol; }
            set { m_DataSecureProtocol = value; }
        }

        public void ChangeSpace(long size)
        {
            m_User.ChangeUsedSpace(size);
        }

        internal void ResetState()
        {
            m_Offset = 0;
        }

        public string Message { get; set; }

        public FtpStatus Status { get; set; }

        public void SetError(string error)
        {
            Message = error;
            Status = FtpStatus.Error;
        }

        public void SetError(string error, params object[] arguments)
        {
            Message = string.Format(error, arguments);
            Status = FtpStatus.Error;
        }
    }
}
