using UnityEngine;

namespace PokeClicker
{
    // ExperienceCurve 식별자는 0) 공통 Enum / Struct (PokemonPrimitives.cs)에 정의되어 있음
    // 이 SO는 실제로 "레벨별 누적 경험치"를 보관/계산하는 역할을 한다.
    // totalExpByLevel: x=레벨(1~100), y=해당 레벨 도달에 필요한 '누적' 경험치

    [CreateAssetMenu(menuName = "PokeClicker/DB/ExperienceCurve")]
    public class ExperienceCurveSO : ScriptableObject
    {
        [Header("경험치 곡선 식별자")]
        public ExperienceCurve id = ExperienceCurve.MediumFast;

        [Header("레벨별 누적 경험치 곡선 (x=1~100, y=누적Exp)")]
        public AnimationCurve totalExpByLevel = new AnimationCurve(
            new Keyframe(1, 0),
            new Keyframe(100, 100000) // 임시 기본치, 게임 밸런스에 맞게 수정
        );

        // OnValidate: 인스펙터에서 값이 바뀔 때마다 자동 정리
        // - x 최소/최대 범위 보정
        // - 레벨 1의 누적Exp는 보통 0으로 고정
        private void OnValidate()
        {
            ClampCurveDomain(1f, 100f);
            ForceLevelOneToZero();
            EnsureMonotonicIncrease();
        }

        // 특정 레벨(1~100)의 "누적 경험치"를 정수로 반환한다.
        public int GetTotalExpForLevel(int level)
        {
            level = Mathf.Clamp(level, 1, 100);
            return Mathf.RoundToInt(totalExpByLevel.Evaluate(level));
        }

        // 해당 레벨에서 "다음 레벨까지" 필요한 경험치(증분)를 반환한다.
        // 100레벨 이상은 더 이상 필요치가 없으므로 int.MaxValue로 처리.
        public int GetNeedExpForNextLevel(int level)
        {
            if (level >= 100) return int.MaxValue;
            int cur = GetTotalExpForLevel(level);
            int next = GetTotalExpForLevel(level + 1);
            return Mathf.Max(0, next - cur);
        }

        // "누적 경험치"를 입력하면 해당하는 레벨을 반환한다.
        // 예: 15000 누적Exp -> 대략 몇 레벨인지 역산
        public int GetLevelFromTotalExp(int totalExp)
        {
            totalExp = Mathf.Max(0, totalExp);

            // 이진 탐색(간단 구현): 1~100 사이에서 누적Exp가 들어갈 레벨을 찾는다.
            int lo = 1;
            int hi = 100;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (GetTotalExpForLevel(mid) <= totalExp)
                    lo = mid;
                else
                    hi = mid - 1;
            }
            return lo;
        }


        // ================== 내부 보조 메서드 ==================

        // 곡선의 x 범위를 1~100으로 보정한다. 경계 밖 키프레임은 삭제.
        private void ClampCurveDomain(float minX, float maxX)
        {
            if (totalExpByLevel == null) return;

            var src = totalExpByLevel.keys;
            var list = new System.Collections.Generic.List<Keyframe>(src.Length);
            for (int i = 0; i < src.Length; i++)
            {
                var k = src[i];
                if (k.time < minX || k.time > maxX) continue;
                // y(경험치)는 음수가 되지 않도록 0 이상으로 클램프
                k.value = Mathf.Max(0f, k.value);
                list.Add(k);
            }

            if (list.Count == 0)
            {
                list.Add(new Keyframe(1f, 0f));
                list.Add(new Keyframe(100f, 100000f));
            }

            totalExpByLevel = new AnimationCurve(list.ToArray());
        }

        // 레벨 1의 누적Exp를 0으로 강제한다. (관례적 처리)
        private void ForceLevelOneToZero()
        {
            if (totalExpByLevel == null) return;

            // 레벨1 근처 키를 직접 0으로 맞춘다
            float v1 = totalExpByLevel.Evaluate(1f);
            if (Mathf.Abs(v1) > 0.01f)
            {
                // 기존 키 삽입/갱신
                AddOrUpdateKey(1f, 0f);
            }
        }

        // 레벨이 증가할수록 누적Exp가 감소하지 않도록 보정한다.
        private void EnsureMonotonicIncrease()
        {
            if (totalExpByLevel == null) return;

            int lastLv = 1;
            int lastTotal = GetTotalExpForLevel(1);
            for (int lv = 2; lv <= 100; lv++)
            {
                int cur = GetTotalExpForLevel(lv);
                if (cur < lastTotal)
                {
                    // 감소가 발생하면 이전 값과 같게 올려 잡는다.
                    AddOrUpdateKey(lv, lastTotal);
                    cur = lastTotal;
                }
                lastLv = lv;
                lastTotal = cur;
            }
        }

        // 해당 time(레벨)에 키가 있으면 값을 갱신, 없으면 새 키프레임을 추가한다.
        private void AddOrUpdateKey(float time, float value)
        {
            if (totalExpByLevel == null)
                totalExpByLevel = new AnimationCurve();

            int idx = FindKeyIndex(time);
            if (idx >= 0)
            {
                var k = totalExpByLevel.keys[idx];
                k.value = value;
                totalExpByLevel.MoveKey(idx, k);
            }
            else
            {
                totalExpByLevel.AddKey(new Keyframe(time, value));
            }
        }

        // 정확히 같은 time(레벨)을 가진 키프레임 인덱스를 찾는다. 없으면 -1
        private int FindKeyIndex(float time)
        {
            if (totalExpByLevel == null) return -1;
            var keys = totalExpByLevel.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                if (Mathf.Approximately(keys[i].time, time))
                    return i;
            }
            return -1;
        }
    }
}
