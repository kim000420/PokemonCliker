using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 트레이너 단위 세이브 (프로필 + 보유 포켓몬) JSON 구현 (ITrainerRepository)
    /// </summary>
    public class JsonTrainerRepository : ITrainerRepository
    {
        private readonly string _rootPath = Path.Combine(Application.persistentDataPath, "Trainer");

        public JsonTrainerRepository()
        {
            Directory.CreateDirectory(_rootPath);
        }

        string ProfilePath(int T_uid) => Path.Combine(_rootPath, $"{T_uid}_profile.json");
        string OwnedPath(int T_uid) => Path.Combine(_rootPath, $"{T_uid}_owned.json");

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
            string backupPath = path + ".bak";

            // 1. 주 세이브 파일을 먼저 읽습니다.
            if (File.Exists(path))
            {
                try
                {
                    return JsonUtility.FromJson<T>(File.ReadAllText(path));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"주 세이브 파일({path})을 읽는 데 실패했습니다. 백업 파일로 복구를 시도합니다. 오류: {e.Message}");
                }
            }

            // 2. 주 파일이 없거나 손상되었다면, 백업 파일을 읽습니다.
            if (File.Exists(backupPath))
            {
                try
                {
                    Debug.Log($"백업 파일({backupPath})에서 데이터를 복구합니다.");
                    return JsonUtility.FromJson<T>(File.ReadAllText(backupPath));
                }
                catch (Exception e)
                {
                    Debug.LogError($"백업 파일({backupPath})조차 읽는 데 실패했습니다. 새 데이터를 생성합니다. 오류: {e.Message}");
                }
            }

            return default;
        }

        void WriteJson<T>(string path, T data)
        {
            string tempPath = path + ".tmp";
            string backupPath = path + ".bak";

            try
            {
                // 1. 임시 파일에 먼저 씁니다.
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(tempPath, json);

                // 2. 기존 파일이 있다면 백업합니다.
                if (File.Exists(path))
                {
                    File.Replace(tempPath, path, backupPath);
                }
                else
                {
                    File.Move(tempPath, path);
                }

                // 3. (선택사항) 성공적으로 저장 후 백업 파일 삭제
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                Debug.Log($"[REPO] Saved JSON to {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON 파일 저장 중 심각한 오류 발생: {path}. 오류: {e.Message}");
            }
        }
    }
}
