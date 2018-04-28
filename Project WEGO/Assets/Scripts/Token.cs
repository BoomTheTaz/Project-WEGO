using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour {

    public SpriteRenderer spriteRenderer;
    public Material MainMaterial;
    public Material AccentMaterial;


    public void SetUp(Color main, Color accent, Sprite s)
    {
        spriteRenderer.color = accent;
        spriteRenderer.sprite = s;


        MainMaterial.color = main;
        AccentMaterial.color = accent;

    }
}
