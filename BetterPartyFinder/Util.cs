using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace BetterPartyFinder;

public static class Util
{
    public static string UpperCaseStr(this ReadOnlySeString s, sbyte article = 0)
    {
        if (article == 1)
            return s.ToString();

        var sb = new StringBuilder(s.ToString());
        var lastSpace = true;
        for (var i = 0; i < sb.Length; ++i)
        {
            if (sb[i] == ' ')
            {
                lastSpace = true;
            }
            else if (lastSpace)
            {
                lastSpace = false;
                sb[i]     = char.ToUpperInvariant(sb[i]);
            }
        }

        return sb.ToString();
    }

    internal static bool ContainsIgnoreCase(this string haystack, string needle)
    {
        return CultureInfo.InvariantCulture.CompareInfo.IndexOf(haystack, needle, CompareOptions.IgnoreCase) >= 0;
    }

    internal static IEnumerable<World> WorldsOnDataCentre(IPlayerCharacter character)
    {
        // 파티 찾기는 데이터센터 단위이므로 홈월드와 같은 DC의 공개 월드를 시트에서 조회한다.
        // KR 클라이언트 시트 데이터가 기대와 다를 경우에 대비해 알려진 한국 월드 ID로 폴백.
        var dataCenter = character.HomeWorld.Value.DataCenter.RowId;
        var worlds = Sheets.WorldSheet
            .Where(world => world.IsPublic && world.DataCenter.RowId == dataCenter)
            .ToList();
        if (worlds.Count > 0)
            return worlds;

        uint[] krServerIds = [2075, 2076, 2077, 2078, 2080];
        return Sheets.WorldSheet.Where(world => krServerIds.Contains(world.RowId));
    }

    /// <summary> Iterate over enumerables with additional index. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static IEnumerable<(T Value, int Index)> WithIndex<T>(this IEnumerable<T> list)
        => list.Select((x, i) => (x, i));

    // From: https://stackoverflow.com/a/1415187
    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        if (name != null)
        {
            var field = type.GetField(name);
            if (field != null)
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    return attr.Description;
        }

        return string.Empty;
    }
}