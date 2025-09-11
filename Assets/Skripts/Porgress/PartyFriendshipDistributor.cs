using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 입력 누적(ClickProgressTracker)을 바탕으로
    /// 파티 1번/2~6번 슬롯에 친밀도 보상을 분배한다.
    /// </summary>
    public class PartyFriendshipDistributor
    {
        private readonly OwnedPokemonManager _owned;
        private readonly ClickProgressTracker _tracker;
        private readonly ClickRewardPolicy _policy;

        public PartyFriendshipDistributor(
            OwnedPokemonManager owned,
            ClickProgressTracker tracker,
            ClickRewardPolicy policy)
        {
            _owned = owned;
            _tracker = tracker;
            _policy = policy;
        }

        /// <summary>
        /// 입력 1회를 처리: 누적을 증가시키고, 지급 주기에 도달했다면 친밀도 보상을 분배한다.
        /// 여러 주기가 한 번에 도달할 수 있으므로 delta(>=0)를 모두 소화한다.
        /// </summary>
        public void OnInput()
        {
            int delta = _tracker.OnInput(_policy.clicksPerFriendship);
            if (delta <= 0) return;

            var party = _owned.Party;
            for (int i = 0; i < party.Count; i++)
            {
                var p = _owned.GetByPuid(party[i]);
                if (p == null) continue;

                int add = _policy.GetFriendshipForSlot(i, delta);
                if (add <= 0) continue;

                // 클램프
                int target = Mathf.Clamp(p.friendship + add, 0, _policy.maxFriendship);
                p.friendship = target;
            }
        }
    }
}
