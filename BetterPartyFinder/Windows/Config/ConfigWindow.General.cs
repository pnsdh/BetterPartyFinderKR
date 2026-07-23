using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace BetterPartyFinder.Windows.Config;

public partial class ConfigWindow
{
    private void General()
    {
        using var tabItem = ImRaii.TabItem("일반");
        if (!tabItem.Success)
            return;

        var changed = false;

        changed |= ImGui.Checkbox("파티 찾기와 동시에 같이 열림", ref Plugin.Config.ShowWhenPfOpen);

        var sideOptions = new[]
        {
            "왼쪽",
            "오른쪽",
        };
        var sideIdx = Plugin.Config.WindowSide == WindowSide.Left ? 0 : 1;

        ImGui.TextUnformatted("파티 찾기 도킹 방향");
        if (ImGui.Combo("###window-side", ref sideIdx, sideOptions, sideOptions.Length))
        {
            Plugin.Config.WindowSide = sideIdx switch
            {
                0 => WindowSide.Left,
                1 => WindowSide.Right,
                _ => Plugin.Config.WindowSide,
            };

            Plugin.Config.Save();
        }

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        changed |= ImGui.Checkbox("서버 내 탭에서 작동 정지", ref Plugin.Config.DisableInWorld);
        changed |= ImGui.Checkbox("비공개 탭에서 작동 정지", ref Plugin.Config.DisableInPrivate);

        if (changed)
            Plugin.Config.Save();
    }
}