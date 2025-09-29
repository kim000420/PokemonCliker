using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ����/�������� JSON ���Ϸ� �����ϴ� ���� (IAccountRepository)
    /// ���� ��ġ: Application.persistentDataPath
    /// </summary>
    public class JsonAccountRepository : IAccountRepository
    {
        private readonly string _folderPath = Path.Combine(Application.persistentDataPath, "Account");
        private string _accountsPath;

        [Serializable] class AccountMap { public List<AccountRecord> list = new(); }

        AccountMap _accounts;

        public JsonAccountRepository()
        {
            _accountsPath = Path.Combine(_folderPath, "accounts.json");
            Directory.CreateDirectory(_folderPath);
            _accounts = ReadJson<AccountMap>(_accountsPath) ?? new AccountMap();
            
            if (_accounts.list == null)
            {
                _accounts.list = new List<AccountRecord>();
            }
        }

        // IAccountRepository -------------------------------
        public bool ExistsId(string id) => _accounts.list.Exists(a => a.Id == id);

        public int GetMaxTrainerUid()
        {
            int max = 0;
            foreach (var a in _accounts.list) if (a.T_uid > max) max = a.T_uid;
            return max;
        }

        public void SaveAccount(AccountRecord record)
        {
            int idx = _accounts.list.FindIndex(a => a.Id == record.Id);
            if (idx >= 0) _accounts.list[idx] = record;
            else _accounts.list.Add(record);
            WriteJson(_accountsPath, _accounts);
        }

        public AccountRecord LoadAccount(string id)
        {
            return _accounts.list.Find(a => a.Id == id);
        }

        /// <summary>
        /// ���� �޸𸮿� �ִ� ���� ��� ��ü�� ���Ͽ� ������ �����մϴ�.
        /// </summary>
        public void ForceSave()
        {
            if (_accounts != null)
            {
                WriteJson(_accountsPath, _accounts);
            }
        }

        // JSON helpers -------------------------------------
        private T ReadJson<T>(string path)
        {
            string backupPath = path + ".bak";

            if (File.Exists(path))
            {
                try { return JsonUtility.FromJson<T>(File.ReadAllText(path)); }
                catch (Exception e) { Debug.LogWarning($"�� ���̺� ����({path}) �б� ����. ��� ���Ϸ� ���� �õ�. ����: {e.Message}"); }
            }

            if (File.Exists(backupPath))
            {
                try
                {
                    Debug.Log($"��� ����({backupPath})���� ������ ����.");
                    return JsonUtility.FromJson<T>(File.ReadAllText(backupPath));
                }
                catch (Exception e) { Debug.LogError($"��� ����({backupPath}) �б� ����. ����: {e.Message}"); }
            }
            return default;
        }
        private void WriteJson<T>(string path, T data)
        {
            string tempPath = path + ".tmp";
            string backupPath = path + ".bak";

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(tempPath, json);

                if (File.Exists(path)) File.Replace(tempPath, path, backupPath);
                else File.Move(tempPath, path);

                if (File.Exists(backupPath)) File.Delete(backupPath);

                Debug.Log($"[REPO] Saved JSON to {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON ���� ���� �� ���� �߻�: {path}. ����: {e.Message}");
            }
        }
    }
}
