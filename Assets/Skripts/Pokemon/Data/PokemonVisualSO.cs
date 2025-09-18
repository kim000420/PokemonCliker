// ����: Scripts/Pokemon/Data/PokemonVisualSO.cs
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ���ϸ� ���� �ð��� ������(������, �ִϸ��̼�)�� ��� ScriptableObject.
    /// �����Ϳ� ���־��� �и��Ͽ� �����մϴ�.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/Data/PokemonVisual")]
    public class PokemonVisualSO : ScriptableObject
    {
        [Tooltip("���� ������ (�Ϲ�)")]
        public Sprite icon;
        [Tooltip("���� ������ (�̷�ġ)")]
        public Sprite shinyIcon;

        [Header("Front Animation")]
        [Tooltip("����� �ִϸ��̼� ��������Ʈ ������ (�Ϲ�)")]
        public Sprite[] frontFrames;
        [Tooltip("����� �ִϸ��̼� ������ �ӵ� (FPS)")]
        public float frontAnimationFps = 6f;

        [Header("Shiny Variants")]
        [Tooltip("����� �ִϸ��̼� ��������Ʈ ������ (�̷�ġ)")]
        public Sprite[] shinyFrontFrames;
        [Tooltip("�̷�ġ �ִϸ��̼� ������ �ӵ� (FPS)")]
        public float shinyFrontAnimationFps = 6f;
    }
}