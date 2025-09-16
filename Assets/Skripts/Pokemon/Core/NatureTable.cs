using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    // 성격이 스탯에 주는 배율(HP는 영향 없음)
    public struct NatureMultiplier
    {
        public float atk; // 공격
        public float def; // 방어
        public float spa; // 특공
        public float spd; // 특방
        public float spe; // 스피드

        public NatureMultiplier(float atk, float def, float spa, float spd, float spe)
        {
            this.atk = atk; this.def = def; this.spa = spa; this.spd = spd; this.spe = spe;
        }
    }

    // 고정 테이블: 성격별 배율
    // 중립 성격은 모든 배율이 1.0
    public static class NatureTable
    {
        // 읽기 전용 테이블
        private static readonly Dictionary<NatureId, NatureMultiplier> _map = new Dictionary<NatureId, NatureMultiplier>
        {
            // 공격↑ 방어↓
            { NatureId.Lonely,  new NatureMultiplier(1.1f, 0.9f, 1.0f, 1.0f, 1.0f) },
            // 공격↑ 특공↓
            { NatureId.Adamant, new NatureMultiplier(1.1f, 1.0f, 0.9f, 1.0f, 1.0f) },
            // 공격↑ 특방↓
            { NatureId.Naughty, new NatureMultiplier(1.1f, 1.0f, 1.0f, 0.9f, 1.0f) },
            // 공격↑ 스피드↓
            { NatureId.Brave,   new NatureMultiplier(1.1f, 1.0f, 1.0f, 1.0f, 0.9f) },

            // 방어↑ 공격↓
            { NatureId.Bold,    new NatureMultiplier(0.9f, 1.1f, 1.0f, 1.0f, 1.0f) },
            // 방어↑ 특공↓
            { NatureId.Impish,  new NatureMultiplier(1.0f, 1.1f, 0.9f, 1.0f, 1.0f) },
            // 방어↑ 특방↓
            { NatureId.Lax,     new NatureMultiplier(1.0f, 1.1f, 1.0f, 0.9f, 1.0f) },
            // 방어↑ 스피드↓
            { NatureId.Relaxed, new NatureMultiplier(1.0f, 1.1f, 1.0f, 1.0f, 0.9f) },

            // 특공↑ 공격↓
            { NatureId.Modest,  new NatureMultiplier(0.9f, 1.0f, 1.1f, 1.0f, 1.0f) },
            // 특공↑ 방어↓
            { NatureId.Mild,    new NatureMultiplier(1.0f, 0.9f, 1.1f, 1.0f, 1.0f) },
            // 특공↑ 특방↓
            { NatureId.Rash,    new NatureMultiplier(1.0f, 1.0f, 1.1f, 0.9f, 1.0f) },
            // 특공↑ 스피드↓
            { NatureId.Quiet,   new NatureMultiplier(1.0f, 1.0f, 1.1f, 1.0f, 0.9f) },

            // 특방↑ 공격↓
            { NatureId.Calm,    new NatureMultiplier(0.9f, 1.0f, 1.0f, 1.1f, 1.0f) },
            // 특방↑ 방어↓
            { NatureId.Gentle,  new NatureMultiplier(1.0f, 0.9f, 1.0f, 1.1f, 1.0f) },
            // 특방↑ 특공↓
            { NatureId.Careful, new NatureMultiplier(1.0f, 1.0f, 0.9f, 1.1f, 1.0f) },
            // 특방↑ 스피드↓
            { NatureId.Sassy,   new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.1f, 0.9f) },

            // 스피드↑ 공격↓
            { NatureId.Timid,   new NatureMultiplier(0.9f, 1.0f, 1.0f, 1.0f, 1.1f) },
            // 스피드↑ 방어↓
            { NatureId.Hasty,   new NatureMultiplier(1.0f, 0.9f, 1.0f, 1.0f, 1.1f) },
            // 스피드↑ 특공↓
            { NatureId.Jolly,   new NatureMultiplier(1.0f, 1.0f, 0.9f, 1.0f, 1.1f) },
            // 스피드↑ 특방↓
            { NatureId.Naive,   new NatureMultiplier(1.0f, 1.0f, 1.0f, 0.9f, 1.1f) },

            // 중립 5종
            { NatureId.Hardy,   new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
            { NatureId.Docile,  new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
            { NatureId.Serious, new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
            { NatureId.Bashful, new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
            { NatureId.Quirky,  new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
        };

        // 성격 배율 얻기
        public static NatureMultiplier Get(NatureId id)
        {
            if (_map.TryGetValue(id, out var m)) return m;
            return new NatureMultiplier(1f, 1f, 1f, 1f, 1f);
        }

        // 디버그/툴용: 성격이 스탯에 주는 배율을 특정 스탯 코드로 가져오기
        // statCode: "atk","def","spa","spd","spe"
        public static float GetStatMultiplier(NatureId id, string statCode)
        {
            var m = Get(id);
            switch (statCode)
            {
                case "atk": return m.atk;
                case "def": return m.def;
                case "spa": return m.spa;
                case "spd": return m.spd;
                case "spe": return m.spe;
                default: return 1f;
            }
        }
    }
}
