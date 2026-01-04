using System.IO;
using System.IO.Pipes;

namespace ImageLancher;

public static class IpcClient
{
    private const string PipeName = "ImageLancher_Pipe";

    public static void Send(string message)
    {
        try
        {
            using var client = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.Out);

            client.Connect(500);

            using var writer = new StreamWriter(client)
            {
                AutoFlush = true
            };
            writer.WriteLine(message);
        }
        catch
        {
            // ワーカーがいなければ何もしない
        }
    }
}