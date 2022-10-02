using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    //public ParticleSystem particle;
    private EffectType effectType;
    private bool disposed;
    public void Setup(EffectType effectType, Vector3 goPosition, float scaleModifier, float sideModifier)
    {
        this.effectType = effectType;
        Vector3 goScale = Vector3.one * scaleModifier;
        goScale.x *= sideModifier;
        transform.localScale = goScale;
        transform.position = goPosition;
        foreach (ParticleSystem particle in GetComponentsInChildren<ParticleSystem>())
        {
            particle.transform.localScale = goScale;
            particle.Play();
        }
    }
    public void Setup(EffectType effectType, Vector3 fromPosition, Vector3 gotoPosition, float scaleModifier, float sideModifier)
    {
        this.effectType = effectType;
        Vector3 goScale = Vector3.one * scaleModifier;
        goScale.x *= sideModifier;
        transform.localScale = goScale;
        //float scaleXMod = (gotoPosition.x < fromPosition.x || effectType != EffectType.ArrowProject) ? 1 : -1;
        transform.position = fromPosition;
        foreach (ParticleSystem particle in GetComponentsInChildren<ParticleSystem>())
        {
            particle.transform.localScale = new Vector3(goScale.x, goScale.y, goScale.z);
            particle.Play();
        }
        if (effectType == EffectType.ArrowProject)
        {
            transform.GetChild(1).localEulerAngles = new Vector3(60, 0, Mathf.Atan2(fromPosition.z - gotoPosition.z, fromPosition.x - gotoPosition.x) * Mathf.Rad2Deg);
        }
        StartCoroutine(TravelAndDestroy(gotoPosition));
    }
    private IEnumerator TravelAndDestroy(Vector3 gotoPosition)
    {
        float timer = 0;
        Vector3 fromPos = transform.position;
        while (timer <= 1)
        {
            transform.position = Vector3.Lerp(fromPos, gotoPosition, timer);
            timer += Time.deltaTime * 5;
            yield return null;
        }
        Dispose();
    }
    private void Update()
    {
        bool particlePlaying = false;
        foreach (ParticleSystem particle in GetComponentsInChildren<ParticleSystem>())
        {
            if (particle.isPlaying)
            {
                particlePlaying = true;
                break;
            }
        }
        if (!disposed && !particlePlaying)
        {
            Dispose();
        }
    }
    public void Dispose()
    {
        foreach (ParticleSystem particle in GetComponentsInChildren<ParticleSystem>())
        {
            particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        BaseUtils.effectPool[effectType].Enqueue(this);
        disposed = true;
        transform.position = Vector3.right * 10000;
    }
}
