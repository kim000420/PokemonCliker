// ����: Scripts/Managers/PokemonFactory.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ��ȹ/���� ���� ���丮.
    /// - ��/��/������ �ɼ�(����/�̷�ġ/����/IVs)�� �������� PokemonSaveData�� �����Ѵ�.
    /// - ���/ǥ�ô� ���⼭ ���� �ʴ´�(StatService�� ����ϴ� �ʿ��� ȣ��).
    /// </summary>
    public class PokemonFactory : MonoBehaviour
    {

        public SpeciesDB speciesDB;                // ������ ����
        public OwnedPokemonManager owned;   // ������ ����
        public int currentTuid;
        
        public struct Options
        {
            public bool rollNature;              // ���� ���� ���� ���� (�⺻: true)
            public bool rollShiny;               // �̷�ġ ���� ���� ���� (�⺻: true)
            public bool rollGender;              // ���� ��å ���� ���� (�⺻: true)
            public bool rollIVs;                 // IVs ���� ���� ���� (�⺻: false ? ���� ������ OFF)
        }

        public static PokemonSaveData Create(
            SpeciesSO species,
            string formKey,
            int level,
            Options? optNullable = null
        )
        {
            if (species == null) throw new ArgumentNullException(nameof(species));
            var opt = optNullable ?? new Options
            {
                rollNature = true,
                rollShiny = true,
                rollGender = true,
                rollIVs = false
            };

            // �⺻�� ����
            if (string.IsNullOrWhiteSpace(formKey)) formKey = "Default";
            level = Mathf.Clamp(level, 1, Mathf.Clamp(species.maxLevel, 1, 100));

            // �ʼ� �ʵ� ä���
            var p = new PokemonSaveData
            {
                P_uid = 0, // ���� �Ŵ����� �ο�
                speciesId = species.speciesId,
                formKey = formKey,
                isShiny = opt.rollShiny ? RandomService.RollShiny() : false,
                level = level,
                currentExp = 0,
                nickname = null,
                gender = opt.rollGender ? RandomService.RollGender(species.genderPolicy) : Gender.Genderless,
                friendship = 0,
                heldItemId = null,
                nature = opt.rollNature ? RandomService.RollNature() : NatureId.Hardy,
                ivs = opt.rollIVs ? RandomService.RollIVsAll() : default,
                metLevel = level,
                metFormKey = formKey
            };

            // ���� ���� ����(����/����)
            p.EnsureValidAfterLoad(species);

            return p;
        }

        // �ӽ�
        public PokemonSaveData GiveSpecificFormPokemon(int speciesId, string formKey, int level)
        {
            var species = speciesDB.GetSpecies(speciesId);
            if (species == null) return null;
            var p = Create(species, formKey, level);
            owned.Add(p);
            return p;
        }

        public PokemonSaveData GiveStarterForSignup(int speciesId, string formKey, int level)
        {
            var species = speciesDB.GetSpecies(speciesId);
            if (species == null) { Debug.LogWarning($"Starter species not found: {speciesId}"); return null; }

            var p = Create(species, formKey, level);   // ���⼭�� P_uid �ο� X
            owned.Add(p);                              // ���⼭ Provider�� ���� P_uid ���� �߱�
            return p;
        }

        // ��ġ: PokemonFactory �� (�޼��� ���� �߰�)
        public PokemonSaveData GiveRandomTest(int level)
        {
            var s = speciesDB.GetRandom();
            if (s == null) return null;

            var forms = s.Forms;
            if (forms == null || forms.Count == 0) return null;

            var key = forms[UnityEngine.Random.Range(0, forms.Count)].formKey;
            var p = Create(s, key, level); // P_uid ���� ����
            owned.Add(p);                  // ���⼭ P_uid �ο�
            return p;
        }

    }
}
