using UnityEngine;

namespace PokeClicker
{
    // �ܺ� �ð�/�κ��丮 ������ �����ϰ� �ϱ� ���� �������̽�
    public interface IGameTime
    {
        int GetHour(); // 0~23
    }

    public interface IInventory
    {
        bool HasItem(string itemId);
    }

    // ��� ��ȭ ������ ���� ���
    public abstract class EvoConditionSO : ScriptableObject
    {
        public abstract bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv);
    }

    // ���� ����
    [CreateAssetMenu(menuName = "PokeClicker/EvoCondition/Level")]
    public class LevelConditionSO : EvoConditionSO
    {
        public int minLevel = 1;

        public override bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            return p.level >= minLevel;
        }
    }

    // ģ�е� ����
    [CreateAssetMenu(menuName = "PokeClicker/EvoCondition/Friendship")]
    public class FriendshipConditionSO : EvoConditionSO
    {
        public int minFriendship = 220;

        public override bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            return p.friendship >= minFriendship;
        }
    }

    // ���� ����
    [CreateAssetMenu(menuName = "PokeClicker/EvoCondition/Gender")]
    public class GenderConditionSO : EvoConditionSO
    {
        public Gender requiredGender = Gender.Male;

        public override bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            return p.gender == requiredGender;
        }
    }

    // ���� ������ ����
    [CreateAssetMenu(menuName = "PokeClicker/EvoCondition/HeldItem")]
    public class HeldItemConditionSO : EvoConditionSO
    {
        public string itemId;

        public override bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            if (string.IsNullOrEmpty(itemId)) return true;

            // �κ��丮�� ����ϴ� ���
            if (inv != null)
                return inv.HasItem(itemId);

            // ��ü�� ���� ��� �ִ� �������� ����� ���
            if (!string.IsNullOrEmpty(p.heldItemId))
                return p.heldItemId == itemId;

            return false;
        }
    }

    // �ð��� �з�
    public enum TimeBand { Any, Morning, Day, Evening, Night }

    // �ð��� ����
    [CreateAssetMenu(menuName = "PokeClicker/EvoCondition/TimeOfDay")]
    public class TimeOfDayConditionSO : EvoConditionSO
    {
        public TimeBand band = TimeBand.Any;

        public override bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            if (band == TimeBand.Any || time == null) return true;

            int h = time.GetHour(); // 0~23
            switch (band)
            {
                case TimeBand.Morning: return h >= 5 && h < 10;
                case TimeBand.Day: return h >= 10 && h < 18;
                case TimeBand.Evening: return h >= 18 && h < 21;
                case TimeBand.Night: return (h >= 21 && h <= 23) || (h >= 0 && h < 5);
                default: return true;
            }
        }
    }

    // �ʿ� �� ��ȯ ���� ���� ���⿡ �߰�
}
