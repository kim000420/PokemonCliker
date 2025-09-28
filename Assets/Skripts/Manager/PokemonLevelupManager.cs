// ����: Scripts/Managers/PokemonLevelupManager.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ������ ���� �Ŵ���.
    /// - ExpService�� ȣ���Ͽ� ����ġ �߰� �� ���� �������� ó��
    /// - �� �ܰ迡�� OnLevelUp �̺�Ʈ ����(�Һ��ڰ� StatService ����)
    /// </summary>
    public class PokemonLevelupManager : MonoBehaviour
    {
        public event Action<int /*uid*/, int /*newLevel*/> OnLevelUp;
        public event Action<int /*puid*/> OnExpGained;

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
            OnExpGained?.Invoke(p.P_uid);

            int cnt = ExpService.AddExpAndHandleLevelUps(
                p, species, curveType, gainedExp,
                newLv => OnLevelUp?.Invoke(p.P_uid, newLv));

            return cnt;
        }

    }
}
