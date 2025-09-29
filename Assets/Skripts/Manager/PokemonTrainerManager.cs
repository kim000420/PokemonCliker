// 파일: Scripts/Trainer/PokemonTrainerManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 트레이너 단위의 데이터 관리
    /// - T_uid 단위로 로드/저장
    /// - TrainerProfile + OwnedPokemonManager 조합
    /// </summary>
    public class PokemonTrainerManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float autoSaveIntervalMinutes = 5.0f; // 자동 저장 간격 (분 단위)

        private ITrainerRepository _repo;
        private OwnedPokemonManager _owned;
        private Coroutine _autoSaveCoroutine; // 자동 저장 코루틴을 제어하기 위한 변수

        public TrainerProfile Profile { get; private set; }
        public OwnedPokemonManager Owned => _owned;
        public ClickProgressTracker Progress { get; private set; }

        public int T_uid => Profile.T_uid;

        // 트레이너 이름 반환
        public string TrainerName => Profile?.TrainerName ?? "Unknown";

        public void Init(ITrainerRepository repo, OwnedPokemonManager owned)
        {
            _repo = repo; 
            _owned = owned;
        }

        // 이전 세션 초기화
        public void ClearData()
        {
            Profile = null;
            Progress = new ClickProgressTracker();
            _owned?.ClearData();
            Debug.Log("[TrainerManager] 데이터 초기화 완료.");
        }

        public void LoadForTrainer(int T_uid)
        {
            Profile = _repo.LoadTrainerProfile(T_uid);
            if (Profile == null) throw new InvalidOperationException("트레이너 프로필 없음");

            Progress = Profile.ProgressTracker ?? new ClickProgressTracker();

            _repo.LoadOwnedPokemon(T_uid, out var table, out var party, out var boxes);
            _owned.LoadFromData(table, party, boxes);

            // 로드 완료후 자동저장 코루틴 시작
            if (_autoSaveCoroutine != null) StopCoroutine(_autoSaveCoroutine);
            _autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
        }

        public void Save()
        {
            if (Profile == null) return; // 저장할 프로필이 없으면 실행하지 않음

            Profile.ProgressTracker = this.Progress;

            _repo.SaveTrainerProfile(Profile);
            _repo.SaveOwnedPokemon(T_uid, _owned.Table, _owned.GetParty(), _owned.GetBoxes());
        }

        // 유니티가 게임 종료 직전에 자동 호출하는 메서드
        private void OnApplicationQuit()
        {
            Debug.Log("게임 종료... 마지막 저장을 시도합니다.");
            Save();
        }

        // 일정 시간마다 Save()를 호출하는 코루틴
        private IEnumerator AutoSaveRoutine()
        {
            while (true)
            {
                // 지정된 시간(분)만큼 기다립니다.
                yield return new WaitForSeconds(autoSaveIntervalMinutes * 60);

                Debug.Log($"자동 저장 실행... (간격: {autoSaveIntervalMinutes}분)");
                Save();
            }
        }

        public void SetTrainerName(string newName)
        {
            Profile.TrainerName = newName;
        }
    }

    [Serializable]
    public class TrainerProfile
    {
        public int T_uid;
        public string TrainerName;
        public DateTime CreatedAt;
        public ClickProgressTracker ProgressTracker;
    }

    public interface ITrainerRepository
    {
        void SaveTrainerProfile(TrainerProfile profile);
        TrainerProfile LoadTrainerProfile(int T_uid);

        void SaveOwnedPokemon(
            int T_uid,
            IReadOnlyDictionary<int, PokemonSaveData> table,
            IReadOnlyList<int> party,
            IReadOnlyList<IReadOnlyList<int>> boxes
        );

        void LoadOwnedPokemon(int T_uid,
            out Dictionary<int, PokemonSaveData> table,
            out List<int> party,
            out List<List<int>> boxes);
    }
}
