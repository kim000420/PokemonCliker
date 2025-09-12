using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 실측치 계산 전담 서비스.
    /// - HP/기타 공식(IV/성격 옵션 반영)
    /// - BaseStats는 이제 SpeciesSO가 아닌 FormSO에서 읽는다.
    /// </summary>

    public static class StatService
    {
        // ─────────────────────────────────────────────────────────────────────
        // Species + FormSO 를 명시적으로 받는다.
        // ─────────────────────────────────────────────────────────────────────
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

            // 레벨/상한 보정
            int level = Mathf.Clamp(p.level, 1, Mathf.Clamp(species.maxLevel, 1, 100));

            var baseStats = form.baseStats;

            // IV 선택적 사용
            if (!useIV) { ivs = default; }
            else ivs.Clamp();

            // 성격 배율
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

        // ─────────────────────────────────────────────────────────────────────
        // 편의 오버로드: formKey로 받아 SpeciesSO에서 조회
        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
        // 공식
        // ─────────────────────────────────────────────────────────────────────
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
