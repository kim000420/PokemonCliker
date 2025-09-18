// ����: Scripts/Pokemon/Data/TypeIconDB.cs
using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ���ϸ� Ÿ�� �������� �߾ӿ��� �����ϴ� ScriptableObject.
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

        [Tooltip("Ÿ�Ժ� ������ ����Ʈ")]
        public List<TypeSpritePair> typeIcons = new List<TypeSpritePair>();

        private Dictionary<TypeEnum, Sprite> _dict;

        public Sprite GetIcon(TypeEnum type)
        {
            // ��Ÿ�ӿ� ��ųʸ� ���� (���� 1ȸ��)
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