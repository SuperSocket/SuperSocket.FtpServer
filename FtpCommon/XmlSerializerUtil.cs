using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace SuperSocket.Ftp.FtpCommon
{
    public static class XmlSerializerUtil
    {
        public static T Deserialize<T>(string filePath)
        {
            XmlSerializer worker = new XmlSerializer(typeof(T));
            Stream stream = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return (T)worker.Deserialize(stream);
            }            
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
            }
        }

        public static void Serialize(string filePath, object target)
        {
            XmlSerializer worker = new XmlSerializer(target.GetType());
            Stream stream = null;
            XmlWriter writer = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write);
                writer = XmlWriter.Create(stream, GetWriterSetting());
                worker.Serialize(writer, target);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }

                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
            }
        }

        private static XmlWriterSettings GetWriterSetting()
        {
            XmlWriterSettings setting = new XmlWriterSettings();
            setting.Encoding = Encoding.UTF8;
            setting.Indent = true;
            return setting;
        }
    }
}
