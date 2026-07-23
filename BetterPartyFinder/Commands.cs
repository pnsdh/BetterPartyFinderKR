using System;
using System.Collections.Generic;
using Dalamud.Game.Command;

namespace BetterPartyFinder;

public class Commands : IDisposable {
    private static readonly Dictionary<string, string> CommandNames = new()
    {
        ["/betterpartyfinder"] = "메인 인터페이스를 엽니다. \"c\" 나 \"config\" 인자를 넣으면 설정 인터페이스를 열 수 있습니다.",
        ["/bpf"] = "/betterpartyfinder의 축약어입니다.",
    };

    private Plugin Plugin { get; }

    internal Commands(Plugin plugin) {
        Plugin = plugin;

        foreach (var (name, help) in CommandNames)
            Plugin.CommandManager.AddHandler(name, new CommandInfo(OnCommand) { HelpMessage = help, });
    }

    public void Dispose()
    {
        foreach (var name in CommandNames.Keys)
            Plugin.CommandManager.RemoveHandler(name);
    }

    private void OnCommand(string command, string args)
    {
        if (args is "c" or "config")
            Plugin.ConfigWindow.Toggle();
        else
            Plugin.MainWindow.Toggle();
    }
}