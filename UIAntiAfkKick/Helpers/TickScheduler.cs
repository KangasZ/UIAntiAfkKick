using System;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Plugin.Services;

namespace UiAntiAfkKick.Helpers;

internal class TickScheduler : IDisposable
{
    private readonly long executeAt;
    private readonly Action function;
    private readonly IFramework framework;

    public TickScheduler(Action function, IFramework framework, long delayMs = 0)
    {
        executeAt = Environment.TickCount64 + delayMs;
        this.function = function;
        this.framework = framework;
        framework.Update += Execute;
    }

    public void Dispose()
    {
        framework.Update -= Execute;
    }

    void Execute(object _)
    {
        if (Environment.TickCount64 < executeAt) return;
        try
        {
            function();
        }
        catch (Exception e)
        {
            PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
        }

        Dispose();
    }
}