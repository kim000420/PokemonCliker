// 파일: Scripts/Pokemon/Data/TypeIconDB.cs
using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 포켓몬 타입 아이콘을 중앙에서 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/DB/TypeIconDB")]
    public class TypeIconDB : ScriptableObject
    {
        [System.Serializable]
        public struct TypeSpritePair
        {
            public TypeEnum type;
            public Sprite icon;
        }

        [Tooltip("타입별 아이콘 리스트")]
        public List<TypeSpritePair> typeIcons = new List<TypeSpritePair>();

        private Dictionary<TypeEnum, Sprite> _dict;

        public Sprite GetIcon(TypeEnum type)
        {
            // 런타임에 딕셔너리 빌드 (최초 1회만)
            if (_dict == null)
            {
                _dict = new Dictionary<TypeEnum, Sprite>();
                foreach (var pair in typeIcons)
                {
                    _dict[pair.type] = pair.icon;
                }
            }

            _dict.TryGetValue(type, out var sprite);
            return sprite;
        }
    }
}