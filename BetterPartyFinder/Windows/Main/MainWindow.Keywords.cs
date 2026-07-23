using System.Collections.Generic;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private bool WhitelistSelected = true;
    private string KeywordText = string.Empty;

    private void DrawKeywordsTab(ConfigurationFilter filter)
    {
        using (ImRaii.ItemWidth(ImGui.GetWindowWidth() * 0.50f))
            ImGui.InputText("###keyword-text", ref KeywordText, 64);

        ImGui.SameLine();
        if (ImGui.Button(WhitelistSelected ? "화이트리스트" : "블랙리스트"))
            WhitelistSelected = !WhitelistSelected;

        if (ImGui.IsItemHovered())
            Helper.Tooltip("클릭하여 화이트리스트와 블랙리스트를 토글 할 수 있습니다.");

        ImGui.SameLine();
        if (Helper.IconButton(FontAwesomeIcon.Plus, "add-keyword"))
        {
            var word = KeywordText.Trim();
            if (!string.IsNullOrEmpty(word))
            {
                (WhitelistSelected ? filter.Keywords.Whitelist : filter.Keywords.Blacklist).Add(word);

                KeywordText = string.Empty;
                Plugin.Config.Save();
            }
        }

        ImGui.NewLine();
        DrawKeywordList("Whitelist", filter.Keywords.Whitelist, filter);

        ImGui.NewLine();
        DrawKeywordList("Blacklist", filter.Keywords.Blacklist, filter);
    }

    private void DrawKeywordList(string label, List<string> keywords, ConfigurationFilter filter)
    {
        if (label == "Whitelist")
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("화이트리스트 모드:");
            ImGui.SameLine();
            if (ImGui.Button($"{(filter.Keywords.Mode == WhitelistMode.All ? "전체 포함" : "하나라도 포함")}"))
            {
                filter.Keywords.Mode = filter.Keywords.Mode == WhitelistMode.All ? WhitelistMode.Any : WhitelistMode.All; // toggle between ALL and ANY
                Plugin.Config.Save();
            }

            ImGuiComponents.HelpMarker("클릭하여 화이트리스트 모드를 전체 포함으로 할지, 하나라도 포함으로 할지 설정할 수 있습니다.");

            ImGui.Separator();

            ImGui.TextUnformatted("화이트리스트:");
        }
        else
        {
            ImGui.Separator();

            ImGui.TextUnformatted("블랙리스트:");
        }

        var toDelete = string.Empty;
        foreach (var word in keywords)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(word);

            ImGui.SameLine();

            if (Helper.IconButton(FontAwesomeIcon.Trash, $"delete-keyword-{word}"))
                toDelete = word;
        }

        if (toDelete != string.Empty)
        {
            keywords.Remove(toDelete);
            Plugin.Config.Save();
        }
    }
}