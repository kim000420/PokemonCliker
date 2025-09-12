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
        [Header("Key")]
        public string formKey = "Default";   // SpeciesSO.NormalizeFormKey �� ����ȭ

        [Header("Typing")]
        public TypePair typePair;            // ��� Ÿ��(����ȭ ����)

        [Header("Base Stats (����)")]
        public StatBlock baseStats;          // ������

        public void NormalizeTypes()
        {
            typePair = TypePair.Create(typePair.primary, typePair.secondary);
        }
    }
}
