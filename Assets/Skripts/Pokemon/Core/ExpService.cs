using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ����ġ �߰��� ������ ������ ����Ѵ�.
    /// - PokemonSaveData.level �� currentExp �� �����Ѵ�.
    /// - SpeciesSO.maxLevel �� ���� �ʵ��� �����Ѵ�.
    /// - ���������� onLevelUp �ݹ��� ȣ���Ѵ�.
    /// </summary>
    public static class ExpService
    {
        /// <summary>
        /// ����ġ�� �߰��ϰ� �ʿ��ϸ� ���� �������� ó���Ѵ�.
        /// ��ȯ��: ���������� ������ ������ Ƚ��
        /// </summary>
        public static int AddExpAndHandleLevelUps(
            PokemonSaveData p,
            SpeciesSO species,
            ExperienceCurve curve,
            int gainedExp,
            Action<int> onLevelUp = null // newLevel ����
        )
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (species == null) throw new ArgumentNullException(nameof(species));

            // ���� ����
            int maxLv = Mathf.Clamp(species.maxLevel, 1, 100); // 1~100 (SpeciesSO ��Ģ)
            if (p.level < 1) p.level = 1;
            if (p.level > maxLv) p.level = maxLv;
            if (gainedExp < 0) gainedExp = 0;

            // �����̸� ����ġ�� �ǹ� ����
            if (p.level >= maxLv)
            {
                p.currentExp = 0; // ���������� 0 ����
                return 0;
            }

            // ����ġ �߰�
            p.currentExp += gainedExp;

            int levelUps = 0;

            // while ����: ���� �������� �ʿ� EXP�� ä��� ������
            while (p.level < maxLv)
            {
                int need = ExperienceCurveService.GetNeedExpForNextLevel(species.ExpCurve, p.level); // ���� �������� �ʿ� EXP
                if (need == int.MaxValue) // ���� ó��
                {
                    p.currentExp = 0;
                    break;
                }

                if (p.currentExp >= need)
                {
                    p.currentExp -= need;
                    p.level += 1;
                    levelUps += 1;

                    onLevelUp?.Invoke(p.level);

                    // ���� ���� �� �ܿ� ����ġ�� �Ұ�
                    if (p.level >= maxLv)
                    {
                        p.level = maxLv;
                        p.currentExp = 0;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // ���� ���������� currentExp ���� ����
            if (p.level >= maxLv) p.currentExp = 0;
            else
            {
                int need = ExperienceCurveService.GetNeedExpForNextLevel(species.ExpCurve, p.level);
                if (need != int.MaxValue)
                    p.currentExp = Mathf.Clamp(p.currentExp, 0, Math.Max(0, need - 1));
            }

            return levelUps;
        }
    }
}
