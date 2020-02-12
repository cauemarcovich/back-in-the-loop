using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {
	public GameObject Player;
	public LifeBar_Sprite[] LifeBar_Sprites;

	PlayerController p_controller;
	ShipRank p_rank;

	Image picture;
	Transform powerups;
	RectTransform lifebar;

	void Awake () {
		p_controller = Player.GetComponent<PlayerController> ();
		p_rank = Player.GetComponent<ShipRank> ();

		picture = transform.Find ("Player/Picture/Image").GetComponent<Image> ();
		powerups = transform.Find ("Player/PowerUps").GetComponent<Transform> ();
		lifebar = transform.Find ("Player/LifeBar/Bar").GetComponent<RectTransform> ();
	}

	public void ShowPlayerHUD () {
        transform.Find ("Player").gameObject.SetActive (true);
    }

	public void UpdateShipImage () {
        picture.sprite = Player.GetComponent<SpriteRenderer> ().sprite;
    }
    public void UpdateLifeBar () {
        var barSize = lifebar.parent.GetComponent<RectTransform> ().rect.width;
        var healthPercent = p_controller.GetHealth () / p_controller.max_health;

        lifebar.sizeDelta = new Vector2 (barSize * healthPercent, 39);
        lifebar.anchoredPosition = new Vector2 (0, 0);
    }
    public void UpdateLifeBarSprite () {
        var currentRank = p_rank.currentRank;
        var lifebar_sprite = LifeBar_Sprites.FirstOrDefault (_ => _.shipType == currentRank.ShipType);

        var lifeBarImage = lifebar.transform.GetComponent<Image> ();
        lifeBarImage.sprite = lifebar_sprite.barSprite;
        lifeBarImage.color = lifebar_sprite.barColor;
    }
    public void UpdateShipRanks () {
        var currentRank = p_rank.currentRank;

        if (currentRank.Level == 1)
            return;

        Transform transform = null;

        if (currentRank.ShipType == ShipType.Normal)
            transform = powerups.Find ("Blue");
        else if (currentRank.ShipType == ShipType.Splitter)
            transform = powerups.Find ("Green");
        else if (currentRank.ShipType == ShipType.Blaster)
            transform = powerups.Find ("Red");
        else if (currentRank.ShipType == ShipType.X)
            transform = powerups.Find ("Orange");
        else { Debug.LogError ("Power Up type not found. UI not updated."); return; }

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

        var child = transform.GetChild (spriteIndex);
        child.gameObject.SetActive (true);
    }
}

[System.Serializable]
public class LifeBar_Sprite {
	public ShipType shipType;
	public Sprite barSprite;
	public Color barColor;
}