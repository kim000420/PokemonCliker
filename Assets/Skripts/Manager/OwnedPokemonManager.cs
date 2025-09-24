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
        private readonly List<int> _party = new();                         // P_uid (<=6)
        private readonly List<List<int>> _boxes = new();                   // �ڽ���: �� �ڽ��� P_uid ����Ʈ
        public event Action OnPartyUpdated; // ��Ƽ ������ ���� �� ȣ��Ǵ� �̺�Ʈ

        private int _nextPuid = 1;              // P_uid �߱� ������ (�⺻ 1����)
        private int _partyLimit = 6;            // ��Ƽ �ִ�� 6 ����

        public void Init(int partyLimit = 6, IPokemonIdProvider idProvider = null)
        {
            _partyLimit = Math.Max(1, partyLimit);
            _idProvider = idProvider;
        }


        // ====== ��ȸ ======
        public IReadOnlyList<int> Party => _party;
        public IReadOnlyList<IReadOnlyList<int>> Boxes => _boxes.Select(b => (IReadOnlyList<int>)b).ToList();
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
            _party.Clear();
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
            if (party != null) _party.AddRange(party);
            if (boxes != null) _boxes.AddRange(boxes);

            RefreshNextPuid(); // �ε�� ������ ������� ���� �߱� ��ȣ ����
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


            int P_uid = data.P_uid;

            // ���̺� ���
            _table[P_uid] = data;
            Debug.Log($"[OWNED] Added P_uid={data.P_uid}, species={data.speciesId}, form={data.formKey}");


            // �⺻ ��ġ: ��Ƽ �켱
            if (_party.Count < _partyLimit)
            {
                _party.Add(P_uid);
                OnPartyUpdated?.Invoke();
            }
            else
            {
                if (_boxes.Count == 0) _boxes.Add(new List<int>());
                _boxes[0].Add(P_uid);
            }
            return P_uid;
        }

        // ���ϸ� ���� (���� ����)
        public void Release(int puid)
        {
            if (IsFirstPartyPokemon(puid))
            {
                Debug.LogWarning("��Ƽ�� ù ��° ���ϸ��� ������ �� �����ϴ�.");
                return;
            }

            RemovePuid(puid); // ����Ʈ���� ����
            _table.Remove(puid); // ���̺��� ������ ����

            OnPartyUpdated?.Invoke();
            Debug.Log($"���ϸ� [P_uid: {puid}]��(��) ����Ǿ����ϴ�.");
        }

        // ====== ��Ƽ/�ڽ� �̵� ======
        public void MoveToParty(int puid, int slotIndex)
        {
            // �� ������ ��Ƽ�� �־�߸� �̵�
            if (slotIndex < 0 || slotIndex >= _partyLimit)
            {
                return;
            }

            // ���� ��ġ���� ����
            RemovePuid(puid);

            // ��Ƽ�� �߰�
            _party[slotIndex] = puid;

            OnPartyUpdated?.Invoke();
        }

        public void MoveToBox(int puid, int boxIndex, int slotIndex)
        {
            if (IsFirstPartyPokemon(puid))
            {
                Debug.LogWarning("��Ƽ�� ù ��° ���ϸ��� PC�� �̵��� �� �����ϴ�.");
                return;
            }

            // ���� ��ġ���� ����
            RemovePuid(puid);

            // �� ��ġ�� �߰�
            if (boxIndex >= _boxes.Count)
            {
                // �� �ڽ� ����
                for (int i = _boxes.Count; i <= boxIndex; i++)
                {
                    _boxes.Add(new List<int>());
                }
            }
            if (slotIndex >= _boxes[boxIndex].Count)
            {
                _boxes[boxIndex].Resize(slotIndex + 1);
            }
            _boxes[boxIndex][slotIndex] = puid;

            OnPartyUpdated?.Invoke();
        }

        private void RemovePuid(int puid)
        {
            int partyIndex = _party.IndexOf(puid);
            if (partyIndex >= 0)
            {
                _party[partyIndex] = 0; // 0���� �����Ͽ� �� ���� ǥ��
            }

            var (boxIndex, slotIndex) = FindInBoxes(puid);
            if (boxIndex >= 0)
            {
                _boxes[boxIndex][slotIndex] = 0; // 0���� �����Ͽ� �� ���� ǥ��
            }
        }

        private bool IsFirstPartyPokemon(int puid)
        {
            return _party.Count > 0 && _party[0] == puid;
        }

        /// <summary>
        /// �� P_uid�� ��ġ�� ���� ��ȯ(��Ƽ����Ƽ, �ڽ���ڽ�, ��Ƽ��ڽ� ��� ���)
        /// </summary>
        public bool Swap(int P_uidA, int P_uidB)
        {
            if (!_table.ContainsKey(P_uidA) || !_table.ContainsKey(P_uidB)) return false;

            // ��Ƽ �ε��� ã��
            int ia = _party.IndexOf(P_uidA);
            int ib = _party.IndexOf(P_uidB);

            if (ia >= 0 && ib >= 0)
            {
                (_party[ia], _party[ib]) = (_party[ib], _party[ia]);
                return true;
            }

            // �ڽ� �� �ε��� ã��
            (int biA, int bjA) = FindInBoxes(P_uidA);
            (int biB, int bjB) = FindInBoxes(P_uidB);

            if (ia >= 0 && biB >= 0)
            {
                // ��Ƽ(P_uidA) <-> �ڽ�(P_uidB)
                _party[ia] = P_uidB;
                _boxes[biB][bjB] = P_uidA;
                return true;
            }
            if (ib >= 0 && biA >= 0)
            {
                _party[ib] = P_uidA;
                _boxes[biA][bjA] = P_uidB;
                return true;
            }

            if (biA >= 0 && biB >= 0)
            {
                (_boxes[biA][bjA], _boxes[biB][bjB]) = (_boxes[biB][bjB], _boxes[biA][bjA]);
                OnPartyUpdated?.Invoke();
                return true;
            }

            return false;
        }

        private (int bi, int bj) FindInBoxes(int P_uid)
        {
            for (int i = 0; i < _boxes.Count; i++)
            {
                var box = _boxes[i];
                int j = box.IndexOf(P_uid);
                if (j >= 0) return (i, j);
            }
            return (-1, -1);
        }

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

    public static class ListExtensions
    {
        public static void Resize<T>(this List<T> list, int size) where T : new()
        {
            int currentSize = list.Count;
            if (size < currentSize)
            {
                list.RemoveRange(size, currentSize - size);
            }
            else if (size > currentSize)
            {
                while (list.Count < size)
                {
                    list.Add(new T());
                }
            }
        }

        public static void Resize<T>(this List<T> list, int size, T value)
        {
            int currentSize = list.Count;
            if (size < currentSize)
            {
                list.RemoveRange(size, currentSize - size);
            }
            else if (size > currentSize)
            {
                while (list.Count < size)
                {
                    list.Add(value);
                }
            }
        }
    }
}
