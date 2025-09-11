using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// �Է� �̺�Ʈ�� �޾Ƽ� EXP/ģ�е� �й��ڿ� �����ϴ� ���� ���ɽ�Ʈ������.
    /// </summary>
    public class GameProgressController : MonoBehaviour
    {
        [Header("Wiring")]
        public InputCapture inputCapture;               // ���� InputCapture �Ҵ�
        public ClickRewardPolicy rewardPolicy;          // ��å SO
        public OwnedPokemonManager owned;               // Ʈ���̳� �ε� �� ����
        public PokemonLevelupManager levelupManager;    // �̺�Ʈ �����
        public ClickProgressTracker tracker;            // Ʈ���̳� ������ �ε�/���̺�

        // ��/� ��ȸ(������ Service Locator ��Ÿ�Ϸ� �Լ� ����)
        public SpeciesDatabase speciesDB;               // ����: speciesId -> SpeciesSO
        public ExperienceCurveDatabase curveDB;         // ����: speciesSO -> ExperienceCurveSO

        private PartyExpDistributor _exp;
        private PartyFriendshipDistributor _friend;

        void Awake()
        {
            // ������ üũ�� ���������� ���� ������Ʈ���� null �˻� ����
            _exp = new PartyExpDistributor(
                owned,
                levelupManager,
                speciesId => speciesDB.GetSpecies(speciesId),
                species => curveDB.GetCurveForSpecies(species));

            _friend = new PartyFriendshipDistributor(
                owned,
                tracker,
                rewardPolicy);
        }

        void OnEnable()
        {
            if (inputCapture != null)
                inputCapture.OnGameInput += HandleGameInput;
        }

        void OnDisable()
        {
            if (inputCapture != null)
                inputCapture.OnGameInput -= HandleGameInput;
        }

        private void HandleGameInput()
        {
            // EXP �й�
            _exp.GiveExpToParty(rewardPolicy.GetExpPerInput());

            // ģ�е� �й�(�ֱ� ���� �ÿ��� ���ο��� ����)
            _friend.OnInput();

            // ���̺� �̺�Ʈ
        }
    }

    // �Ʒ� �� Ŭ������ ���ÿ� �����Դϴ�. ������Ʈ�� ���� DB/�ε����� ���� �ٲټ���.
    public class SpeciesDatabase : MonoBehaviour
    {
        public SpeciesSO[] all;
        public SpeciesSO GetSpecies(int id)
        {
            foreach (var s in all) if (s != null && s.speciesId == id) return s;
            return null;
        }
    }
    public class ExperienceCurveDatabase : MonoBehaviour
    {
        public ExperienceCurveSO defaultCurve;
        public ExperienceCurveSO GetCurveForSpecies(SpeciesSO s)
        {
            // ���� ���� �ٸ� ��� ����Ѵٸ� ���⿡�� ����
            return defaultCurve;
        }
    }
}
