// ����: Scripts/Pokemon/Data/MiscIcons.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PokeClicker
{
    /// <summary>
    /// ����, IV �� ��Ÿ ���� �������� �����ϴ� ScriptableObject.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/DB/MiscIcons")]
    public class MiscIconSO : ScriptableObject
    {
        [Header("Gender Icons")]
        public Sprite male;         // ���� ���� ������
        public Sprite female;       // ���� ���� ������
        public Sprite genderless;   // ������ ������

        [Tooltip("IV ��޺� ������ ����Ʈ. ���� ��޺��� ���� ��� ������ �������ּ���.")]
        public List<IvRankSprite> ivRankIcons = new List<IvRankSprite>(); // ��޺� ������ ����Ʈ

        [System.Serializable]
        public struct IvRankSprite
        {
            public int minIvValue;          // �� ����� ����Ǵ� �ּ� IV �� (����)
            public int maxIvValue;          // �� ����� ����Ǵ� �ִ� IV �� (����)
            public Sprite rankIcon;         // ��� ��ũ ������ ��������Ʈ (S, A, B, C, D, F) 
            public Sprite starIcon;         // ��� ����� ������ ��������Ʈ (�������� ���� ������ ������)
        }

        /// <summary>
        /// �־��� IV ���� �ش��ϴ� ��� �������� ��ȯ
        /// </summary>
        public Sprite GetIvRankIcon(int iv)
        {
            // IV ��� ������ �´� ������ ã��
            var match = ivRankIcons.Find(r => iv >= r.minIvValue && iv <= r.maxIvValue);
            return match.rankIcon;
        }

        public Sprite GetIvStarIcon(int iv)
        {
            // IV ��� ������ �´� ������ ã��
            var match = ivRankIcons.Find(r => iv >= r.minIvValue && iv <= r.maxIvValue);
            return match.starIcon;
        }

    }
}