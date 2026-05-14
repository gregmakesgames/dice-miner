using System;
using System.Collections.Generic;
using GameData;

namespace DiceMiner.Gameplay.Data
{
    public class DiceGameplayData
    {
        public DiceTypeData Type { get; private set; }

        public static DiceGameplayData FromSave(DiceGameplaySaveData saveData)
        {
            return new DiceGameplayData()
            {
                Type = GameDataRegistry.Get<DiceTypeData>(saveData.typeId),
            };
        }

        public DiceGameplaySaveData ToSave()
        {
            return new DiceGameplaySaveData()
            {
                typeId = Type.Id,
            };
        }
    }

    [Serializable]
    public class DiceGameplaySaveData
    {
        public string typeId;
    }
}