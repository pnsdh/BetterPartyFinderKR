using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private void DrawCustomTab(ConfigurationFilter filter)
    {
        ImGui.TextUnformatted("빠른 키워드 필터링");
        ImGui.Separator();

        var twoLoot = filter.TwoLoot;
        if (ImGui.Checkbox("2상자 & 파밍", ref twoLoot))
        {
            filter.TwoLoot = twoLoot;
            this.Plugin.Config.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("2상, 파밍");

        var noLoot = filter.NoLoot;
        if (ImGui.Checkbox("상자 무관", ref noLoot))
        {
            filter.NoLoot = noLoot;
            this.Plugin.Config.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("상무, 무상, 상자무관, 1상, 낱장");

        ImGui.Separator();

        var blind = filter.Blind;
        if (ImGui.Checkbox("헤딩", ref blind))
        {
            filter.Blind = blind;
            this.Plugin.Config.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("헤딩, 해딩");

        var fresh = filter.Fresh;
        if (ImGui.Checkbox("초행", ref fresh))
        {
            filter.Fresh = fresh;
            this.Plugin.Config.Save();
        }

        var aimToClear = filter.AimToClear;
        if (ImGui.Checkbox("클목 & 성불", ref aimToClear))
        {
            filter.AimToClear = aimToClear;
            this.Plugin.Config.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("클목, 성불, 클리어 목적");

        var practice = filter.Practice;
        if (ImGui.Checkbox("연습", ref practice))
        {
            filter.Practice = practice;
            this.Plugin.Config.Save();
        }

        var parseRun = filter.ParseRun;
        if (ImGui.Checkbox("갱신", ref parseRun))
        {
            filter.ParseRun = parseRun;
            this.Plugin.Config.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("갱신, 경신, 딜갱, 딜경");

        var grind = filter.Grind;
        if (ImGui.Checkbox("N클", ref grind))
        {
            filter.Grind = grind;
            this.Plugin.Config.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("[0-9]클");

        ImGui.Separator();

        var earlyDisband = filter.EarlyDisband;
        if (ImGui.Checkbox("조기 해산", ref earlyDisband))
        {
            filter.EarlyDisband = earlyDisband;
            this.Plugin.Config.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("까지.*(트라이|진행|종료|해산|쫑), [0-9](트.*쫑|음식)");

        var suicide = filter.Suicide;
        if (ImGui.Checkbox("클각 시 자살", ref suicide))
        {
            filter.Suicide = suicide;
            this.Plugin.Config.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("자살, die, 클.*죽음, 클.*포기");

        var scheduled = filter.Scheduled;
        if (ImGui.Checkbox("출발 시각 지정", ref scheduled))
        {
            filter.Scheduled = scheduled;
            this.Plugin.Config.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("[0-9]분, [0-9]시, \\d{2}:\\d{2}, \\d{2}출");

        ImGui.Separator();

        var tankOne = filter.TankOne;
        if (ImGui.Checkbox("T1 & MT", ref tankOne))
        {
            filter.TankOne = tankOne;
            this.Plugin.Config.Save();
        }
        PrintExperiment();

        var tankTwo = filter.TankTwo;
        if (ImGui.Checkbox("T2 & ST", ref tankTwo))
        {
            filter.TankTwo = tankTwo;
            this.Plugin.Config.Save();
        }
        PrintExperiment();

        var dpsOne = filter.DPSOne;
        if (ImGui.Checkbox("D1", ref dpsOne))
        {
            filter.DPSOne = dpsOne;
            this.Plugin.Config.Save();
        }
        PrintExperiment();

        var dpsTwo = filter.DPSTwo;
        if (ImGui.Checkbox("D2", ref dpsTwo))
        {
            filter.DPSTwo = dpsTwo;
            this.Plugin.Config.Save();
        }
        PrintExperiment();

        ImGui.Separator();
        ImGui.NewLine();
        ImGui.TextUnformatted("직업 필터링 확장");

        var fixedJobReservation = filter.FixedJobReservation;
        if (ImGui.Checkbox("확정 직업 예약 판정", ref fixedJobReservation))
        {
            filter.FixedJobReservation = fixedJobReservation;
            this.Plugin.Config.Save();
        }

        var noDuplication = filter.NoDuplication;
        if (ImGui.Checkbox("직업 중복 없음", ref noDuplication))
        {
            filter.NoDuplication = noDuplication;
            this.Plugin.Config.Save();
        }
    }

    private void PrintExperiment()
    {
        ImGui.SameLine();

        using (ImRaii.PushColor(ImGuiCol.Text, new System.Numerics.Vector4(1.0f, 0.8f, 0.2f, 1.0f)))
            using (ImRaii.PushFont(UiBuilder.IconFont))
                ImGui.TextUnformatted(FontAwesomeIcon.ExclamationTriangle.ToIconString());

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("주의: 실험적 기능, 제대로 동작하지 않을 수 있음");        
    }
}