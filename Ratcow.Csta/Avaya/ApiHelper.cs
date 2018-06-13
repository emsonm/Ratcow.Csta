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


using Ratcow.Csta.Events;
using System;
using System.Threading;


/// <summary>
/// This is an Avaya specific implementation of the client API.
/// </summary>
namespace Ratcow.Csta.Avaya.Helpers
{
    /// <summary>
    /// this is not entiterly desirrable
    /// </summary>
    public static class ApiHelper
    {
        public static T WaitFor<T>(this Api api, int invokeId, int timeout = 5000)
        {
            var result = default(T);

            using (var operationLock = new ManualResetEvent(false))
            {
                EventHandler<CstaEventArgs> response = (s, e) =>
                {
                    if (e.InvokeId == invokeId && e.RawEventData is T ev)
                    {
                        result = ev;
                        operationLock.Set();
                    }
                };

                try
                {
                    api.CtsaEvent += response;
                    operationLock.WaitOne(timeout);
                }
                finally
                {
                    api.CtsaEvent -= response;
                }
            }

            return result;
        }
    }

}
