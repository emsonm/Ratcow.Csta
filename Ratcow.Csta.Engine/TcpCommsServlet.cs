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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;

namespace Ratcow.Csta.Engine
{
    using Serialization;
    using Responses;
    using Logs.Logging;
    using Ecma354;
    using Ecma323.Ed3.AvayaExtensions;
    using Avaya.Dmcc;
    using Avaya.Dmcc.Server;
    using IPAddress = System.Net.IPAddress;


    public class TcpCommsServlet : TcpCommsBase
    {
        static byte seed = 0;
        readonly string sessionId = null;
        static int systemRegisterResponseIdBase = 50000;
        readonly int systemRegisterResponseId;

        readonly ILog logger = LogProvider.For<TcpCommsServlet>();

        IDmccServerDataProtocol protocol = null;
        

        public TcpCommsServlet() : base()
        {
            sessionId = $"{Guid.NewGuid().ToString()}-{++seed}"; //vaguely the avaya format
            systemRegisterResponseId = systemRegisterResponseIdBase + ++seed;
        }

        public Task ProcessingTask { get; private set; }

        public TcpCommsServer Parent { get; internal set; }
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

        public bool Running { get; private set; }
        public EventProcessor EventProcessor { get; internal set; }
        public ResourceManager ResourceManager { get; internal set; }

        public bool StartProcessing()
        {
            logger.Debug("Enter Start Processing");

            //start up the clients internals
            var success = SetUpTcpClient(Parent.Address, Parent.Secure);
            if (success == ConnectionResponse.Success)
            {
                Running = true;
                ProcessingTask = Task.Run(() => InternalEventListenerLoop());
                return true;
            }

            return false;
        }

