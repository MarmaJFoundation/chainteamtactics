using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextController : MonoBehaviour
{
    public RectTransform rectTransform;
    public CustomText customText;

    public void Setup(Transform fromTransform, string textString, Color goColor, int damage, bool followTransform)
    { 
        customText.SetString(textString, goColor);
        BaseUtils.activeTexts.Add(this);
        StartCoroutine(FloatAndDisappear(fromTransform, damage, goColor, followTransform));
    }
    private IEnumerator FloatAndDisappear(Transform fromTransform, int damage, Color goColor, bool followTransform)
    {
        float timer = 0;
        float goScale = 1 + Mathf.Clamp(damage * .001f, .3f, .5f);
        //Vector3 fromPos = BaseUtils.mainCam.WorldToScreenPoint(fromTransform.position + Vector3.up * 2 + Vector3.forward * 10) + Vector3.up * 20;
        Vector3 fromPos = fromTransform.position + Vector3.up * 30f.ToScale();
        Vector3 randomPos = Vector3.right * BaseUtils.RandomInt(3, 5) * BaseUtils.RandomSign() * BaseUtils.mainCanvas.parent.localScale.x;
        Vector3 upPos = Vector3.up * BaseUtils.RandomInt(10, 20) * BaseUtils.mainCanvas.parent.localScale.x;
        while (timer <= 1)
        {
            if (followTransform)
            {
                //fromPos = BaseUtils.mainCam.WorldToScreenPoint(fromTransform.position + Vector3.up * 2 + Vector3.forward * 10) + Vector3.up * 20;
                fromPos = fromTransform.position + Vector3.up * 30f.ToScale();
            }
            rectTransform.position = Vector3.Lerp(fromPos, fromPos + upPos + randomPos, timer.Evaluate(CurveType.EaseOut));
            rectTransform.localScale = Vector3.Lerp(Vector3.one * goScale * .75f, Vector3.one * goScale, timer.Evaluate(CurveType.PeakParabol));
            customText.SetString(Color.Lerp(goColor, Color.white, timer.Evaluate(CurveType.PeakParabol)));
            timer += Time.deltaTime * 1.5f;
            yield return null;
        }
        for (int i = 0; i < 3; i++)
        {
            rectTransform.position = fromPos + upPos + randomPos;
            yield return new WaitForSeconds(.05f / (i + 1));
            rectTransform.position = Vector3.left * 9000;
            yield return new WaitForSeconds(.05f / (i + 1));
        }
        BaseUtils.activeTexts.Remove(this);
        Dispose();
    }
    public void Dispose()
    {
        rectTransform.position = Vector3.right * 10000;
        BaseUtils.textPool.Enqueue(this);
    }
}
