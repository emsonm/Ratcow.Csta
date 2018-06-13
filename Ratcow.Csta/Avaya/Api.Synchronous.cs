/* Ratcow.Csta.Avaya is an Avaya AES XML API integration for MS.Net.
 * Copyright (C) 2018 Ratcow Software and Matt Emson. 
 * 
 * This software is dual licensed. It may be freely used under the GPL3,
 * but any for any proprietry commercial use, it must me licensed under 
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
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Ratcow.Csta.Avaya
{

    using Ecma323.Ed3.AvayaExtensions;
    using Ecma354;
    using Events;
    using Serialization;
    using Logs.Logging;
    
    using DeviceId = Ratcow.Csta.Ecma323.Ed3.AvayaExtensions.DeviceID;

    partial class Api
    {
        /// <summary>
        /// This starts the application logic
        /// </summary>
        public bool StartApplicationSessionAndWait()
        {
            var request = protocol.NewStartApplicationSession(User, Password);
            var invokeId = comms.SendRequest(request);

            var (Response, InvokeId) = comms.ReadXMLMessage(true); //todo - we should not wait here
            var rootElement = XmlHelper.GetRootElementName(Response);

            if (string.Compare(rootElement, nameof(StartApplicationSessionPosResponse)) == 0) //responseString.Contains("StartApplicationSessionPosResponse"))
            {
                Debug.Assert(invokeId == InvokeId);
                var response = new CstaEventArgs<StartApplicationSessionPosResponse>(Response)
                {
                    InvokeId = invokeId
                };

                CtsaEvent?.Invoke(this, response);

                SessionId = response?.EventData?.sessionID;

                StartAutoKeepAlive(SessionId, 55000);

                return true;
            }
            else if (string.Compare(rootElement, nameof(StartApplicationSessionNegResponse)) == 0) //responseString.Contains("StartApplicationSessionNegResponse"))
            {
                Debug.Assert(invokeId == InvokeId);
                var response = new CstaEventArgs<StartApplicationSessionNegResponse>(Response)
                {
                    InvokeId = InvokeId
                };

                CtsaEvent?.Invoke(this, response);

                //return response?.EventData?.errorCode?.ToString()??string.Empty;
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DeviceId ObtainDeviceId(string extension, object userData = null)
        {
            return ObtainDeviceId(extension, CallServer, userData);
        }

        /// <summary>
        /// 
        /// </summary>
        public DeviceId ObtainDeviceId(string extension, string switchIpAddress, object userData = null)
        {
            var (Response, Result, _) = WaitFor<GetThirdPartyDeviceIdResponse>(GetDeviceId(extension, switchIpAddress, userData));

            if (Result)
            {
                return Response?.device ?? null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DeviceId ObtainThirdPartyDeviceId(string extension, object userData = null)
        {
            return ObtainThirdPartyDeviceId(extension, CallServerName, userData);
        }

        /// <summary>
        /// 
        /// </summary>
        public DeviceId ObtainThirdPartyDeviceId(string extension, string switchName, object userData = null)
        {
            var (Response, Result, _) = WaitFor<GetThirdPartyDeviceIdResponse>(GetThirdPartyDeviceId(extension, switchName, userData));

            if (Result)
            {
                return Response?.device ?? null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SystemRegisterAndWait(string switchName = null, object userData = null)
        {
            var (_, Result, _) = WaitFor<SystemRegisterResponse>(SystemRegister(switchName, userData));
            return Result;
        }

        /// <summary>
        /// Made this more expressive - we now have a flag on success and a value we returned.
        /// The value is only guaranteed to be set if the method succeeds. We also trap for 
        /// errors - and we fall through and set the lock even if we get nothing specific, so 
        /// we don't block needlessly.
        /// 
        /// We now also have the error (well, so long as it was a UniversalFailure/CSTAErrorCode.)
        /// </summary>
        private (T Response, bool Result, UniversalFailure Error) WaitFor<T>(int invokeId, int timeout = 5000)
        {
            var response = default(T);
            var result = false;
            var error = default(UniversalFailure);

            var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                using (var operationLock = new ManualResetEvent(false))
                {
                    EventHandler<CstaEventArgs> responseHandler = (s, e) =>
                    {
                        if (e.InvokeId == invokeId)
                        {
                            if (e.RawEventData is T ev)
                            {
                                response = ev;
                                result = true;
                                operationLock.Set();
                            }
                            else if (e.RawEventData is UniversalFailure f)
                            {
                            //TODO - do something useful with this error...
                            error = f;
                                operationLock.Set();
                            }
                            else
                            {
                            //this is here because if we receive something we are not expecting, we should not block.
                            operationLock.Set();
                            }
                        }
                    };

                    try
                    {
                        CtsaEvent += responseHandler;
                        while (timeout > 0)
                        {
                            if (operationLock.WaitOne(100))
                            {
                                break;
                            }

                            timeout -= 200;
                            System.Threading.Thread.Sleep(0);

                        }
                    }
                    finally
                    {
                        CtsaEvent -= responseHandler;
                    }
                }
            }).Wait(timeout);

            return (response, result, error);
        }
    }
}
