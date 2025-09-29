using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// Ʈ���̳� ���� ���̺� (������ + ���� ���ϸ�) JSON ���� (ITrainerRepository)
    /// </summary>
    public class JsonTrainerRepository : ITrainerRepository
    {
        private readonly string _rootPath = Path.Combine(Application.persistentDataPath, "Trainer");
        private const int _pokemonPerBox = 30;  // �ڽ� ���� 30 ����

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

            public int boxCount = 0; // �� �ڽ� ��
            public List<int> allBoxedPokemon = new(); // ��� �ڽ� ���ϸ��� �ϳ��� ����Ʈ�� ����
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
            dto.boxCount = boxes.Count;
            foreach (var box in boxes)
            {
                // �� �ڽ��� 30ĭ�� ä�쵵�� ���� (�� ĭ�� 0)
                var boxData = new int[_pokemonPerBox];
                for (int i = 0; i < box.Count; i++)
                {
                    boxData[i] = box.ElementAt(i);
                }
                dto.allBoxedPokemon.AddRange(boxData);
            }

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
            boxes = new List<List<int>>();
            if (dto.allBoxedPokemon != null)
            {
                for (int i = 0; i < dto.boxCount; i++)
                {
                    var box = new List<int>();
                    int startIndex = i * _pokemonPerBox;
                    for (int j = 0; j < _pokemonPerBox; j++)
                    {
                        int currentIndex = startIndex + j;
                        if (currentIndex < dto.allBoxedPokemon.Count)
                        {
                            box.Add(dto.allBoxedPokemon[currentIndex]);
                        }
                        else
                        {
                            box.Add(0); // �����Ͱ� �����ϸ� 0���� ä��
                        }
                    }
                    boxes.Add(box);
                }
            }
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
