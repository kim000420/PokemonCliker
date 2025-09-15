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
    public static class PokemonFactory
    {
        [Serializable]
        public struct Options
        {
            public bool rollNature;              // ���� ���� ���� ���� (�⺻: true)
            public bool rollShiny;               // �̷�ġ ���� ���� ���� (�⺻: true)
            public int shinyOneOver;             // �̷�ġ Ȯ�� �и�(1/x). 0 �Ǵ� ������ 4096���� ����
            public bool rollGender;              // ���� ��å ���� ���� (�⺻: true)
            public bool rollIVs;                 // IVs ���� ���� ���� (�⺻: false ? ���� ������ OFF)
            public DateTime obtainedAt;          // ȹ�� �ð� (default�̸� Now)
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
                rollIVs = false,
                obtainedAt = default
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
                nature = opt.rollNature ? NatureTable.PickRandom() : NatureId.Hardy,
                ivs = opt.rollIVs ? RandomService.RollIVsAll() : default,
                obtainedAt = opt.obtainedAt == default ? DateTime.Now : opt.obtainedAt,
                metLevel = level,
                metFormKey = formKey
            };

            // ���� ���� ����(����/����)
            p.EnsureValidAfterLoad(species, species.curveType);

            return p;
        }
    }
}
