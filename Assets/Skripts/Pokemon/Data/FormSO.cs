using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// �� ���� "��"�� ���� �޶����� ���� ����.
    /// - Ÿ��(���), ������(BaseStats) ��
    /// - ������/�ִϸ��̼�/ġ��/Ư�� � ���� �ͼӵȴٸ� ���⿡ �߰�
    /// </summary>
    public class FormSO : ScriptableObject
    {
        [Header("Form ID")]
        public int formId;

        [Header("From Key")]
        public string formKey = "Default";   // SpeciesSO.NormalizeFormKey �� ����ȭ

        [Tooltip("����")] public int generation = 1; // ���� ����

        [Header("Typing")]
        public TypePair typePair;            // ��� Ÿ��(����ȭ ����)

        [Header("Base Stats (����)")]
        public StatBlock baseStats;          // ������
        
        [Header("Visuals (SO)")]
        public PokemonVisualSO visual;       // �����ܰ� �ִϸ��̼� ������ ����
    }
}
