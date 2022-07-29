using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;


namespace UiAntiAfkKick;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "UI Anti AFK Kick";

    private const string commandName = "/antiafk";

    private DalamudPluginInterface pluginInterface { get; init; }
    private CommandManager commandManager { get; init; }
    private Configuration configInterface { get; }
    private Ui pluginUi { get; }
    private AntiAfkKick afkThread;
    
    
    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;

        configInterface = this.pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        configInterface.Initialize(this.pluginInterface);

        // you might normally want to embed resources and load them from the manifest stream
        pluginUi = new Ui(configInterface);

        this.commandManager.AddHandler("/antiafk", new CommandInfo(SettingsCommand)
        {
            HelpMessage = "Opens configuration for anti afk plugin",
            ShowInHelp = true
        });
        
        this.pluginInterface.UiBuilder.Draw += DrawUi;
        this.pluginInterface.UiBuilder.OpenConfigUi += OpenUi;
        afkThread = new AntiAfkKick(pluginInterface, configInterface);
    }

    public void Dispose()
    {
        commandManager.RemoveHandler("/antiafk");
        afkThread.Dispose();
    }
    
    private void SettingsCommand(string command, string args)
    {
        // in response to the slash command, just display our main ui
        OpenUi();
    }

    private void DrawUi()
    {
        pluginUi.Draw();
    }

    private void OpenUi()
    {
        pluginUi.Visible = true;
    }
}