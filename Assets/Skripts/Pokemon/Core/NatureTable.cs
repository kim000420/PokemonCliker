using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    // ������ ���ȿ� �ִ� ����(HP�� ���� ����)
    public struct NatureMultiplier
    {
        public float atk; // ����
        public float def; // ���
        public float spa; // Ư��
        public float spd; // Ư��
        public float spe; // ���ǵ�

        public NatureMultiplier(float atk, float def, float spa, float spd, float spe)
        {
            this.atk = atk; this.def = def; this.spa = spa; this.spd = spd; this.spe = spe;
        }
    }

    // ���� ���̺�: ���ݺ� ����
    // �߸� ������ ��� ������ 1.0
    public static class NatureTable
    {
        // �б� ���� ���̺�
        private static readonly Dictionary<NatureId, NatureMultiplier> _map = new Dictionary<NatureId, NatureMultiplier>
        {
            // ���ݡ� ����
            { NatureId.Lonely,  new NatureMultiplier(1.1f, 0.9f, 1.0f, 1.0f, 1.0f) },
            // ���ݡ� Ư����
            { NatureId.Adamant, new NatureMultiplier(1.1f, 1.0f, 0.9f, 1.0f, 1.0f) },
            // ���ݡ� Ư���
            { NatureId.Naughty, new NatureMultiplier(1.1f, 1.0f, 1.0f, 0.9f, 1.0f) },
            // ���ݡ� ���ǵ��
            { NatureId.Brave,   new NatureMultiplier(1.1f, 1.0f, 1.0f, 1.0f, 0.9f) },

            // ���� ���ݡ�
            { NatureId.Bold,    new NatureMultiplier(0.9f, 1.1f, 1.0f, 1.0f, 1.0f) },
            // ���� Ư����
            { NatureId.Impish,  new NatureMultiplier(1.0f, 1.1f, 0.9f, 1.0f, 1.0f) },
            // ���� Ư���
            { NatureId.Lax,     new NatureMultiplier(1.0f, 1.1f, 1.0f, 0.9f, 1.0f) },
            // ���� ���ǵ��
            { NatureId.Relaxed, new NatureMultiplier(1.0f, 1.1f, 1.0f, 1.0f, 0.9f) },

            // Ư���� ���ݡ�
            { NatureId.Modest,  new NatureMultiplier(0.9f, 1.0f, 1.1f, 1.0f, 1.0f) },
            // Ư���� ����
            { NatureId.Mild,    new NatureMultiplier(1.0f, 0.9f, 1.1f, 1.0f, 1.0f) },
            // Ư���� Ư���
            { NatureId.Rash,    new NatureMultiplier(1.0f, 1.0f, 1.1f, 0.9f, 1.0f) },
            // Ư���� ���ǵ��
            { NatureId.Quiet,   new NatureMultiplier(1.0f, 1.0f, 1.1f, 1.0f, 0.9f) },

            // Ư��� ���ݡ�
            { NatureId.Calm,    new NatureMultiplier(0.9f, 1.0f, 1.0f, 1.1f, 1.0f) },
            // Ư��� ����
            { NatureId.Gentle,  new NatureMultiplier(1.0f, 0.9f, 1.0f, 1.1f, 1.0f) },
            // Ư��� Ư����
            { NatureId.Careful, new NatureMultiplier(1.0f, 1.0f, 0.9f, 1.1f, 1.0f) },
            // Ư��� ���ǵ��
            { NatureId.Sassy,   new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.1f, 0.9f) },

            // ���ǵ�� ���ݡ�
            { NatureId.Timid,   new NatureMultiplier(0.9f, 1.0f, 1.0f, 1.0f, 1.1f) },
            // ���ǵ�� ����
            { NatureId.Hasty,   new NatureMultiplier(1.0f, 0.9f, 1.0f, 1.0f, 1.1f) },
            // ���ǵ�� Ư����
            { NatureId.Jolly,   new NatureMultiplier(1.0f, 1.0f, 0.9f, 1.0f, 1.1f) },
            // ���ǵ�� Ư���
            { NatureId.Naive,   new NatureMultiplier(1.0f, 1.0f, 1.0f, 0.9f, 1.1f) },

            // �߸� 5��
            { NatureId.Hardy,   new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
            { NatureId.Docile,  new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
            { NatureId.Serious, new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
            { NatureId.Bashful, new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
            { NatureId.Quirky,  new NatureMultiplier(1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
        };

        // ���� ���� ���
        public static NatureMultiplier Get(NatureId id)
        {
            if (_map.TryGetValue(id, out var m)) return m;
            return new NatureMultiplier(1f, 1f, 1f, 1f, 1f);
        }

        // �����/����: ������ ���ȿ� �ִ� ������ Ư�� ���� �ڵ�� ��������
        // statCode: "atk","def","spa","spd","spe"
        public static float GetStatMultiplier(NatureId id, string statCode)
        {
            var m = Get(id);
            switch (statCode)
            {
                case "atk": return m.atk;
                case "def": return m.def;
                case "spa": return m.spa;
                case "spd": return m.spd;
                case "spe": return m.spe;
                default: return 1f;
            }
        }
    }
}
