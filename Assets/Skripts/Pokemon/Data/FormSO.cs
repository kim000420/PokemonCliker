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

        [Tooltip("����")] public int generation = 1; // ���� ����

        [Header("Key")]
        public string formKey = "Default";   // SpeciesSO.NormalizeFormKey �� ����ȭ

        [Header("Typing")]
        public TypePair typePair;            // ��� Ÿ��(����ȭ ����)

        [Header("Base Stats (����)")]
        public StatBlock baseStats;          // ������
    }
}
