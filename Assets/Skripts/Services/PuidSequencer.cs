// ����: Scripts/Services/PuidSequencer.cs
using System.Linq;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// P_uid�� 00001���� ���� �߱��ϴ� Provider.
    /// - ���� ���� �����Ͱ� ������ �� �� �ִ�+1���� ����
    /// - OwnedPokemonManager.Init(..., this)�� ���� �����ؼ� ���
    /// </summary>
    public class PuidSequencer : MonoBehaviour, OwnedPokemonManager.IPokemonIdProvider
    {
        [SerializeField] private int _next = 1;

        /// <summary>OwnedPokemonManager�� ���� ���̺��� ��ĵ�� ���۰��� �����.</summary>
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
