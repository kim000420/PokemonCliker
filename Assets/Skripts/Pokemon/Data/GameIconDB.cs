// ����: Scripts/Pokemon/Data/GameIconDB.cs
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ���� �� ��� ������(Ÿ��, ����, IV ��)�� ���� �����ϴ� ScriptableObject.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/DB/GameIconDB")]
    public class GameIconDB : ScriptableObject
    {
        public TypeIconSO typeIcons;
        public MiscIconSO miscIcons;
    }
}