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
using System.Text;
using System.Xml;

using System.Linq;


namespace Ratcow.Csta.Avaya.Dmcc.Client
{
    using Client;

    /// <summary>
    /// Factory to create a Dmcc protocol instance for the correct API version
    /// </summary>
    public static class DmccClientDataProtocolFactory
    {
        public static IDmccClientDataProtocol Create(string protocol)
        {
            return Create(DmccProtocols.ToDmccProtocolType(protocol));
        }

        public static IDmccClientDataProtocol Create(DmccProtocolType protocol)
        {
            switch (protocol)
            {
                case DmccProtocolType.v63:
                    return new DmccV63ClientProtocol();

                default:
                    return null;
            }
        }
    }
}
