using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Linq;
using System.Threading;
using UiAntiAfkKick.Helpers;

namespace UiAntiAfkKick;
public unsafe class AntiAfkKick : IDisposable
{
    internal volatile bool running = true;
    //long NextKeyPress = 0;
    private IntPtr baseAddress = IntPtr.Zero;
    private float* afkTimer;
    private float* afkTimer2;
    private float* afkTimer3;
    private const int LControlKey = 162;
    private const int Space = 32;
    private const uint WM_KEYUP = 0x101;
    private const uint WM_KEYDOWN = 0x100;
    private DalamudPluginInterface pluginInterface { get; set; }
    private Configuration configInterface { get; set; }

    delegate long UnkFunc(IntPtr a1, float a2);
    Hook<UnkFunc> UnkFuncHook;

    public AntiAfkKick(DalamudPluginInterface pluginInterface, Configuration configuration)
    {
        this.pluginInterface = pluginInterface;
        this.configInterface = configuration;
        pluginInterface.Create<Svc>();
        UnkFuncHook = new(Svc.SigScanner.ScanText("48 8B C4 48 89 58 18 48 89 70 20 55 57 41 55"), UnkFunc_Dtr);
        UnkFuncHook.Enable();
    }

    public void Dispose()
    {
        if (!UnkFuncHook.IsDisposed)
        {
            if (UnkFuncHook.IsEnabled)
            {
                UnkFuncHook.Disable();
            }
            UnkFuncHook.Dispose();
        }
        running = false;
    }
    
    private void BeginWork()
    {
        afkTimer = (float*)(baseAddress + 20);
        afkTimer2 = (float*)(baseAddress + 24);
        afkTimer3 = (float*)(baseAddress + 28);
        new Thread((ThreadStart)delegate
        {
            while (running)
            {
                try
                {
                    PluginLog.Debug($"Afk timers: {*afkTimer}/{*afkTimer2}/{*afkTimer3}");
                    if (Max(*afkTimer, *afkTimer2, *afkTimer3) > configInterface.Seconds)
                    {
                        if (EventsInterface.TryFindGameWindow(out var mwh))
                        {
                            PluginLog.Debug($"Afk timer before: {*afkTimer}/{*afkTimer2}/{*afkTimer3}");
                            PluginLog.Debug($"Sending anti-afk keypress: {mwh:X16}");
                            EventsInterface.SendKeystroke(mwh, LControlKey, 200, WM_KEYDOWN, WM_KEYUP);
                            PluginLog.Debug($"Afk timer after: {*afkTimer}/{*afkTimer2}/{*afkTimer3}");

                        }
                        else
                        {
                            PluginLog.Error("Could not find game window");
                        }
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                }
            }
            PluginLog.Debug("Thread has stopped");
        }).Start();
    }

    private long UnkFunc_Dtr(IntPtr a1, float a2)
    {
        baseAddress = a1;
        PluginLog.Information($"Obtained base address: {baseAddress:X16}");
        new TickScheduler(delegate 
        {
            if (!UnkFuncHook.IsDisposed)
            {
                if (UnkFuncHook.IsEnabled)
                {
                    UnkFuncHook.Disable();
                }
                UnkFuncHook.Dispose();
                PluginLog.Debug("Hook disposed");
            }
            BeginWork();
        }, Svc.Framework);
        return UnkFuncHook.Original(a1, a2);
    }

    private static float Max(params float[] values)
    {
        return values.Max();
    }
}