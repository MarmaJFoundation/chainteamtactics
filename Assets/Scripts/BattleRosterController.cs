using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitCellData
{
    public int databaseIndex;
    public int visualIndex;

    public UnitCellData(int databaseIndex, int visualIndex)
    {
        this.databaseIndex = databaseIndex;
        this.visualIndex = visualIndex;
    }
}
public class BattleRosterController : MonoBehaviour, IEnhancedScrollerDelegate
{
    private readonly SmallList<UnitCellData> elementData = new SmallList<UnitCellData>();
    public MainMenuController mainMenuController;
    public EnhancedScroller scroller;
    public EnhancedScrollerCellView cellViewPrefab;
    public GameObject rosterObj;
    public GameObject doneObj;
    public GameObject staticBarPrefab;
    public Transform leftBars;
    public Transform rightBars;
    public CustomButton doneButton;
    public CustomText descriptionText;
    public Color lockedColor;
    public TooltipController tooltipController;
    [HideInInspector]
    public bool placingPurple;
    public readonly HashSet<int> placedUnits = new HashSet<int>();
    [HideInInspector]
    public int selectUnit;
    //private Image selectImage;
    public void Setup(bool placingPurple)
    {
        this.placingPurple = placingPurple;
        selectUnit = -1;
        placedUnits.Clear();
        scroller.Delegate = this;
        rosterObj.SetActive(true);
        doneObj.SetActive(false);
        descriptionText.SetString("0/6 units placed");
        ReloadList(false);
    }
    public void ShowUnitTooltip(RectTransform target, UnitInfo unitInfo, bool isPurple)
    {
        mainMenuController.ShowTooltip(target, unitInfo, unitInfo.health, isPurple);
    }
    public void HideUnitTooltip()
    {
        mainMenuController.HideTooltip();
    }
    public void OnFinishPlacement()
    {
        rosterObj.SetActive(false);
    }
    public void OnRemoveUnit(int unitID)
    {
        placedUnits.Remove(unitID);
        descriptionText.SetString($"{placedUnits.Count}/6 units placed", placedUnits.Count == 6 ? lockedColor : Color.white);
        doneObj.SetActive(placedUnits.Count == 6);
        ReloadList(true);
    }
    public void OnPlaceUnit(UnitController unitController)
    {
        placedUnits.Add(unitController.unitID);
        selectUnit = -1;
        descriptionText.SetString($"{placedUnits.Count}/6 units placed", placedUnits.Count == 6 ? lockedColor : Color.white);
        doneObj.SetActive(placedUnits.Count == 6);
        ReloadList(true);
    }
    private void ReloadList(bool keepPos)
    {
        elementData.Clear();
        for (int i = 0; i < Database.databaseStruct.ownedUnits.Count; i++)
        {

            int unitID = Database.GetUnitFromVisualIndex(i);
            int databaseIndex = Database.GetUnitFromID(unitID);
            if (placedUnits.Contains(Database.databaseStruct.ownedUnits[databaseIndex].unitID) || Database.databaseStruct.ownedUnits[databaseIndex].price != 0)
            {
                continue;
            }
            elementData.Add(new UnitCellData(databaseIndex, i));
        }
        scroller.ReloadData(keepPos ? scroller.NormalizedScrollPosition : 0);
    }
    public void OnSelectUnit(UnitInfo unitInfo, Image selectImage)
    {
        //this.selectImage = selectImage;
        selectUnit = unitInfo.unitID;
        scroller.ReloadData(scroller.NormalizedScrollPosition);
    }
    public UnitInfo GetSelectUnit()
    {
        for (int i = 0; i < Database.databaseStruct.ownedUnits.Count; i++)
        {
            if (Database.databaseStruct.ownedUnits[i].unitID == selectUnit)
            {
                return Database.databaseStruct.ownedUnits[i];
            }
        }
        return new UnitInfo();
    }
    #region EnhancedScroller Callbacks
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return elementData.Count;
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 24f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        UnitCell cellView = scroller.GetCellView(cellViewPrefab) as UnitCell;
        //cellView.name = "rankCell";
        cellView.SetData(this, mainMenuController, dataIndex, elementData[dataIndex]);
        return cellView;
    }
    #endregion
}
