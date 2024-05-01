using System.Net.Sockets;

namespace IoTInterfacing.Interfaces;

public interface IConnectionController
{
    Task EstablishConnection(int port);
    Task HandleClientAsync(TcpClient client);
}