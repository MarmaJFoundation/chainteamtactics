using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RandomUnitTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnitType unitType;
    public Image buttonImage;
    public Image unitImage;
    public MainMenuController mainMenuController;
    public CustomTooltip customTooltip;
    private UnitInfo unitInfo;
    private void Start()
    {
        int randomID = BaseUtils.RandomInt(-10000, 10000);
        ScriptableUnit scriptableUnit = BaseUtils.unitDict[unitType];
        if (customTooltip != null)
        {
            customTooltip.tooltipText[0] = scriptableUnit.unitType.ToString();
        }
        buttonImage.sprite = BaseUtils.tierSlots[scriptableUnit.unitTier-1];
        unitInfo = new UnitInfo(unitType, BaseUtils.GetUnitColorInfo(randomID), randomID, scriptableUnit.speed, scriptableUnit.damage, scriptableUnit.maxHealth, 0);
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
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.material = BaseUtils.highlightUI;
        mainMenuController.ShowTooltip(unitImage.rectTransform, unitInfo, unitInfo.health, false, false, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.material = BaseUtils.normalUI;
        mainMenuController.HideTooltip();
    }
}
