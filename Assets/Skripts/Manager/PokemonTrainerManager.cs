// ����: Scripts/Trainer/PokemonTrainerManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// Ʈ���̳� ������ ������ ����
    /// - T_uid ������ �ε�/����
    /// - TrainerProfile + OwnedPokemonManager ����
    /// </summary>
    public class PokemonTrainerManager : MonoBehaviour
    {
        private ITrainerRepository _repo;
        private OwnedPokemonManager _owned;

        public TrainerProfile Profile { get; private set; }
        public OwnedPokemonManager Owned => _owned;

        public int T_uid => Profile.T_uid;

        // Ʈ���̳� �̸� ��ȯ
        public string TrainerName => Profile?.TrainerName ?? "Unknown";

        public void Init(ITrainerRepository repo, OwnedPokemonManager owned)
        {
            _repo = repo; 
            _owned = owned;
        }


        public void LoadForTrainer(int T_uid)
        {
            Profile = _repo.LoadTrainerProfile(T_uid);
            if (Profile == null) throw new InvalidOperationException("Ʈ���̳� ������ ����");

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
        // �߰��� �ɼ�/��ȭ/��ô�� �� Ȯ�� ����
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
