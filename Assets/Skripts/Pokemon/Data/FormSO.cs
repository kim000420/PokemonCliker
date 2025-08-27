using UnityEngine;

namespace PokeClicker
{
    // �� ��(Species) �ȿ� �����ϴ� �ϳ��� ��(�⺻/������ ��)�� �����Ѵ�.
    // Ÿ��, ������, ������ �ִϸ����� ���� ���� ������ �� �������� �����Ѵ�.
    [CreateAssetMenu(menuName = "PokeClicker/DB/Form")]
    public class FormSO : ScriptableObject
    {
        [Header("�Ҽ� �� / �� �ĺ�")]
        public SpeciesSO species;           // � ���� ���ϴ� ������
        public string formKey = "Default";  // "Default", "Alola", "Galar", "Hisui", "Paldea" ��

        [Header("Ÿ��(��� ����)")]
        public TypePair types = TypePair.Create(TypeEnum.None, TypeEnum.None);

        [System.Serializable]
        public struct VisualSet
        {
            public Sprite icon;                                // ����/��Ͽ��� ���� ������
            public RuntimeAnimatorController frontAnimator;    // ������� ���� �ִϸ�����
        }

        [Header("ǥ�� ���ҽ� (�Ϲ� / �̷�ġ)")]
        public VisualSet normal;
        public VisualSet shiny;

        // �ν����Ϳ��� ���� �ٲ� �� �ڵ� ����
        private void OnValidate()
        {
            // formKey ���� ����
            if (string.IsNullOrWhiteSpace(formKey))
                formKey = "Default";

            // Ÿ�� �Է� ����ȭ (primary�� None�̸� secondary�� None, �ߺ� ���� ��)
            types.Normalize();

            // �� ������ ����: ���� �� ����Ʈ�� ���ٸ� ������ �ܰ迡�� �������� ���߱� ����
            // (�ڵ� ������ �ǵ�ġ ���� ������ ������ �߻���ų �� �־� ���⼭�� �������� �ʴ´�)
        }

        // ���� ���� ���� Ÿ�� ����(0,1,2)
        public int GetTypeCount()
        {
            return types.Count();
        }

        // Ư�� Ÿ���� �����ϴ���
        public bool HasType(TypeEnum t)
        {
            return types.Has(t);
        }

        // isShiny ���ο� ���� ǥ�ÿ� �������� ��ȯ
        public Sprite GetIcon(bool isShiny)
        {
            return isShiny ? shiny.icon : normal.icon;
        }

        // isShiny ���ο� ���� ������ �ִϸ����͸� ��ȯ
        public RuntimeAnimatorController GetFrontAnimator(bool isShiny)
        {
            return isShiny ? shiny.frontAnimator : normal.frontAnimator;
        }
    }
}
