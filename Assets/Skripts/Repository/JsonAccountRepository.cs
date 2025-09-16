using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 계정/프로필을 JSON 파일로 저장하는 구현 (IAccountRepository)
    /// 저장 위치: Application.persistentDataPath
    /// </summary>
    public class JsonAccountRepository : MonoBehaviour, IAccountRepository
    {
        [SerializeField] string folderName = "Account";
        string Folder => Path.Combine(Application.persistentDataPath, folderName);
        string AccountsPath => Path.Combine(Folder, "accounts.json"); // id -> AccountRecord
        string ProfilesPath => Path.Combine(Folder, "profiles.json"); // T_uid -> TrainerProfile

        [Serializable] class AccountMap { public List<AccountRecord> list = new(); }
        [Serializable] class ProfileMap { public List<TrainerProfile> list = new(); }

        AccountMap _accounts;
        ProfileMap _profiles;

        void Awake()
        {
            Directory.CreateDirectory(Folder);
            _accounts = LoadJson<AccountMap>(AccountsPath) ?? new AccountMap();
            _profiles = LoadJson<ProfileMap>(ProfilesPath) ?? new ProfileMap();
        }

        // IAccountRepository -------------------------------
        public bool ExistsId(string id) => _accounts.list.Exists(a => a.Id == id);

        public int GetMaxTrainerUid()
        {
            int max = 0;
            foreach (var p in _profiles.list) if (p.T_uid > max) max = p.T_uid;
            return max;
        }

        public void SaveAccount(AccountRecord record)
        {
            int idx = _accounts.list.FindIndex(a => a.Id == record.Id);
            if (idx >= 0) _accounts.list[idx] = record;
            else _accounts.list.Add(record);
            SaveJson(AccountsPath, _accounts);
        }

        public AccountRecord LoadAccount(string id)
        {
            return _accounts.list.Find(a => a.Id == id);
        }

        public void SaveTrainerProfile(TrainerProfile profile)
        {
            int idx = _profiles.list.FindIndex(p => p.T_uid == profile.T_uid);
            if (idx >= 0) _profiles.list[idx] = profile;
            else _profiles.list.Add(profile);
            SaveJson(ProfilesPath, _profiles);
        }

        public TrainerProfile LoadTrainerProfile(int T_uid)
        {
            return _profiles.list.Find(p => p.T_uid == T_uid);
        }

        // JSON helpers -------------------------------------
        T LoadJson<T>(string path)
        {
            if (!File.Exists(path)) return default;
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        void SaveJson<T>(string path, T obj)
        {
            var json = JsonUtility.ToJson(obj, true);
            File.WriteAllText(path, json);
        }
    }
}
