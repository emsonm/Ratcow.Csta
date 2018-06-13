using System;
using System.Collections.Generic;
using System.Text;
using Ratcow.Csta.Ecma323.Ed3.AvayaExtensions;
using Ratcow.Csta.Ecma354;

namespace Ratcow.Csta.Avaya.Dmcc.Server
{
    public class DmccV63ServerProtocol : DmccServerDataProtocolBase, IDmccServerDataProtocol
    {
        public override DmccProtocolType Protocol { get { return DmccProtocolType.v63; } }

        public StartApplicationSessionPosResponse NewStartApplicationSessionPosResponse(string sessionId, string protocol, string sessionDuration)
        {
            return new StartApplicationSessionPosResponse
            {
                actualProtocolVersion = protocol,
                actualSessionDuration = sessionDuration,
                sessionID = sessionId,
            };
        }

        SystemRegisterPrivateData NewSystemRegisterPrivateData(bool invertFilter)
        {
            return new SystemRegisterPrivateData
            {
                invertFilter = invertFilter,
                invertFilterSpecified = true
            };
        }

        public SystemRegisterResponse NewSystemRegisterResponse(int systemRegisterResponseId)
        {
            return new SystemRegisterResponse
            {
                sysStatRegisterID = systemRegisterResponseId.ToString(),
                actualStatusFilter = new StatusFilter
                {
                    initializing = false,
                    initializingSpecified = true,
                    enabled = false,
                    enabledSpecified = true,
                    normal = true,
                    normalSpecified = true,
                    messageLost = false,
                    messageLostSpecified = true,
                    disabled = true,
                    disabledSpecified = true,
                    partiallyDisabled = false,
                    partiallyDisabledSpecified = true,
                    overloadImminent = false,
                    overloadImminentSpecified = true,
                    overloadReached = false,
                    overloadReachedSpecified = true,
                    overloadRelieved = false,
                    overloadRelievedSpecified = true,
                },
                extensions = NewCSTACommonArguments(NewSystemRegisterPrivateData(true)),
            };
        }

        public GetDeviceIdResponse NewGetDeviceIdResponse(DeviceID device)
        {
            return new GetDeviceIdResponse
            {
                device = device,
            };
        }

        public GetThirdPartyDeviceIdResponse NewGetThirdPartyDeviceIdResponse(DeviceID device)
        {
            return new GetThirdPartyDeviceIdResponse
            {
                device = device
            };
        }

        public UniversalFailure NewUniversalFailure(object error)
        {
            return new UniversalFailure
            {
                Item = error
            };
        }

        public ReleaseDeviceIdResponse NewReleaseDeviceIdResponse()
        {
            return new ReleaseDeviceIdResponse();
        }
    }
}
