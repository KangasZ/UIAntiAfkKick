using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace UiAntiAfkKick;

class Services
{
    [PluginService] static internal IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] static internal ICondition Condition { get; private set; }
    [PluginService] static internal IFramework Framework { get; private set; }
    [PluginService] static internal ISigScanner SigScanner { get; private set; }
    [PluginService] static internal IGameInteropProvider Hook { get; private set; }
    [PluginService] static internal IPluginLog PluginLog { get; private set; }
}