using System;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace BetterPartyFinder;


public unsafe class HookManager
{
    // Fallible: 게임 패치로 시그니처가 깨져도 플러그인 로드는 유지하고 새로고침만 비활성화한다
    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 0F 10 81 ?? ?? ?? ?? 8B 99", Fallibility = Fallibility.Fallible)]
    private readonly delegate* unmanaged<AgentLookingForGroup*, byte, byte> RequestPartyFinderListings = null!;

    public HookManager()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);

        if (RequestPartyFinderListings == null)
            Plugin.Log.Warning("파티 찾기 새로고침 시그니처를 찾지 못했습니다. 프리셋 변경 시 자동 새로고침이 비활성화됩니다.");
    }

    public void RefreshListings()
    {
        if (RequestPartyFinderListings == null)
            return;

        var agent = AgentLookingForGroup.Instance();
        if (agent == null)
            return;

        RequestPartyFinderListings(agent, agent->CategoryTab);
    }
}