/* Ratcow.Csta is an ECMA-323/ECMA-354 XML API integration for MS.Net.
 * Copyright (C) 2018 Ratcow Software and Matt Emson. 
 * 
 * This software is dual licensed. It may be freely used under the GPL3,
 * but any for any proprietary commercial use, it must me licensed under 
 * the terms of a commercial license, and may only be included in any 
 * release after the appropriate fees have been paid.
 * 
 * The GPL3 license is as follows:
 *
 
    This file is part of the Ratcow.Csta namespace..

    Ratcow.Csta is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Ratcow.Csta is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Ratcow.Csta.  If not, see <http://www.gnu.org/licenses/>.
 *
 */


using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ratcow.Csta.Engine
{
    using Serialization;
    using Responses;
    using Logs.Logging;

    /// <summary>
    /// This is only implementing just enough to get by
    /// </summary>
    public class TcpCommsServer
    {
        readonly ILog logger = LogProvider.For<TcpCommsServer>();

        EventProcessor eventProcessor = null;
        ResourceManager resourceManager = null;
        TcpListener server = null;
        public string Address { get; set; }
        public int Port { get; set; }
        public bool Secure { get; set; }
        public string CallServerName { get; set; }

        public bool Running { get; private set; }
        public string CallServerIpAddress { get; set; }

        List<TcpCommsServlet> servlets = new List<TcpCommsServlet>();

        public TcpCommsServer(EventProcessor ep, ResourceManager rm)
        {
            eventProcessor = ep;
            resourceManager = rm;
        }

        public void Start()
        {
            logger.Debug("Enter Start");
            Task.Run(() =>
            {
                server = new TcpListener(IPAddress.Parse(Address), Port);
                server.Start();
                Running = true;
                while (Running)
                {
                    logger.Debug("Waiting for connection...");
                    var client = server.AcceptTcpClient();
                    if (client != null)
                    {
                        logger.Debug("Received connection");
                        AddClient(client); //TODO - remove clients
                    }
                }
            });
            logger.Debug("Exit Start");
        }

        public void Stop()
        {
            Running = false;
            server.Stop();
        }

        void AddClient(TcpClient client)
        {
            logger.Debug("Adding client");

            var servlet = new TcpCommsServlet
            {
                Parent = this,
                Client = client,
                EventProcessor = eventProcessor,
                ResourceManager = resourceManager,
            };
            servlets.Add(servlet);
            servlet.StartProcessing();
        }
    }
}
