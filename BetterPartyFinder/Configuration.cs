using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Configuration;
using Dalamud.Game.Gui.PartyFinder.Types;

namespace BetterPartyFinder;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public Guid? SelectedPreset;
    public Dictionary<Guid, ConfigurationFilter> Presets { get; } = new();

    public bool ShowWhenPfOpen;
    public WindowSide WindowSide = WindowSide.Left;

    public bool DisableInWorld = true;
    public bool DisableInPrivate = true;

    internal static Configuration? Load()
    {
        MigrateLegacyConfig();
        return (Configuration?) Plugin.Interface.GetPluginConfig();
    }

    // InternalName이 BetterPartyFinder → BetterPartyFinderKR로 바뀌면서 설정 파일 이름과
    // 직렬화된 $type의 어셈블리 표기가 달라졌다. 새 설정 파일이 없으면 이전 파일에서 옮겨온다.
    private static void MigrateLegacyConfig()
    {
        try
        {
            var newFile = Plugin.Interface.ConfigFile;
            if (newFile.Exists)
                return;

            var oldFile = new FileInfo(Path.Combine(newFile.DirectoryName!, "BetterPartyFinder.json"));
            if (!oldFile.Exists)
                return;

            var json = File.ReadAllText(oldFile.FullName);
            json = Regex.Replace(json, @", BetterPartyFinder(?=[""\]])", ", BetterPartyFinderKR");
            File.WriteAllText(newFile.FullName, json);
            Plugin.Log.Information($"이전 설정 파일을 옮겨왔습니다: {oldFile.FullName} → {newFile.FullName}");
        }
        catch (Exception e)
        {
            Plugin.Log.Warning(e, "이전 설정 파일 마이그레이션에 실패했습니다.");
        }
    }

    internal void Save()
    {
        Plugin.Interface.SavePluginConfig(this);
    }
}

public class ConfigurationFilter
{
    public string Name { get; set; } = "<이름 없는 프리셋>";

    public ListMode DutiesMode { get; set; } = ListMode.Blacklist;
    public HashSet<uint> Duties { get; set; } = [];

    public HashSet<UiCategory> Categories { get; set; } = [];

    public List<JobFlags> Jobs { get; set; } = [];
    // default to true because that's the PF's default
    // use nosol if trying to avoid spam

    public SearchAreaFlags SearchArea { get; set; } = ~SearchAreaFlags.None;
    public MirroredLootRuleFlags LootRule { get; set; } = ~(MirroredLootRuleFlags)0;
    public DutyFinderSettingsFlags DutyFinderSettings { get; set; } = ~DutyFinderSettingsFlags.None;
    public ConditionFlags Conditions { get; set; } = ~(ConditionFlags)0;
    public MirroredObjectiveFlags Objectives { get; set; } = ~(MirroredObjectiveFlags)0;

    public bool AllowHugeItemLevel { get; set; } = true;

    public bool TwoLoot { get; set; } = true;
    public bool NoLoot { get; set; } = true;

    public bool EarlyDisband { get; set; } = true;
    public bool Suicide { get; set; } = true;
    public bool Scheduled { get; set; } = true;

    public bool Fresh { get; set; } = true;
    public bool Blind { get; set; } = true;
    public bool AimToClear { get; set; } = true;
    public bool ParseRun { get; set; } = true;
    public bool Grind { get; set; } = true;
    public bool Practice { get; set; } = true;

    public bool DPSOne { get; set; } = true;
    public bool DPSTwo { get; set; } = true;
    public bool TankOne { get; set; } = true;
    public bool TankTwo { get; set; } = true;

    public bool FixedJobReservation { get; set; } = true;
    public bool NoDuplication { get; set; } = true;


    public uint? MinItemLevel { get; set; }
    public uint? MaxItemLevel { get; set; }

    public KeywordsInfo Keywords { get; set; } = new([], [], WhitelistMode.Any);

    public HashSet<PlayerInfo> Players { get; set; } = [];

    internal bool this[SearchAreaFlags flags]
    {
        get => (SearchArea & flags) > 0;
        set
        {
            if (value)
                SearchArea |= flags;
            else
                SearchArea &= ~flags;
        }
    }

