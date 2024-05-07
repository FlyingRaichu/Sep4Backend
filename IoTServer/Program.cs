using IoTInterfacing.Implementations;

class Program
{
    public static async Task Main(string[] args)
    {
        var connectionController = new ConnectionController();
        await connectionController.EstablishConnection(23);
    }
}