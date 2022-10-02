using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Animation
{
    public string name;
    public Sprite[] sprites;
    public float speed;
    public bool reverseLoop;
    public bool dontLoop;
}
public class SpriteAnimator : MonoBehaviour
{
    public Animation[] animations;
    public SpriteRenderer spriteRenderer;
    public bool skipRandomness;
    private string currentAnim;
    private Coroutine animCoroutine;
    public void Setup()
    {
        animCoroutine = StartCoroutine(AnimateSprite(animations[0], true));
    }
    private void OnEnable()
    {
        Setup();
    }
    private void OnDisable()
    {
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
        }
    }
    public void Animate(string animName)
    {
        if (currentAnim == animName)
        {
            return;
        }
        currentAnim = animName;
        for (int i = 0; i < animations.Length; i++)
        {
            if (animations[i].name == animName)
            {
                if (animCoroutine != null)
                {
                    StopCoroutine(animCoroutine);
                    animCoroutine = StartCoroutine(AnimateSprite(animations[i]));
                }
            }
        }
    }
    public void StopAnimation(string animName, int frame)
    {
        StopCoroutine(animCoroutine);
        currentAnim = animName;
        for (int i = 0; i < animations.Length; i++)
        {
            if (animations[i].name == animName)
            {
                spriteRenderer.sprite = animations[i].sprites[frame];
            }
        }
    }
    private IEnumerator AnimateSprite(Animation animation, bool firstTime = false)
    {
        if (!skipRandomness && firstTime)
        {
            yield return new WaitForSeconds(BaseUtils.RandomFloat(0, .5f));
        }
        while (true)
        {
            for (int i = 0; i < animation.sprites.Length; i++)
            {
                spriteRenderer.sprite = animation.sprites[i];
                yield return new WaitForSeconds(animation.speed);
            }
            if (animation.reverseLoop)
            {
                for (int i = animation.sprites.Length - 1; i >= 0; i--)
                {
                    spriteRenderer.sprite = animation.sprites[i];
                    yield return new WaitForSeconds(animation.speed);
                }
            }
            if (animation.dontLoop)
            {
                break;
            }
        }
    }
}