        void InternalEventListenerLoop()
        {
            while (Running)
            {
                if (!HasData)
                {
                    Task.Delay(200);
                    continue;
                }
                {
                    var (Request, InvokeId) = ReadXMLMessage();
                    var root = XmlHelper.GetRootElementName(Request);

                    switch (root)
                    {
                        case nameof(StartApplicationSession):
                            //TODO - we in no way validate the login credentials here, mainly because we don't 
                            //really care about them for this simple simulation - we are trying to simulate
                            //a switch, so the login security as not really important till the bulk of the
                            //functions are implemented.
                            var startApplicationSession = SerializationHelper.Deserialize<StartApplicationSession>(Request);
                            var requestedProtocol = startApplicationSession.requestedProtocolVersions?.Length > 0 ? startApplicationSession.requestedProtocolVersions[0]: string.Empty;
                            logger.Debug($"requested protocol was {requestedProtocol}");

                            //set the protocol:
                            if(EventProcessor?.Protocols?.Length > 0)
                            {
                                var requestedProtocolType = DmccProtocols.ToDmccProtocolType(requestedProtocol);
                                protocol = EventProcessor.Protocols.FirstOrDefault(p => p.Protocol == requestedProtocolType);
                            }
                            else
                            {
                                //fall back...
                                protocol = DmccServerDataProtocolFactory.Create(requestedProtocol); //TODO - remove this.. we should not fall back
                            }

                            if (protocol != null)
                            {
                                //we just respond that we are happy
                                //var startApplicationResponse = $"<?xml version=\"1.0\" encoding=\"UTF-8\" ?><StartApplicationSessionPosResponse xmlns=\"http://www.ecma-international.org/standards/ecma-354/appl_session\"><sessionID>{sessionId}</sessionID><actualProtocolVersion>http://www.ecma-international.org/standards/ecma-323/csta/ed3/priv2</actualProtocolVersion><actualSessionDuration>{startApplicationSession.requestedSessionDuration}</actualSessionDuration></StartApplicationSessionPosResponse>";
                                //var startApplicationPositiveResponse = new StartApplicationSessionPosResponse
                                //{
                                //    actualProtocolVersion = requestedProtocol,
                                //    actualSessionDuration = startApplicationSession.requestedSessionDuration,
                                //    sessionID = sessionId,
                                //};
                                var startApplicationPositiveResponse = protocol.NewStartApplicationSessionPosResponse(sessionId, requestedProtocol, startApplicationSession.requestedSessionDuration);
                                SendResponse(InvokeId, startApplicationPositiveResponse);
                            }
                            else
                            {
                                //because we have no protocol at this point, we use a common static helper function.
                                var startApplicationNegativeResponse = DmccServerDataProtocolBase.NewStartApplicationSessionNegResponse(StartApplicationSessionNegResponseErrorCodeDefinedError.requestedProtocolVersionNotSupported);
                                SendResponse(InvokeId, startApplicationNegativeResponse);
                            }
                            break;

                        case nameof(SystemRegister):
                            /*<?xml version="1.0" encoding="UTF-8"?>
                              <SystemRegisterResponse 
                                  xmlns="http://www.ecma-international.org/standards/ecma-323/csta/ed3">
                                <sysStatRegisterID>500046</sysStatRegisterID>
                                <actualStatusFilter>
                                    <initializing>false</initializing>
                                    <enabled>false</enabled>
                                    <normal>true</normal>
                                    <messageLost>false</messageLost>
                                    <disabled>true</disabled>
                                    <partiallyDisabled>false</partiallyDisabled>
                                    <overloadImminent>false</overloadImminent>
                                    <overloadReached>false</overloadReached>
                                    <overloadRelieved>false</overloadRelieved>
                                </actualStatusFilter>
                                <extensions>
                                    <privateData>
                                        <private>
                                            <ns1:SystemRegisterPrivateData 
                                                xmlns:ns1="http://www.avaya.com/csta" 
                                                xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
                                                xsi:type="ns1:SystemRegisterPrivateData">
                                            <ns1:invertFilter>true</ns1:invertFilter>
                                            </ns1:SystemRegisterPrivateData>
                                        </private>
                                    </privateData>
                                </extensions>
                            </SystemRegisterResponse>
                            */
                            var systemRegister = SerializationHelper.Deserialize<SystemRegister>(Request);
                            //let's just check the sessionId is what we previously sent
                            if (systemRegister.extensions.privateData.Item is CSTAPrivateDataPrivate cpd)
                            {
                                var any = cpd.Any[0].OuterXml;
                                var systemRegisterPrivateData = SerializationHelper.Deserialize<SystemRegisterPrivateData>(any);
                                if (string.Compare(Parent.CallServerName, systemRegisterPrivateData.switchName) == 0)
                                {
                                    var systemRegisterResponse = protocol.NewSystemRegisterResponse(systemRegisterResponseId);
                                    SendResponse(InvokeId, systemRegisterResponse);
                                }
                                else
                                {
                                    SendUniversalFailure(InvokeId, OperationErrors.generic); //TODO - this should be better
                                }
                            }
                            break;

                        case nameof(GetDeviceId):
                            var getDeviceId = SerializationHelper.Deserialize<GetDeviceId>(Request);

                            if (ResourceManager.Devices.ContainsKey(getDeviceId.extension))
                            {
                                var device = ResourceManager.Devices[getDeviceId.extension];
                                //TODO - should we ever be using the ManagingSwitchName here? I've thrown it in for good measure as a valid 
                                //       alternative... but I've only ever used the switchIPInterface
                                if (device?.ManagingSwitchIpAddress == getDeviceId.switchIPInterface || device?.ManagingSwitchName == getDeviceId.switchName)
                                {
                                    if (device.Alocated)
                                    {
                                        SendUniversalFailure(InvokeId, OperationErrors.generic); //TODO - what do we actually get here?
                                    }
                                    else
                                    {
                                        device.Alocated = true;
                                        var getDeviceIdResponse = protocol.NewGetDeviceIdResponse(device.Device);
                                        SendResponse(InvokeId, getDeviceIdResponse);
                                    }
                                }
                                else
                                {
                                    SendUniversalFailure(InvokeId, OperationErrors.invalidDeviceID);
                                }
                            }
                            else
                            {

                                SendUniversalFailure(InvokeId, OperationErrors.invalidDeviceID);
                            }
                            break;

                        case nameof(GetThirdPartyDeviceId):
                            var getThirdPartyDeviceId = SerializationHelper.Deserialize<GetThirdPartyDeviceId>(Request);

                            if (ResourceManager.Devices.ContainsKey(getThirdPartyDeviceId.extension))
                            {
                                var device = ResourceManager.Devices[getThirdPartyDeviceId.extension];
                                if (device?.ManagingSwitchName == getThirdPartyDeviceId.switchName)
                                {
                                    if (device.Alocated)
                                    {
                                        SendUniversalFailure(InvokeId, OperationErrors.generic); //TODO - what do we actually get here?
                                    }
                                    else
                                    {
                                        device.Alocated = true;
                                        var getDeviceIdResponse = protocol.NewGetThirdPartyDeviceIdResponse(device.Device);
                                        SendResponse(InvokeId, getDeviceIdResponse);
                                    }
                                }
                                else
                                {
                                    SendUniversalFailure(InvokeId, OperationErrors.invalidDeviceID);
                                }
                            }
                            else
                            {

                                SendUniversalFailure(InvokeId, OperationErrors.invalidDeviceID);
                            }
                            break;

                        case nameof(ReleaseDeviceId):
                            var releaseDeviceId = SerializationHelper.Deserialize<ReleaseDeviceId>(Request);
                            var deviceId = releaseDeviceId?.device?.Value??string.Empty;

                            if (ResourceManager.Devices.Values.Any(d => d?.Device?.Value == deviceId))
                            {
                                var device = ResourceManager.Devices.Values.FirstOrDefault(d => d?.Device?.Value == deviceId);
                                if (device != null)
                                {
                                    device.Alocated = false;
                                }
                                var response = protocol.NewReleaseDeviceIdResponse();
                                SendResponse(InvokeId, response);
                            }
                            else
                            {
                                SendUniversalFailure(InvokeId, OperationErrors.invalidDeviceID);
                            }
                            break;

                        default:
                            logger.Debug(Request);
                            break;
                    }
                }
            }
        }

