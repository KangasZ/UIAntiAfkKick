using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Linq;
using System.Threading;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
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
    private readonly IPluginLog PluginLog;

    public AntiAfkKick(Configuration configuration, IPluginLog pluginLog)
    {
        configInterface = configuration;
        this.PluginLog = pluginLog;
        BeginWork();
    }

    public void Dispose()
    {
        running = false;
    }

    private void SendKey(float timer)
    {
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
        afkTimer = &UIModule.Instance()->GetInputTimerModule()->AfkTimer;
        afkTimer2 = &UIModule.Instance()->GetInputTimerModule()->ContentInputTimer;
        afkTimer3 = &UIModule.Instance()->GetInputTimerModule()->InputTimer;
        new Thread((ThreadStart)delegate
        {
            while (running)
            {
                if (configInterface.Enabled) // todo: if the player exists in gameworld :)
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

    private static float Max(params float[] values)
    {
        return values.Max();
    }
}