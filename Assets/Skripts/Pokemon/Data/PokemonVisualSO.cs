// 파일: Scripts/Pokemon/Data/PokemonVisualSO.cs
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 포켓몬 폼의 시각적 데이터(아이콘, 애니메이션)를 담는 ScriptableObject.
    /// 데이터와 비주얼을 분리하여 관리합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/Data/PokemonVisual")]
    public class PokemonVisualSO : ScriptableObject
    {
        [Tooltip("도감 아이콘 (일반)")]
        public Sprite icon;
        [Tooltip("도감 아이콘 (이로치)")]
        public Sprite shinyIcon;

        [Header("Front Animation")]
        [Tooltip("전면부 애니메이션 스프라이트 프레임 (일반)")]
        public Sprite[] frontFrames;
        [Tooltip("전면부 애니메이션 프레임 속도 (FPS)")]
        public float frontAnimationFps = 6f;

        [Header("Shiny Variants")]
        [Tooltip("전면부 애니메이션 스프라이트 프레임 (이로치)")]
        public Sprite[] shinyFrontFrames;
        [Tooltip("이로치 애니메이션 프레임 속도 (FPS)")]
        public float shinyFrontAnimationFps = 6f;
    }
}