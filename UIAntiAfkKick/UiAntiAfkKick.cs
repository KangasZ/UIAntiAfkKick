using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace UiAntiAfkKick;

public sealed class UiAntiAfkKick : IDalamudPlugin
{
    public string Name => "UI Anti AFK Kick";
    private AntiAfkLogic antiAfkLogic { get; set; }
    private PluginCommands pluginCommands { get; set; }
    private Configuration configuration { get; set; }
    private PluginUi pluginUi { get; set; }
    
    public UiAntiAfkKick(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager)
    {
        pluginInterface.Create<Services>(); // Todo: Remove this

        configuration = new Configuration(pluginInterface);
        pluginUi = new PluginUi(pluginInterface, configuration);
        pluginCommands = new PluginCommands(commandManager, pluginUi);
        antiAfkLogic = new AntiAfkLogic(configuration);
    }

    public void Dispose()
    {
        pluginCommands.Dispose();
        antiAfkLogic.Dispose();
    }
}