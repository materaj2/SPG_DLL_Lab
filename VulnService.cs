using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

public class VulnService : ServiceBase
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr LoadLibrary(string lpFileName);

    private Thread worker;

    public VulnService() { this.ServiceName = "LabService"; }

    protected override void OnStart(string[] args)
    {
        worker = new Thread(() =>
        {
            try { LoadLibrary("HijackMe.dll"); }
            catch (Exception ex) { try { EventLog.WriteEntry("VulnService: " + ex.Message, EventLogEntryType.Error); } catch {} }
        });
        worker.IsBackground = true;
        worker.Start();
    }

    protected override void OnStop()
    {
        try { if (worker != null && worker.IsAlive) worker.Abort(); } catch {}
    }

    public static void Main() { ServiceBase.Run(new VulnService()); }
}
