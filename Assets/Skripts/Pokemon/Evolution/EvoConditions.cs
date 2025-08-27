using UnityEngine;

namespace PokeClicker
{
    // 외부 시간/인벤토리 의존을 느슨하게 하기 위한 인터페이스
    public interface IGameTime
    {
        int GetHour(); // 0~23
    }

    public interface IInventory
    {
        bool HasItem(string itemId);
    }

    // 모든 진화 조건의 공통 기반
    public abstract class EvoConditionSO : ScriptableObject
    {
        public abstract bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv);
    }

    // 레벨 조건
    [CreateAssetMenu(menuName = "PokeClicker/EvoCondition/Level")]
    public class LevelConditionSO : EvoConditionSO
    {
        public int minLevel = 1;

        public override bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            return p.level >= minLevel;
        }
    }

    // 친밀도 조건
    [CreateAssetMenu(menuName = "PokeClicker/EvoCondition/Friendship")]
    public class FriendshipConditionSO : EvoConditionSO
    {
        public int minFriendship = 220;

        public override bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            return p.friendship >= minFriendship;
        }
    }

    // 성별 조건
    [CreateAssetMenu(menuName = "PokeClicker/EvoCondition/Gender")]
    public class GenderConditionSO : EvoConditionSO
    {
        public Gender requiredGender = Gender.Male;

        public override bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            return p.gender == requiredGender;
        }
    }

    // 소지 아이템 조건
    [CreateAssetMenu(menuName = "PokeClicker/EvoCondition/HeldItem")]
    public class HeldItemConditionSO : EvoConditionSO
    {
        public string itemId;

        public override bool IsSatisfied(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            if (string.IsNullOrEmpty(itemId)) return true;

            // 인벤토리를 사용하는 방식
            if (inv != null)
                return inv.HasItem(itemId);

            // 개체가 직접 들고 있는 아이템을 사용할 경우
            if (!string.IsNullOrEmpty(p.heldItemId))
                return p.heldItemId == itemId;

            return false;
        }
    }

    // 시간대 분류
    public enum TimeBand { Any, Morning, Day, Evening, Night }

    // 시간대 조건
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

    // 필요 시 교환 조건 등을 여기에 추가
}
