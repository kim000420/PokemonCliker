// 파일: Scripts/UI/PokemonIconView.cs
using UnityEngine;
using UnityEngine.UI;

public class PokemonIconView : MonoBehaviour
{
    [SerializeField] private Image img;

    public async void SetData(IconDatabase db, PokeFormKey key)
    {
        var sp = await db.GetPokemonIconAsync(key);
        img.sprite = sp;
        img.SetNativeSize(); // 픽셀아트라면 케이스에 따라 유지
    }
}
