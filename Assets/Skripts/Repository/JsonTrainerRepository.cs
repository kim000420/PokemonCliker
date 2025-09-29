using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// Ʈ���̳� ���� ���̺� (������ + ���� ���ϸ�) JSON ���� (ITrainerRepository)
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

        // ===== ������ =====
        public void SaveTrainerProfile(TrainerProfile profile)
        {
            WriteJson(ProfilePath(profile.T_uid), profile);
        }

        public TrainerProfile LoadTrainerProfile(int T_uid)
        {
            return ReadJson<TrainerProfile>(ProfilePath(T_uid));
        }

        // ===== ���� ���ϸ� =====
        [Serializable]
        private class OwnedDto
        {
            public List<PokemonSaveData> tableList = new(); // Dictionary<int,PokemonSaveData> ��ü
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

            // table(Dictionary) �� list �� ��ȯ (JsonUtility�� Dictionary ����ȭ�� ����)
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
                    // P_uid Ű ����(���̺� �������� P_uid�� �� Ű)
                    if (p.P_uid <= 0) continue;
                    table[p.P_uid] = p;
                }
            }
            party = dto.party ?? new List<int>();
            boxes = dto.boxes ?? new List<List<int>>();
        }

        // ===== IO ��ƿ =====
        T ReadJson<T>(string path)
        {
            string backupPath = path + ".bak";

            // 1. �� ���̺� ������ ���� �н��ϴ�.
            if (File.Exists(path))
            {
                try
                {
                    return JsonUtility.FromJson<T>(File.ReadAllText(path));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"�� ���̺� ����({path})�� �д� �� �����߽��ϴ�. ��� ���Ϸ� ������ �õ��մϴ�. ����: {e.Message}");
                }
            }

            // 2. �� ������ ���ų� �ջ�Ǿ��ٸ�, ��� ������ �н��ϴ�.
            if (File.Exists(backupPath))
            {
                try
                {
                    Debug.Log($"��� ����({backupPath})���� �����͸� �����մϴ�.");
                    return JsonUtility.FromJson<T>(File.ReadAllText(backupPath));
                }
                catch (Exception e)
                {
                    Debug.LogError($"��� ����({backupPath})���� �д� �� �����߽��ϴ�. �� �����͸� �����մϴ�. ����: {e.Message}");
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
                // 1. �ӽ� ���Ͽ� ���� ���ϴ�.
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(tempPath, json);

                // 2. ���� ������ �ִٸ� ����մϴ�.
                if (File.Exists(path))
                {
                    File.Replace(tempPath, path, backupPath);
                }
                else
                {
                    File.Move(tempPath, path);
                }

                // 3. (���û���) ���������� ���� �� ��� ���� ����
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                Debug.Log($"[REPO] Saved JSON to {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON ���� ���� �� �ɰ��� ���� �߻�: {path}. ����: {e.Message}");
            }
        }
    }
}
