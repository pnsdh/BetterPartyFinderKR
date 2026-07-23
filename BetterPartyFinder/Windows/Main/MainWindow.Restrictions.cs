using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private bool Save;

    private void DrawRestrictionsTab(ConfigurationFilter filter)
    {
        using var table = ImRaii.Table("CategoryTable", 2, ImGuiTableFlags.BordersInnerV);
        if (!table.Success)
            return;

        Save = false;

        ImGui.TableSetupColumn("##Show");
        ImGui.TableSetupColumn("##Hide");

        ImGui.TableNextColumn();
        Helper.TextColored(ImGuiColors.HealerGreen, "보이기:");
        ImGui.Separator();

        ImGui.TableNextColumn();
        Helper.TextColored(ImGuiColors.ParsedOrange, "숨기기:");
        ImGui.Separator();

        filter[MirroredObjectiveFlags.None] = DrawRestrictionEntry("목적 설정 없음", filter[MirroredObjectiveFlags.None]);
        filter[MirroredObjectiveFlags.DutyCompletion] = DrawRestrictionEntry("완료 목적", filter[MirroredObjectiveFlags.DutyCompletion]);
        filter[MirroredObjectiveFlags.Practice] = DrawRestrictionEntry("연습", filter[MirroredObjectiveFlags.Practice]);        
        filter[MirroredObjectiveFlags.Loot] = DrawRestrictionEntry("반복 공략", filter[MirroredObjectiveFlags.Loot]);

        DrawSeparator();

        filter[ConditionFlags.None] = DrawRestrictionEntry("완료 조건 설정 없음", filter[ConditionFlags.None]);
        filter[ConditionFlags.DutyComplete] = DrawRestrictionEntry("공략 완료", filter[ConditionFlags.DutyComplete]);
        filter[ConditionFlags.DutyCompleteWeeklyUnclaimed] = DrawRestrictionEntry("이번 주 보상 미획득", filter[ConditionFlags.DutyCompleteWeeklyUnclaimed]);
        filter[ConditionFlags.DutyIncomplete] = DrawRestrictionEntry("미완료", filter[ConditionFlags.DutyIncomplete]);        
        
        DrawSeparator();

        filter[DutyFinderSettingsFlags.UnrestrictedParty] = DrawRestrictionEntry("제한 해제", filter[DutyFinderSettingsFlags.UnrestrictedParty]);
        filter[DutyFinderSettingsFlags.MinimumIL] = DrawRestrictionEntry("최저 아이템 레벨", filter[DutyFinderSettingsFlags.MinimumIL]);
        filter[DutyFinderSettingsFlags.SilenceEcho] = DrawRestrictionEntry("초월하는 힘 무효화", filter[DutyFinderSettingsFlags.SilenceEcho]);

        DrawSeparator();

        filter[MirroredLootRuleFlags.Normal] = DrawRestrictionEntry("전리품 규칙 설정 없음", filter[MirroredLootRuleFlags.Normal]);
        filter[MirroredLootRuleFlags.GreedOnly] = DrawRestrictionEntry("선입찰 금지", filter[MirroredLootRuleFlags.GreedOnly]);
        filter[MirroredLootRuleFlags.Lootmaster] = DrawRestrictionEntry("파티장 분배", filter[MirroredLootRuleFlags.Lootmaster]);

        DrawSeparator();

        //filter[SearchAreaFlags.DataCenter] = DrawRestrictionEntry("Data Centre Parties", filter[SearchAreaFlags.DataCenter]);
        filter[SearchAreaFlags.World] = DrawRestrictionEntry("서버 내 한정 파티", filter[SearchAreaFlags.World]);
        filter[SearchAreaFlags.OnePlayerPerJob] = DrawRestrictionEntry("잡 중복 없음", filter[SearchAreaFlags.OnePlayerPerJob]);

        if (Save)
            Plugin.Config.Save();
    }

    private bool DrawRestrictionEntry(string name, bool state)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(state ? 0 : 1);

        if (ImGui.Selectable(name))
        {
            state = !state;
            Save = true;
        }

        return state;
    }

    private void DrawSeparator()
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.Separator();
        ImGui.TableNextColumn();
        ImGui.Separator();
    }
}