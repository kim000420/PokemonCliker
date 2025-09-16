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
        public OwnedPokemonManager ownedManager;   // ������ ����
        
        public struct Options
        {
            public bool rollNature;              // ���� ���� ���� ���� (�⺻: true)
            public bool rollShiny;               // �̷�ġ ���� ���� ���� (�⺻: true)
            public int shinyOneOver;             // �̷�ġ Ȯ�� �и�(1/x). 0 �Ǵ� ������ 4096���� ����
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
                shinyOneOver = 4096,
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
                isShiny = opt.rollShiny ? RandomService.RollShiny(opt.shinyOneOver > 0 ? opt.shinyOneOver : 4096) : false,
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
            p.EnsureValidAfterLoad(species, species.curveType);

            return p;
        }

        public PokemonSaveData GiveSpecificFormPokemon(int speciesId, string formKey, int level)
        {
            var species = speciesDB.GetSpecies(speciesId);
            if (species == null) return null;
            var p = Create(species, formKey, level);
            ownedManager.Add(p);
            return p;
        }

        // 4) ��ȹ ����
        public PokemonSaveData CatchPokemon(SpeciesSO species, string formKey, int level, bool guaranteed = false)
        {
            int rate = species.catchRate;
            bool success = guaranteed || UnityEngine.Random.Range(0, 100) < rate;
            if (!success) return null;

            var p = Create(species, formKey, level);
            ownedManager.Add(p);
            return p;
        }
    }
}
