using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;

namespace UiAntiAfkKick;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "UI Anti AFK Kick";

    private const string commandName = "/antiafk";

    private DalamudPluginInterface PluginInterface { get; init; }
    private CommandManager CommandManager { get; init; }
    private Configuration Configuration { get; init; }
    private Ui PluginUi { get; init; }
    private AntiAfkKick afkThread;
    
    
    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        // you might normally want to embed resources and load them from the manifest stream
        PluginUi = new Ui(Configuration);

        CommandManager.AddHandler(commandName, new CommandInfo(SettingsCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });
        
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        
        afkThread = new AntiAfkKick(pluginInterface);
    }

    public void Dispose()
    {
        PluginUi.Dispose();
        CommandManager.RemoveHandler(commandName);
        afkThread.Dispose();
    }

    private void SettingsCommand(string command, string args)
    {
        // in response to the slash command, just display our main ui
        PluginUi.Visible = true;
    }

    private void DrawUI()
    {
        PluginUi.Draw();
    }

    private void DrawConfigUI()
    {
        PluginUi.SettingsVisible = true;
    }
}