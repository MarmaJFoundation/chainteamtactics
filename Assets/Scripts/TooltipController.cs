using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public RectTransform rectTransform;
    public CustomText unitNameText;
    public CustomText healthText;
    public CustomText speedText;
    public CustomText powerText;
    public CustomText saleText;
    public CustomText[] descriptionTexts;
    public Image unitImage;
    public RectTransform healthBar;
    public RectTransform speedBar;
    public RectTransform powerBar;

    public void Setup(UnitInfo unitInfo, int currentHealth, bool isPurple, bool randomUnit, bool fromMarket)
    {
        gameObject.SetActive(true);
        if (randomUnit)
        {
            unitNameText.SetString(unitInfo.unitType.ToString());
        }
        else
        {
            unitNameText.SetString(BaseUtils.GetUnitName(unitInfo));
        }
        saleText.gameObject.SetActive(unitInfo.price != 0 && !fromMarket);
        saleText.SetString($"unit on sale for {unitInfo.price} pxt");
        healthText.SetString($"health: {currentHealth}/{unitInfo.health}");
        healthBar.transform.localScale = new Vector3(Mathf.Clamp01((float)currentHealth/unitInfo.health), 1, 1);
        powerText.SetString($"power: {unitInfo.damage}");
        powerBar.transform.localScale = new Vector3(Mathf.Clamp01((float)unitInfo.damage / 350), 1, 1);
        speedText.SetString($"speed: {unitInfo.speed}");
        speedBar.transform.localScale = new Vector3(Mathf.Clamp01((float)unitInfo.speed / 80), 1, 1);
        ScriptableUnit scriptableUnit = BaseUtils.unitDict[unitInfo.unitType];
        unitImage.sprite = scriptableUnit.defaultImage;
        unitImage.transform.localScale = new Vector3(isPurple ? -2 : 2, 2, 2);
        Material goMaterial = new Material(isPurple ? BaseUtils.purpleMaterialUI : BaseUtils.yellowMaterialUI);
        goMaterial.SetColor("_HairColor", unitInfo.unitColorInfo.hairColor);
        goMaterial.SetColor("_SkinColor", unitInfo.unitColorInfo.skinColor);
        goMaterial.SetColor("_RarityOneColor", unitInfo.unitColorInfo.rarityColors[0]);
        goMaterial.SetColor("_RarityTwoColor", unitInfo.unitColorInfo.rarityColors[1]);
        goMaterial.SetColor("_RarityThreeColor", unitInfo.unitColorInfo.rarityColors[2]);
        goMaterial.SetColor("_RarityFourColor", unitInfo.unitColorInfo.rarityColors[3]);
        goMaterial.SetFloat("_HasRarity", unitInfo.unitColorInfo.hasRarity ? 1f : 0f);
        unitImage.material = goMaterial;

        for (int i = 0; i < descriptionTexts.Length; i++)
        {
            descriptionTexts[i].gameObject.SetActive(i < scriptableUnit.description.Length);
            if (i < scriptableUnit.description.Length)
            {
                descriptionTexts[i].SetString(scriptableUnit.description[i]);
            }
        }
    }
    public void Dispose()
    {
        gameObject.SetActive(false);
    }
}
