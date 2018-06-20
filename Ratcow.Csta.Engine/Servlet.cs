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
using System.Threading.Tasks;
using System.Linq;

namespace Ratcow.Csta.Engine
{
    using Serialization;
    using Core;
    using Core.Responses;
    using Logs.Logging;
    using Ecma354;
    using Ecma323.Ed3.AvayaExtensions;
    using Avaya.Dmcc;
    using Avaya.Dmcc.Server;


    public class Servlet : TcpCommsServer, IServlet
    {
        static byte seed = 0;
        readonly string sessionId = null;
        static int systemRegisterResponseIdBase = 50000;
        readonly int systemRegisterResponseId;

        readonly ILog logger = LogProvider.For<Servlet>();

        IDmccServerDataProtocol protocol = null;
        
        public Servlet() : base()
        {
            sessionId = $"{Guid.NewGuid().ToString()}-{++seed}"; //vaguely the avaya format
            systemRegisterResponseId = systemRegisterResponseIdBase + ++seed;
        }

        public Task ProcessingTask { get; private set; }

        public ServerManager Parent { get; internal set; }

        public bool Running { get; private set; }
        public IEventProcessor EventProcessor { get; internal set; }
        public ResourceManager ResourceManager { get; internal set; }

        public bool StartProcessing()
        {
            logger.Debug("Enter Start Processing");

            //start up the clients internals
            var success = IniializeConnection(Parent.Address, Parent.Secure);
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

        void SendUniversalFailure(int invokeId, object error)
        {
            var response = protocol.NewUniversalFailure(error);
            SendResponse(invokeId, response);
        }       
    }
}
