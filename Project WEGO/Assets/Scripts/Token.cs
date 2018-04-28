using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour {

    public SpriteRenderer spriteRenderer;
    public Material MainMaterial;
    public Material AccentMaterial;


    // Only called first time, sets material colors and sprite
    public void SetUp(Color main, Color accent, Sprite s)
    {
        spriteRenderer.color = accent;
        spriteRenderer.sprite = s;


        MainMaterial.color = main;
        AccentMaterial.color = accent;

    }


    // Function for when material has already been set,
    // Only necessary to change sprite renderer color and set sprite
    public void SetUp(Color accent, Sprite s)
    {
        spriteRenderer.color = accent;
        spriteRenderer.sprite = s;
    }
}
