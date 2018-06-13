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

namespace Ratcow.Csta.Avaya.Dmcc.Server
{
    using Ecma354;
    using Ecma323.Ed3.AvayaExtensions;

    public interface IDmccServerDataProtocol: IDmccCommonDataProtocol
    {
        UniversalFailure NewUniversalFailure(object error);
        StartApplicationSessionPosResponse NewStartApplicationSessionPosResponse(string sessionId, string protocol, string sessionDuration);
        SystemRegisterResponse NewSystemRegisterResponse(int systemRegisterResponseId);
        GetDeviceIdResponse NewGetDeviceIdResponse(DeviceID device);
        GetThirdPartyDeviceIdResponse NewGetThirdPartyDeviceIdResponse(DeviceID device);
        ReleaseDeviceIdResponse NewReleaseDeviceIdResponse();
    }
}
