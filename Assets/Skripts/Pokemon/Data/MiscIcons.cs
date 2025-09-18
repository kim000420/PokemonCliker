// ����: Scripts/Pokemon/Data/MiscIcons.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PokeClicker
{
    /// <summary>
    /// ����, IV �� ��Ÿ ���� �������� �����ϴ� ScriptableObject.
    /// IV ��� ������ �ý����� �����ϵ��� Ȯ���߽��ϴ�.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/DB/MiscIcons")]
    public class MiscIcons : ScriptableObject
    {
        [Header("Gender Icons")]
        public Sprite male;         // ���� ���� ������
        public Sprite female;       // ���� ���� ������
        public Sprite genderless;   // ������ ������

        [Header("IV Icons")]
        [Tooltip("�ְ�ġ(31) IV�� ǥ���� ������ (��)")]
        public Sprite maxIvStarIcon; // IV�� 31�� �� ǥ���� �� ������

        [Tooltip("IV ��޺� ������ ����Ʈ. ���� ��޺��� ���� ��� ������ �������ּ���.")]
        public List<IvRankSprite> ivRankIcons = new List<IvRankSprite>(); // ��޺� ������ ����Ʈ

        [System.Serializable]
        public struct IvRankSprite
        {
            public string rank;         // ��� �̸� (��: A, B, C)
            public int minIvValue;      // �� ����� ����Ǵ� �ּ� IV ��
            public Sprite icon;         // ��� ������ ��������Ʈ
        }

        private Dictionary<int, Sprite> _ivIconLookup; // IV �� -> ������ ���� ��ųʸ�

        /// <summary>
        /// �־��� IV ���� �ش��ϴ� ��� �������� ��ȯ�մϴ�.
        /// </summary>
        public Sprite GetIvRankIcon(int iv)
        {
            // IV ���� ���� ��� �������� ��ȯ (�ְ� IV�� 31�� ���� ó��)
            if (iv >= 31 && maxIvStarIcon != null)
            {
                return maxIvStarIcon;
            }

            // ��ųʸ��� �ʱ�ȭ���� �ʾ����� �ʱ�ȭ
            if (_ivIconLookup == null || _ivIconLookup.Count == 0)
            {
                BuildIvIconLookup();
            }

            // IV ��� ������ �´� ������ ã��
            // FindLast�� ����Ͽ� ���� ���� �������� Ȯ��
            var match = ivRankIcons.FindLast(r => iv >= r.minIvValue);
            return match.icon;
        }

        private void BuildIvIconLookup()
        {
            _ivIconLookup = new Dictionary<int, Sprite>();
            // IVs 0���� 31���� �� ���� �ش��ϴ� �������� �̸� ����
            for (int i = 0; i <= 31; i++)
            {
                var rank = ivRankIcons.FindLast(r => i >= r.minIvValue);
                if (rank.icon != null)
                {
                    _ivIconLookup[i] = rank.icon;
                }
            }
        }
    }
}