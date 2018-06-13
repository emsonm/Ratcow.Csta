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

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Ratcow.Csta.Engine
{
    using Serialization;
    using Responses;
    using Logs.Initialization;
    using Logs.Logging;

    /// <summary>
    /// This is heavily based on the RPTC example DMCC XML example project.
    /// </summary>
    public class TcpCommsBase
    {
        readonly ILog logger = LogProvider.For<TcpCommsBase>();

        //this is taken from the Avaya example project as the comms is fairly similar
        protected TcpClient client = null;
        protected BinaryWriter writer = null;
        protected BinaryReader reader = null;
        protected NetworkStream networkStream = null;
        protected SslStream sslStream = null;

        protected const int XML_HEADER_LEN = 8;
        protected const int INVOKE_ID_LEN = 4;
        protected const int VERSION_LEN = 2;

        public TcpCommsBase()
        {
        }

        public bool HasData
        {
            get
            {
                return networkStream.DataAvailable;
            }
        }

        /// <summary>
        /// This is almost identical to the example project
        /// </summary>
        public ConnectionResponse Close()
        {
            try
            {
                /* Close the reader and writer.*/
                if (reader != null)
                {
                    reader.Close();
                }

                reader = null;

                if (writer != null)
                {
                    writer.Close();
                }

                writer = null;

                /* Closing the network stream.*/
                if (networkStream != null)
                {
                    networkStream.Close();
                }

                networkStream = null;

                /* Closing the secure socket layer stream.*/
                if (sslStream != null)
                {
                    sslStream.Close();
                }

                sslStream = null;

                /* Closing the TCP client connection.*/
                if (client != null)
                {
                    client.Close();
                }

                client = null;
                return ConnectionResponse.Success;
            }
            catch (Exception)
            {
                return ConnectionResponse.Failed;
            }
        }

        protected ConnectionResponse SetUpTcpClient(string address, bool secure)
        {
            networkStream = client.GetStream();

            if (secure)
            {
                sslStream = new SslStream(
                    networkStream,
                    false,
                    (object sender,
                       X509Certificate certificate,
                       X509Chain chain,
                       SslPolicyErrors sslPolicyErrors) =>
                    {
                        if (sslPolicyErrors == SslPolicyErrors.None)
                        {
                            return true;
                        }

                        if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
                        {
                            return true;
                        }

                        return false;
                    },
                    null
                );

                if (sslStream == null)
                {
                    return ConnectionResponse.Unsupported;
                }

                sslStream.AuthenticateAsClient(address);
                if (sslStream.IsAuthenticated)
                {
                    /* Initializes the reader and writer.*/
                    writer = new BinaryWriter(sslStream);
                    reader = new BinaryReader(sslStream);
                }
                else
                {
                    return ConnectionResponse.SslAuthenticationError;
                }
            }
            else
            {
                writer = new BinaryWriter(networkStream);
                reader = new BinaryReader(networkStream);
            }

            return ConnectionResponse.Success;
        }

        /// <summary>
        /// I think this could be replaced by a Encoding.UTF8
        /// </summary>
        protected byte[] ToByteArray(string sourcestring)
        {
            var byteArray = new byte[sourcestring.Length];
            for (int index = 0; index < sourcestring.Length; index++)
            {
                byteArray[index] = (byte)sourcestring[index];
            }

            return byteArray;
        }

        
    }
}