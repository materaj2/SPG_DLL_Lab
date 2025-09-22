// File: VulnService.cs
using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

public class VulnService : ServiceBase
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr LoadLibrary(string lpFileName);

    private Thread worker;

    public VulnService()
    {
        this.ServiceName = "LabService";
        this.CanStop = true;
        this.CanPauseAndContinue = false;
        this.AutoLog = true;
    }

    protected override void OnStart(string[] args)
    {
        worker = new Thread(() =>
        {
            try
            {
                // intentionally load DLL without full path (vulnerable to DLL hijack)
                LoadLibrary("HijackMe.dll");
                // keep thread alive for demo; real work could be here
                while (true)
                {
                    Thread.Sleep(10000);
                }
            }
            catch (ThreadAbortException) { /* stopping */ }
            catch (Exception ex)
            {
                try { EventLog.WriteEntry(this.ServiceName, "OnStart exception: " + ex.Message, EventLogEntryType.Error); } catch {}
            }
        });
        worker.IsBackground = true;
        worker.Start();
    }

    protected override void OnStop()
    {
        try
        {
            if (worker != null && worker.IsAlive)
            {
                worker.Abort();
                worker.Join(2000);
            }
        }
        catch { }
    }

    // Entry point for service and console/debug mode
    public static void Main(string[] args)
    {
        bool runAsConsole = false;
        foreach (var a in args)
        {
            if (string.Equals(a, "-console", StringComparison.OrdinalIgnoreCase))
            {
                runAsConsole = true;
                break;
            }
        }

        if (Environment.UserInteractive || runAsConsole)
        {
            // Run as console for debugging
            Console.WriteLine("Starting VulnService in console mode...");
            VulnService svc = new VulnService();
            svc.OnStart(args);
            Console.WriteLine("Press Enter to stop...");
            Console.ReadLine();
            svc.OnStop();
        }
        else
        {
            // Run as Windows Service
            ServiceBase.Run(new VulnService());
        }
    }
}
