using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DepthSorter : MonoBehaviour
{
    private HashSet<SpriteRenderer> spriteRenderers = new HashSet<SpriteRenderer>();
    private bool setSprites;
    private void Start()
    {
        GetAllSprites();
    }
    private void Update()
    {
        if (!setSprites)
        {
            GetAllSprites();
        }
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt((spriteRenderer.transform.position.y - (spriteRenderer.sprite.rect.height / 2 * .06f)) * -100);
        }
    }
    private void GetAllSprites()
    {
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt((spriteRenderer.transform.position.y - (spriteRenderer.sprite.rect.height / 2 * .06f)) * -100);
            if (spriteRenderer.name.Contains("Character"))
            {
                spriteRenderers.Add(spriteRenderer);
            }
        }
        setSprites = true;
    }
}
