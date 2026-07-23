using Dalamud.Game.Gui.PartyFinder.Types;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BetterPartyFinder;

public class Filter : IDisposable
{
    // 파티 구성 안내 파싱용 문자들
    private const string TankChars = "나전암건";
    private const string HealerChars = "백학점현";
    private const string DpsChars = "사바몽용닌리맆음기무흑적픽소솬";
    private const string BlankChars = "_0oㅇㅁ○□";

    // 목록 하나마다 호출되는 경로이므로 정규식은 한 번만 컴파일한다
    private static readonly Regex SuicideRegex = new("자살|die|클.*죽음|클.*포기", RegexOptions.Compiled);
    private static readonly Regex ScheduledRegex = new("[0-9]분|[0-9]시|\\d{2}:\\d{2}|\\d{2}출", RegexOptions.Compiled);
    private static readonly Regex GrindRegex = new("[0-9]클", RegexOptions.Compiled);
    private static readonly Regex EarlyDisbandRegex = new("까지.*(트라이|진행|종료|해산|쫑)|[0-9](트.*쫑|음식)", RegexOptions.Compiled);
    private static readonly Regex PartyCompRegex = new($"([{TankChars}{BlankChars}]{{2}}).*([{HealerChars}{BlankChars}]{{2}}).*([{DpsChars}{BlankChars}]{{4}})", RegexOptions.Compiled);

    private Plugin Plugin { get; }

    internal Filter(Plugin plugin)
    {
        Plugin = plugin;

        Plugin.PartyFinderGui.ReceiveListing += ReceiveListing;
    }

    public void Dispose()
    {
        Plugin.PartyFinderGui.ReceiveListing -= ReceiveListing;
    }

    private bool DescriptionFilter(string description, string[] includes)
    {
        return DescriptionFilter(description, includes, []);
    }

