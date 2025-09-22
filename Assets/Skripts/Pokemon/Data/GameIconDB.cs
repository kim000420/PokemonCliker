// 파일: Scripts/Pokemon/Data/GameIconDB.cs
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 게임 내 모든 아이콘(타입, 성별, IV 등)을 통합 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/DB/GameIconDB")]
    public class GameIconDB : ScriptableObject
    {
        public TypeIconSO typeIcons;
        public MiscIconSO miscIcons;
    }
}