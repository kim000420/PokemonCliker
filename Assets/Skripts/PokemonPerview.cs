// PokemonPreview.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PokemonPreview : MonoBehaviour
{
    [SerializeField] private Image img;  // PetPreview¿« Image
    [SerializeField] private PokemonPet current;
    [SerializeField] private bool playing;

    Coroutine playCo;

    public void SetPet(PokemonPet pet)
    {
        current = pet;
        if (playing) Restart();
    }

    public void Play(bool on)
    {
        playing = on;
        if (!gameObject.activeSelf) gameObject.SetActive(on);

        if (on) Restart();
        else if (playCo != null) { StopCoroutine(playCo); playCo = null; }
    }

    void Restart()
    {
        if (playCo != null) StopCoroutine(playCo);
        playCo = StartCoroutine(CoPlay());
    }

    IEnumerator CoPlay()
    {
        if (current == null || current.frames == null || current.frames.Length == 0)
        {
            img.enabled = false;
            yield break;
        }
        img.enabled = true;
        int i = 0;
        float dt = 1f / Mathf.Max(1f, current.fps);

        while (playing)
        {
            img.sprite = current.frames[i];
            i = (i + 1) % current.frames.Length;
            yield return new WaitForSeconds(dt);
        }
    }
}
