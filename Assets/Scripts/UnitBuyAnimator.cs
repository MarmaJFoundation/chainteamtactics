using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBuyAnimator : MonoBehaviour
{
    public GameObject[] effectObjs;
    public GameObject[] exploObjs;
    public CustomText[] textLines;
    public Image unitImage;
    public Image edgeImage;

    public void Setup(UnitInfo unitInfo)
    {
        gameObject.SetActive(true);
        ScriptableUnit scriptableUnit = BaseUtils.unitDict[unitInfo.unitType];
        edgeImage.sprite = BaseUtils.tierSlots[scriptableUnit.unitTier - 1];
        unitImage.sprite = scriptableUnit.defaultImage;
        Material goMaterial = new Material(BaseUtils.yellowMaterialUI);
        goMaterial.SetColor("_HairColor", unitInfo.unitColorInfo.hairColor);
        goMaterial.SetColor("_SkinColor", unitInfo.unitColorInfo.skinColor);
        goMaterial.SetColor("_RarityOneColor", unitInfo.unitColorInfo.rarityColors[0]);
        goMaterial.SetColor("_RarityTwoColor", unitInfo.unitColorInfo.rarityColors[1]);
        goMaterial.SetColor("_RarityThreeColor", unitInfo.unitColorInfo.rarityColors[2]);
        goMaterial.SetColor("_RarityFourColor", unitInfo.unitColorInfo.rarityColors[3]);
        goMaterial.SetFloat("_HasRarity", unitInfo.unitColorInfo.hasRarity ? 1f : 0f);
        unitImage.material = goMaterial;
        effectObjs[scriptableUnit.unitTier - 1].SetActive(true);
        exploObjs[scriptableUnit.unitTier - 1].SetActive(true);
        textLines[1].SetString($"you hired {BaseUtils.GetUnitName(unitInfo)}");
        textLines[2].SetString($"{scriptableUnit.pronoun} is now part of your party!");
        StartCoroutine(AnimateUnit(scriptableUnit));
        StartCoroutine(BumpUnit());
    }
    private IEnumerator AnimateUnit(ScriptableUnit scriptableUnit)
    {
        GameObject unitObj = Instantiate(scriptableUnit.unitPrefab);
        Animation animation = unitObj.GetComponentInChildren<SpriteAnimator>().animations[1];
        Destroy(unitObj);
        while (true)
        {
            for (int i = 0; i < animation.sprites.Length; i++)
            {
                unitImage.sprite = animation.sprites[i];
                yield return new WaitForSeconds(animation.speed);
            }
            if (animation.reverseLoop)
            {
                for (int i = animation.sprites.Length - 1; i >= 0; i--)
                {
                    unitImage.sprite = animation.sprites[i];
                    yield return new WaitForSeconds(animation.speed);
                }
            }
            if (animation.dontLoop)
            {
                break;
            }
        }
    }
    private IEnumerator BumpUnit()
    {
        float timer = 0;
        while (timer <= 1)
        {
            unitImage.transform.localScale = Vector3.Lerp(Vector3.one * 4, Vector3.one * 6, timer.Evaluate(CurveType.PeakParabol));
            timer += Time.deltaTime * 2;
            yield return null;
        }
    }
    public void OnExitClick()
    {
        StopAllCoroutines();
        for (int i = 0; i < 4; i++)
        {
            effectObjs[i].SetActive(false);
            exploObjs[i].SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
