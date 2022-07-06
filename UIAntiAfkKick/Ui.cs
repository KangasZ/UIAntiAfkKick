﻿using ImGuiNET;
using System;
using System.Numerics;

namespace UiAntiAfkKick;

class Ui
{
    private Configuration configuration;

    //private ImGuiScene.TextureWrap goatImage;

    // this extra bool exists for ImGui, since you can't ref a property
    private bool visible = false;
    public bool Visible
    {
        get { return this.visible; }
        set { this.visible = value; }
    }

    private bool settingsVisible = false;
    public bool SettingsVisible
    {
        get { return settingsVisible; }
        set { settingsVisible = value; }
    }

    // passing in the image here just for simplicity
    public Ui(Configuration configuration)
    {
        this.configuration = configuration;
        //this.goatImage = goatImage;
    }

    public void Draw()
    {
        // This is our only draw handler attached to UIBuilder, so it needs to be
        // able to draw any windows we might have open.
        // Each method checks its own visibility/state to ensure it only draws when
        // it actually makes sense.
        // There are other ways to do this, but it is generally best to keep the number of
        // draw delegates as low as possible.

        DrawMainWindow();
    }

    public void DrawMainWindow()
    {
        if (!Visible)
        {
            return;
        }

        ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Anti AFK Kick", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            ImGui.Text("UI Anti Afk Kick is a modification on EterniaS AntiAfkKick for the purpose of practice and dungeons.");
            ImGui.Spacing();
            ImGui.Text($"Plugin Enabled: {configuration.Enabled}");

            // can't ref a property, so use a local copy
            var configValue = configuration.Enabled;
            if (ImGui.Checkbox("Enable", ref configValue))
            {
                configuration.Enabled = configValue;
                // can save immediately on change, if you don't want to provide a "Save and Close" button
                configuration.Save();
            }

            ImGui.Spacing();
        }
        ImGui.End();
    }
}