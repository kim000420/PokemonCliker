// ÆÄÀÏ: Scripts/UI/TypeIconView.cs
using UnityEngine;
using UnityEngine.UI;

public class TypeIconView : MonoBehaviour
{
    [SerializeField] private Image imgA;
    [SerializeField] private Image imgB; // µà¾ó ¾Æ´Ò ¶© ºñÈ°¼º

    public async void SetTypes(IconDatabase db, PokeType t1, PokeType? t2 = null)
    {
        if (t2 == null)
        {
            imgA.sprite = await db.GetTypeSpriteAsync(t1);
            imgB.gameObject.SetActive(false);
            return;
        }
        var (a, b) = await db.GetDualTypeSpritesAsync(t1, t2.Value);
        imgA.sprite = a;
        imgB.sprite = b;
        imgB.gameObject.SetActive(true);
    }
}
