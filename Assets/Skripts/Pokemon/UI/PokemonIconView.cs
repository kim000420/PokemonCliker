// ����: Scripts/UI/PokemonIconView.cs
using UnityEngine;
using UnityEngine.UI;

public class PokemonIconView : MonoBehaviour
{
    [SerializeField] private Image img;

    public async void SetData(IconDatabase db, PokeFormKey key)
    {
        var sp = await db.GetPokemonIconAsync(key);
        img.sprite = sp;
        img.SetNativeSize(); // �ȼ���Ʈ��� ���̽��� ���� ����
    }
}
