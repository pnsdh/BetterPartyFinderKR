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
        // KR 클라이언트 시트에서는 동적 조회가 불가능해 하드코딩한다 (7.5 기준 검증):
        // - KR 월드는 전부 IsPublic=False라서 업스트림의 IsPublic 필터는 빈 결과가 나온다.
        // - DC(201)·UserType·Region으로 거르면 실서버 5개 외에 미운영 월드 '오메가'(2081)가 섞이고,
        //   이를 구분할 수 있는 컬럼이 시트에 없다.
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