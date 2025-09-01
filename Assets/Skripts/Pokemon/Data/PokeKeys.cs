// ����: Scripts/Data/PokeKeys.cs
using System;
using UnityEngine;

public enum PokeType { Normal, Fire, Water, Grass, Electric, Ice, Fighting, Poison, Ground, Flying, Psychic, Bug, Rock, Ghost, Dragon, Dark, Steel, Fairy, /* ���� ���� */ }
public enum Gender { Male, Female, Genderless }
public enum Variant { Normal, Shiny }
public enum AssetKind { Icon, Anim }

[Serializable]
public struct PokeFormKey : IEquatable<PokeFormKey>
{
    public int speciesId;   // 1..n
    public int formId;      // 0=�⺻, 1=����, 2=�ް� ��
    public Variant variant; // Normal/Shiny
    public AssetKind kind;  // Icon/Anim

    public override int GetHashCode() => HashCode.Combine(speciesId, formId, variant, kind);
    public bool Equals(PokeFormKey other) =>
        speciesId == other.speciesId && formId == other.formId && variant == other.variant && kind == other.kind;

    public override bool Equals(object obj) => obj is PokeFormKey other && Equals(other);

    public string ToAddressKey()
    {
        // ��: poke/icon/0911_0_Normal
        var root = kind == AssetKind.Icon ? "poke/icon" : "poke/anim";
        return $"{root}/{speciesId:D4}_{formId}_{variant}";
    }
}
