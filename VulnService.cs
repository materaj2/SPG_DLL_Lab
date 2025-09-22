// VulnService.cs
using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Diagnostics; // <- สำคัญ

public class VulnService : ServiceBase
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr LoadLibrary(string lpFileName);

    private Thread worker;
    private volatile bool _stopRequested = false;

    public VulnService()
    {
        this.ServiceName = "LabService";
        this.CanStop = true;
        this.AutoLog = true;
    }

    protected override void OnStart(string[] args)
    {
        _stopRequested = false;
        worker = new Thread(() =>
        {
            try
            {
                // intentionally load DLL without full path (vulnerable)
                LoadLibrary("HijackMe.dll");

                // keep running so service doesn't exit (demo)
                while (!_stopRequested)
                {
                    Thread.Sleep(10000);
                }
            }
            catch (Exception ex)
            {
                try { EventLog.WriteEntry(this.ServiceName, "OnStart exception: " + ex.ToString(), EventLogEntryType.Error); } catch { }
            }
        });
        worker.IsBackground = true;
        worker.Start();
    }

    protected override void OnStop()
    {
        _stopRequested = true;
        if (worker != null && worker.IsAlive)
        {
            worker.Join(3000);
        }
    }

    public static void Main(string[] args)
    {
        bool runAsConsole = false;
        foreach (var a in args)
            if (string.Equals(a, "-console", StringComparison.OrdinalIgnoreCase)) runAsConsole = true;

        if (Environment.UserInteractive || runAsConsole)
        {
            Console.WriteLine("Starting VulnService in console mode...");
            var svc = new VulnService();
            svc.OnStart(args);
            Console.WriteLine("Press Enter to stop...");
            Console.ReadLine();
            svc.OnStop();
        }
        else
        {
            ServiceBase.Run(new VulnService());
        }
    }
}
