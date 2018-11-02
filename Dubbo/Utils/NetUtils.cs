using log4net;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Dubbo.Utils
{
    public static class NetUtils
    {
        private static ILog _log = LogManager.GetLogger(typeof(NetUtils));
        public const string LocalHost = "127.0.0.1";
        public const string AnyHost = "0.0.0.0";
        private static volatile IPAddress LocalAddress;

        public static bool IsIPv6(this IPAddress address)
        {
            return address.AddressFamily == AddressFamily.InterNetworkV6;
        }

        public static bool IsAnyHost(string host)
        {
            return AnyHost == host;
        }

        public static bool ValidAddress(IPAddress address)
        {
            if (address == null || IPAddress.IsLoopback(address))
                return false;
            if (address.IsIPv6())
            {
                return Socket.OSSupportsIPv6;
            }
            var host = address.ToString();
            return AnyHost != (host) && LocalHost != (host) && Regex.IsMatch(host, @"\d{1,3}(\.\d{1,3}){3,5}$", RegexOptions.Compiled);
        }

        public static string GetLocalHost()
        {
            if (LocalAddress != null)
                return LocalAddress.ToString();
            LocalAddress = GetLocalAddress();
            return LocalAddress == null ? LocalHost : LocalAddress.ToString();
        }

        public static IPAddress GetLocalAddress()
        {
            try
            {
                foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList.OrderBy(addr => addr.IsIPv6() ? 1 : 0))
                {
                    if (ValidAddress(address))
                    {
                        return address;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Warn("get local address failed", e);
            }

            try
            {
                var nets = NetworkInterface.GetAllNetworkInterfaces();
                if (nets.Length <= 0)
                {
                    return null;
                }
                foreach (var net in nets)
                {
                    foreach (var ipAddress in net.GetIPProperties().UnicastAddresses.Select(a => a.Address).Where(a => a.AddressFamily == AddressFamily.InterNetwork || a.AddressFamily == AddressFamily.InterNetworkV6).OrderBy(a => a.IsIPv6() ? 1 : 0))
                    {
                        if (ValidAddress(ipAddress))
                        {
                            return ipAddress;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Warn("get local address failed", e);
            }

            return null;
        }

    }
}
