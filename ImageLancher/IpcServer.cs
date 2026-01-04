using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace ImageLancher;

public static class IpcServer
{
    private const string PipeName = "ImageLancher_Pipe";

    public static event Action<string>? MessageReceived;

    public static void Start()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                using var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync();

                using var reader = new StreamReader(server);
                string? message = await reader.ReadLineAsync();

                if (message != null)
                {
                    MessageReceived?.Invoke(message);
                }
            }
        });
    }
}