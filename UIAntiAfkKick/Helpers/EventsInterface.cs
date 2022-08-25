using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace UiAntiAfkKick.Helpers;
class EventsInterface
{
    private static readonly EventsInterface Instance = new EventsInterface();

    public static EventsInterface GetEventsInterface() // Singleton
    {
        return Instance;
    }
    private EventsInterface()
    {
        // This shouldnt be needed as most things are static. Just incase...
    }
    private struct LastInputInfo
    {
        public uint CbSize;

        public uint DwTime;
    }
    
    public static void SendKeystroke(IntPtr hwnd, int key, int delayMs, uint messageOne, uint messageTwo)
    {
        new TickScheduler(delegate
        {
            SendMessage(hwnd, messageOne, (IntPtr)key, (IntPtr)0);
            new TickScheduler(delegate
            {
                SendMessage(hwnd, messageTwo, (IntPtr)key, (IntPtr)0);
            }, Svc.Framework, delayMs);
        }, Svc.Framework, 0);
    }
    
    public static uint GetIdleTime()
    {
        LastInputInfo lastInPut = new LastInputInfo();
        lastInPut.CbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
        GetLastInputInfo(ref lastInPut);

        return ((uint)Environment.TickCount - lastInPut.DwTime);
    }

    public static long GetLastInputTime()
    {
        LastInputInfo lastInPut = new LastInputInfo();
        lastInPut.CbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
        if (!GetLastInputInfo(ref lastInPut))
        {
            throw new Exception(GetLastError().ToString());
        }
        return lastInPut.DwTime;
    }

    public static bool TryFindGameWindow(out IntPtr hwnd)
    {
        hwnd = IntPtr.Zero;
        do
        {
            hwnd = FindWindowEx(IntPtr.Zero, hwnd, "FFXIVGAME", null);
        } while (hwnd != IntPtr.Zero && GetProcessId(hwnd) != Process.GetCurrentProcess().Id);
        return hwnd != IntPtr.Zero;
    }

    private static int GetProcessId(IntPtr hWnd)
    {
        GetWindowThreadProcessId(hWnd, out var pid);
        return pid;
    }
    
    // DLL Imports
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("User32.dll")]
    private static extern bool GetLastInputInfo(ref LastInputInfo plii);

    [DllImport("Kernel32.dll")]
    private static extern uint GetLastError();

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetForegroundWindow();
    
}
