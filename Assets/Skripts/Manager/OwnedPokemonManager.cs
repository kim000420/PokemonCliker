using System;
using System.Collections.Generic;
using System.Linq;
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
            }
            else
            {
                if (_boxes.Count == 0) _boxes.Add(new List<int>());
                _boxes[0].Add(P_uid);
            }
            return P_uid;
        }

        /// <summary>
        /// ����(����). ��Ƽ/�ڽ����� ���� �� ���̺��� ����.
        /// </summary>
        public bool Release(int P_uid)
        {
            bool removed = _party.Remove(P_uid);
            if (!removed)
            {
                for (int i = 0; i < _boxes.Count; i++)
                {
                    if (_boxes[i].Remove(P_uid)) { removed = true; break; }
                }
            }
            if (removed) _table.Remove(P_uid);
            return removed;
        }

        // ====== ��Ƽ/�ڽ� �̵� ======
        public bool MoveToParty(int P_uid, int slot)
        {
            if (!_table.ContainsKey(P_uid)) return false;
            if (slot < 0 || slot >= _partyLimit) return false;

            // �̹� ��Ƽ�� ������ ��ġ�� ����
            int idx = _party.IndexOf(P_uid);
            if (idx >= 0)
            {
                // ����
                if (slot < _party.Count)
                {
                    (_party[idx], _party[slot]) = (_party[slot], _party[idx]);
                }
                else
                {
                    // �� �������� �̵�
                    _party.RemoveAt(idx);
                    _party.Add(P_uid);
                }
                return true;
            }

            // �ڽ����� ����
            for (int i = 0; i < _boxes.Count; i++)
            {
                if (_boxes[i].Remove(P_uid)) break;
            }

            // ��Ƽ ���� Ȯ��
            if (_party.Count < _partyLimit)
            {
                if (slot <= _party.Count) _party.Insert(slot, P_uid);
                else _party.Add(P_uid);
                return true;
            }
            else
            {
                // ���� á�ٸ� ����(��å�� �������� ó���ϰ� ������ ���� API ���)
                return false;
            }
        }

        public bool MoveToBox(int P_uid, int boxIdx, int? slot = null)
        {
            if (!_table.ContainsKey(P_uid)) return false;
            if (boxIdx < 0) return false;

            while (_boxes.Count <= boxIdx) _boxes.Add(new List<int>());

            // ��Ƽ/�ٸ� �ڽ����� ����
            _party.Remove(P_uid);
            for (int i = 0; i < _boxes.Count; i++)
            {
                if (i == boxIdx) continue;
                _boxes[i].Remove(P_uid);
            }

            var box = _boxes[boxIdx];
            if (slot.HasValue && slot.Value >= 0 && slot.Value <= box.Count)
                box.Insert(slot.Value, P_uid);
            else
                box.Add(P_uid);

            return true;
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
}
