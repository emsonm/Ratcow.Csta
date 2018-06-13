
using System;
using System.Collections.Generic;
using System.Text;

namespace Ratcow.Csta.Avaya.Dmcc.Server
{
    using Ecma354;

    public abstract class DmccServerDataProtocolBase: DmccCommonDataProtocol
    {
        public static StartApplicationSessionNegResponse NewStartApplicationSessionNegResponse(StartApplicationSessionNegResponseErrorCodeDefinedError error)
        {
            return new StartApplicationSessionNegResponse
            {
                errorCode = new StartApplicationSessionNegResponseErrorCode
                {
                    Item = error
                }
            };
        }
    }
}
