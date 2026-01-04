using System.Linq;
using System.Windows;

namespace ImageLancher;

public partial class App : Application
{
    private static Mutex? _mutex;
    private const string MutexName = "ImageLancher_Mutex";   
    protected override void OnStartup(StartupEventArgs e)
    {
        bool createdNew;
        _mutex = new Mutex(true, MutexName, out createdNew);

        if (createdNew)
        {
            // ワーカーとして起動
            IpcServer.Start();
            base.OnStartup(e);

            var window = new MainWindow();
            window.Show();
            if (e.Args.Length > 0)
            {
                string? filePath = e.Args.FirstOrDefault();
                if (filePath is not null)
                {
                    window.ReceiveImage(filePath);
                }
            }
        }
        else
        {
            // クライアントとして起動
            string message = e.Args.FirstOrDefault() ?? "(no argument)";
            IpcClient.Send(message);

            Shutdown();
        }

    }
}