// PokemonPet.cs  (ScriptableObject)
using UnityEngine;

[CreateAssetMenu(menuName = "Pet/PokemonPet")]
public class PokemonPet : ScriptableObject
{
    public string petId;
    public Sprite[] frames;   // ������� �ִ� ������
    public float fps = 6f;    // �ʴ� ������
}
