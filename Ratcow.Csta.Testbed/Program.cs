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

using Ratcow.Csta.Avaya;
using Ratcow.Csta.Ecma323.Ed3.AvayaExtensions;
using Ratcow.Csta.Ecma354;
using Ratcow.Csta.Events;
using System;

namespace Ratcow.Csta.Testbed
{
    using Logs.Initialization;
    using Logs.Logging;
    using System.Diagnostics;
    using System.Threading;

    class Program
    {
        const string TEST_CONST = "A test";

        static void Main(string[] args)
        {
            //initiate the logging framework
            LogInitialiser.InitLog4Net();

            var logger = LogProvider.For<Program>();

            var extension = new DeviceData
            {
                Extension = 1233
            };

            var extension2 = new DeviceData
            {
                Extension = 1237
            };

            var vdn = new DeviceData
            {
                Extension = 1335
            };

            //var makecallInvokeId = int.MinValue;

            var vdnrrInvokeId = int.MinValue;
            var vdrRouteRegistered = string.Empty;

            var callServer = "127.0.0.1";
            var dmccServer = "127.0.0.1";
            var user = "testuser";
            var password = "testpassword";

            var api = new Api
            {
                CallServer = callServer,
                CallServerName = "RATCOW",
                DmccServer = dmccServer,
                User = user,
                Password = password
            };

            //hook the event handler so we can display some "useful" data.
            api.CtsaEvent += (object s, CstaEventArgs e) =>
            {
                logger.Debug(e.RawEventData.ToString());


                if (e.RawEventData is StartApplicationSessionPosResponse saspr)
                {
                    Console.WriteLine($"Success {saspr.sessionID}");
                }
                else if (e.RawEventData is UnmappedData u)
                {
                    Console.WriteLine($"{u.Data}"); //debug an UnmappedData event
                }
                else if (extension.GetDeviceIdInvokeId == e.InvokeId && e.RawEventData is GetDeviceIdResponse gdidr)
                {
                    //check the user data
                    if (e.UserData is TestData td)
                    {
                        Debug.Assert(string.Compare(td.Data, TEST_CONST) == 0, "UserData validity failed");
                    }

                    //we got the deviceId we requested
                    extension.DeviceId = gdidr.device;
                    Console.WriteLine($"Success {gdidr.device.Value}, InvokeId: {e.InvokeId}");

                    //we will now monitor the device
                    extension.MonitorInvokeId = api.MonitorStart(extension.DeviceId, true);
                    Console.WriteLine($"MonitorStartRequest {gdidr.device.Value}, InvokeId: {extension.MonitorInvokeId}");
                }
                else if (extension.GetDeviceIdInvokeId == e.InvokeId && e.RawEventData is GetThirdPartyDeviceIdResponse gtpdidr)
                {
                    //we got the deviceId we requested
                    extension.DeviceId = gtpdidr.device;
                    Console.WriteLine($"Success {gtpdidr.device.Value}, InvokeId: {e.InvokeId}");

                    //we will now monitor the device
                    extension.MonitorInvokeId = api.MonitorStart(extension.DeviceId, true);
                    Console.WriteLine($"MonitorStartRequest {gtpdidr.device.Value}, InvokeId: {extension.MonitorInvokeId}");
                }
                else if (extension2.GetDeviceIdInvokeId == e.InvokeId && e.RawEventData is GetDeviceIdResponse gdidr2)
                {
                    //we got the deviceId we requested
                    extension2.DeviceId = gdidr2.device;
                    Console.WriteLine($"Success {gdidr2.device.Value}, InvokeId: {e.InvokeId}");

                    //we will now make call
                    //makecallInvokeId = api.MakeCall(extensionDeviceId, extension2DeviceId);
                }
                else if (extension.MonitorInvokeId == e.InvokeId && e.RawEventData is MonitorStartResponse msr)
                {
                    extension.MonitorCrossReferenceId = msr.monitorCrossRefID;
                    Console.WriteLine($"MonitorStartRequest {msr.monitorCrossRefID}, InvokeId: {e.InvokeId}");
                }
                else if (vdn.MonitorInvokeId == e.InvokeId && e.RawEventData is MonitorStartResponse vdnmsr)
                {
                    vdn.MonitorCrossReferenceId = vdnmsr.monitorCrossRefID;
                    Console.WriteLine($"MonitorStartRequest {vdnmsr.monitorCrossRefID}, InvokeId: {e.InvokeId}");


                    vdnrrInvokeId = api.RouteRegister(vdn.DeviceId);
                }
                //else if (vdnInvokeId == e.InvokeId && e.RawEventData is GetDeviceIdResponse vgdidr)
                //{
                //    //we got the deviceId we requested
                //    vdnDeviceId = vgdidr.device;
                //    Console.WriteLine($"Success {vgdidr.device.Value}, InvokeId: {e.InvokeId}");

                //    vdnMsInvokeId = api.MonitorStart(vdnDeviceId, true);
                //}

                //else if (vdnInvokeId == e.InvokeId && e.RawEventData is GetThirdPartyDeviceIdResponse tpvgdidr)
                //{
                //    //we got the deviceId we requested
                //    vdnDeviceId = tpvgdidr.device;
                //    Console.WriteLine($"Success {tpvgdidr.device.Value}, InvokeId: {e.InvokeId}");

                //    //we will now monitor the device
                //    vdnMsInvokeId = api.MonitorStart(vdnDeviceId, false);                    
                //}
                else if (vdnrrInvokeId == e.InvokeId && e.RawEventData is RouteRegisterResponse rrr)
                {
                    vdrRouteRegistered = rrr.routeRegisterReqID;

                    Console.WriteLine($"Success {rrr.routeRegisterReqID}, InvokeId: {e.InvokeId}");
                }
                else if (e.RawEventData is UniversalFailure uf)
                {
                    Console.WriteLine($"UniversalFailure:: {uf.Item}, InvokeId: {e.InvokeId}"); //debug an unknown event
                }
                else
                {
                    Console.WriteLine($"{e.RawEventType.Name}, InvokeId: {e.InvokeId}"); //debug an unknown event
                }

            };

            if (api.Init() && api.StartApplicationSessionAndWait())
            {
                api.StartEventListener();

                if (api.SystemRegisterAndWait())
                {

                    api.RequestSystemStatus();

                    //extension.GetDeviceIdInvokeId = TestGetDeviceId(api, extension.Extension);
                    //extension2InvokeId = TestGetDeviceId(api, extension2);

                    //extension.MonitorInvokeId = api.MonitorStart(extension.DeviceId, true);

                    vdn.DeviceId = api.ObtainThirdPartyDeviceId(vdn.Extension.ToString());

                    vdn.MonitorInvokeId = api.MonitorStart(vdn.DeviceId, true);

                    //api.GetMonitorList();


                    Console.WriteLine("Running - press a key to begin shot down");
                    Console.ReadLine();

                    //Okay - this is just me being lazy and ensuring events are processed before we exit.
                    Console.WriteLine("Cleaning up....Monitors");

                    if (!string.IsNullOrEmpty(extension.MonitorCrossReferenceId))
                    {
                        api.MonitorStop(extension.MonitorCrossReferenceId);
                    }

                    if (!string.IsNullOrEmpty(vdn.MonitorCrossReferenceId))
                    {
                        api.MonitorStop(vdn.MonitorCrossReferenceId);
                    }

                    Console.ReadLine();
                    Console.WriteLine("Cleaning up....devices");
                    api.ReleaseDeviceId(vdn.DeviceId);
                    api.ReleaseDeviceId(extension.DeviceId);
                    //api.ReleaseDeviceId(extension2DeviceId);
                }
                else
                {
                    Console.WriteLine("System register failed");
                }

                api.RequestSystemStatus();

                Console.ReadLine();
                api.StopEventListener();
                api.Done();
            }
        }

        private static int TestGetDeviceId(Api api, int extension)
        {
            var deviceInvokeId = api.GetThirdPartyDeviceId($"{extension}", new TestData { Data = TEST_CONST });
            Console.WriteLine($"api.GetDeviceId(\"{extension}\") => InvokeId: {deviceInvokeId}");
            return deviceInvokeId;
        }
    }

    public class TestData
    {
        public string Data { get; set; }
    }
}
