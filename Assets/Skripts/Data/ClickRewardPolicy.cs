// ����: Scripts/Policy/ClickRewardPolicy.cs
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// Ŭ��/�Է� ���� �뷱�� ���� �����ϴ� ��å(SO).
    /// �����͸� ���(���/�й�� �ٸ� ����).
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/Policy/ClickRewardPolicy")]
    public class ClickRewardPolicy : ScriptableObject
    {
        [Header("EXP")]
        [Min(0)] public int expPerInput = 1;

        [Header("ģ�е�")]
        [Min(1)] public int clicksPerFriendship = 1000;
        [Min(0)] public int friendshipSlot1 = 2;   // ��Ƽ 1��
        [Min(0)] public int friendshipOthers = 1;  // ��Ƽ 2~6��
        [Range(0, 255)] public int maxFriendship = 255;

        [Header("�߰� ���(����)")]
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
