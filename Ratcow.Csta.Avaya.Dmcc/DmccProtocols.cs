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

namespace Ratcow.Csta.Avaya.Dmcc
{
    public enum DmccProtocolType { None, v30, v31, v40, v41, v42, v52, v61, v62, v63, v631, v633, v70}

    /// <summary>
    /// Adapter that makes using the Dmcc Api protocol strings simpler
    /// </summary>
    public static class DmccProtocols
    {
        const string ECMA = "http://www.ecma-international.org/standards/ecma-323/csta/";
        const string V30 = "3.0";
        const string V31 = ECMA + "ed2/priv1";
        const string V40 = ECMA + "ed3/priv1";
        const string V41 = ECMA + "ed3/priv2";
        const string V42 = ECMA + "ed3/priv3";
        const string V52 = ECMA + "ed3/priv4";
        const string V61 = ECMA + "ed3/priv5";
        const string V62 = ECMA + "ed3/priv6";
        const string V63 = ECMA + "ed3/priv7";
        const string V631 = ECMA + "ed3/priv8";
        const string V633 = ECMA + "ed3/priv9";
        const string V70 = ECMA + "ed3/privA";

        public static string ToString(DmccProtocolType protocol)
        {
            switch(protocol)
            {
                case DmccProtocolType.v30:
                    return V30;
                case DmccProtocolType.v31:
                    return V31;
                case DmccProtocolType.v40:
                    return V40;
                case DmccProtocolType.v41:
                    return V41;
                case DmccProtocolType.v42:
                    return V42;
                case DmccProtocolType.v52:
                    return V52;
                case DmccProtocolType.v61:
                    return V61;
                case DmccProtocolType.v62:
                    return V62;
                case DmccProtocolType.v63:
                    return V63;
                case DmccProtocolType.v631:
                    return V631;
                case DmccProtocolType.v633:
                    return V633;
                case DmccProtocolType.v70:
                    return V70;
                case DmccProtocolType.None:
                default:
                    return string.Empty;
            }
        }

        public static DmccProtocolType ToDmccProtocolType(string protocol)
        {
            switch (protocol)
            {
                case V30:
                    return DmccProtocolType.v30;
                case V31:
                    return DmccProtocolType.v31;
                case V40:
                    return DmccProtocolType.v40;
                case V41:
                    return DmccProtocolType.v41;
                case V42:
                    return DmccProtocolType.v42;
                case V52:
                    return DmccProtocolType.v52;
                case V61:
                    return DmccProtocolType.v61;
                case V62:
                    return DmccProtocolType.v62;
                case V63:
                    return DmccProtocolType.v63;
                case V631:
                    return DmccProtocolType.v631;
                case V633:
                    return DmccProtocolType.v633;
                case V70:
                    return DmccProtocolType.v70;
                default:
                    return default(DmccProtocolType);
            }
        }
    }
}
