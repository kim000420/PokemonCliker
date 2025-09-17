// ����: Scripts/Account/AccountService.cs
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PokeClicker
{
    /// <summary>
    /// ���� ���/�α��� ����
    /// - ID/Password ����
    /// - T_uid �߱�
    /// - ��й�ȣ�� �ؽ�+��Ʈ ����
    /// </summary>
    public class AccountService
    {
        private readonly IAccountRepository _repo;
        private int _nextTrainerUid = 1;

        public AccountService(IAccountRepository repo)
        {
            _repo = repo;
            _nextTrainerUid = _repo.GetMaxTrainerUid() + 1;
        }

        public int Register(string id, string password, string displayName)
        {
            if (_repo.ExistsId(id))
                throw new InvalidOperationException("�̹� �����ϴ� ID�Դϴ�.");

            var tUid = _nextTrainerUid++;
            string salt = Guid.NewGuid().ToString("N");
            string hash = HashPassword(password, salt);

            var record = new AccountRecord
            {
                Id = id,
                PasswordHash = hash,
                Salt = salt,
                T_uid = tUid,
                CreatedAt = DateTime.Now
            };
            _repo.SaveAccount(record);

            return tUid;
        }

        public int Login(string id, string password)
        {
            var record = _repo.LoadAccount(id);
            if (record == null) throw new InvalidOperationException("ID�� �������� �ʽ��ϴ�.");

            string hash = HashPassword(password, record.Salt);
            if (hash != record.PasswordHash)
                throw new InvalidOperationException("��й�ȣ�� �ùٸ��� �ʽ��ϴ�.");

            return record.T_uid;
        }

        private string HashPassword(string password, string salt)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    public class AccountRecord
    {
        public string Id;
        public string PasswordHash;
        public string Salt;
        public int T_uid;
        public DateTime CreatedAt;
    }

    public interface IAccountRepository
    {
        bool ExistsId(string id);
        int GetMaxTrainerUid();
        void SaveAccount(AccountRecord record);
        AccountRecord LoadAccount(string id);

        void SaveTrainerProfile(TrainerProfile profile);
        TrainerProfile LoadTrainerProfile(int T_uid);
    }
}
