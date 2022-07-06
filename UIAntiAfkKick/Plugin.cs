using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using System;
using System.Dynamic;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Network;


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

        CommandManager.AddHandler("/antiafk", new CommandInfo(SettingsCommand)
        {
            HelpMessage = "Opens configuration for anti afk plugin",
            ShowInHelp = true
        });
        
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawUI;
        
        afkThread = new AntiAfkKick(pluginInterface);
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler("/antiafk");
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
}