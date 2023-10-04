﻿using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Linq;
using System.Threading;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
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
    private Configuration configInterface { get; set; }
    private ICondition condition => Services.Condition;
    private const string sigScan = "48 8B C4 48 89 58 18 48 89 70 20 55 57 41 54 41 56 41 57 48 8D 68 A1";
    
    delegate long UnkFunc(IntPtr a1, float a2);
    Hook<UnkFunc> UnkFuncHook;

    public AntiAfkKick(Configuration configuration)
    {
        configInterface = configuration;
        //baseAdress = Svc.SigScanner.ScanText(sigScan);
        UnkFuncHook = Services.Hook.HookFromAddress<UnkFunc>(Services.SigScanner.ScanText(sigScan), UnkFunc_Dtr);
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

    private void SendKey(float timer)
    {
        PluginLog.Debug($"Afk timers: {*afkTimer}/{*afkTimer2}/{*afkTimer3}");

        if (timer < configInterface.Seconds)
        {
            return;
        }

        if (configInterface.Instance && !condition[ConditionFlag.BoundByDuty])
        {
            PluginLog.Debug("Not bound by duty - not sending");
            return;
        }

        if (!EventsInterface.TryFindGameWindow(out var mwh))
        {
            PluginLog.Error("Could not find game window");
            return;
        }

        PluginLog.Debug($"Afk timer before: {timer}");
        PluginLog.Debug($"Sending anti-afk keypress: {mwh:X16}");
        EventsInterface.SendKeystroke(mwh, LControlKey, 200, WM_KEYDOWN, WM_KEYUP);
        PluginLog.Debug($"Afk timer after: {timer}");
        
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
                if (configInterface.Enabled)
                {
                    try
                    {
                        PluginLog.Debug($"Afk timers: {*afkTimer}/{*afkTimer2}/{*afkTimer3}");
                        SendKey(Max(*afkTimer, *afkTimer2, *afkTimer3));
                    }
                    catch (Exception e)
                    {
                        PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                    }
                }
                Thread.Sleep((int)(1000 * (configInterface.Seconds / 2)));
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
        }, Services.Framework);
        return UnkFuncHook.Original(a1, a2);
    }

    private static float Max(params float[] values)
    {
        return values.Max();
    }
}