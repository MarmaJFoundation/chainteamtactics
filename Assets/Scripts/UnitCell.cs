using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitCell : EnhancedScrollerCellView
{
    public int DataIndex { get; private set; }
    //public CustomText unitName;
    public Image unitImage;
    public Image edgeImage;
    public GameObject selectObj;
    public CustomTooltip customTooltip;
    public UnitInfo unitInfo;
    [HideInInspector]
    public bool emptyCell;
    [HideInInspector]
    public int visualIndex;
    public bool marketCell;
    private bool isPurple;
    private MainMenuController mainMenuController;
    private BattleRosterController battleRosterController;
    public void SetData(UnitCellData unitCellData)
    {
        visualIndex = unitCellData.visualIndex;
        selectObj.SetActive(false);
        isPurple = false;
        if (unitCellData.databaseIndex == -1)
        {
            emptyCell = true;
            unitImage.sprite = BaseUtils.emptyUnit;
            edgeImage.sprite = BaseUtils.tierSlots[1];
            unitImage.material = new Material(unitImage.material);
            unitImage.color = Color.white;
        }
        else
        {
            emptyCell = false;
            unitInfo = Database.databaseStruct.ownedUnits[unitCellData.databaseIndex];
            SetUnitVisual();
        }
    }
    public void SetData(BattleRosterController battleRosterController, MainMenuController mainMenuController, int dataIndex, UnitCellData unitCellData)
    {
        this.dataIndex = dataIndex;
        this.mainMenuController = mainMenuController;
        this.battleRosterController = battleRosterController;
        visualIndex = unitCellData.visualIndex;
        //unitName.SetString(unitInfo.unitType.ToString());
        unitInfo = Database.databaseStruct.ownedUnits[unitCellData.databaseIndex];
        isPurple = battleRosterController.placingPurple;
        SetUnitVisual();
        selectObj.SetActive(battleRosterController.selectUnit == unitInfo.unitID);
    }
    public void SetData(MainMenuController mainMenuController, UnitInfo unitInfo)
    {
        this.unitInfo = unitInfo;
        this.mainMenuController = mainMenuController;
        if (unitInfo.unitType == UnitType.None)
        {
            return;
        }
        SetUnitVisual();
    }
    private void SetUnitVisual()
    {
        ScriptableUnit scriptableUnit = BaseUtils.unitDict[unitInfo.unitType];
        if (customTooltip != null)
        {
            customTooltip.tooltipText[0] = scriptableUnit.unitType.ToString();
        }
        edgeImage.sprite = BaseUtils.tierSlots[scriptableUnit.unitTier - 1];
        unitImage.sprite = scriptableUnit.defaultImage;
        unitImage.transform.localScale = isPurple ? new Vector3(-2, 2, 2) : Vector3.one * 2;
        Material goMaterial = new Material(isPurple ? BaseUtils.purpleMaterialUI : BaseUtils.yellowMaterialUI);
        goMaterial.SetColor("_HairColor", unitInfo.unitColorInfo.hairColor);
        goMaterial.SetColor("_SkinColor", unitInfo.unitColorInfo.skinColor);
        goMaterial.SetColor("_RarityOneColor", unitInfo.unitColorInfo.rarityColors[0]);
        goMaterial.SetColor("_RarityTwoColor", unitInfo.unitColorInfo.rarityColors[1]);
        goMaterial.SetColor("_RarityThreeColor", unitInfo.unitColorInfo.rarityColors[2]);
        goMaterial.SetColor("_RarityFourColor", unitInfo.unitColorInfo.rarityColors[3]);
        goMaterial.SetFloat("_HasRarity", unitInfo.unitColorInfo.hasRarity ? 1f : 0f);
        unitImage.material = goMaterial;
        unitImage.material.SetFloat("_UseOutline", (unitInfo.price != 0 && !marketCell) ? 1 : 0);
        unitImage.color = (unitInfo.price != 0 && !marketCell) ? BaseUtils.sellingColor : Color.white;
    }
    public void OnSelect()
    {
        battleRosterController.OnSelectUnit(unitInfo, unitImage);
    }
    public void OnHover()
    {
        mainMenuController.ShowTooltip(unitImage.rectTransform, unitInfo, unitInfo.health, isPurple, false, false, marketCell);
    }
    public void OnExit()
    {
        mainMenuController.HideTooltip();
    }
}
