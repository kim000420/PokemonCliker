// 파일: Scripts/UI/IconDatabase.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class IconDatabase : MonoBehaviour
{
    [Header("Type Icons (싱글 타입 전용)")]
    [Tooltip("Addressables 키: type/{Type}")]
    public bool loadTypeByAddressables = false;
    [SerializeField] private Sprite[] typeSpritesByIndex; // 인덱스 = (int)PokeType
    [SerializeField] private string typeAddressRoot = "type/"; // ex) type/Grass

    [Header("Gender")]
    public Sprite male; public Sprite female; public Sprite genderless;

    private readonly Dictionary<PokeFormKey, Sprite> _iconCache = new();
    private readonly Dictionary<PokeFormKey, Sprite[]> _animCache = new();
    private readonly LinkedList<PokeFormKey> _lru = new();
    [SerializeField] private int iconCacheLimit = 256;
    [SerializeField] private int animCacheLimit = 64;

    // 타입 아이콘 얻기 (싱글)
    public async Task<Sprite> GetTypeSpriteAsync(PokeType t)
    {
        if (!loadTypeByAddressables)
            return typeSpritesByIndex[(int)t];

        var key = $"{typeAddressRoot}{t}";
        var handle = Addressables.LoadAssetAsync<Sprite>(key);
        return await handle.Task;
    }

    // 듀얼 타입 → 두 장 반환
    public async Task<(Sprite a, Sprite b)> GetDualTypeSpritesAsync(PokeType t1, PokeType t2)
    {
        // 정렬 고정
        if ((int)t2 < (int)t1) (t1, t2) = (t2, t1);
        var a = await GetTypeSpriteAsync(t1);
        var b = await GetTypeSpriteAsync(t2);
        return (a, b);
    }

    // 몬스터 아이콘
    public async Task<Sprite> GetPokemonIconAsync(PokeFormKey key)
    {
        key.kind = AssetKind.Icon;
        if (_iconCache.TryGetValue(key, out var sp)) { TouchLRU(key); return sp; }

        var handle = Addressables.LoadAssetAsync<Sprite>(key.ToAddressKey());
        sp = await handle.Task;
        _iconCache[key] = sp; TouchLRU(key);
        TrimIconCache();
        return sp;
    }

    // 몬스터 애니 (Sprite[] 프레임)
    public async Task<Sprite[]> GetPokemonAnimAsync(PokeFormKey key)
    {
        key.kind = AssetKind.Anim;
        if (_animCache.TryGetValue(key, out var frames)) return frames;

        var handle = Addressables.LoadAssetAsync<Sprite[]>(key.ToAddressKey());
        frames = await handle.Task;
        _animCache[key] = frames;
        TrimAnimCache();
        return frames;
    }

    // 간단 LRU
    private void TouchLRU(PokeFormKey key)
    {
        _lru.Remove(key); _lru.AddFirst(key);
    }
    private void TrimIconCache()
    {
        while (_iconCache.Count > iconCacheLimit)
        {
            var last = _lru.Last.Value; _lru.RemoveLast();
            if (_iconCache.Remove(last))
                Addressables.Release(_iconCache); // 주의: 실제 핸들 추적 방식에 맞게 Release 관리
        }
    }
    private void TrimAnimCache()
    {
        if (_animCache.Count <= animCacheLimit) return;
        // 애니는 사용처가 명확하니 메뉴 전환 시 일괄 Release를 추천
    }
}
