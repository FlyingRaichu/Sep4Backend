using System.Net;
using System.Net.Sockets;
using System.Text;
using IoTInterfacing.Interfaces;
using System;

namespace IoTInterfacing.Implementations;

public class ConnectionController : IConnectionController
{
    //Right, so the idea here is to set up a conn to listen to. From there, the client would be the Arduino. We could from
    //there to have methods that handle request/response protocols, one for requests from here to the arduino, and one for vise-versa
    public bool IsConnected { get; set; } = true;

    public async Task EstablishConnection(int port)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Waiting for Arduino connection on port {port}...");

        while (IsConnected)
        {
            // Accept the client connection
            var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Arduino connected!");

            // Handle the client asynchronously
            _ = HandleClientAsync(client);
        }
    }

    public async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            // Get the network stream for reading/writing
            var stream = client.GetStream();

            while (client.Connected)
            {
                // Read data from the client
                var buffer = new byte[256];
                var bytesRead = await stream.ReadAsync(buffer);

                // Check if any data was received
                if (bytesRead == 0)
                {
                    break;
                }

                // Data was received, process it and send a response
                var request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received from Arduino: {request}");

                // Process the request (if needed) and send a response
                var response = "8008";
                var responseData = Encoding.ASCII.GetBytes(response);
                await stream.WriteAsync(responseData);
                Console.WriteLine($"Sent to Arduino: {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
        }
        finally
        {
            // Close the connection
            client.Close();
            Console.WriteLine("Arduino disconnected!");
        }
    }
}