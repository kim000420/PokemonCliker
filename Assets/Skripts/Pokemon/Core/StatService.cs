using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ����ġ ��� ���� ����.
    /// - HP/��Ÿ ����(IV/���� �ɼ� �ݿ�)
    /// - BaseStats�� ���� SpeciesSO�� �ƴ� FormSO���� �д´�.
    /// </summary>

    public static class StatService
    {
        // ������������������������������������������������������������������������������������������������������������������������������������������
        // Species + FormSO �� ��������� �޴´�.
        // ������������������������������������������������������������������������������������������������������������������������������������������
        public static DerivedStats ComputeFor(
            PokemonSaveData p,
            SpeciesSO species,
            FormSO form,
            IVs ivs,
            NatureId nature,
            bool useIV,
            bool useNature)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (species == null) throw new ArgumentNullException(nameof(species));
            if (form == null) throw new ArgumentNullException(nameof(form));

            // ����/���� ����
            int level = Mathf.Clamp(p.level, 1, Mathf.Clamp(species.maxLevel, 1, 100));

            var baseStats = form.baseStats;

            // IV ������ ���
            if (!useIV) { ivs = default; }
            else ivs.Clamp();

            // ���� ����
            float atkMul = 1f, defMul = 1f, spaMul = 1f, spdMul = 1f, speMul = 1f;
            if (useNature)
            {
                atkMul = NatureTable.GetStatMultiplier(nature, "atk");
                defMul = NatureTable.GetStatMultiplier(nature, "def");
                spaMul = NatureTable.GetStatMultiplier(nature, "spa");
                spdMul = NatureTable.GetStatMultiplier(nature, "spd");
                speMul = NatureTable.GetStatMultiplier(nature, "spe");
            }

            DerivedStats d;
            d.hp = ComputeHP(baseStats.hp, ivs.hp, level);
            d.atk = ComputeStat(baseStats.atk, ivs.atk, level, atkMul);
            d.def = ComputeStat(baseStats.def, ivs.def, level, defMul);
            d.spa = ComputeStat(baseStats.spa, ivs.spa, level, spaMul);
            d.spd = ComputeStat(baseStats.spd, ivs.spd, level, spdMul);
            d.spe = ComputeStat(baseStats.spe, ivs.spe, level, speMul);

            return d;
        }

        // ������������������������������������������������������������������������������������������������������������������������������������������
        // ���� �����ε�: formKey�� �޾� SpeciesSO���� ��ȸ
        // ������������������������������������������������������������������������������������������������������������������������������������������
        public static DerivedStats ComputeFor(
            PokemonSaveData p,
            SpeciesSO species,
            string formKey,
            IVs ivs,
            NatureId nature,
            bool useIV,
            bool useNature)
        {
            var form = species?.GetForm(formKey);
            if (form == null)
                throw new InvalidOperationException($"Form not found: species={species?.speciesId}, formKey='{formKey}'");
            return ComputeFor(p, species, form, ivs, nature, useIV, useNature);
        }

        // ������������������������������������������������������������������������������������������������������������������������������������������
        // ����
        // ������������������������������������������������������������������������������������������������������������������������������������������
        private static int ComputeHP(int baseStat, int iv, int level)
        {
            // HP = ((2*Base + IV) * Level / 100) + Level + 10
            int val = ((2 * baseStat + iv) * level) / 100 + level + 10;
            return Mathf.Max(1, val);
        }

        private static int ComputeStat(int baseStat, int iv, int level, float natureMul)
        {
            // Stat = ( ((2*Base + IV) * Level / 100) + 5 ) * NatureMul
            int val = ((2 * baseStat + iv) * level) / 100 + 5;
            val = Mathf.FloorToInt(val * natureMul);
            return Mathf.Max(1, val);
        }
    }
}
