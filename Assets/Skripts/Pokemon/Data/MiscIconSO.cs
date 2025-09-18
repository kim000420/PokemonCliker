// 파일: Scripts/Pokemon/Data/MiscIcons.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PokeClicker
{
    /// <summary>
    /// 성별, IV 등 기타 공용 아이콘을 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/DB/MiscIcons")]
    public class MiscIconSO : ScriptableObject
    {
        [Header("Gender Icons")]
        public Sprite male;         // 남성 성별 아이콘
        public Sprite female;       // 여성 성별 아이콘
        public Sprite genderless;   // 무성별 아이콘

        [Tooltip("IV 등급별 아이콘 리스트. 낮은 등급부터 높은 등급 순으로 정렬해주세요.")]
        public List<IvRankSprite> ivRankIcons = new List<IvRankSprite>(); // 등급별 아이콘 리스트

        [System.Serializable]
        public struct IvRankSprite
        {
            public int minIvValue;          // 이 등급이 적용되는 최소 IV 값 (포함)
            public int maxIvValue;          // 이 등급이 적용되는 최대 IV 값 (포함)
            public Sprite rankIcon;         // 등급 랭크 아이콘 스프라이트 (S, A, B, C, D, F) 
            public Sprite starIcon;         // 등급 별모양 아이콘 스프라이트 (높을수록 별이 꽉차는 아이콘)
        }

        /// <summary>
        /// 주어진 IV 값에 해당하는 등급 아이콘을 반환
        /// </summary>
        public Sprite GetIvRankIcon(int iv)
        {
            // IV 등급 범위에 맞는 아이콘 찾기
            var match = ivRankIcons.Find(r => iv >= r.minIvValue && iv <= r.maxIvValue);
            return match.rankIcon;
        }

        public Sprite GetIvStarIcon(int iv)
        {
            // IV 등급 범위에 맞는 아이콘 찾기
            var match = ivRankIcons.Find(r => iv >= r.minIvValue && iv <= r.maxIvValue);
            return match.starIcon;
        }

    }
}