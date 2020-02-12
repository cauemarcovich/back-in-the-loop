using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Helpers;
using UnityEngine;

public class ShipRank : MonoBehaviour {
    public Rank currentRank;
    [SerializeField] List<Rank> ranks = new List<Rank> ();

    [Header ("ShipRankConfiguration")]
    public List<Sprite> NormalRankSprites;
    public List<Sprite> SplitterRankSprites;
    public List<Sprite> BlasterRankSprites;
    public List<Sprite> XRankSprites;

    PlayerHUD UI;

    void Start () {
        UI = GameObject.Find ("UI").GetComponent<PlayerHUD> ();
        ChangeRank (ShipType.Normal);
    }

    public void Leveling (ShipType powerUpType, bool downgrade = false) {
        if (currentRank.ShipType == powerUpType) {
            LevelUpRank (powerUpType, downgrade);
        } else {
            ChangeRank (powerUpType);
        }
    }

    void LevelUpRank (ShipType powerUpType, bool downgrade) {
        var newLevel = currentRank.Level + (!downgrade ? 1 : -1);

        if (currentRank.ShipType != ShipType.X) {
            if (currentRank.Level < 3)
                GetComponent<PlayerController> ().max_health += 50;
            else if (currentRank.Level == 3)
                GetComponent<PlayerController> ().max_health += 100;

            currentRank.Level = Mathf.Clamp (newLevel, 1, 4);
        } else {
            if (currentRank.Level == 4 || currentRank.Level == 8 || currentRank.Level == 10)
                GetComponent<PlayerController> ().max_health += 20;
            else if (currentRank.Level < 10)
                GetComponent<PlayerController> ().max_health += 5;

            currentRank.Level = Mathf.Clamp (newLevel, 1, 10);
        }

        UpdateShipSprite ();

        UI.UpdateShipImage ();
        UI.UpdateShipRanks ();

        if (currentRank.ShipType != ShipType.X && ranks.Count >= 3 && ranks.Where (_ => _.ShipType != ShipType.X).All (_ => _.Level == 4))
            ChangeRank (ShipType.X);
    }

    void ChangeRank (ShipType powerUpType) {
        if (currentRank.ShipType == ShipType.X) return;

        var powerup = ranks.FirstOrDefault (_ => _.ShipType == powerUpType);

        if (powerup != null) {
            currentRank = powerup;
        } else {
            var newRank = new Rank () { ShipType = powerUpType, Level = 1 };
            currentRank = newRank;
            ranks.Add (currentRank);
        }
        UpdateShipSprite ();

        UI.UpdateShipImage ();
        UI.UpdateShipRanks ();

        UI.UpdateLifeBarSprite ();
    }

    void UpdateShipSprite () {
        int spriteIndex = 0;
        if (currentRank.ShipType != ShipType.X) {
            spriteIndex = Mathf.Clamp (currentRank.Level - 2, 0, 2);
        } else {
            if (currentRank.Level.IsBetween (1, 4))
                spriteIndex = 0;
            else if (currentRank.Level.IsBetween (5, 9))
                spriteIndex = 1;
            else if (currentRank.Level == 10)
                spriteIndex = 2;
        }

        var spriteRenderer = GetComponent<SpriteRenderer> ();

        switch (currentRank.ShipType) {
            case ShipType.Normal:
                spriteRenderer.sprite = NormalRankSprites[spriteIndex];
                break;
            case ShipType.Splitter:
                spriteRenderer.sprite = SplitterRankSprites[spriteIndex];
                break;
            case ShipType.Blaster:
                spriteRenderer.sprite = BlasterRankSprites[spriteIndex];
                break;
            case ShipType.X:
                spriteRenderer.sprite = XRankSprites[spriteIndex];
                break;
            default:
                Debug.LogError ("Ship sprite not found.");
                break;
        }

        GetComponent<Vfx> ().ChangeShipFire (currentRank);
    }

    public int GetCurrentLevel () {
        return ranks.Sum (_ => _.Level);
    }
}

[System.Serializable]
public class Rank {
    public ShipType ShipType = ShipType.Normal;
    public int Level = 1;
}
public enum ShipType {
    Normal = 0,
    Splitter = 1,
    Blaster = 2,
    X = 3,
    Unknown = 4
}