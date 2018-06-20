using System.Net.Sockets;

namespace Ratcow.Csta.Engine
{
    public interface IServlet
    {
        TcpClient Client { get; set; }

        bool StartProcessing();
    }
}