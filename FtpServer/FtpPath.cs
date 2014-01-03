using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Ftp.FtpService
{
    public static class FtpPath
    {
        public const string Root = "/";
        public const string PathSeperator = "/";

        public static string Combine(string parent, string child)
        {
            if (string.IsNullOrEmpty(child))
                throw new ArgumentNullException("child");

            if (string.IsNullOrEmpty(parent))
            {
                if (child.StartsWith(Root))
                    return child;

                return Root + child;
            }

            if(!parent.StartsWith(Root))
                throw new ArgumentException("Parent must be a rooted path", "parent");

            if(child.StartsWith(Root))
                throw new ArgumentException("Child cannot be a rooted path", "child");

            if (parent.EndsWith(Root))
            {
                return parent + child;
            }
            else
            {
                return parent + PathSeperator + child;
            }
        }
    }
}
