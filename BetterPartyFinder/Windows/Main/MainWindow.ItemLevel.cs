using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private void DrawItemLevelTab(ConfigurationFilter filter)
    {
        var hugePfs = filter.AllowHugeItemLevel;
        if (ImGui.Checkbox("최고 템렙을 초과한 모집 제거", ref hugePfs))
        {
            filter.AllowHugeItemLevel = hugePfs;
            Plugin.Config.Save();
        }

        ImGui.NewLine();
        var width = ImGui.GetContentRegionAvail().X / 3;
        var minLevel = (int?)filter.MinItemLevel ?? 0;
        ImGui.TextUnformatted("최소/최대 아이템 레벨");
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("0으로 설정하면 기능이 꺼집니다.");
        ImGui.SetNextItemWidth(width);
        if (ImGui.InputInt("###min-ilvl", ref minLevel))
        {
            filter.MinItemLevel = minLevel == 0 ? null : (uint)minLevel;
            Plugin.Config.Save();
        }

        ImGui.SameLine();

        var maxLevel = (int?)filter.MaxItemLevel ?? 0;
        ImGui.SetNextItemWidth(width);
        if (ImGui.InputInt("###max-ilvl", ref maxLevel))
        {
            filter.MaxItemLevel = maxLevel == 0 ? null : (uint)maxLevel;
            Plugin.Config.Save();
        }
    }
}