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


using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


/// <summary>
/// This is an Avaya specific implementation of the client API.
/// </summary>
namespace Ratcow.Csta.Avaya
{
    using Logs.Initialization;
    using Logs.Logging;
    using Ecma323.Ed3.AvayaExtensions;
    using Ecma354;
    using Engine.Core;
    using Engine.Core.Responses;
    using Events;
    using Serialization;
    using Dmcc;
    using Dmcc.Client;

    /// <summary>
    /// This is the base API object for the Avaya CSTA XML API.
    /// </summary>
    public partial class Api
    {
        readonly ILog logger = null;
        readonly IDmccClientDataProtocol protocol = null;

        /// <summary>
        /// TODO - refactor this in to a more secure and reliable abstraction.
        /// </summary>
        readonly Dictionary<int, object> UserData = new Dictionary<int, object>();

        public string CallServerName { get; set; }
        public string CallServer { get; set; }
        public string DmccServer { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public DmccProtocolType DmccProtocolVersion { get; private set; }
        public int Port
        {
            get
            {
                if (Secure)
                {
                    return 4722;
                }
                else
                {
                    return 4721;
                }
            }
        }

        bool eventListenerFlag = false;

        public bool Secure { get; set; }

        public string SessionId { get; private set; }
        public string SystemStatusRegisterId { get; private set; }

        /// <summary>
        /// This is raised for all CSTA events
        /// </summary>
        public EventHandler<CstaEventArgs> CtsaEvent;

        private TcpCommsClient comms = null;

        /// <summary>
        /// DmccProtocolType defaults to v63 as we can easily test agains that level.
        /// </summary>        
        public Api(DmccProtocolType protocol = DmccProtocolType.v63)
        {
            LogInitialiser.InitLog4Net();
            logger = LogProvider.For<Api>();

            DmccProtocolVersion = protocol;
            this.protocol = DmccClientDataProtocolFactory.Create(DmccProtocolVersion);
            SessionId = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Init()
        {
            comms = new TcpCommsClient { };

            return comms?.Open(DmccServer, Port, Secure) == ConnectionResponse.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Done()
        {
            StopAutoKeepAlive();
        }

        /// <summary>
        /// 
        /// </summary>
        public Task StartEventListener()
        {
            eventListenerFlag = true;

            return Task.Run(() => InternalEventListenerLoop());
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopEventListener()
        {
            eventListenerFlag = false;
        }

        /// <summary>
        /// The entry point for processing event data returned from the AES
        /// </summary>
        void InternalEventListenerLoop()
        {
            var response = default(CstaEventArgs);

            while (eventListenerFlag)
            {
                if (!comms.HasData)
                {
                    Task.Delay(200);
                    continue;
                }
                else
                {
                    var responseValue = comms.ReadXMLMessage();
                    var root = XmlHelper.GetRootElementName(responseValue.Data);

                    switch (root)
                    {
                        #region Events

                        case nameof(OriginatedEvent):
                            response = new CstaEventArgs<OriginatedEvent>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(DeliveredEvent):
                            response = new CstaEventArgs<DeliveredEvent>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(EstablishedEvent):
                            response = new CstaEventArgs<EstablishedEvent>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(ConnectionClearedEvent):
                            response = new CstaEventArgs<ConnectionClearedEvent>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(CallClearedEvent):
                            response = new CstaEventArgs<CallClearedEvent>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(FailedEvent):
                            response = new CstaEventArgs<FailedEvent>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(TransferedEvent):
                            response = new CstaEventArgs<TransferedEvent>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(ConferencedEvent):
                            response = new CstaEventArgs<ConferencedEvent>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        #endregion

                        #region Responses

                        case nameof(AnswerCallResponse):
                            response = new CstaEventArgs<AnswerCallResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(MakeCallResponse):
                            response = new CstaEventArgs<MakeCallResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(ResetApplicationSessionTimerPosResponse):
                            response = new CstaEventArgs<ResetApplicationSessionTimerPosResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(GetDeviceIdResponse):
                            response = new CstaEventArgs<GetDeviceIdResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(ReleaseDeviceIdResponse):
                            response = new CstaEventArgs<ReleaseDeviceIdResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(GetThirdPartyDeviceIdResponse):
                            response = new CstaEventArgs<GetThirdPartyDeviceIdResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(GetMonitorListResponse):
                            response = new CstaEventArgs<GetMonitorListResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(GetMonitorListEvent):
                            response = new CstaEventArgs<GetMonitorListEvent>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(MonitorStartResponse):
                            response = new CstaEventArgs<MonitorStartResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(MonitorStopResponse):
                            response = new CstaEventArgs<MonitorStopResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(RouteRegisterResponse):
                            response = new CstaEventArgs<RouteRegisterResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(RouteRegisterCancelResponse):
                            response = new CstaEventArgs<RouteRegisterCancelResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(StartApplicationSessionPosResponse):
                            response = new CstaEventArgs<StartApplicationSessionPosResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };

                            //we should also set the SessionId
                            if (response.RawEventData is StartApplicationSessionPosResponse spr)
                            {
                                if (string.Compare(SessionId, spr.sessionID) != 0)
                                {
                                    SessionId = spr.sessionID;
                                }
                            }
                            break;

                        case nameof(StartApplicationSessionNegResponse):
                            response = new CstaEventArgs<StartApplicationSessionNegResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case nameof(SystemRegisterResponse):
                            response = new CstaEventArgs<SystemRegisterResponse>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };

                            //we should also set the SystemStatusRegisterId
                            if (response.RawEventData is SystemRegisterResponse srr)
                            {
                                if (string.Compare(SystemStatusRegisterId, srr.sysStatRegisterID) != 0)
                                {
                                    SystemStatusRegisterId = srr.sysStatRegisterID;
                                }
                            }

                            break;

                        #endregion

                        #region Errors

                        case nameof(CSTAException):
                            response = new CstaEventArgs<CSTAException>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };
                            break;

                        case "CSTAErrorCode":
                            response = new CstaEventArgs<UniversalFailure>(responseValue.Data)
                            {
                                InvokeId = responseValue.InvokeId,
                                UserData = GetUserData(responseValue.InvokeId)
                            };

                            break;

                        #endregion

                        default:
                            response = CreateRawData(responseValue);
                            break;
                    }

                    //TODO - filter events with no actual value
                    if (response != null)
                    {
                        CtsaEvent?.Invoke(this, response);
                    }
                    response = null;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        CstaEventArgs CreateRawData((string Response, int InvokeId) responseValue)
        {
            CstaEventArgs response;
            //There is no class for this, so we pass the raw data.
            var unmappedData = new UnmappedData
            {
                Data = responseValue.Response,
            };

            response = new CstaEventArgs<UnmappedData>
            {
                RawEventData = unmappedData,
                EventData = unmappedData,
                InvokeId = responseValue.InvokeId,
                UserData = GetUserData(responseValue.InvokeId)
            };
            return response;
        }


        #region TODO - This will need to be refactored as a Timer is probably not the best mechanism.

        Timer sessionKeepAliveTimer = null;

        public void StartAutoKeepAlive(string session_id, int duration)
        {
            if (sessionKeepAliveTimer == null)
            {
                sessionKeepAliveTimer = new Timer(new TimerCallback(SessionKeepAlive), session_id, duration, duration);
            }
            else
            {
                sessionKeepAliveTimer.Change(duration, duration);
            }
        }

        /* This function disposes the keep alive thread */
        public void StopAutoKeepAlive()
        {
            if (sessionKeepAliveTimer != null)
            {
                sessionKeepAliveTimer.Dispose();
                sessionKeepAliveTimer = null;
            }
        }

        /* This function sends the Keep Alive messages to keep the session active */
        void SessionKeepAlive(Object stateInfo)
        {
            var request = new ResetApplicationSessionTimer
            {
                sessionID = (string)stateInfo,
                requestedSessionDuration = "180"
            };
            var invokeId = comms.SendRequest(request);

            //DEBUG
            Console.WriteLine($"SessionKeepAlive, InvokeId: {invokeId}");
        }

        #endregion

    }

}
