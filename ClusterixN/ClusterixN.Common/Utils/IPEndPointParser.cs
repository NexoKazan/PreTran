#region Copyright

/*
 * Copyright 2018 Roman Klassen
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy
 * of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 */

#endregion

using System;
using System.Linq;
using System.Net;

namespace ClusterixN.Common.Utils
{
    /// <summary>
    ///     <see cref="https://stackoverflow.com/questions/2727609/best-way-to-create-ipendpoint-from-string"></see>
    /// </summary>
    public static class IPEndPointParser
    {
        public static IPEndPoint Parse(string endpointstring)
        {
            return Parse(endpointstring, -1);
        }

        public static IPEndPoint Parse(string endpointstring, int defaultport)
        {
            if (string.IsNullOrEmpty(endpointstring)
                || endpointstring.Trim().Length == 0)
                throw new ArgumentException("Endpoint descriptor may not be empty.");

            if (defaultport != -1 &&
                (defaultport < IPEndPoint.MinPort
                 || defaultport > IPEndPoint.MaxPort))
                throw new ArgumentException(string.Format("Invalid default port '{0}'", defaultport));

            var values = endpointstring.Split(':');
            IPAddress ipaddy;
            int port;

            //check if we have an IPv6 or ports
            if (values.Length <= 2) // ipv4 or hostname
            {
                if (values.Length == 1)
                    //no port is specified, default
                    port = defaultport;
                else
                    port = GetPort(values[1]);

                //try to use the address as IPv4, otherwise get hostname
                if (!IPAddress.TryParse(values[0], out ipaddy))
                    ipaddy = GetIPfromHost(values[0]);
            }
            else if (values.Length > 2) //ipv6
            {
                //could [a:b:c]:d
                if (values[0].StartsWith("[") && values[values.Length - 2].EndsWith("]"))
                {
                    var ipaddressstring = string.Join(":", values.Take(values.Length - 1).ToArray());
                    ipaddy = IPAddress.Parse(ipaddressstring);
                    port = GetPort(values[values.Length - 1]);
                }
                else //[a:b:c] or a:b:c
                {
                    ipaddy = IPAddress.Parse(endpointstring);
                    port = defaultport;
                }
            }
            else
            {
                throw new FormatException(string.Format("Invalid endpoint ipaddress '{0}'", endpointstring));
            }

            if (port == -1)
                throw new ArgumentException(string.Format("No port specified: '{0}'", endpointstring));

            return new IPEndPoint(ipaddy, port);
        }

        private static int GetPort(string p)
        {
            int port;

            if (!int.TryParse(p, out port)
                || port < IPEndPoint.MinPort
                || port > IPEndPoint.MaxPort)
                throw new FormatException(string.Format("Invalid end point port '{0}'", p));

            return port;
        }

        private static IPAddress GetIPfromHost(string p)
        {
            var hosts = Dns.GetHostAddresses(p);

            if (hosts == null || hosts.Length == 0)
                throw new ArgumentException(string.Format("Host not found: {0}", p));

            return hosts[0];
        }
    }
}