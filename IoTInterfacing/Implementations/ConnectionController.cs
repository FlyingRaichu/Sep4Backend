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
    private bool IsConnected { get; set; } = true;
    private TcpClient _client = new TcpClient();

    //Method for opening a server to communicate with the Arduino
    public async Task EstablishConnectionAsync(int port)
    {
        var ipAddress = IPAddress.Any;
        var listener = new TcpListener(ipAddress, port);
        listener.Start();
        Console.WriteLine(
            $"Server's up on on ip {ipAddress.ToString()}, Waiting for Arduino connection on port {port}...");

        while (IsConnected)
        {
            // Accept the client connection
            var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Arduino connected!");

            //Set instance variable to connected client object
            _client = client;
        }
    }


    //Method to be used for making requests to the Arduino
    public async Task<string> SendRequestToArduinoAsync(string apiParameters)
    {
        var responseData = "No response returned.";
        try
        {
            //Checking for client connection
            if (_client.Connected)
            {
                // Get the network stream for reading/writing
                var stream = _client.GetStream();

                // Convert API parameters to bytes and send to Arduino
                var requestData = Encoding.ASCII.GetBytes(apiParameters);
                await stream.WriteAsync(requestData);

                // Read response from Arduino
                var buffer = new byte[256];
                var bytesRead = await stream.ReadAsync(buffer);
                responseData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"Received from Arduino: {responseData}");

                return responseData;
            }
            else
            {
                Console.WriteLine("No Arduino connected.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
        }

        return responseData;
    }
}