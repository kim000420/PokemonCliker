// PokemonPet.cs  (ScriptableObject)
using UnityEngine;

[CreateAssetMenu(menuName = "Pet/PokemonPet")]
public class PokemonPet : ScriptableObject
{
    public string petId;
    public Sprite[] frames;   // 순서대로 애니 프레임
    public float fps = 6f;    // 초당 프레임
}
