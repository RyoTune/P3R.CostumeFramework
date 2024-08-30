using P3R.CostumeFramework.Costumes;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeAlloutService
{
    private readonly Dictionary<Character, FBtlAlloutFinishTexture> defaultAllouts = [];
    private readonly Dictionary<Character, int> characterCostumeIds = [];

    public CostumeAlloutService(IDataTables dt)
    {
        foreach (var character in Enum.GetValues<Character>())
        {
            if (character > Character.Shinjiro) break;
            if (character == Character.NONE) continue;
            this.characterCostumeIds[character] = -1;
        }

        dt.FindDataTable("DT_BtlAlloutFinishTexture", this.BtlAlloutFinishTextureFound);
    }

    private void BtlAlloutFinishTextureFound(DataTable table)
    {
        // Save default allout values.
        for (int i = 0; i < 10; i++)
        {
            this.defaultAllouts[(Character)i + 1] = *(FBtlAlloutFinishTexture*)table.Rows[i].Self;
        }

        this.SetAllouts(table);
    }

    private void SetAllouts(DataTable dtAllout)
    {
        foreach (var item in this.characterCostumeIds)
        {
            var character = item.Key;
            var costumeId = item.Value;

            var alloutRowName = $"PC{(int)character}";
            var alloutRow = (FBtlAlloutFinishTexture*)dtAllout.Rows.First(x => x.Name == alloutRowName).Self;

            if (costumeId >= 1000)
            {
                var modAlloutRowName = $"PC{(int)character}_{costumeId}";
                var modAlloutRowObj = dtAllout.Rows.FirstOrDefault(x => x.Name == modAlloutRowName);

                if (modAlloutRowObj != null)
                {
                    var modAlloutRow = (FBtlAlloutFinishTexture*)modAlloutRowObj.Self;
                    alloutRow->TextureNormal = modAlloutRow->TextureNormal;
                    alloutRow->TextureNormalMask = modAlloutRow->TextureNormalMask;
                }
            }
            else
            {
                // Reset textures to defaults.
                *alloutRow = this.defaultAllouts[character];
            }
        }
    }

    public void UpdateCharacterAllout(Character character, int costumeId) => this.characterCostumeIds[character] = costumeId;


    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct FBtlAlloutFinishTexture
    {
        public SoftObjectProperty TextureNormal;
        public SoftObjectProperty TextureNormalMask;
        public SoftObjectProperty TexturSpecialOutfit;
        public SoftObjectProperty TextureSpecialMask;
        public SoftObjectProperty TextureText;
        public SoftObjectProperty TexturePlg;
    }

    [StructLayout(LayoutKind.Sequential, Size = 48)]
    private unsafe struct SoftObjectProperty
    {
    }
}
