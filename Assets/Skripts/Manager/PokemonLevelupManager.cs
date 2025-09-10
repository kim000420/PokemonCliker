// ����: Scripts/Managers/PokemonLevelupManager.cs
using System;

namespace PokeClicker
{
    /// <summary>
    /// ������ ���� �Ŵ���.
    /// - ExpService�� ȣ���Ͽ� ����ġ �߰� �� ���� �������� ó��
    /// - �� �ܰ迡�� OnLevelUp �̺�Ʈ ����(�Һ��ڰ� StatService ����)
    /// </summary>
    public class PokemonLevelupManager
    {
        public event Action<int /*uid*/, int /*newLevel*/> OnLevelUp;

        /// <summary>
        /// ����ġ �߰� �� ���� ������ ó��.
        /// ��ȯ: �� ������ Ƚ��
        /// </summary>
        public int AddExpAndHandleLevelUps(
            PokemonSaveData p,
            SpeciesSO species,
            ExperienceCurveSO curve,
            int gainedExp)
        {
            int cnt = ExpService.AddExpAndHandleLevelUps(
                p, species, curve, gainedExp,
                newLv => OnLevelUp?.Invoke(p.P_uid, newLv));

            return cnt;
        }
    }
}
