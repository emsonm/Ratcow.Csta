/* Ratcow.Csta.Avaya is an Avaya AES XML API integration for MS.Net.
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

using System.Xml;


namespace Ratcow.Csta.Avaya.Dmcc.Client
{
    using Ecma354;
    using Ecma323.Ed3.AvayaExtensions;
    using Serialization;

    /// <summary>
    /// Implements version 6.3 of the Avaya Dmcc Xml protocol generation.
    /// </summary>
    public class DmccV63ClientProtocol : DmccCommonDataProtocol, IDmccClientDataProtocol
    {
        public override DmccProtocolType Protocol { get { return DmccProtocolType.v63; } }
        /// <summary>
        /// TODO - make this nicer. This sucks greatly, and to fix this we need to hack around with the generated XML.
        /// </summary>
        XmlElement NewSessionLoginInfo(string username, string password, int sessionCleanUpDelay)
        {
            return XmlHelper.StringToElement($"<ns1:SessionLoginInfo xmlns=\"http://www.avaya.com/csta\" xmlns:ns1=\"http://www.avaya.com/csta\"><ns1:userName>{username}</ns1:userName><ns1:password>{password}</ns1:password><ns1:sessionCleanupDelay>{sessionCleanUpDelay}</ns1:sessionCleanupDelay></ns1:SessionLoginInfo>");
        }

        XmlElement NewBasicAvayaEvent()
        {
            var result = new AvayaEvents
            {
                invertFilter = true,
                invertFilterSpecified = true,
                callControlPrivate = new CallControlPrivateEvents
                {
                    enteredDigits = true,
                    enteredDigitsSpecified = true                    
                }
            };

            return XmlHelper.InstanceToElement(result);
        }

        XmlElement NewExtendedAvayaEvent()
        {
            var result = new AvayaEvents
            {
                invertFilter = true,
                invertFilterSpecified = true,
                callControlPrivate = new CallControlPrivateEvents
                {
                    enteredDigits = true,
                    enteredDigitsSpecified = true                  
                },
                logicalDeviceFeaturePrivate = new LogicalDeviceFeaturePrivateEvents
                {
                    agentLoginExtension = true,
                    agentLoginExtensionSpecified = true                    
                },
                endpointRegistrationStateEvents = new EndpointRegistrationStateEventsFilter
                {
                    registered = true,
                    registeredSpecified = true,
                    unregistered = true,
                    unregisteredSpecified = true
                }
            };

            return XmlHelper.InstanceToElement(result);
        }

        public StartApplicationSession NewStartApplicationSession(string username, string password, int sessionDuration = 180, int sessionCleanUpDelay = 60, string applicationId = "Ratcow.Csta")
        {
            var result = new StartApplicationSession
            {
                applicationInfo = new StartApplicationSessionApplicationInfo
                {
                    applicationID = applicationId,
                    applicationSpecificInfo = new StartApplicationSessionApplicationInfoApplicationSpecificInfo
                    {
                        Any = new XmlElement[]
                        {
                           NewSessionLoginInfo(username, password, sessionCleanUpDelay)
                        }
                    }
                },
                requestedProtocolVersions = new string[]
                {
                    DmccProtocols.ToString(DmccProtocolType.v63)
                },
                requestedSessionDuration = sessionDuration.ToString()

            };

            return result;
        }

        public MonitorStart NewMonitorStartRequest(DeviceID deviceId, bool callViaDevice = false)
        {
            if (callViaDevice)
            {
                return NewMonitorStartRequest_CallViaDevice(deviceId);
            }
            else
            {
                return NewMonitorStartRequest_Standard(deviceId);
            }
        }

        MonitorStart NewMonitorStartRequest_Standard(DeviceID deviceId)
        {
            var result = new MonitorStart
            {
                monitorObject = new MonitorObject { Item = deviceId },
                monitorType = MonitorType.device,
                requestedMonitorFilter = new MonitorFilter
                {
                    physicalDeviceFeature = new PhysicalDeviceFeatureEvents
                    {
                        buttonInformation = true,
                        buttonInformationSpecified = true,
                        displayUpdated = true,
                        displayUpdatedSpecified = true,
                        hookswitch = true,
                        hookswitchSpecified = true,
                        lampMode = true,
                        lampModeSpecified = true,
                        ringerStatus = true,
                        ringerStatusSpecified = true,
                        speakerMute = true,
                        speakerMuteSpecified = true
                    },
                    callcontrol = new CallControlEvents
                    {
                        //These should probably be configured centrally
                        conferenced = true,
                        conferencedSpecified = true,
                        connectionCleared = true,
                        connectionClearedSpecified = true,
                        delivered = true,
                        deliveredSpecified = true,
                        established = true,
                        establishedSpecified = true,
                        failed = true,
                        failedSpecified = true,
                        originated = true,
                        originatedSpecified = true,
                        retrieved = true,
                        retrievedSpecified = true,
                        transferred = true,
                        transferredSpecified = true,
                    },
                    logicalDeviceFeature = new LogicalDeviceFeatureEvents
                    {

                    }
                },
                extensions = new CSTACommonArguments
                {
                    privateData = new CSTAPrivateData
                    {
                        Item = new CSTAPrivateDataPrivate
                        {
                            Any = new XmlElement[]
                            {
                                NewBasicAvayaEvent()
                            }
                        }
                    }
                }
            };
            return result;
        }

        MonitorStart NewMonitorStartRequest_CallViaDevice(DeviceID deviceId)
        {
            var result = new MonitorStart
            {
                monitorObject = new MonitorObject { Item = deviceId },
                monitorType = MonitorType.call,
                monitorTypeSpecified = true,
                requestedMonitorFilter = new MonitorFilter
                {
                    //physicalDeviceFeature = new PhysicalDeviceFeatureEvents
                    //{
                    //    buttonInformation = true,
                    //    buttonInformationSpecified = true,
                    //    displayUpdated = true,
                    //    displayUpdatedSpecified = true,
                    //    hookswitch = true,
                    //    hookswitchSpecified = true,
                    //    lampMode = true,
                    //    lampModeSpecified = true,
                    //    ringerStatus = true,
                    //    ringerStatusSpecified = true,
                    //    speakerMute = true,
                    //    speakerMuteSpecified = true
                    //},
                    callcontrol = new CallControlEvents
                    {
                        //These should probably be configured centrally
                        callCleared = true,
                        callClearedSpecified = true,
                        conferenced = true,
                        conferencedSpecified = true,
                        connectionCleared = true,
                        connectionClearedSpecified = true,
                        delivered = true,
                        deliveredSpecified = true,
                        diverted = true,
                        divertedSpecified = true,
                        established = true,
                        establishedSpecified = true,
                        failed = true,
                        failedSpecified = true,
                        held = true,
                        heldSpecified = true,
                        networkReached = true,
                        networkReachedSpecified = true,
                        originated = true,
                        originatedSpecified = true,
                        queued = true,
                        queuedSpecified = true,
                        retrieved = true,
                        retrievedSpecified = true,
                        serviceInitiated = true,
                        serviceInitiatedSpecified = true,
                        transferred = true,
                        transferredSpecified = true,
                    },
                    logicalDeviceFeature = new LogicalDeviceFeatureEvents
                    {
                        //agentLoggedOff = true,
                        //agentLoggedOffSpecified = true,
                        //agentLoggedOn = true,
                        //agentLoggedOnSpecified = true,
                        //agentReady = true,
                        //agentReadySpecified = true,
                        //agentNotReady = true,
                        //agentNotReadySpecified = true,
                        //agentWorkingAfterCall = true,
                        //agentWorkingAfterCallSpecified = true,
                        //doNotDisturb = true,
                        //doNotDisturbSpecified = true,
                        //forwarding = true,
                        //forwardingSpecified = true,
                    }
                },
                extensions = new CSTACommonArguments
                {
                    privateData = new CSTAPrivateData
                    {
                        Item = new CSTAPrivateDataPrivate
                        {
                            Any = new XmlElement[]
                            {
                                NewBasicAvayaEvent(),                                
                            }
                        }
                    }
                }
            };
            return result;
        }

        public RouteRegisterPrivateData NewRouteRegisterPrivateData(string switchNameList)
        {
            var result = new RouteRegisterPrivateData
            {
                switchNameList = switchNameList,
            };
            return result;
        }

        public RouteRegister NewRouteRegister(DeviceID deviceId, string switchName)
        {
            var privateData = NewRouteRegisterPrivateData(switchName);

            var result = new RouteRegister
            {
                routeingDevice = deviceId,
                extensions = new CSTACommonArguments
                {
                    privateData = new CSTAPrivateData
                    {
                        Item = new CSTAPrivateDataPrivate
                        {
                            Any = new XmlElement[] { XmlHelper.InstanceToElement(privateData) }
                        }
                    }
                }
            };

            return result;
        }
       
        public SystemRegister NewSystemRequest(string switchName)
        {
            var requestPrivateData = new SystemRegisterPrivateData
            {
                invertFilter = true,
                invertFilterSpecified = true,
                switchName = switchName,
            };
            var requestPrivateDataElement = XmlHelper.InstanceToElement<SystemRegisterPrivateData>(requestPrivateData);

            var request = new SystemRegister
            {
                requestTypes = new RequestTypes
                {
                    systemStatus = true,
                    systemStatusSpecified = true,
                    //requestSystemStatus = true,
                    //requestSystemStatusSpecified = true,
                },
                requestedStatusFilter = new StatusFilter
                {
                    normal = true,
                    normalSpecified = true,
                    disabled = true,
                    disabledSpecified = true,
                },
                extensions = new CSTACommonArguments
                {
                    privateData = new CSTAPrivateData
                    {
                        Item = new CSTAPrivateDataPrivate
                        {
                            Any = new XmlElement[] { requestPrivateDataElement }
                        }
                    }
                }

            };
            return request;
        }

        public GetThirdPartyDeviceId NewGetThirdPartyDeviceId(string extension, string switchName)
        {
            return new GetThirdPartyDeviceId
            {
                switchName = switchName,
                extension = extension,
                deviceInstance = DeviceInstance.Item0
            };
        }

        public ReleaseDeviceId NewReleaseDeviceId(DeviceID deviceId)
        {
            return new ReleaseDeviceId
            {
                device = deviceId
            };
        }

        public GetDeviceId NewGetDeviceId(string extension, string switchIpAddress, string switchName)
        {
            return new GetDeviceId
            {
                switchIPInterface = switchIpAddress,
                extension = extension,
                switchName = switchName
            };
        }

        public GetDeviceId NewGetDeviceId(string extension, string switchIpAddress)
        {
            return new GetDeviceId
            {
                switchIPInterface = switchIpAddress,
                extension = extension
            };
        }

        public GetMonitorList NewGetMonitorList(string sessionId)
        {
            return new GetMonitorList
            {
                sessionID = sessionId                
            };
        }

        public MonitorStop NewMonitorStop(string monitorCrossReferenceId)
        {
            return new MonitorStop
            {
                monitorCrossRefID = monitorCrossReferenceId
            };
        }

        public MakeCall NewMakeCall(DeviceID callingDevice, DeviceID calledDirectoryNumber)
        {
            return new MakeCall
            {
                callingDevice = callingDevice,
                calledDirectoryNumber = calledDirectoryNumber
            };
        }

        public AnswerCall NewAnswerCall(string callToBeAnsweredDeviceId, string callToBeAnsweredCallId)
        {
            var request = new AnswerCall
            {
                callToBeAnswered = new ConnectionID
                {
                    Items = new object[] { null, callToBeAnsweredCallId }
                }
            };
            request.callToBeAnswered.Items[0] = new LocalDeviceID
            {
                bitRate = LocalDeviceIDBitRate.constant,
                typeOfNumber = LocalDeviceIDTypeOfNumber.other,
                Value = callToBeAnsweredDeviceId
            };
            return request;
        }

        public RequestSystemStatus NewRequestSystemStatus(string systemStatusRegisterId, string switchName)
        {
            var requestSystemStatusPrivateData = new RequestSystemStatusPrivateData
            {
                getTlinkStatus = new RequestSystemStatusPrivateDataGetTlinkStatus
                {
                    switchName = switchName
                }
            };
            return new RequestSystemStatus
            {
                sysStatRegisterID = systemStatusRegisterId,
                extensions = NewCSTACommonArguments<RequestSystemStatusPrivateData>(requestSystemStatusPrivateData)
            };
        }

    }
}