    internal bool this[MirroredLootRuleFlags flags]
    {
        get => (LootRule & flags) > 0;
        set
        {
            if (value)
                LootRule |= flags;
            else
                LootRule &= ~flags;
        }
    }

    internal bool this[DutyFinderSettingsFlags flags]
    {
        get => (DutyFinderSettings & flags) > 0;
        set
        {
            if (value)
                DutyFinderSettings |= flags;
            else
                DutyFinderSettings &= ~flags;
        }
    }

    internal bool this[ConditionFlags flags]
    {
        get => (Conditions & flags) > 0;
        set
        {
            if (value)
                Conditions |= flags;
            else
                Conditions &= ~flags;
        }
    }

    internal bool this[MirroredObjectiveFlags flags]
    {
        get => (Objectives & flags) > 0;
        set
        {
            if (value)
                Objectives |= flags;
            else
                Objectives &= ~flags;
        }
    }

    internal ConfigurationFilter Clone()
    {
        var categories = Categories.ToHashSet();
        var duties = Duties.ToHashSet();
        var jobs = Jobs.ToList();
        var players = Players.Select(info => info.Clone()).ToHashSet();
        var keywords = Keywords.Clone();

        return new ConfigurationFilter
        {
            Categories = categories,
            Conditions = Conditions,
            Duties = duties,
            Jobs = jobs,
            Name = new string(Name),
            Objectives = Objectives,
            DutiesMode = DutiesMode,
            LootRule = LootRule,
            SearchArea = SearchArea,
            DutyFinderSettings = DutyFinderSettings,
            MaxItemLevel = MaxItemLevel,
            MinItemLevel = MinItemLevel,
            AllowHugeItemLevel = AllowHugeItemLevel,
            TwoLoot = TwoLoot,
            NoLoot = NoLoot,
            Fresh = Fresh,
            Blind = Blind,
            Suicide = Suicide,
            Scheduled = Scheduled,
            NoDuplication = NoDuplication,
            FixedJobReservation = FixedJobReservation,
            AimToClear = AimToClear,
            ParseRun = ParseRun,
            Grind = Grind,
            EarlyDisband = EarlyDisband,
            DPSOne = DPSOne,
            DPSTwo = DPSTwo,
            Practice = Practice,
            TankOne = TankOne,
            TankTwo = TankTwo,
            Players = players,
            Keywords = keywords,
        };
    }

    internal static ConfigurationFilter Create()
    {
        return new ConfigurationFilter { Categories = Enum.GetValues<UiCategory>().ToHashSet(), };
    }
}

public class PlayerInfo
{
    public string Name { get; }
    public uint World { get; }

    public PlayerInfo(string name, uint world)
    {
        Name = name;
        World = world;
    }

    internal PlayerInfo Clone()
    {
        return new PlayerInfo(Name, World);
    }

    private bool Equals(PlayerInfo other)
    {
        return Name == other.Name && World == other.World;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj.GetType() == GetType() && Equals((PlayerInfo) obj);
    }

    public override int GetHashCode()
    {
        unchecked { return (Name.GetHashCode() * 397) ^ (int) World; }
    }
}

public class KeywordsInfo(List<string> whitelist, List<string> blacklist, WhitelistMode whitelistMode)
{
    public List<string> Whitelist { get; } = whitelist;
    public List<string> Blacklist { get; } = blacklist;

    public WhitelistMode Mode { get; set; } = whitelistMode;

    internal KeywordsInfo Clone()
    {
        return new KeywordsInfo(Whitelist.ToList(), Blacklist.ToList(), Mode);
    }

    public bool CheckDescription(string description)
    {
        if (Blacklist.Any(description.ContainsIgnoreCase))
            return false;

        if (Mode == WhitelistMode.Any)
            return Whitelist.Any(description.ContainsIgnoreCase);

        return Whitelist.All(description.ContainsIgnoreCase);
    }

    //create override for the Count method
    public int Count()
    {
        return Whitelist.Count + Blacklist.Count;
    }

}

public enum WhitelistMode
{
    Any,
    All,
}

public enum ListMode
{
    Whitelist,
    Blacklist,
}

public enum WindowSide
{
    Left,
    Right,
}