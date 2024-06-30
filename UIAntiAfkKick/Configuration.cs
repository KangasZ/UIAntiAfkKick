using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace UiAntiAfkKick;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled { get; set; } = true;
    
    public float Seconds { get; set; } = 30f;

    public bool Instance { get; set; } = false;
    // the below exist just to make saving less cumbersome

    [NonSerialized]
    private IDalamudPluginInterface pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        pluginInterface!.SavePluginConfig(this);
    }
}