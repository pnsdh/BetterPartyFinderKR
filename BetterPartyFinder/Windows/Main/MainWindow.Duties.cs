using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private string DutySearchQuery = string.Empty;

    private void DrawDutiesTab(ConfigurationFilter filter)
    {
        var listModeStrings = new[]
        {
            "선택한 임무만 표시",
            "선택하지 않은 임무만 표시",
        };

        Helper.TextColored(ImGuiColors.DalamudOrange, "표시 설정:");
        ImGui.Separator();

        var listModeIdx = filter.DutiesMode == ListMode.Blacklist ? 1 : 0;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.Combo("###list-mode", ref listModeIdx, listModeStrings, listModeStrings.Length))
        {
            filter.DutiesMode = listModeIdx == 0 ? ListMode.Whitelist : ListMode.Blacklist;
            Plugin.Config.Save();
        }

        ImGuiComponents.IconButton(FontAwesomeIcon.Search);
        if (DutyAddPopup("DutyAddPopup", out var row, filter))
        {
            filter.Duties.Add(row);
            Plugin.Config.Save();
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip("추가할 새 임무 검색");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Eraser))
        {
            filter.Duties.Clear();
            Plugin.Config.Save();
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip("선택된 모든 임무 리스트 삭제");

        Helper.TextColored(ImGuiColors.HealerGreen, "선택됨:");
        ImGui.Separator();

        using var child = ImRaii.Child("duty-selection", Vector2.Zero);
        if (!child.Success)
            return;

        foreach (var cf in filter.Duties.Order().ToArray())
        {
            // 게임 업데이트로 사라진 임무 row가 설정에 남아 있을 수 있다
            var dutyRow = Sheets.ContentFinderSheet.GetRowOrDefault(cf);
            if (dutyRow == null)
                continue;

            if (!ImGui.Selectable(dutyRow.Value.Name.UpperCaseStr()))
                continue;

            filter.Duties.Remove(cf);
            Plugin.Config.Save();
        }
    }

    private ContentFinderCondition[]? FilteredDuties;


    private void ExcelSheetSearchInput()
    {
        if (ImGui.IsWindowAppearing() && ImGui.IsWindowFocused() && !ImGui.IsAnyItemActive())
        {
            FilteredDuties = null;
            DutySearchQuery = string.Empty;

            ImGui.SetKeyboardFocusHere(0);
        }

        if (ImGui.InputTextWithHint("##DutySheetSearch", "...", ref DutySearchQuery, 128, ImGuiInputTextFlags.AutoSelectAll))
            FilteredDuties = null;

        FilteredDuties ??= Sheets.DutyCache.Where(duty => duty.Name.ExtractText().ContainsIgnoreCase(DutySearchQuery)).ToArray();
    }

    private bool DutyAddPopup(string id, out uint selectedRow, ConfigurationFilter filter)
    {
        selectedRow = 0;

        ImGui.SetNextWindowSize(new Vector2(0, 250 * ImGuiHelpers.GlobalScale));
        using var popup = ImRaii.ContextPopupItem(id, ImGuiPopupFlags.None);
        if (!popup.Success)
            return false;

        ExcelSheetSearchInput();

        using var child = ImRaii.Child("DutySheetList", Vector2.Zero, true);
        if (!child.Success)
            return false;

        var ret = false;
        foreach (var duty in FilteredDuties!.Where(d => !filter.Duties.Contains(d.RowId)))
        {
            using var pushedId = ImRaii.PushId((int)duty.RowId);
            if (!ImGui.Selectable(duty.Name.UpperCaseStr()))
                continue;

            selectedRow = duty.RowId;
            ret = true;
        }

        return ret;
    }
}