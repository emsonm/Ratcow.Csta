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

namespace Ratcow.Csta.Server.Stub
{
    using Engine;
    //using Ecma323.Ed3;
    using Ecma323.Ed3.AvayaExtensions;


    /// <summary>
    /// Because this is a dummy, and because we only want to represent 
    /// one switch - we force all resources to belong to us.
    /// </summary>
    public class DummyResourceManager : ResourceManager
    {
        public string SwitchName { get; set; }
        public string SwitchIpAddress { get; set; }

        /// <summary>
        /// Standard voice device
        /// </summary>
        public void AddExtension(string dn)
        {
            AddDevice(
                dn,
                new ResourceManagerItem
                {
                    DirectoryNumber = dn,
                    ResourceType = ResourceManagerItemType.extension,
                    ManagingSwitchIpAddress = SwitchIpAddress,
                    ManagingSwitchName = SwitchName,
                    Device = new DeviceID
                    {
                        typeOfNumber = DeviceIDTypeOfNumber.other,
                        mediaClass = new MediaClassComponents[]
                        {
                            MediaClassComponents.voice
                        },
                        bitRate = DeviceIDBitRate.constant,
                        Value = $"{dn}:{SwitchName}:{SwitchIpAddress}:0"
                    }
                }
            );
        }

        /// <summary>
        /// VDN seems to be the same....
        /// </summary>
        public void AddVdn(string dn)
        {
            AddDevice(
                dn,
                new ResourceManagerItem
                {
                    DirectoryNumber = dn, 
                    ResourceType = ResourceManagerItemType.vdn,
                    ManagingSwitchIpAddress = SwitchIpAddress,
                    ManagingSwitchName = SwitchName,
                    Device = new DeviceID
                    {
                        typeOfNumber = DeviceIDTypeOfNumber.other,
                        mediaClass = new MediaClassComponents[]
                        {
                            MediaClassComponents.voice
                        },
                        bitRate = DeviceIDBitRate.constant,
                        Value = $"{dn}:{SwitchName}:{SwitchIpAddress}:0"
                    }
                }
            );
        }
    }
}
