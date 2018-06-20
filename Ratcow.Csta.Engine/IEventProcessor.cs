using Ratcow.Csta.Avaya.Dmcc.Server;
using Ratcow.Csta.Engine.Events;

namespace Ratcow.Csta.Engine
{
    public interface IEventProcessor
    {
        IDmccServerDataProtocol[] Protocols { get; }

        void CreateProtocols();
        void ProcessMessage<T>(MessageInProcessorEventArgs<T> e);
    }
}