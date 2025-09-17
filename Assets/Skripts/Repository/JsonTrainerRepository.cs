using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 트레이너 단위 세이브 (프로필 + 보유 포켓몬) JSON 구현 (ITrainerRepository)
    /// </summary>
    public class JsonTrainerRepository : MonoBehaviour, ITrainerRepository
    {
        [SerializeField] string folderName = "Trainer";
        string Root => Path.Combine(Application.persistentDataPath, folderName);

        string ProfilePath(int T_uid) => Path.Combine(Root, $"{T_uid}_profile.json");
        string OwnedPath(int T_uid) => Path.Combine(Root, $"{T_uid}_owned.json");

        void Awake() => Directory.CreateDirectory(Root);

        // ===== 프로필 =====
        public void SaveTrainerProfile(TrainerProfile profile)
        {
            WriteJson(ProfilePath(profile.T_uid), profile);
        }

        public TrainerProfile LoadTrainerProfile(int T_uid)
        {
            return ReadJson<TrainerProfile>(ProfilePath(T_uid));
        }

        // ===== 보유 포켓몬 =====
        [Serializable]
        private class OwnedDto
        {
            public List<PokemonSaveData> tableList = new(); // Dictionary<int,PokemonSaveData> 대체
            public List<int> party = new();
            public List<List<int>> boxes = new();
        }

        public void SaveOwnedPokemon(
            int T_uid,
            IReadOnlyDictionary<int, PokemonSaveData> table,
            IReadOnlyList<int> party,
            IReadOnlyList<IReadOnlyList<int>> boxes)
        {
            var dto = new OwnedDto();

            // table(Dictionary) → list 로 변환 (JsonUtility는 Dictionary 직렬화가 약함)
            foreach (var kv in table)
                dto.tableList.Add(kv.Value);

            dto.party = new List<int>(party);
            dto.boxes = new List<List<int>>();
            foreach (var b in boxes)
                dto.boxes.Add(new List<int>(b));

            WriteJson(OwnedPath(T_uid), dto);
        }

        public void LoadOwnedPokemon(int T_uid,
            out Dictionary<int, PokemonSaveData> table,
            out List<int> party,
            out List<List<int>> boxes)
        {
            var dto = ReadJson<OwnedDto>(OwnedPath(T_uid)) ?? new OwnedDto();
            table = new Dictionary<int, PokemonSaveData>();
            foreach (var p in dto.tableList)
            {
                if (p != null)
                {
                    // P_uid 키 보정(세이브 데이터의 P_uid가 곧 키)
                    if (p.P_uid <= 0) continue;
                    table[p.P_uid] = p;
                }
            }
            party = dto.party ?? new List<int>();
            boxes = dto.boxes ?? new List<List<int>>();
        }

        // ===== IO 유틸 =====
        T ReadJson<T>(string path)
        {
            if (!File.Exists(path)) return default;
            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
        void WriteJson<T>(string path, T data)
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);

            UnityEngine.Debug.Log($"[REPO] Saved JSON to {path}");
        }
    }
}