        private void SendUniversalFailure(int invokeId, object error)
        {
            var response = protocol.NewUniversalFailure(error);
            SendResponse(invokeId, response);
        }       

        string GetInvokeIdString(int invokeId)
        {
            return invokeId.ToString().PadLeft(INVOKE_ID_LEN, '0');
        }

        public void SendResponse<T>(string invokeId, T request)
        {
            //TODO - this needs more protection.
            RawSendResponse(invokeId, SerializationHelper.Serialize(request));
        }

        public void SendResponse<T>(int invokeId, T request)
        {
            var invokeIdString = GetInvokeIdString(invokeId);
            //TODO - this needs more protection.
            RawSendResponse(invokeIdString, SerializationHelper.Serialize(request));
        }

        /// <summary>
        /// This is almost identical to the example project
        /// </summary>
        void RawSendResponse(string invokeId, string request)
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

            if (writer == null)
            {
                return;
            }

            try
            {
                // HEADER : VERSION
                var version = IPAddress.HostToNetworkOrder((short)0);
                var verHeader = BitConverter.GetBytes(version);

                writer.Write(verHeader); // using the BinaryWriter

                // HEADER : LENGTH (PAYLOAD + HEADER(8))
                var totalLength = IPAddress.HostToNetworkOrder((short)(request.Length + XML_HEADER_LEN));
                var lengthHeader = BitConverter.GetBytes(totalLength);
                writer.Write(lengthHeader); // using the BinaryWriter

                // HEADER : INVOKE ID
                var invokeIdByte = ToByteArray(invokeId);
                writer.Write(invokeIdByte); // using the BinaryWriter

                // MESSAGE : XML PAYLOAD
                var requestMessage = ToByteArray(request);
                writer.Write(requestMessage); // using the BinaryWriter
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred in SendRequest.\n");
                Console.WriteLine(e.Message);
            }
            finally
            {
                writer.Flush();
            }
        }

        /// <summary>
        /// This is almost identical to the example project
        /// </summary>
        public (string Request, int InvokeId) ReadXMLMessage(bool addWait = false)
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
                reader.ReadBytes(VERSION_LEN);
                // Read the length of the response in byte 3, 4
                var requestLength = (long)IPAddress.NetworkToHostOrder(reader.ReadInt16());
                // String to store the response ASCII representation.
                var data = new byte[requestLength];

                // Read ahead in the header. Invoke ID: byte 5,6,7,8	
                var requestInvokeIdString = new string(reader.ReadChars(INVOKE_ID_LEN));
                if (!int.TryParse(requestInvokeIdString, out int requestInvokeId))
                {
                    requestInvokeId = -1;//????
                }

                // Read the XML Payload which is found at offset 8.
                // The length of XML payload = XML message length - XML header length.
                var totalbytesremaining = data.Length - XML_HEADER_LEN;
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
                var requestData = Encoding.ASCII.GetString(data, 0, data.Length);
                return (requestData, requestInvokeId);
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
    }


}
