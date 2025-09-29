// ����: Scripts/Trainer/PokemonTrainerManager.cs
using System;
using System.Collections;
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
        [Header("Settings")]
        [SerializeField] private float autoSaveIntervalMinutes = 5.0f; // �ڵ� ���� ���� (�� ����)

        private ITrainerRepository _repo;
        private OwnedPokemonManager _owned;
        private Coroutine _autoSaveCoroutine; // �ڵ� ���� �ڷ�ƾ�� �����ϱ� ���� ����

        public TrainerProfile Profile { get; private set; }
        public OwnedPokemonManager Owned => _owned;
        public ClickProgressTracker Progress { get; private set; }

        public int T_uid => Profile.T_uid;

        // Ʈ���̳� �̸� ��ȯ
        public string TrainerName => Profile?.TrainerName ?? "Unknown";

        public void Init(ITrainerRepository repo, OwnedPokemonManager owned)
        {
            _repo = repo; 
            _owned = owned;
        }

        // ���� ���� �ʱ�ȭ
        public void ClearData()
        {
            Profile = null;
            Progress = new ClickProgressTracker();
            _owned?.ClearData();
            Debug.Log("[TrainerManager] ������ �ʱ�ȭ �Ϸ�.");
        }

        public void LoadForTrainer(int T_uid)
        {
            Profile = _repo.LoadTrainerProfile(T_uid);
            if (Profile == null) throw new InvalidOperationException("Ʈ���̳� ������ ����");

            Progress = Profile.ProgressTracker ?? new ClickProgressTracker();

            _repo.LoadOwnedPokemon(T_uid, out var table, out var party, out var boxes);
            _owned.LoadFromData(table, party, boxes);

            // �ε� �Ϸ��� �ڵ����� �ڷ�ƾ ����
            if (_autoSaveCoroutine != null) StopCoroutine(_autoSaveCoroutine);
            _autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
        }

        public void Save()
        {
            if (Profile == null) return; // ������ �������� ������ �������� ����

            Profile.ProgressTracker = this.Progress;

            _repo.SaveTrainerProfile(Profile);
            _repo.SaveOwnedPokemon(T_uid, _owned.Table, _owned.GetParty(), _owned.GetBoxes());
        }

        // ����Ƽ�� ���� ���� ������ �ڵ� ȣ���ϴ� �޼���
        private void OnApplicationQuit()
        {
            Debug.Log("���� ����... ������ ������ �õ��մϴ�.");
            Save();
        }

        // ���� �ð����� Save()�� ȣ���ϴ� �ڷ�ƾ
        private IEnumerator AutoSaveRoutine()
        {
            while (true)
            {
                // ������ �ð�(��)��ŭ ��ٸ��ϴ�.
                yield return new WaitForSeconds(autoSaveIntervalMinutes * 60);

                Debug.Log($"�ڵ� ���� ����... (����: {autoSaveIntervalMinutes}��)");
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
