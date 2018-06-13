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
using System.Threading;
using System.Xml;

namespace Ratcow.Csta.Avaya
{
    using Ecma323.Ed3.AvayaExtensions;
    using Ecma354;
    using Logs.Logging;
    using DeviceId = Ecma323.Ed3.AvayaExtensions.DeviceID;


    partial class Api
    {


        /// <summary>
        /// The async version of the startup
        /// </summary>
        public int StartApplicationSession(object userData = null)
        {
            var request = protocol.NewStartApplicationSession(User, Password);
            return SetUserData(comms.SendRequest(request), userData);
        }

        /// <summary>
        /// Stop the session
        /// </summary>
        public int StopApplicationSession(string sessionId = null, object userData = null)
        {
            var sessionIdToUse = sessionId;
            if (string.IsNullOrEmpty(sessionIdToUse))
            {
                sessionIdToUse = SessionId; //the global session id - assuming it was set.
            }

            if (!string.IsNullOrEmpty(sessionIdToUse))
            {
                var request = new StopApplicationSession
                {
                    sessionID = sessionIdToUse,
                    sessionEndReason = new StopApplicationSessionSessionEndReason
                    {
                        Item = "Application Request"
                    }
                };
                return SetUserData(comms.SendRequest(request), userData);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// we need to call this to make the system startup correctly.
        /// </summary>
        public int SystemRegister(string switchName = null, object userData = null)
        {
            var switchNameToUse = switchName;
            if (string.IsNullOrEmpty(switchNameToUse))
            {
                switchNameToUse = CallServerName; //the global switch (call server) name - assuming it was set.
            }

            if (!string.IsNullOrEmpty(switchNameToUse))
            {
                var request = protocol.NewSystemRequest(switchNameToUse);
                return SetUserData(comms.SendRequest(request), userData);
            }
            else
            {
                return -1;
            }
        }

        public int SetUserData(int invokeId, object userData)
        {
            try
            {
                UserData[invokeId] = userData;
            }
            catch (Exception ex)
            {
                logger.ErrorException("Set user data failed", ex);
            }

            return invokeId;
        }

        public object GetUserData(int invokeId)
        {
            if (UserData.ContainsKey(invokeId))
            {
                return UserData[invokeId];
            }

            return null;
        }

        /// <summary>
        /// Requests a device Id for the given extension
        /// </summary>
        public int GetDeviceId(string extension, object userData = null)
        {
            var request = protocol.NewGetDeviceId(extension, CallServer);
            return SetUserData(comms.SendRequest(request), userData);
        }

        public int GetDeviceId(string extension, string switchIpAddress, object userData = null)
        {
            var request = protocol.NewGetDeviceId(extension, switchIpAddress);
            return SetUserData(comms.SendRequest(request), userData);
        }

        public int GetDeviceId(string extension, string switchIpAddress, string switchName, object userData = null)
        {
            var request = protocol.NewGetDeviceId(extension, switchIpAddress, switchName);
            return SetUserData(comms.SendRequest(request), userData);
        }

        /// <summary>
        /// Releases a device Id - and we should call this when cleaning up else
        /// we can mess up the state on the remote switch/AES.
        /// </summary>
        public int ReleaseDeviceId(DeviceId deviceId, object userData = null)
        {
            var request = protocol.NewReleaseDeviceId(deviceId);
            return SetUserData(comms.SendRequest(request), userData);
        }

        /// <summary>
        /// Requests a device Id for the given extension
        /// </summary>
        public int GetThirdPartyDeviceId(string extension, object userData = null)
        {
            return GetThirdPartyDeviceId(extension, CallServerName, userData);
        }

        /// <summary>
        /// Requests a device Id for the given extension
        /// </summary>
        public int GetThirdPartyDeviceId(string extension, string switchName, object userData = null)
        {
            var request = protocol.NewGetThirdPartyDeviceId(extension, switchName);
            return SetUserData(comms.SendRequest(request), userData);
        }



        /// <summary>
        /// Releases a device Id - and we should call this when cleaning up else
        /// we can mess up the state on the remote switch/AES.
        /// </summary>
        public int ReleaseThirdPartyDeviceId(DeviceId deviceId, object userData = null)
        {
            var request = protocol.NewReleaseDeviceId(deviceId);
            return SetUserData(comms.SendRequest(request), userData);
        }

        public int GetMonitorList(object userData = null)
        {
            var request = protocol.NewGetMonitorList(SessionId);
            return SetUserData(comms.SendRequest(request), userData);
        }

        /// <summary>
        /// Requests a device monitor to be set
        /// </summary>
        public int MonitorStart(DeviceId deviceId, bool callViaDevice, object userData = null)
        {
            var request = protocol.NewMonitorStartRequest(deviceId, callViaDevice);
            return SetUserData(comms.SendRequest(request), userData);
        }


        /// <summary>
        /// Requests a device monitor to be stopped        
        /// </summary>
        public int MonitorStop(string monitorCrossReferenceId, object userData = null)
        {
            var request = protocol.NewMonitorStop(monitorCrossReferenceId);
            return SetUserData(comms.SendRequest(request), userData);
        }

        /// <summary>
        /// Make call
        /// </summary>
        public int MakeCall(DeviceId callingDevice, DeviceId calledDirectoryNumber, object userData = null)
        {
            MakeCall request = protocol.NewMakeCall(callingDevice, calledDirectoryNumber);
            return SetUserData(comms.SendRequest(request), userData);
        }

        /// <summary>
        /// Answer a call - this is currently un tested.
        /// </summary>
        public int AnsweCall(CallIdentifier callToBeAnswered, object userData = null)
        {
            var request = protocol.NewAnswerCall(callToBeAnswered.DeviceId, callToBeAnswered.CallId);
            return SetUserData(comms.SendRequest(request), userData);
        }

        /// <summary>
        /// 
        /// </summary>
        public int RouteRegister(DeviceId device, object userData = null)
        {
            return RouteRegister(device, CallServerName, userData);
        }

        /// <summary>
        /// 
        /// </summary>
        public int RouteRegister(DeviceId device, string switchName, object userData = null)
        {
            var request = protocol.NewRouteRegister(device, switchName);
            return SetUserData(comms.SendRequest(request), userData);
        }

        /// <summary>
        /// 
        /// </summary>
        public int RouteRegisterCancel(object userData = null)
        {
            var request = new RouteRegisterCancel
            {
                //TODO - fill me in
            };
            return SetUserData(comms.SendRequest(request), userData);
        }

        public int RequestSystemStatus(object userData = null)
        {
            if (!string.IsNullOrEmpty(SystemStatusRegisterId))
            {
                return RequestSystemStatus(SystemStatusRegisterId, userData);
            }
            else
            {
                return -1;
            }
        }

        public int RequestSystemStatus(string systemStatusRegisterId, object userData = null)
        {
            if (!string.IsNullOrEmpty(CallServerName))
            {
                return RequestSystemStatus(SystemStatusRegisterId, CallServerName, userData);
            }
            else
            {
                return -1;
            }
        }

        public int RequestSystemStatus(string systemStatusRegisterId, string switchName, object userData = null)
        {
            var request = protocol.NewRequestSystemStatus(systemStatusRegisterId, switchName);
            return SetUserData(comms.SendRequest(request), userData);
        }
    }
}
