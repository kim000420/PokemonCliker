using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ����/����/��ġ ����(��Ƽ���ڽ�����ü ���̺�).
    /// - P_uid(��ü UID) �߱��� �ܺ�(IPokemonIdProvider)���� ����
    /// - ��Ƽ(<=6), �ڽ�(����) �̵�/��ü/����
    /// - �ܺ� ����ȭ(ITrainerRepository ���� �޼���)�� ���� ����/�ε�
    /// </summary>
    public class OwnedPokemonManager : MonoBehaviour
    {
        // ===== �ܺ� �߱ޱ� =====
        public interface IPokemonIdProvider
        {
            int NextPuid(); // �� P_uid �߱�
        }

        private IPokemonIdProvider _idProvider;  // null ����(�� ��� Add �� P_uid �ʼ�)

        // ===== ������ ���� =====
        private readonly Dictionary<int, PokemonSaveData> _table = new(); // P_uid -> data
        private int[] _party; // int[] �迭�� ����
        private readonly List<int[]> _boxes = new(); // int[] �迭�� ����Ʈ�� ����
        public event Action OnPartyUpdated; // ��Ƽ ������ ���� �� ȣ��Ǵ� �̺�Ʈ

        private int _nextPuid = 1;              // P_uid �߱� ������ (�⺻ 1����)
        private int _partyLimit = 6;            // ��Ƽ �ִ�� 6 ����
        private const int _pokemonPerBox = 30;  // �ڽ� ���� 30 ����
        public void Init(int partyLimit = 6, IPokemonIdProvider idProvider = null)
        {
            _party = new int[_partyLimit];
            _idProvider = idProvider;
        }


        // ====== ��ȸ ======
        public int[] GetParty() => _party;
        public List<int[]> GetBoxes() => _boxes; // ��ȯ Ÿ���� List<int[]>�� ����
        public IReadOnlyDictionary<int, PokemonSaveData> Table => _table;

        public PokemonSaveData GetByPuid(int P_uid)
        {
            return _table.TryGetValue(P_uid, out var p) ? p : null;
        }

        public IEnumerable<PokemonSaveData> EnumerateAll()
        {
            return _table.Values;
        }

        /// <summary>���� ���̺� �� �ִ� P_uid�� ��ĵ�� ���� �߱� ��ȣ�� ����</summary>
        public void RefreshNextPuid()
        {
            int max = 0;
            foreach (var kv in _table)
            {
                if (kv.Value != null && kv.Value.P_uid > max)
                    max = kv.Value.P_uid;
            }
            _nextPuid = max + 1;
        }

        /// <summary>���ο� P_uid �߱� (�ܺ� Provider�� ������ ����, ������ ������ ���)</summary>
        private int IssuePuid()
        {
            if (_idProvider != null) return _idProvider.NextPuid();
            return _nextPuid++;
        }


        // ====== �ܺηκ��� �ε� ======
        /// <summary>
        /// TrainerRepository���� �ҷ��� ���� �����͸� �״�� ����.
        /// </summary>
        public void LoadFromData(Dictionary<int, PokemonSaveData> table, List<int> party, List<List<int>> boxes)
        {
            _table.Clear();
            _party = new int[_partyLimit];
            _boxes.Clear();

            if (table != null)
            {
                foreach (var kv in table)
                {
                    // Ű�� ���� P_uid ��ġ ����(����)
                    if (kv.Key != kv.Value.P_uid)
                        kv.Value.P_uid = kv.Key;
                    _table[kv.Key] = kv.Value;
                }
            }
            if (party != null)
            {
                for (int i = 0; i < Math.Min(party.Count, _partyLimit); i++)
                {
                    _party[i] = party[i];
                }
            }
            if (boxes != null)
            {
                foreach (var boxList in boxes)
                {
                    int[] boxArray = new int[_pokemonPerBox];
                    for (int i = 0; i < Math.Min(boxList.Count, _pokemonPerBox); i++)
                    {
                        boxArray[i] = boxList[i];
                    }
                    _boxes.Add(boxArray);
                }
            }

            RefreshNextPuid();
        }

        // ====== �߰�/���� ======
        /// <summary>
        /// ��ü �߰� (��Ƽ ������ ������ ��Ƽ��, �ƴϸ� 0�� �ڽ���).
        /// - data.P_uid �� 0 �����̸�:
        ///   - _idProvider �� ������ �߱� �޾� ����
        ///   - ������ ���� �߻�(������ P_uid�� �ο��ؼ� �Ѱܾ� ��)
        /// </summary>
        public int Add(PokemonSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            // P_uid Ȯ��
            if (data.P_uid <= 0)
            {
                data.P_uid = IssuePuid(); // �� ������ throw ����, ���� �������� �߱�
            }


            int puid = data.P_uid;

            // ���̺� ���
            _table[puid] = data;
            Debug.Log($"[OWNED] Added P_uid={data.P_uid}, species={data.speciesId}, form={data.formKey}");


            // �⺻ ��ġ: ��Ƽ �켱
            int firstEmptySlot = Array.IndexOf(_party, 0);
            if (firstEmptySlot >= 0)
            {
                _party[firstEmptySlot] = puid;
            }
            else
            {
                if (_boxes.Count == 0) _boxes.Add(new int[_pokemonPerBox]);
                int boxIndex = Array.FindIndex(_boxes.ToArray(), b => Array.IndexOf(b, 0) >= 0);
                if (boxIndex >= 0)
                {
                    int slotIndex = Array.IndexOf(_boxes[boxIndex], 0);
                    _boxes[boxIndex][slotIndex] = puid;
                }
            }

            OnPartyUpdated?.Invoke();
            return puid;
        }

        // ���ϸ� ���� (���� ����)
        public bool Release(int puid)
        {
            if (_party[0] == puid)
            {
                Debug.LogWarning("��Ƽ�� ù ��° ���ϸ��� ������ �� �����ϴ�."); // TODO: �˾� �޼����� ���� �ؾ��� 
                return false;
            }

            bool removed = false;
            for (int i = 0; i < _party.Length; i++)
            {
                if (_party[i] == puid)
                {
                    _party[i] = 0;
                    removed = true;
                    break;
                }
            }
            if (!removed)
            {
                for (int i = 0; i < _boxes.Count; i++)
                {
                    int slotIndex = Array.IndexOf(_boxes[i], puid);
                    if (slotIndex >= 0)
                    {
                        _boxes[i][slotIndex] = 0;
                        removed = true;
                        break;
                    }
                }
            }
            if (removed) _table.Remove(puid);
            OnPartyUpdated?.Invoke();
            Debug.Log($"���ϸ� [P_uid: {puid}]��(��) ����Ǿ����ϴ�."); // TODO: �˾� �޼����� ���� �ؾ��� 
            return removed;
        }

        // ====== ��Ƽ/�ڽ� �̵� ======
        public bool MoveToParty(int puid, int slotIndex)
        {
            // �� ������ ��Ƽ�� �־�߸� �̵�
            if (slotIndex < 0 || slotIndex >= _partyLimit) return false;
            if (_party[slotIndex] != 0) return false;

            // ���� ��ġ���� ����
            RemovePuidFromContainers(puid);

            // ��Ƽ�� �߰�
            _party[slotIndex] = puid;

            OnPartyUpdated?.Invoke();
            return true;
        }

        public bool MoveToBox(int puid, int boxIndex, int slotIndex)
        {
            if (IsFirstPartyPokemon(puid))
            {
                Debug.LogWarning("��Ƽ�� ù ��° ���ϸ��� PC�� �̵��� �� �����ϴ�.");  // TODO: �˾� �޼����� ���� �ؾ��� 
                return false;
            }

            // ���� ��ġ���� ����
            RemovePuidFromContainers(puid);

            // �� ��ġ�� �߰�
            while (_boxes.Count <= boxIndex)
            {
                _boxes.Add(new int[_pokemonPerBox]);
            }
            if (slotIndex >= _pokemonPerBox) return false;

            _boxes[boxIndex][slotIndex] = puid;

            OnPartyUpdated?.Invoke();
            return true;
        }

        /// <summary>
        /// �� P_uid�� ��ġ�� ���� ��ȯ(��Ƽ����Ƽ, �ڽ���ڽ�, ��Ƽ��ڽ� ��� ���)
        /// </summary>
        public bool Swap(int puidA, int puidB)
        {
            var indexA = FindPuidIndex(puidA);
            var indexB = FindPuidIndex(puidB);

            if (indexA.boxIndex == -1 && indexB.boxIndex == -1) // ��Ƽ-��Ƽ ����
            {
                if (indexA.slotIndex == 0 && indexB.slotIndex != 0)
                {
                    // ��Ƽ 0�� ������ �ٸ� ���ϸ���� ������ ���
                    (_party[indexA.slotIndex], _party[indexB.slotIndex]) = (_party[indexB.slotIndex], _party[indexA.slotIndex]);
                }
                else if (indexB.slotIndex == 0 && indexA.slotIndex != 0)
                {
                    (_party[indexA.slotIndex], _party[indexB.slotIndex]) = (_party[indexB.slotIndex], _party[indexA.slotIndex]);
                }
                else
                {
                    (_party[indexA.slotIndex], _party[indexB.slotIndex]) = (_party[indexB.slotIndex], _party[indexA.slotIndex]);
                }
            }
            else if (indexA.boxIndex != -1 && indexB.boxIndex != -1) // �ڽ�-�ڽ� ����
            {
                (_boxes[indexA.boxIndex][indexA.slotIndex], _boxes[indexB.boxIndex][indexB.slotIndex]) =
                (_boxes[indexB.boxIndex][indexB.slotIndex], _boxes[indexA.boxIndex][indexA.slotIndex]);
            }
            else if (indexA.boxIndex == -1) // ��Ƽ-�ڽ� ����
            {
                if (indexA.slotIndex == 0)
                {
                    (_party[indexA.slotIndex], _boxes[indexB.boxIndex][indexB.slotIndex]) =
                    (_boxes[indexB.boxIndex][indexB.slotIndex], _party[indexA.slotIndex]);
                }
                else
                {
                    (_party[indexA.slotIndex], _boxes[indexB.boxIndex][indexB.slotIndex]) =
                    (_boxes[indexB.boxIndex][indexB.slotIndex], _party[indexA.slotIndex]);
                }
            }
            else // �ڽ�-��Ƽ ����
            {
                if (indexB.slotIndex == 0)
                {
                    (_boxes[indexA.boxIndex][indexA.slotIndex], _party[indexB.slotIndex]) =
                    (_party[indexB.slotIndex], _boxes[indexA.boxIndex][indexA.slotIndex]);
                }
                else
                {
                    (_boxes[indexA.boxIndex][indexA.slotIndex], _party[indexB.slotIndex]) =
                    (_party[indexB.slotIndex], _boxes[indexA.boxIndex][indexA.slotIndex]);
                }
            }

            OnPartyUpdated?.Invoke();
            return true;
        }
        private void RemovePuidFromContainers(int puid)
        {
            for (int i = 0; i < _party.Length; i++)
            {
                if (_party[i] == puid)
                {
                    _party[i] = 0;
                    return;
                }
            }
            for (int i = 0; i < _boxes.Count; i++)
            {
                int slotIndex = Array.IndexOf(_boxes[i], puid);
                if (slotIndex >= 0)
                {
                    _boxes[i][slotIndex] = 0;
                    return;
                }
            }
        }

        private (int boxIndex, int slotIndex) FindPuidIndex(int puid)
        {
            for (int i = 0; i < _party.Length; i++)
            {
                if (_party[i] == puid) return (-1, i);
            }
            for (int i = 0; i < _boxes.Count; i++)
            {
                int slotIndex = Array.IndexOf(_boxes[i], puid);
                if (slotIndex >= 0) return (i, slotIndex);
            }
            return (-1, -1);
        }

        private bool IsFirstPartyPokemon(int puid) => _party[0] == puid;

        // ====== �ܺ� ����/�ε� ȣȯ ��ƿ ======
        public void SaveToRepository(ITrainerRepository repo, int T_uid)
        {
            if (repo == null) throw new ArgumentNullException(nameof(repo));
            repo.SaveOwnedPokemon(T_uid,
                new Dictionary<int, PokemonSaveData>(_table),
                new List<int>(_party),
                _boxes.Select(b => new List<int>(b)).ToList());
        }

        public void LoadFromRepository(ITrainerRepository repo, int T_uid)
        {
            if (repo == null) throw new ArgumentNullException(nameof(repo));
            repo.LoadOwnedPokemon(T_uid, out var table, out var party, out var boxes);
            LoadFromData(table, party, boxes);
        }
    }
}
