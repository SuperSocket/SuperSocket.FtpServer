using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;


namespace SuperSocket.Ftp.FtpService
{
    class DataConnection
    {
        private static Random m_Random = new Random();

        private string m_Address = string.Empty;

        public string Address
        {
            get { return m_Address; }
            set { m_Address = value; }
        }

        private int m_Port = 0;

        public int Port
        {
            get { return m_Port; }
            set { m_Port = value; }
        }

        public Socket Client { get; private set; }

        private Socket m_Listener;

        public SslProtocols SecureProtocol { get; set; }

        private FtpSession m_Session;

        private bool m_IsClosed = false;

        private Stream m_Stream;

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Stream GetStream(FtpContext context)
        {
            if (m_Stream == null)
            {
                InitStream(context);
            }

            return m_Stream;
        }

        private void InitStream(FtpContext context)
        {
            switch (SecureProtocol)
            {
                case (SslProtocols.Tls):
                case (SslProtocols.Ssl3):
                case (SslProtocols.Ssl2):
                    SslStream sslStream = new SslStream(new NetworkStream(Client), false);
                    sslStream.AuthenticateAsServer(m_Session.AppServer.Certificate, false, SslProtocols.Default, true);
                    m_Stream = sslStream as Stream;
                    break;
                default:
                    m_Stream = new NetworkStream(Client);
                    break;
            }
        }
        
        public DataConnection(FtpSession session, Socket listenSocket, int port)
        {
            m_Session = session;
            m_Address = session.Config.Ip;
            SecureProtocol = session.Context.DataSecureProtocol;
            m_Listener = listenSocket;
            m_Port = port;
        }

        public Task RunDataConnection()
        {
            var taskSource = new TaskCompletionSource<bool>();
            SocketAsyncEventArgs acceptEventArgs = new SocketAsyncEventArgs();
            acceptEventArgs.UserToken = taskSource;
            acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(acceptEventArgs_Completed);
            if (!m_Listener.AcceptAsync(acceptEventArgs))
                ProcessAccept(acceptEventArgs);
            return taskSource.Task;
        }

        void acceptEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        void ProcessAccept(SocketAsyncEventArgs e)
        {
            this.Client = e.AcceptSocket;

            var taskSource = e.UserToken as TaskCompletionSource<bool>;

            try
            {
                InitStream(m_Session.Context);
                taskSource.SetResult(true);
            }
            catch (Exception exc)
            {
                taskSource.SetException(exc);
            }

            StopListener();
        }

        void StopListener()
        {
            if (m_Listener != null)
            {
                try
                {
                    m_Listener.Close();
                }
                catch (Exception)
                {
                }
                finally
                {
                    m_Listener = null;
                }
            }
        }

        private static Socket TryListenSocketPort(IPAddress address, int tryPort)
        {
            Socket listener = null;

            try
            {
                IPEndPoint endPoint = new IPEndPoint(address, tryPort);
                listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(endPoint);
                listener.Listen(1);
                return listener;
            }
            catch (Exception)
            {
                if (listener != null)
                {
                    try
                    {
                        listener.Close();
                    }
                    catch (Exception)
                    {

                    }
                }

                return null;
            }
        }

        internal static bool TryOpenDataConnection(FtpSession session, int port, out DataConnection dataConnection)
        {
            IPAddress ipAddress = session.LocalEndPoint.Address;
            var listenSocket = TryListenSocketPort(ipAddress, port);

            if (listenSocket != null)
            {
                dataConnection = new DataConnection(session, listenSocket, port);
                return true;
            }

            dataConnection = null;
            return false;
        }

        internal static bool TryOpenDataConnection(FtpSession session, out DataConnection dataConnection)
        {
            dataConnection = null;

            int tryPort = session.AppServer.FtpServiceProvider.GetRandomPort();
            int previousPort = tryPort;
            int tryTimes = 0;

            IPAddress ipAddress = session.LocalEndPoint.Address;

            while (true)
            {
                var listenSocket = TryListenSocketPort(ipAddress, tryPort);

                if (listenSocket != null)
                {
                    dataConnection = new DataConnection(session, listenSocket, tryPort);
                    return true;
                }

                tryTimes++;

                if (tryTimes > 5)
                {
                    return false;
                }

                tryPort = session.AppServer.FtpServiceProvider.GetRandomPort();

                if (previousPort == tryPort)
                {
                    return false;
                }
            }
        }

        private const string DELIM = " ";
        private const string NEWLINE = "\r\n";

        private static readonly CultureInfo m_DateTimeCulture = CultureInfo.GetCultureInfo("en-US");

        public void SendResponse(FtpContext context, List<ListItem> list)
        {
            Stream stream = GetStream(context);

            if (list == null || list.Count <= 0)
                return;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i].Permission);

                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append(DELIM);

                sb.Append("1");
                sb.Append(DELIM);
                sb.Append("user");
                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append("group");

                sb.Append(DELIM);

                sb.Append(GetFixedLength(list[i].Length));

                sb.Append(DELIM);

                sb.Append(GetListTimeString(list[i].LastModifiedTime));

                sb.Append(DELIM);

                sb.Append(list[i].Name);

                sb.Append(NEWLINE);

                byte[] data = context.Charset.GetBytes(sb.ToString());
                stream.Write(data, 0, data.Length);

                sb.Remove(0, sb.Length);
            }

            stream.Flush();
        }

        private string GetListTimeString(DateTime time)
        {
            if (time.Year == DateTime.Now.Year)
                return time.ToString("MMM dd hh:mm", m_DateTimeCulture);
            else
                return time.ToString("MMM dd yyyy", m_DateTimeCulture);
        }

        private string GetFixedLength(long length)
        {
            string size = length.ToString();

            size = size.PadLeft(11, ' ');

            return size;
        }

        public virtual void Close()
        {
            StopListener();

            if (Client != null && !m_IsClosed)
            {
                try
                {
                    Client.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    m_Session.Logger.Error(e);
                }

                try
                {
                    Client.Close();
                }
                catch (Exception e)
                {
                    m_Session.Logger.Error(e);
                }
                finally
                {
                    Client = null;
                    m_Session = null;
                    m_IsClosed = true;
                    OnClose();
                }
            }
        }

        /// <summary>
        /// Called when [close].
        /// </summary>
        protected virtual void OnClose()
        {
            m_IsClosed = true;
        }
    }
}
