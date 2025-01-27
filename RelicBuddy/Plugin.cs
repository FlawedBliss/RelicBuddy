using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using RelicBuddy.Helpers;
using RelicBuddy.Helpers.Strings;
using RelicBuddy.Models;
using RelicBuddy.Windows;

namespace RelicBuddy;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService]
    internal static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService]
    internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;
    
    [PluginService]
    internal static IAddonEventManager AddonEventManager { get; private set; } = null!;
    
    [PluginService]
    internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    
    private const string RelicBuddyCommandName = "/rb";
    private const string RelicBuddyDebugCommandName = "/rbd";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("RelicBuddy");
    private DebugWindow DebugWindow { get; init; }
    
    private MainWindow MainWindow { get; init; }
    
    private TextWindow TextWindow { get; init; }
    
    internal ItemSourceWindow ItemSourceWindow { get; init; }

    public List<RelicData> RelicData { get; set; } = [];

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        InitRelicData();
        StringsDict.Instance.Init(this);
        DebugWindow = new DebugWindow(this);
        ItemSourceWindow = new ItemSourceWindow(this);
        MainWindow = new MainWindow(this);
        TextWindow = new TextWindow(this);

        WindowSystem.AddWindow(DebugWindow);
        WindowSystem.AddWindow(ItemSourceWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(TextWindow);
        CommandManager.AddHandler(RelicBuddyCommandName, new CommandInfo(OnRelicBuddyCommand)
        {
            HelpMessage = "Opens the relic window"
        });
        CommandManager.AddHandler(RelicBuddyDebugCommandName, new CommandInfo(OnRelicBuddyDebugCommand)
        {
            HelpMessage = "Opens the debug window"
        });
        CommandManager.AddHandler("/rbt", new CommandInfo((command, arguments) =>
        {
            TextWindow.Toggle();
        }));
        PluginInterface.UiBuilder.OpenMainUi += () => MainWindow.Toggle();
        PluginInterface.UiBuilder.Draw += DrawUI;

        // // This adds a button to the plugin installer entry of this plugin which allows
        // // to toggle the display status of the configuration ui
        // PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        //
        // // Adds another button that is doing the same but for the main ui of the plugin
        // PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectString", InventoryHelper.Instance.UpdateRetainerInventory);
        
    }

    private void InitRelicData()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RelicBuddy.Data.relic_data.json");
        if (stream is null)
        {
            throw new Exception("Cannot find relic weapon data");
        }

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        RelicData = JsonConvert.DeserializeObject<List<RelicData>>(json)!;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        DebugWindow.Dispose();
        MainWindow.Dispose();
        ItemSourceWindow.Dispose();
        TextWindow.Dispose();
        CommandManager.RemoveHandler(RelicBuddyCommandName);
        CommandManager.RemoveHandler(RelicBuddyDebugCommandName);
        
        AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "SelectString", InventoryHelper.Instance.UpdateRetainerInventory);
    }

    private void OnRelicBuddyCommand(string command, string args)
    {
        MainWindow.Toggle();
    }

    private void OnRelicBuddyDebugCommand(string command, string args)
    {
        DebugWindow.Toggle();
    }

    private void DrawUI() => WindowSystem.Draw();
}