    private bool DescriptionFilter(string description, string[] includes, string[] excludes)
    {
        var isInclude = false;
        var isExclude = false;

        foreach (var include in includes)
        {
            if (description.Contains(include))
            {
                isInclude = true;
                break;
            }
        }

        if (isInclude)
        {
            foreach (var exclude in excludes)
            {
                if (description.Contains(exclude))
                {
                    isExclude = true;
                    break;
                }
            }
        }

        if (isInclude && !isExclude)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ReceiveListing(IPartyFinderListing listing, IPartyFinderListingEventArgs args)
    {
        args.Visible = args.Visible && ListingVisible(listing);
    }

    private unsafe bool ListingVisible(IPartyFinderListing listing)
    {
        var instance = AgentLookingForGroup.Instance();
        if (instance == null)
            return true;

        if (instance->SearchAreaTab == 1 && Plugin.Config.DisableInWorld)
        {
            Plugin.Log.Verbose("Disabled in world tab.");
            return true;
        }

        if (instance->SearchAreaTab == 2 && Plugin.Config.DisableInPrivate)
        {
            Plugin.Log.Verbose("Disabled in private tab.");
            return true;
        }

        // get the current preset or mark all pfs as visible
        var selectedId = Plugin.Config.SelectedPreset;
        if (selectedId == null || !Plugin.Config.Presets.TryGetValue(selectedId.Value, out var filter))
        {
            Plugin.Log.Verbose("early exit 1");
            return true;
        }

        // check max item level
        if (!filter.AllowHugeItemLevel && Sheets.MaxItemLevel > 0 && listing.MinimumItemLevel > Sheets.MaxItemLevel)
        {
            Plugin.Log.Verbose("early exit 2");
            return false;
        }

        // filter based on duty whitelist/blacklist
        if (filter.Duties.Count > 0)
        {
            var inList = filter.Duties.Contains(listing.RawDuty);            
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (filter.DutiesMode)
            {
                case ListMode.Blacklist when listing.DutyType == DutyType.Normal && inList:
                    return false;

                case ListMode.Whitelist when !inList:
                    return false;
            }
        }

        // filter based on item level range
        // 0은 UI에서 "꺼짐"으로 안내되고, 과거 버전이 기본값 0을 저장한 설정도 있으므로 null과 동일하게 취급한다
        if (filter.MinItemLevel is > 0 && listing.MinimumItemLevel < filter.MinItemLevel)
        {
            Plugin.Log.Verbose("early exit 3");
            return false;
        }

        if (filter.MaxItemLevel is > 0 && listing.MinimumItemLevel > filter.MaxItemLevel)
        {
            Plugin.Log.Verbose("early exit 4");
            return false;
        }

        // filter based on restrictions
        // make sure the listing doesn't contain any of the toggled off search areas
        if (((listing.SearchArea ^ filter.SearchArea) & ~filter.SearchArea) > 0)
        {
            Plugin.Log.Verbose("early exit 5");
            return false;
        }

        if (!filter.LootRule.HasFlag((MirroredLootRuleFlags)(1 << (int)listing.LootRules)))
        //if (listing.LootRules != LootRuleFlags.Normal && !filter.LootRule.HasFlag((MirroredLootRuleFlags)(1 << (int)listing.LootRules)))
        {
            Plugin.Log.Verbose("early exit 6");
            return false;
        }

        if (((listing.DutyFinderSettings ^ filter.DutyFinderSettings) & ~filter.DutyFinderSettings) > 0) {
            Plugin.Log.Verbose("early exit 7");
            return false;
        }

        if (!listing[filter.Conditions])
        {
            Plugin.Log.Verbose("early exit 8");
            return false;
        }

        if (!filter.Objectives.HasFlag((MirroredObjectiveFlags)listing.Objective))
        {
            Plugin.Log.Verbose("early exit 9");
            return false;
        }

        //filter based on keywords
        if (filter.Keywords.Count() > 0 && listing.Description.TextValue.Length > 0)
        {
            if (!filter.Keywords.CheckDescription(listing.Description.TextValue))
            {
                Plugin.Log.Verbose("early exit 10");
                return false;
            }
        }

        var description = listing.Description.ToString().ToLower().Replace(" ", string.Empty);

        // 2상 체크 풀려있으면 2상 안내 되어있는 파티 제외
        if (!filter.TwoLoot)
        {
            string[] include = ["2상", "파밍"];
            string[] exclude = ["낱장"];

            if (DescriptionFilter(description, include, exclude))
            {
                return false;
            }
        }

        // 상무 체크 풀려있으면 상무 안내되어있는 파티 제외
        if (!filter.NoLoot)
        {
            string[] include = ["상무", "무상", "상자무관", "1상", "낱장"];

            if (DescriptionFilter(description, include))
            {
                return false;
            }
        }

        // 초행 제외
        if (!filter.Fresh)
        {
            string[] include = ["초행"];
            string[] exclude = ["부클"];

            if (DescriptionFilter(description, include, exclude))
            {
                return false;
            }
        }

        // 헤딩 제외
        if (!filter.Blind)
        {
            string[] include = ["헤딩", "해딩"];

            if (DescriptionFilter(description, include))
            {
                return false;
            }
        }

        // 클목 제외
        if (!filter.AimToClear)
        {
            string[] include = ["클목", "성불", "클리어목적"];

            if (DescriptionFilter(description, include))
            {
                return false;
            }
        }

        // 갱신 제외
        if (!filter.ParseRun)
        {
            string[] include = ["갱신", "경신", "딜갱", "딜경"];

            if (DescriptionFilter(description, include))
            {
                return false;
            }
        }

        // 연습 파티 제외
        if (!filter.Practice)
        {
            if (description.Contains("연습"))
            {
                return false;
            }
        }

        // 클각 시 자살 제외
        if (!filter.Suicide)
        {
            if (SuicideRegex.IsMatch(description))
            {
                return false;
            }
        }

        // 시간 지정된 파티 제외
        if (!filter.Scheduled)
        {
            if (ScheduledRegex.IsMatch(description))
            {
                return false;
            }
        }

        // N클 파티 제외
        if (!filter.Grind)
        {
            if (GrindRegex.IsMatch(description))
            {
                return false;
            }
        }

        // 조기 쫑 파티 제외
        if (!filter.EarlyDisband)
        {
            if (EarlyDisbandRegex.IsMatch(description))
            {
                return false;
            }
        }

        if (!filter.DPSOne || !filter.DPSTwo || !filter.TankOne || !filter.TankTwo)
        {
            // 차있으면 true, 비어있으면 false
            // 만일 파티 서술이 되어있지 않다면 전부 빈 자리(false)
            var partySlot = new bool[8];

            // 파티 구성 안내가 설명에 적혀 있을 경우 이를 참조함
            var match = PartyCompRegex.Match(description);
            if (match.Success)
            {
                var partyDescription = "";

                for (int i = 1; i < match.Groups.Count; i++)
                {
                    partyDescription += match.Groups[i];
                }

                for (int i = 0; i < partyDescription.Length && i < partySlot.Length; i++)
                {
                    if (!BlankChars.Contains(partyDescription[i]))
                    {
                        partySlot[i] = true;
                    }
                }
            }

            // 탱1
            if (!filter.TankOne)
            {
                if (partySlot[0])
                {
                    return false;
                }

                string[] include = ["t1", "ㅌ1", "탱1", "탱커1", "mt", "메인탱", "멘탱"];
                string[] exclude = ["무관"];

                if (DescriptionFilter(description, include, exclude))
                {
                    return false;
                }
            }

            // 탱2
            if (!filter.TankTwo)
            {
                if (partySlot[1])
                {
                    return false;
                }

                string[] include = ["t2", "ㅌ2", "탱2", "탱커2", "st", "서브탱", "섭탱"];
                string[] exclude = ["무관"];

                if (DescriptionFilter(description, include, exclude))
                {
                    return false;
                }
            }

            // 딜러1
            if (!filter.DPSOne)
            {
                if (partySlot[4])
                {
                    return false;
                }

                string[] include = ["d1", "ㄷ1", "딜1", "딜러1"];
                string[] exclude = ["무관"];

                if (DescriptionFilter(description, include, exclude))
                {
                    return false;
                }
            }

            // 딜러2
            if (!filter.DPSTwo)
            {
                if (partySlot[5])
                {
                    return false;
                }

                string[] include = ["d2", "ㄷ2", "딜2", "딜러2"];
                string[] exclude = ["무관"];

                if (DescriptionFilter(description, include, exclude))
                {
                    return false;
                }
            }
        }

        // filter based on category (slow)
        if (!filter.Categories.Any(category => category.ListingMatches(listing)))
        {
            Plugin.Log.Verbose("LISTINGMATCHES WAS FALSE");
            return false;
        }

        // filter based on jobs (slow?)
        if (filter.Jobs.Count > 0 && !listing[SearchAreaFlags.AllianceRaid])
        {
            var slots = listing.Slots.ToArray();
            var present = listing.RawJobsPresent.ToArray();
            var presentNonRaw = listing.JobsPresent.ToArray();

            // create a list of sets containing the slots each job is able to join
            var jobs = new HashSet<int>[filter.Jobs.Count];
            for (var i = 0; i < jobs.Length; i++)
                jobs[i] = [];

            for (var idx = 0; idx < filter.Jobs.Count; idx++)
            {
                var wanted = filter.Jobs[idx];

                if (filter.NoDuplication)
                {
                    // 잡 중복 제외
                    foreach (var item in presentNonRaw)
                    {
                        try
                        {
                            if (wanted.HasFlag(Enum.Parse<JobFlags>(item.Value.NameEnglish.ToString().Replace(" ", ""))))
                            {
                                return false;
                            }
                        }
                        // 전투직 말고 기타 직업의 경우 리스트에 존재하지 않기 때문에 예외처리
                        catch (Exception) { }
                    }
                }

                if (filter.FixedJobReservation)
                {
                    // 확직예약 거르기
                    foreach (var slot in slots)
                    {
                        // 슬롯이 지원하는 직업들을 받아옴
                        var accept = slot.Accepting.ToList();

                        // 슬롯의 직업 모집이 단 하나고, 해당 직업이 내가 선택한 직업군에 있다면
                        if (accept.Count == 1 && wanted.HasFlag(accept[0]))
                        {
                            return false;
                        }
                    }
                }

                for (var i = 0; i < listing.SlotsAvailable; i++)
                {
                    // if the slot is already full or the job can't fit into it, skip
                    if (present[i] != 0 || !slots[i][wanted])
                        continue;

                    // check for one player per job
                    if (listing[SearchAreaFlags.OnePlayerPerJob])
                    {
                        // make sure at least one job in the wanted set isn't taken
                        foreach (var possibleJob in Enum.GetValues<JobFlags>())
                        {
                            if (!wanted.HasFlag(possibleJob))
                                continue;

                            var job = possibleJob.ClassJob(Plugin.Data);
                            if (job is null)
                                continue;

                            if (present.Contains((byte) job.Value.RowId))
                                continue;

                            jobs[idx].Add(i);
                            break;
                        }
                    }
                    else
                    {
                        // not one player per job
                        jobs[idx].Add(i);
                    }
                }

                // if this job couldn't match any slot, can't join the party
                if (jobs[idx].Count == 0)
                    return false;
            }

            // ensure the number of total slots with possibles joins is at least the number of jobs
            // note that this doesn't make sure it's joinable, see below
            var numSlots = jobs
                .Aggregate((acc, x) => acc.Union(x).ToHashSet())
                .Count;

            if (numSlots < jobs.Length)
                return false;

            // loop through each unique pair of jobs
            for (var i = 0; i < jobs.Length; i++)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var j = 0; j < jobs.Length; j++)
                {
                    if (i >= j)
                        continue;

                    var a = jobs[i];
                    var b = jobs[j];

                    // check if the slots either job can join have overlap
                    if (a.Intersect(b).Count() != 1)
                        continue;

                    // if there is overlap, check the difference between the sets
                    // if there is no difference, the party can't be joined
                    // note that if the overlap is more than one slot, we don't need to check
                    if (!a.Except(b).Any())
                        return false;
                }
            }
        }

        // filter based on player
        if (filter.Players.Count > 0)
            if (filter.Players.Any(info => info.Name == listing.Name.TextValue && info.World == listing.HomeWorld.Value.RowId))
                return false;

        return true;
    }
}