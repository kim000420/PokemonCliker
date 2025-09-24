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
        private int[] _party; // int[] 배열로 변경
        private readonly List<int[]> _boxes = new(); // int[] 배열의 리스트로 변경
        public event Action OnPartyUpdated; // 파티 데이터 변경 시 호출되는 이벤트

        private int _nextPuid = 1;              // P_uid 발급 시퀀스 (기본 1부터)
        private int _partyLimit = 6;            // 파티 최대수 6 고정
        private const int _pokemonPerBox = 30;  // 박스 공간 30 고정
        public void Init(int partyLimit = 6, IPokemonIdProvider idProvider = null)
        {
            _party = new int[_partyLimit];
            _idProvider = idProvider;
        }


        // ====== 조회 ======
        public int[] GetParty() => _party;
        public List<int[]> GetBoxes() => _boxes; // 반환 타입을 List<int[]>로 변경
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
            _party = new int[_partyLimit];
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


            int puid = data.P_uid;

            // 테이블에 등록
            _table[puid] = data;
            Debug.Log($"[OWNED] Added P_uid={data.P_uid}, species={data.speciesId}, form={data.formKey}");


            // 기본 배치: 파티 우선
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

        // 포켓몬 방출 (영구 삭제)
        public bool Release(int puid)
        {
            if (_party[0] == puid)
            {
                Debug.LogWarning("파티의 첫 번째 포켓몬은 방출할 수 없습니다."); // TODO: 팝업 메세지로 변경 해야함 
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
            Debug.Log($"포켓몬 [P_uid: {puid}]이(가) 방출되었습니다."); // TODO: 팝업 메세지로 변경 해야함 
            return removed;
        }

        // ====== 파티/박스 이동 ======
        public bool MoveToParty(int puid, int slotIndex)
        {
            // 빈 슬롯이 파티에 있어야만 이동
            if (slotIndex < 0 || slotIndex >= _partyLimit) return false;
            if (_party[slotIndex] != 0) return false;

            // 기존 위치에서 제거
            RemovePuidFromContainers(puid);

            // 파티에 추가
            _party[slotIndex] = puid;

            OnPartyUpdated?.Invoke();
            return true;
        }

        public bool MoveToBox(int puid, int boxIndex, int slotIndex)
        {
            if (IsFirstPartyPokemon(puid))
            {
                Debug.LogWarning("파티의 첫 번째 포켓몬은 PC로 이동할 수 없습니다.");  // TODO: 팝업 메세지로 변경 해야함 
                return false;
            }

            // 기존 위치에서 제거
            RemovePuidFromContainers(puid);

            // 새 위치에 추가
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
        /// 두 P_uid의 위치를 서로 교환(파티↔파티, 박스↔박스, 파티↔박스 모두 허용)
        /// </summary>
        public bool Swap(int puidA, int puidB)
        {
            var indexA = FindPuidIndex(puidA);
            var indexB = FindPuidIndex(puidB);

            if (indexA.boxIndex == -1 && indexB.boxIndex == -1) // 파티-파티 스왑
            {
                if (indexA.slotIndex == 0 && indexB.slotIndex != 0)
                {
                    // 파티 0번 슬롯은 다른 포켓몬과의 스왑은 허용
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
            else if (indexA.boxIndex != -1 && indexB.boxIndex != -1) // 박스-박스 스왑
            {
                (_boxes[indexA.boxIndex][indexA.slotIndex], _boxes[indexB.boxIndex][indexB.slotIndex]) =
                (_boxes[indexB.boxIndex][indexB.slotIndex], _boxes[indexA.boxIndex][indexA.slotIndex]);
            }
            else if (indexA.boxIndex == -1) // 파티-박스 스왑
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
            else // 박스-파티 스왑
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
}
