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
    }

    protected override void OnStart(string[] args)
    {
        // Run work on separate thread to avoid blocking SCM
        worker = new Thread(() =>
        {
            try
            {
                // intentionally load DLL without full path (vulnerable)
                LoadLibrary("HijackMe.dll");
            }
            catch (Exception ex)
            {
                // optional: write to EventLog
                try { EventLog.WriteEntry("VulnService failed: " + ex.Message, EventLogEntryType.Error); } catch {}
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
                worker.Abort(); // simple lab approach (not recommended for prod)
            }
        }
        catch { }
    }

    public static void Main()
    {
        ServiceBase.Run(new VulnService());
    }
}
