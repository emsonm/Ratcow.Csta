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

namespace Ratcow.Csta.Engine.Core
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
        protected readonly IInvokeIdService invokeIdService = null;

        //this is taken from the Avaya example project as the comms is fairly similar
        protected TcpClient client = null;
        protected BinaryWriter writer = null;
        protected BinaryReader reader = null;
        protected NetworkStream networkStream = null;
        protected SslStream sslStream = null;

        public TcpCommsBase()
        {
        }

        public TcpCommsBase(IInvokeIdService invokeIdService = null)
        {
            if (invokeIdService == null)
            {
                this.invokeIdService = new InvokeIdService();
            }
            else
            {
                this.invokeIdService = invokeIdService;
            }
        }

        public bool HasData
        {
            get
            {
                return networkStream.DataAvailable;
            }
        }

        public TcpClient Client
        {
            get
            {
                return client;
            }
            set
            {
                client = value;
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

        public ConnectionResponse IniializeConnection(string address = null, bool secure = false)
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

        /// <summary>
        /// This is almost identical to the example project
        /// </summary>
        public (string Data, int InvokeId) ReadXMLMessage(bool addWait = false)
        {
            /*
             * Removing the Header information.
             * 
             * | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 |   
             *  VERSION|LENGTH |   INVOKE ID   |   XML PAYLOAD
             * 
             * VERSION: 2 bytes
             * LENGTH: 2 bytes
             * INVOKE ID: 4 bytes
             */

            /* In most of the cases application starts reading the xml response immediately 
             * after sending XML request.. so waiting for some time to get the complete response. 
             */
            if (addWait)
            {
                Thread.Sleep(150);
            }

            try
            {
                // Skip the CSTA version in byte 1, 2
                reader.ReadBytes(TcpCommsConstants.VERSION_LEN);
                // Read the length of the response in byte 3, 4
                var dataLength = (long)IPAddress.NetworkToHostOrder(reader.ReadInt16());
                // String to store the response ASCII representation.
                var data = new byte[dataLength];

                // Read ahead in the header. Invoke ID: byte 5,6,7,8	
                var invokeIdString = new string(reader.ReadChars(TcpCommsConstants.INVOKE_ID_LEN));
                if (!int.TryParse(invokeIdString, out int dataInvokeId))
                {
                    dataInvokeId = -1;//????
                }

                // Read the XML Payload which is found at offset 8.
                // The length of XML payload = XML message length - XML header length.
                var totalbytesremaining = data.Length - TcpCommsConstants.XML_HEADER_LEN;
                while (totalbytesremaining > 0)
                {
                    //This was a horrible bug in the original example code - we just waitied 
                    //a bit for every event, so if the data was less than expected, the 
                    //XML became b0rked. If you remove this 150 milisecond "hedge" longer 
                    //data messages fail horribly to arrive.
                    var bytes = reader.Read(data, 0, totalbytesremaining);
                    totalbytesremaining -= bytes;
                }

                // Get the ASCII representation of XML payload.
                var dataString = Encoding.ASCII.GetString(data, 0, data.Length);

                logger.Debug($"ReadXMLMessage:: InvokeId {dataInvokeId}{Environment.NewLine}{dataString}");

                return (dataString, dataInvokeId);
            }
            catch (ThreadAbortException ex)
            {

                /* When user presses 'any key' to stop the application, reader thread is aborted to immediately
                 * stop the processing. This results in receiveing ThreadAbortException here.
                 * Just clean the response data varaible and return as application is about to terminate.
                 */
                //Console.WriteLine("Threadabortexception");
                return ($"ThreadAbortException occurred.\n{ex.Message}\n{ex.StackTrace}", -1);
            }
            catch (IOException ex)
            {
                if (ex.InnerException is ThreadAbortException)
                {
                    return ($"ThreadAbortException occurred.\n{ex.Message}\n{ex.StackTrace}", -1);
                }
                return ($"EndOfStreamException occurred.\n{ex.Message}\n{ex.StackTrace}", -1);
            }
            catch (Exception ex)
            {
                return ($"Exception occurred.\n{ex.Message}\n{ex.StackTrace}", -1);
            }
        }

        public bool WriteXmlMessage(string invokeId, string request)
        {
            /*
             * According to CSTA-ECMA 323 Standard ANNEX G
             * Section G.2.  CSTA XML without SOAP
             * 
             * The Header is  8 bytes long.
             * | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 |   
             * |VERSION|LENGTH |   INVOKE ID   |   XML PAYLOAD
             * 
             * VERSION: 2 bytes
             * LENGTH: 2 bytes information that contains the total size 
             * (XML payload + Header)
             * INVOKE ID: 4 bytes.  
             * 
             */

            var result = false;

            if (writer == null)
            {
                return result;
            }

            try
            {
                // HEADER : VERSION
                var version = IPAddress.HostToNetworkOrder((short)0);
                var verHeader = BitConverter.GetBytes(version);

                writer.Write(verHeader); // using the BinaryWriter

                // HEADER : LENGTH (PAYLOAD + HEADER(8))
                var totalLength = IPAddress.HostToNetworkOrder((short)(request.Length + TcpCommsConstants.XML_HEADER_LEN));
                var lengthHeader = BitConverter.GetBytes(totalLength);
                writer.Write(lengthHeader); // using the BinaryWriter

                // HEADER : INVOKE ID
                var invokeIdByte = ToByteArray(invokeId);
                writer.Write(invokeIdByte); // using the BinaryWriter

                // MESSAGE : XML PAYLOAD
                var requestMessage = ToByteArray(request);
                writer.Write(requestMessage); // using the BinaryWriter

                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred in SendRequest.\n");
                Console.WriteLine(e.Message);
                result = false;
            }
            finally
            {
                writer.Flush();
            }

            return result;
        }
    }
}