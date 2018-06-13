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

using System.Linq;
using System.Xml;

namespace Ratcow.Csta.Avaya.Dmcc
{
    using Ecma354;
    using Ecma323.Ed3.AvayaExtensions;
    using Serialization;

    public abstract class DmccCommonDataProtocol: IDmccCommonDataProtocol
    {
        /// <summary>
        /// The classes that impment the protocol must override this
        /// </summary>
        public abstract DmccProtocolType Protocol { get; }

        /// <summary>
        /// Creates a CSTACommonArguments from a single item
        /// </summary>
        public CSTACommonArguments NewCSTACommonArguments<T>(T value)
        {
            return NewCSTACommonArguments(new XmlElement[] { XmlHelper.InstanceToElement(value) });

            //return new CSTACommonArguments
            //{
            //    privateData = new CSTAPrivateData
            //    {
            //        Item = new CSTAPrivateDataPrivate
            //        {
            //            Any = new XmlElement[] { XmlHelper.InstanceToElement(value) }
            //        }
            //    }
            //};
        }

        /// <summary>
        /// Creates a CSTACommonArguments from multiple items
        /// </summary>
        public CSTACommonArguments NewCSTACommonArguments(params object[] value)
        {
            return NewCSTACommonArguments(value.Select(c => XmlHelper.InstanceToElement(c.GetType(), c))?.ToArray());

            //return new CSTACommonArguments
            //{
            //    privateData = new CSTAPrivateData
            //    {
            //        Item = new CSTAPrivateDataPrivate
            //        {
            //            Any = value.Select(c => XmlHelper.InstanceToElement(c.GetType(), c)).ToArray()
            //        }
            //    }
            //};
        }

        /// <summary>
        /// Creates a CSTACommonArguments from multiple items
        /// </summary>
        public CSTACommonArguments NewCSTACommonArguments(XmlElement[] value)
        {
            return new CSTACommonArguments
            {
                privateData = new CSTAPrivateData
                {
                    Item = new CSTAPrivateDataPrivate
                    {
                        Any = value
                    }
                }
            };
        }
    }
}
