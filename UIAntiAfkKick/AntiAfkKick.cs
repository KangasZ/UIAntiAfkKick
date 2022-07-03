﻿using UiAntiAfkKick;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static UiAntiAfkKick.Native.Keypress;

namespace UiAntiAfkKick;
public unsafe class AntiAfkKick : IDisposable
{
    internal volatile bool running = true;
    //long NextKeyPress = 0;
    IntPtr BaseAddress = IntPtr.Zero;
    float* AfkTimer;
    float* AfkTimer2;
    float* AfkTimer3;

    delegate long UnkFunc(IntPtr a1, float a2);
    Hook<UnkFunc> UnkFuncHook;

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

    public AntiAfkKick(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Svc>();
        UnkFuncHook = new(Svc.SigScanner.ScanText("48 8B C4 48 89 58 18 48 89 70 20 55 57 41 55"), UnkFunc_Dtr);
        UnkFuncHook.Enable();
    }

    void BeginWork()
    {
        AfkTimer = (float*)(BaseAddress + 20);
        AfkTimer2 = (float*)(BaseAddress + 24);
        AfkTimer3 = (float*)(BaseAddress + 28);
        new Thread((ThreadStart)delegate
        {
            while (running)
            {
                try
                {
                    PluginLog.Debug($"Afk timers: {*AfkTimer}/{*AfkTimer2}/{*AfkTimer3}");
                    if (Max(*AfkTimer, *AfkTimer2, *AfkTimer3) > 2f * 10f) // TODO: Change this because 20s isnt enough
                    {
                        if (Native.TryFindGameWindow(out var mwh))
                        {
                            PluginLog.Debug($"Afk timer before: {*AfkTimer}/{*AfkTimer2}/{*AfkTimer3}");
                            PluginLog.Debug($"Sending anti-afk keypress: {mwh:X16}");
                            new TickScheduler(delegate
                            {
                                SendMessage(mwh, WM_KEYDOWN, (IntPtr)LControlKey, (IntPtr)0);
                                new TickScheduler(delegate
                                {
                                    SendMessage(mwh, WM_KEYUP, (IntPtr)LControlKey, (IntPtr)0);
                                    PluginLog.Debug($"Afk timer after: {*AfkTimer}/{*AfkTimer2}/{*AfkTimer3}");
                                }, Svc.Framework, 200);
                            }, Svc.Framework, 0);
                        }
                        else
                        {
                            PluginLog.Error("Could not find game window");
                        }
                    }
                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                }
            }
            PluginLog.Debug("Thread has stopped");
        }).Start();
    }

    long UnkFunc_Dtr(IntPtr a1, float a2)
    {
        BaseAddress = a1;
        PluginLog.Information($"Obtained base address: {BaseAddress:X16}");
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

    public static float Max(params float[] values)
    {
        return values.Max();
    }
}