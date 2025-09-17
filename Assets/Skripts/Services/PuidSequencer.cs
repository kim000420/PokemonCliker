// 파일: Scripts/Services/PuidSequencer.cs
using System.Linq;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// P_uid를 00001부터 순차 발급하는 Provider.
    /// - 기존 보유 데이터가 있으면 그 중 최댓값+1부터 시작
    /// - OwnedPokemonManager.Init(..., this)를 통해 주입해서 사용
    /// </summary>
    public class PuidSequencer : MonoBehaviour, OwnedPokemonManager.IPokemonIdProvider
    {
        [SerializeField] private int _next = 1;

        /// <summary>OwnedPokemonManager의 현재 테이블을 스캔해 시작값을 맞춘다.</summary>
        public void InitializeFrom(OwnedPokemonManager owned)
        {
            if (owned == null) { _next = 1; return; }
            var max = owned.Table.Count > 0
                ? owned.Table.Values.Max(p => p?.P_uid ?? 0)
                : 0;
            _next = Mathf.Max(1, max + 1);
        }

        public int NextPuid()
        {
            return _next++;
        }
    }
}
