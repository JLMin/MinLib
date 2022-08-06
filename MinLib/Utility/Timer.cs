using System.Diagnostics;

namespace MinLib.Utility;

public class Timer : IDisposable
{
    private static Stopwatch? Watch;
    private Stopwatch watch;
    public static Timer TimeThis => new Timer();

    private Timer()
    {
        this.watch = Stopwatch.StartNew();
    }

    public static void Start()
    {
        if (Watch == null)
        {
            Watch = Stopwatch.StartNew();
        }
    }

    public static void Stop()
    {
        if (Watch != null)
        {
            Watch.Stop();
            double time = Watch.ElapsedMilliseconds / 1000.0;
            ConsoleLog.Debug($"\n------------------" +
                             $"\nElapsed: {time:F2} sec");
            Watch = null;
        }
    }

    public void Dispose()
    {
        watch.Stop();
        double time = this.watch.ElapsedMilliseconds / 1000.0;
        ConsoleLog.Debug($"\n------------------" +
                         $"\nElapsed: {time:F2} sec");
        GC.SuppressFinalize(this);
    }
}
