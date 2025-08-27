using UnityEngine;

namespace PokeClicker
{
    // ExperienceCurve �ĺ��ڴ� 0) ���� Enum / Struct (PokemonPrimitives.cs)�� ���ǵǾ� ����
    // �� SO�� ������ "������ ���� ����ġ"�� ����/����ϴ� ������ �Ѵ�.
    // totalExpByLevel: x=����(1~100), y=�ش� ���� ���޿� �ʿ��� '����' ����ġ

    [CreateAssetMenu(menuName = "PokeClicker/DB/ExperienceCurve")]
    public class ExperienceCurveSO : ScriptableObject
    {
        [Header("����ġ � �ĺ���")]
        public ExperienceCurve id = ExperienceCurve.MediumFast;

        [Header("������ ���� ����ġ � (x=1~100, y=����Exp)")]
        public AnimationCurve totalExpByLevel = new AnimationCurve(
            new Keyframe(1, 0),
            new Keyframe(100, 100000) // �ӽ� �⺻ġ, ���� �뷱���� �°� ����
        );

        // OnValidate: �ν����Ϳ��� ���� �ٲ� ������ �ڵ� ����
        // - x �ּ�/�ִ� ���� ����
        // - ���� 1�� ����Exp�� ���� 0���� ����
        private void OnValidate()
        {
            ClampCurveDomain(1f, 100f);
            ForceLevelOneToZero();
            EnsureMonotonicIncrease();
        }

        // Ư�� ����(1~100)�� "���� ����ġ"�� ������ ��ȯ�Ѵ�.
        public int GetTotalExpForLevel(int level)
        {
            level = Mathf.Clamp(level, 1, 100);
            return Mathf.RoundToInt(totalExpByLevel.Evaluate(level));
        }

        // �ش� �������� "���� ��������" �ʿ��� ����ġ(����)�� ��ȯ�Ѵ�.
        // 100���� �̻��� �� �̻� �ʿ�ġ�� �����Ƿ� int.MaxValue�� ó��.
        public int GetNeedExpForNextLevel(int level)
        {
            if (level >= 100) return int.MaxValue;
            int cur = GetTotalExpForLevel(level);
            int next = GetTotalExpForLevel(level + 1);
            return Mathf.Max(0, next - cur);
        }

        // "���� ����ġ"�� �Է��ϸ� �ش��ϴ� ������ ��ȯ�Ѵ�.
        // ��: 15000 ����Exp -> �뷫 �� �������� ����
        public int GetLevelFromTotalExp(int totalExp)
        {
            totalExp = Mathf.Max(0, totalExp);

            // ���� Ž��(���� ����): 1~100 ���̿��� ����Exp�� �� ������ ã�´�.
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


        // ================== ���� ���� �޼��� ==================

        // ��� x ������ 1~100���� �����Ѵ�. ��� �� Ű�������� ����.
        private void ClampCurveDomain(float minX, float maxX)
        {
            if (totalExpByLevel == null) return;

            var src = totalExpByLevel.keys;
            var list = new System.Collections.Generic.List<Keyframe>(src.Length);
            for (int i = 0; i < src.Length; i++)
            {
                var k = src[i];
                if (k.time < minX || k.time > maxX) continue;
                // y(����ġ)�� ������ ���� �ʵ��� 0 �̻����� Ŭ����
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

        // ���� 1�� ����Exp�� 0���� �����Ѵ�. (������ ó��)
        private void ForceLevelOneToZero()
        {
            if (totalExpByLevel == null) return;

            // ����1 ��ó Ű�� ���� 0���� �����
            float v1 = totalExpByLevel.Evaluate(1f);
            if (Mathf.Abs(v1) > 0.01f)
            {
                // ���� Ű ����/����
                AddOrUpdateKey(1f, 0f);
            }
        }

        // ������ �����Ҽ��� ����Exp�� �������� �ʵ��� �����Ѵ�.
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
                    // ���Ұ� �߻��ϸ� ���� ���� ���� �÷� ��´�.
                    AddOrUpdateKey(lv, lastTotal);
                    cur = lastTotal;
                }
                lastLv = lv;
                lastTotal = cur;
            }
        }

        // �ش� time(����)�� Ű�� ������ ���� ����, ������ �� Ű�������� �߰��Ѵ�.
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

        // ��Ȯ�� ���� time(����)�� ���� Ű������ �ε����� ã�´�. ������ -1
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
