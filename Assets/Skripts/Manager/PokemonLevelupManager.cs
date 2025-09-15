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
            ExperienceCurve curveType,
            int gainedExp)
        {
            int cnt = ExpService.AddExpAndHandleLevelUps(
                p, species, curveType, gainedExp,
                newLv => OnLevelUp?.Invoke(p.P_uid, newLv));

            return cnt;
        }
    }
}
