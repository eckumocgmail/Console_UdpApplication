using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text;
 
using System.Threading;

namespace Console23_TcpServer
{
    using static System.Text.Json.JsonSerializer;
    using static System.Console;
    using static System.Net.NetworkInformation.IPGlobalProperties;
    using static System.Threading.Thread;
    using static System.Threading.Tasks.Task;
    
    /// <summary>
    /// 
    /// </summary>
    internal class TcpServerProgram
    {


        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        internal static void Run(string[] args)
        {
            //TcpServerEndpoint.Run("0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.1");
            TcpServerEndpoint.Run("127.0.0.1",13000);
            Thread.Sleep(Timeout.Infinite);
        }

        static void Trace() { 
            bool active = true;
            while (active)            
            {
                Clear();
                var properties = GetIPGlobalProperties();
                PrintLine(new
                {
                    DomainName = $"{properties.DomainName}",
                    HostName = $"{properties.HostName}",
                    DhcpScopeName = $"{properties.DhcpScopeName}",
                    IsWinsProxy = $"{properties.IsWinsProxy}"
                });
                PrintLine(new
                {
                    TcpConnectionsCount = $"{properties.GetActiveTcpConnections().Count()}",
                    UnicastAddressesCount = $"{properties.GetUnicastAddresses().Count()}",
                    DhcpScopeName = $"{properties.DhcpScopeName}",
                    IsWinsProxy = $"{properties.IsWinsProxy}"
                });
                PrintLine(new
                {

                    
                    UnicastAddresses = properties.GetUnicastAddresses().Select(x => new
                    {
                        //EndpointProvider = TcpServerEndpoint.Run(ToString(x.Address.GetAddressBytes().ToList().Select(x => (int)x))),
                        AddressBytes = ToString(x.Address.GetAddressBytes().ToList().Select(x => (int)x)),
                        AddressFamily = x.Address.AddressFamily.ToString(),                 
                        IsTransient = x.IsTransient,
                        IsDnsVisible= x.IsDnsEligible,
                        PrefixLength=  x.PrefixLength
                    })
                });
                Thread.Sleep(100000);
            }
        }

        public static string ToString(IEnumerable<int> addr)
        {
            string res = "";
            foreach(var id in addr)
            {
                res += "." + id;
            }
            return res.Substring(1);
        }


        /// <summary>
        /// Печать нформации по портам
        /// </summary>        
        static void LogInformation( )
        {
            var target = GetOpenPort();
            PrintLine(target);
        }

        private static void PrintLine(object target)
        {
            WriteLine(Serialize(target, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }




        /// <summary>
        /// 
        /// </summary>    
        public static List<PortInfo> GetOpenPort()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
            TcpConnectionInformation[] tcpConnections = properties.GetActiveTcpConnections();

            return tcpConnections.Select(p =>
            {
                return new PortInfo(
                    i: p.LocalEndPoint.Port,
                    local: String.Format("{0}:{1}", p.LocalEndPoint.Address, p.LocalEndPoint.Port),
                    remote: String.Format("{0}:{1}", p.RemoteEndPoint.Address, p.RemoteEndPoint.Port),
                    state: p.State.ToString());
            }).ToList();
        }


        /// <summary>
        /// 
        /// </summary>
        public class PortInfo
        {
            public int PortNumber { get; set; }
            public string Local { get; set; }
            public string Remote { get; set; }
            public string State { get; set; }

            public PortInfo(int i, string local, string remote, string state)
            {
                PortNumber = i;
                Local = local;
                Remote = remote;
                State = state;
            }
        }
    }
}
