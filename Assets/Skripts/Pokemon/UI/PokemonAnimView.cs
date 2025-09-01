// ÆÄÀÏ: Scripts/UI/PokemonAnimView.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PokemonAnimView : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private float fps = 8f;

    private Coroutine _co;

    public async void SetData(IconDatabase db, PokeFormKey key)
    {
        var frames = await db.GetPokemonAnimAsync(key);
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Play(frames));
    }

    private IEnumerator Play(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) yield break;
        var dt = 1f / fps;
        var i = 0;
        while (true)
        {
            img.sprite = frames[i];
            i = (i + 1) % frames.Length;
            yield return new WaitForSeconds(dt);
        }
    }
}
