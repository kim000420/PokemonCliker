// 파일: Scripts/Pokemon/Data/MiscIcons.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PokeClicker
{
    /// <summary>
    /// 성별, IV 등 기타 공용 아이콘을 관리하는 ScriptableObject.
    /// IV 등급 아이콘 시스템을 포함하도록 확장했습니다.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/DB/MiscIcons")]
    public class MiscIcons : ScriptableObject
    {
        [Header("Gender Icons")]
        public Sprite male;         // 남성 성별 아이콘
        public Sprite female;       // 여성 성별 아이콘
        public Sprite genderless;   // 무성별 아이콘

        [Header("IV Icons")]
        [Tooltip("최고치(31) IV에 표시할 아이콘 (별)")]
        public Sprite maxIvStarIcon; // IV가 31일 때 표시할 별 아이콘

        [Tooltip("IV 등급별 아이콘 리스트. 낮은 등급부터 높은 등급 순으로 정렬해주세요.")]
        public List<IvRankSprite> ivRankIcons = new List<IvRankSprite>(); // 등급별 아이콘 리스트

        [System.Serializable]
        public struct IvRankSprite
        {
            public string rank;         // 등급 이름 (예: A, B, C)
            public int minIvValue;      // 이 등급이 적용되는 최소 IV 값
            public Sprite icon;         // 등급 아이콘 스프라이트
        }

        private Dictionary<int, Sprite> _ivIconLookup; // IV 값 -> 아이콘 매핑 딕셔너리

        /// <summary>
        /// 주어진 IV 값에 해당하는 등급 아이콘을 반환합니다.
        /// </summary>
        public Sprite GetIvRankIcon(int iv)
        {
            // IV 값에 따라 등급 아이콘을 반환 (최고 IV인 31은 별도 처리)
            if (iv >= 31 && maxIvStarIcon != null)
            {
                return maxIvStarIcon;
            }

            // 딕셔너리가 초기화되지 않았으면 초기화
            if (_ivIconLookup == null || _ivIconLookup.Count == 0)
            {
                BuildIvIconLookup();
            }

            // IV 등급 범위에 맞는 아이콘 찾기
            // FindLast을 사용하여 가장 높은 범위부터 확인
            var match = ivRankIcons.FindLast(r => iv >= r.minIvValue);
            return match.icon;
        }

        private void BuildIvIconLookup()
        {
            _ivIconLookup = new Dictionary<int, Sprite>();
            // IVs 0부터 31까지 각 값에 해당하는 아이콘을 미리 매핑
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