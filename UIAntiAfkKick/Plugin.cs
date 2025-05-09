﻿using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;


namespace UiAntiAfkKick;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "UI Anti AFK Kick";

    private const string commandName = "/antiafk";

    private IDalamudPluginInterface pluginInterface { get; init; }
    private ICommandManager commandManager { get; init; }
    private Configuration configInterface { get; }
    private Ui pluginUi { get; }
    private AntiAfkKick afkThread;


    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IPluginLog pluginLog)
    {
        this.pluginInterface = pluginInterface;
        this.pluginInterface.Create<Services>();
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
        afkThread = new AntiAfkKick(configInterface, pluginLog);
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