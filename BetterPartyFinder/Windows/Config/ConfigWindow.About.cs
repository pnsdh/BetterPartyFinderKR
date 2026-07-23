using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace BetterPartyFinder.Windows.Config;

public partial class ConfigWindow
{
    private const float SeparatorPadding = 1.0f;
    private static float GetSeparatorPaddingHeight => SeparatorPadding * ImGuiHelpers.GlobalScale;

    private void About()
    {
        using var tabItem = ImRaii.TabItem("플러그인 정보");
        if (!tabItem.Success)
            return;

        var bottomContentHeight = ImGui.GetFrameHeightWithSpacing() * 2 + ImGui.GetStyle().WindowPadding.Y + GetSeparatorPaddingHeight;
        using (var contentChild = ImRaii.Child("AboutContent", new Vector2(0, -bottomContentHeight)))
        {
            if (contentChild.Success)
            {
                ImGuiHelpers.ScaledDummy(5.0f);

                ImGui.TextUnformatted("제작자:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, Plugin.Interface.Manifest.Author);

                ImGui.TextUnformatted("디스코드:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, "@infi");

                ImGui.TextUnformatted("수정:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, "Enzyu");

                ImGui.TextUnformatted("버전:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedOrange, Plugin.Interface.Manifest.AssemblyVersion.ToString() + " (KR Custom)");
            }
        }

        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(SeparatorPadding);

        using var bottomChild = ImRaii.Child("AboutBottomBar", new Vector2(0, 0), false, 0);
        if (!bottomChild.Success)
            return;

        using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.ParsedBlue))
        {
            if (ImGui.Button("공식 디스코드 스레드"))
                Dalamud.Utility.Util.OpenLink("https://discord.com/channels/581875019861328007/1274940471273197638");
        }

        ImGui.SameLine();

        using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.DPSRed))
        {
            if (ImGui.Button("이슈"))
                Dalamud.Utility.Util.OpenLink("https://github.com/Infiziert90/BetterPartyFinder/issues");
        }

        ImGui.SameLine();

        using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.12549f, 0.74902f, 0.33333f, 0.6f)))
        {
            if (ImGui.Button("Ko-Fi 팁"))
                Dalamud.Utility.Util.OpenLink("https://ko-fi.com/infiii");
        }

        using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.ParsedPurple))
        {
            if (ImGui.Button("달라살려 디스코드"))
                Dalamud.Utility.Util.OpenLink("https://discord.gg/q9VvFC6RTH");
        }
    }
}