using System;
using UnityEngine;
using static RockinRackinPrototype;

[CreateAssetMenu(fileName = "UpgradeItemDictionary", menuName = "Scriptable Objects/UpgradeItemDictionary")]
public class UpgradeItemDictionary : ScriptableObject
{
    public UpgradeItemSprite[] upgradeItemSprite;

    public Sprite GetSprite(UpgradeKind upgradeKind)
    {
        if (upgradeItemSprite == null)
        {
            return null;
        }

        for (int i = 0; i < upgradeItemSprite.Length; i++)
        {
            if (upgradeItemSprite[i].upgradeKind == upgradeKind)
            {
                return upgradeItemSprite[i].sprite;
            }
        }

        return null;
    }

    [Serializable]
    public struct UpgradeItemSprite
    {
        public Sprite sprite;
        public UpgradeKind upgradeKind;
    }
}
