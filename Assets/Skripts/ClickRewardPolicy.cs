// 파일: Scripts/Policy/ClickRewardPolicy.cs
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 클릭/입력 보상 밸런스 값을 보관하는 정책(SO).
    /// 데이터만 담당(계산/분배는 다른 계층).
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/Policy/ClickRewardPolicy")]
    public class ClickRewardPolicy : ScriptableObject
    {
        [Header("EXP")]
        [Min(0)] public int expPerInput = 1;

        [Header("친밀도")]
        [Min(1)] public int clicksPerFriendship = 1000;
        [Min(0)] public int friendshipSlot1 = 2;   // 파티 1번
        [Min(0)] public int friendshipOthers = 1;  // 파티 2~6번
        [Range(0, 255)] public int maxFriendship = 255;

        [Header("추가 배수(선택)")]
        [Min(0f)] public float expMultiplier = 1f;
        [Min(0f)] public float friendshipMultiplier = 1f;

        public int GetExpPerInput() => Mathf.RoundToInt(expPerInput * expMultiplier);
        public int GetFriendshipForSlot(int slotIndex0, int baseDelta)
        {
            int baseGain = (slotIndex0 == 0 ? friendshipSlot1 : friendshipOthers) * baseDelta;
            return Mathf.RoundToInt(baseGain * friendshipMultiplier);
        }
    }
}
