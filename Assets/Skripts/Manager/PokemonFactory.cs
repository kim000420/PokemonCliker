// 파일: Scripts/Managers/PokemonFactory.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 포획/지급 전용 팩토리.
    /// - 종/폼/레벨과 옵션(성격/이로치/성별/IVs)을 기준으로 PokemonSaveData를 생성한다.
    /// - 계산/표시는 여기서 하지 않는다(StatService를 사용하는 쪽에서 호출).
    /// </summary>
    public class PokemonFactory : MonoBehaviour
    {

        public SpeciesDB speciesDB;                // 씬에서 연결
        public OwnedPokemonManager owned;   // 씬에서 연결
        public int currentTuid;
        public PokemonPoolDB poolDB;
        void Awake()
        {
            // 게임 시작 시 PoolDB 초기화
            if (poolDB != null)
            {
                poolDB.Initialize();
            }
        }

        public struct Options
        {
            public bool rollNature;              // 성격 랜덤 적용 여부 (기본: true)
            public bool rollShiny;               // 이로치 판정 적용 여부 (기본: true)
            public bool rollGender;              // 성별 정책 적용 여부 (기본: true)
            public bool rollIVs;                 // IVs 랜덤 굴림 여부 (기본: false ? 전투 없으면 OFF)
        }

        public static PokemonSaveData Create(
            SpeciesSO species,
            string formKey,
            int level,
            Options? optNullable = null
        )
        {
            if (species == null) throw new ArgumentNullException(nameof(species));
            var opt = optNullable ?? new Options
            {
                rollNature = true,
                rollShiny = true,
                rollGender = true,
                rollIVs = true
            };

            // 기본값 보정
            if (string.IsNullOrWhiteSpace(formKey)) formKey = "Default";
            level = Mathf.Clamp(level, 1, Mathf.Clamp(species.maxLevel, 1, 100));

            // 필수 필드 채우기
            var p = new PokemonSaveData
            {
                P_uid = 0, // 소유 매니저가 부여
                speciesId = species.speciesId,
                formKey = formKey,
                isShiny = opt.rollShiny ? RandomService.RollShiny() : false,
                level = level,
                currentExp = 0,
                nickname = null,
                gender = opt.rollGender ? RandomService.RollGender(species.genderPolicy) : Gender.Genderless,
                friendship = 0,
                heldItemId = null,
                nature = opt.rollNature ? RandomService.RollNature() : NatureId.Hardy,
                ivs = opt.rollIVs ? RandomService.RollIVsAll() : default,
                metLevel = level,
                metFormKey = formKey
            };

            // 저장 안전 보정(상한/하한)
            p.EnsureValidAfterLoad(species);

            Debug.Log($"[Factory] 포켓몬 생성 완료");

            return p;
        }

        // 임시
        public PokemonSaveData GiveSpecificFormPokemon(int speciesId, string formKey, int level)
        {
            var species = speciesDB.GetSpecies(speciesId);
            if (species == null) return null;
            var p = Create(species, formKey, level);
            owned.Add(p);
            return p;
        }

        public PokemonSaveData GiveStarterForSignup(int speciesId, string formKey, int level)
        {
            var species = speciesDB.GetSpecies(speciesId);
            if (species == null) { Debug.LogWarning($"Starter species not found: {speciesId}"); return null; }

            var p = Create(species, formKey, level);   // 여기서는 P_uid 부여 X
            owned.Add(p);                              // 여기서 Provider를 통해 P_uid 순차 발급
            return p;
        }

        public PokemonSaveData GiveRandomTest(int level)
        {
            var s = speciesDB.GetRandom();
            if (s == null) return null;

            var forms = s.Forms;
            if (forms == null || forms.Count == 0) return null;

            var key = forms[UnityEngine.Random.Range(0, forms.Count)].formKey;
            var p = Create(s, key, level); // P_uid 없이 생성
            owned.Add(p);                  // 여기서 P_uid 부여
            return p;
        }

        /// <summary>
        /// 포획 전의 야생 포켓몬 데이터를 생성만 하고 반환합니다. (소유 처리 X)
        /// </summary>
        public PokemonSaveData CreateRandomWildPokemon(int level)
        {
            var s = speciesDB.GetRandom();
            if (s == null) return null;

            var forms = s.Forms;
            if (forms == null || forms.Count == 0) return null;

            var key = forms[UnityEngine.Random.Range(0, forms.Count)].formKey;

            // Create 메서드는 포켓몬 데이터를 생성만 하므로,
            // owned.Add()를 호출하지 않으면 소유되지 않은 상태로 남습니다.
            var p = Create(s, key, level);
            return p;
        }

        public PokemonSaveData PullFromAllPool()
        {
            return PullFromPool("all_pool");
        }

        public PokemonSaveData PullFromEventPool()
        {
            return PullFromPool("event_pool");
        }

        public PokemonSaveData PullFromShinyPool()
        {
            // 이로치 뽑기는 모든 포켓몬 중에서 랜덤으로 뽑되, isShiny만 true로 설정
            var p = CreateRandomWildPokemon(1);
            if (p == null) return null;
            p.isShiny = true;
            owned.Add(p);
            return p;
        }

        public PokemonSaveData PullFromLegendaryPool()
        {
            return PullFromPool("legendary_pool");
        }

        private PokemonSaveData PullFromPool(string poolName)
        {
            var pool = poolDB.GetPool(poolName);
            if (pool == null || pool.entries.Count == 0)
            {
                Debug.LogWarning($"{poolName} is empty or not found.");
                return null;
            }

            // 1. 풀에서 랜덤으로 하나 선택
            var randomEntry = pool.entries[RandomService.NextInt(pool.entries.Count)];

            // 2. 해당 정보로 포켓몬 생성
            var species = speciesDB.GetSpecies(randomEntry.speciesId);
            var p = Create(species, randomEntry.formKey, 1); // 레벨은 1로 고정

            // 3. 소유 처리
            owned.Add(p);
            return p;
        }
    }
}
