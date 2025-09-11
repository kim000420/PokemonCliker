using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// �Է� ����(ClickProgressTracker)�� ��������
    /// ��Ƽ 1��/2~6�� ���Կ� ģ�е� ������ �й��Ѵ�.
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
        /// �Է� 1ȸ�� ó��: ������ ������Ű��, ���� �ֱ⿡ �����ߴٸ� ģ�е� ������ �й��Ѵ�.
        /// ���� �ֱⰡ �� ���� ������ �� �����Ƿ� delta(>=0)�� ��� ��ȭ�Ѵ�.
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

                // Ŭ����
                int target = Mathf.Clamp(p.friendship + add, 0, _policy.maxFriendship);
                p.friendship = target;
            }
        }
    }
}
