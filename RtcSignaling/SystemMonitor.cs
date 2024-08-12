namespace RtcSignaling;
using System.Timers;
using Serilog;
using Timer = System.Timers.Timer;

public class SystemMonitor
{
    private readonly AppContext _context;
    private readonly Timer _timer;
    
    public SystemMonitor(AppContext ctx)
    {
        _context = ctx;
        
        _timer = new Timer();
        _timer.AutoReset = true;
        _timer.Interval = 2000;
        _timer.Enabled = true;
        _timer.Elapsed += OnTimerOut;
        _timer.Start();
    }
    
    private void OnTimerOut(object? sender, ElapsedEventArgs e)
    {
        CheckSystem();
    }
    
    private void CheckSystem()
    {
        _context.GetRoomManager().CleanEmptyRooms();
        _context.GetClientManager().CleanOfflineClients();
    }
    
}