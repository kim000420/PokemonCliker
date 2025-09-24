using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 보유/보관/위치 관리(파티·박스·개체 테이블).
    /// - P_uid(개체 UID) 발급은 외부(IPokemonIdProvider)에서 수행
    /// - 파티(<=6), 박스(가변) 이동/교체/방출
    /// - 외부 영속화(ITrainerRepository 하위 메서드)를 통해 저장/로드
    /// </summary>
    public class OwnedPokemonManager : MonoBehaviour
    {
        // ===== 외부 발급기 =====
        public interface IPokemonIdProvider
        {
            int NextPuid(); // 새 P_uid 발급
        }

        private IPokemonIdProvider _idProvider;  // null 가능(그 경우 Add 시 P_uid 필수)

        // ===== 데이터 보관 =====
        private readonly Dictionary<int, PokemonSaveData> _table = new(); // P_uid -> data
        private readonly List<int> _party = new();                         // P_uid (<=6)
        private readonly List<List<int>> _boxes = new();                   // 박스들: 각 박스는 P_uid 리스트
        public event Action OnPartyUpdated; // 파티 데이터 변경 시 호출되는 이벤트

        private int _nextPuid = 1;              // P_uid 발급 시퀀스 (기본 1부터)
        private int _partyLimit = 6;            // 파티 최대수 6 고정

        public void Init(int partyLimit = 6, IPokemonIdProvider idProvider = null)
        {
            _partyLimit = Math.Max(1, partyLimit);
            _idProvider = idProvider;
        }


        // ====== 조회 ======
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

        /// <summary>현재 테이블 내 최대 P_uid를 스캔해 다음 발급 번호를 갱신</summary>
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

        /// <summary>새로운 P_uid 발급 (외부 Provider가 있으면 위임, 없으면 시퀀스 사용)</summary>
        private int IssuePuid()
        {
            if (_idProvider != null) return _idProvider.NextPuid();
            return _nextPuid++;
        }


        // ====== 외부로부터 로드 ======
        /// <summary>
        /// TrainerRepository에서 불러온 원시 데이터를 그대로 주입.
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
                    // 키와 내부 P_uid 일치 보정(안전)
                    if (kv.Key != kv.Value.P_uid)
                        kv.Value.P_uid = kv.Key;
                    _table[kv.Key] = kv.Value;
                }
            }
            if (party != null) _party.AddRange(party);
            if (boxes != null) _boxes.AddRange(boxes);

            RefreshNextPuid(); // 로드된 데이터 기반으로 다음 발급 번호 보정
        }

        // ====== 추가/삭제 ======
        /// <summary>
        /// 개체 추가 (파티 여유가 있으면 파티로, 아니면 0번 박스로).
        /// - data.P_uid 가 0 이하이면:
        ///   - _idProvider 가 있으면 발급 받아 설정
        ///   - 없으면 예외 발생(사전에 P_uid를 부여해서 넘겨야 함)
        /// </summary>
        public int Add(PokemonSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            // P_uid 확보
            if (data.P_uid <= 0)
            {
                data.P_uid = IssuePuid(); // ← 이전의 throw 제거, 내부 시퀀스로 발급
            }


            int P_uid = data.P_uid;

            // 테이블에 등록
            _table[P_uid] = data;
            Debug.Log($"[OWNED] Added P_uid={data.P_uid}, species={data.speciesId}, form={data.formKey}");


            // 기본 배치: 파티 우선
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

        // 포켓몬 방출 (영구 삭제)
        public void Release(int puid)
        {
            if (IsFirstPartyPokemon(puid))
            {
                Debug.LogWarning("파티의 첫 번째 포켓몬은 방출할 수 없습니다.");
                return;
            }

            RemovePuid(puid); // 리스트에서 제거
            _table.Remove(puid); // 테이블에서 데이터 제거

            OnPartyUpdated?.Invoke();
            Debug.Log($"포켓몬 [P_uid: {puid}]이(가) 방출되었습니다.");
        }

        // ====== 파티/박스 이동 ======
        public void MoveToParty(int puid, int slotIndex)
        {
            // 빈 슬롯이 파티에 있어야만 이동
            if (slotIndex < 0 || slotIndex >= _partyLimit)
            {
                return;
            }

            // 기존 위치에서 제거
            RemovePuid(puid);

            // 파티에 추가
            _party[slotIndex] = puid;

            OnPartyUpdated?.Invoke();
        }

        public void MoveToBox(int puid, int boxIndex, int slotIndex)
        {
            if (IsFirstPartyPokemon(puid))
            {
                Debug.LogWarning("파티의 첫 번째 포켓몬은 PC로 이동할 수 없습니다.");
                return;
            }

            // 기존 위치에서 제거
            RemovePuid(puid);

            // 새 위치에 추가
            if (boxIndex >= _boxes.Count)
            {
                // 새 박스 생성
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
                _party[partyIndex] = 0; // 0으로 설정하여 빈 슬롯 표시
            }

            var (boxIndex, slotIndex) = FindInBoxes(puid);
            if (boxIndex >= 0)
            {
                _boxes[boxIndex][slotIndex] = 0; // 0으로 설정하여 빈 슬롯 표시
            }
        }

        private bool IsFirstPartyPokemon(int puid)
        {
            return _party.Count > 0 && _party[0] == puid;
        }

        /// <summary>
        /// 두 P_uid의 위치를 서로 교환(파티↔파티, 박스↔박스, 파티↔박스 모두 허용)
        /// </summary>
        public bool Swap(int P_uidA, int P_uidB)
        {
            if (!_table.ContainsKey(P_uidA) || !_table.ContainsKey(P_uidB)) return false;

            // 파티 인덱스 찾기
            int ia = _party.IndexOf(P_uidA);
            int ib = _party.IndexOf(P_uidB);

            if (ia >= 0 && ib >= 0)
            {
                (_party[ia], _party[ib]) = (_party[ib], _party[ia]);
                return true;
            }

            // 박스 내 인덱스 찾기
            (int biA, int bjA) = FindInBoxes(P_uidA);
            (int biB, int bjB) = FindInBoxes(P_uidB);

            if (ia >= 0 && biB >= 0)
            {
                // 파티(P_uidA) <-> 박스(P_uidB)
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

        // ====== 외부 저장/로드 호환 유틸 ======
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
