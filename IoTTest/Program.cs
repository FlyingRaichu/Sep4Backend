using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IoTTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const int port = 23; // Port to listen on
            
            // Start listening for incoming connections
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Waiting for Arduino connection on port {port}...");

            while (true)
            {
                // Accept the client connection
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Arduino connected!");

                // Handle the client asynchronously
                _ = HandleClientAsync(client);
            }
        }

        static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                // Get the network stream for reading/writing
                NetworkStream stream = client.GetStream();

                while (client.Connected)
                {
                    // Read data from the client
                    byte[] buffer = new byte[256];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    // Check if any data was received
                    if (bytesRead == 0)
                    {
                        // No data was received, so the client has disconnected
                        break; // Exit the loop
                    }

                    // Data was received, process it and send a response
                    string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received from Arduino: {request}");

                    // Process the request (if needed) and send a response
                    string response = "8008";
                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                    Console.WriteLine($"Sent to Arduino: {response}");

                    await Task.Delay(2000); // Simulate some processing time
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
}
