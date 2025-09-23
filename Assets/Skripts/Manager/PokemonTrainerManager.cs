// 파일: Scripts/Trainer/PokemonTrainerManager.cs
using System;
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
        private ITrainerRepository _repo;
        private OwnedPokemonManager _owned;

        public TrainerProfile Profile { get; private set; }
        public OwnedPokemonManager Owned => _owned;

        public int T_uid => Profile.T_uid;

        // 트레이너 이름 반환
        public string TrainerName => Profile?.TrainerName ?? "Unknown";

        public void Init(ITrainerRepository repo, OwnedPokemonManager owned)
        {
            _repo = repo; 
            _owned = owned;
        }


        public void LoadForTrainer(int T_uid)
        {
            Profile = _repo.LoadTrainerProfile(T_uid);
            if (Profile == null) throw new InvalidOperationException("트레이너 프로필 없음");

            _repo.LoadOwnedPokemon(T_uid, out var table, out var party, out var boxes);
            _owned.LoadFromData(table, party, boxes);
        }

        public void Save()
        {
            _repo.SaveTrainerProfile(Profile);
            _repo.SaveOwnedPokemon(T_uid, _owned.Table, _owned.Party, _owned.Boxes);
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
        // 추가로 옵션/재화/진척도 등 확장 가능
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
