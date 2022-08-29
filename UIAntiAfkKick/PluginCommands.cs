﻿using System;
using Dalamud.Game.Command;

namespace UiAntiAfkKick;

public class PluginCommands : IDisposable
{
    private CommandManager commandManager;
    private PluginUi pluginUi;
    
    public PluginCommands(CommandManager commandManager, PluginUi pluginUi)
    {
        this.pluginUi = pluginUi;
        this.commandManager = commandManager;
        this.commandManager.AddHandler("/antiafk", new CommandInfo(SettingsCommand)
        {
            HelpMessage = "Opens configuration for anti afk plugin",
            ShowInHelp = true
        });
    }
    
    private void SettingsCommand(string command, string args)
    {
        pluginUi.OpenUi();
    }
    
    public void Dispose()
    {
        commandManager.RemoveHandler("/antiafk");
    }
}