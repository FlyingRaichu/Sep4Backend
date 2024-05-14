using System.Net.Sockets;

namespace IoTInterfacing.Interfaces;

public interface IConnectionController
{
    Task EstablishConnectionAsync(int port);
    Task<string> SendRequestToArduinoAsync(string apiParameters);
}