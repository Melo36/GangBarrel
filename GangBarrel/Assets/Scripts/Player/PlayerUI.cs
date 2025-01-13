using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Sprite lifeSprite_3;
    [SerializeField] private Sprite lifeSprite_2;
    [SerializeField] private Sprite lifeSprite_1;
    [SerializeField] private Sprite lifeSprite_0;

    [SerializeField] private Image lifeImage;

    public void UpdateLifeUI(int healthValue)
    {
        switch (healthValue)
        {
            case 0:
                lifeImage.sprite = lifeSprite_0;
                break;
            case 1:
                lifeImage.sprite = lifeSprite_1;
                break;
            case 2:
                lifeImage.sprite = lifeSprite_2;
                break;
            case 3:
                lifeImage.sprite = lifeSprite_3;
                break;
            default:
                Debug.LogError("This life value is not allowed!");
                break;
        }
    }

}
