using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace UiAntiAfkKick;

class Services
{
    [PluginService] static internal DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] static internal ICondition Condition { get; private set; }
    [PluginService] static internal IFramework Framework { get; private set; }
    [PluginService] static internal ISigScanner SigScanner { get; private set; }
    [PluginService] static internal IGameInteropProvider GameInteropProvider { get; private set; }
}