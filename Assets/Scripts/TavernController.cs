using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TavernController : MonoBehaviour
{
    public MainMenuController mainMenuController;
    public List<Transform> tavernTiles;
    public NearHelper nearHelper;
    public MapController mapController;
    public List<UnitCell> unitCells;
    public CustomText pageCounter;
    public PageButton[] pageButtons;
    public RectTransform[] cellPages;
    private int currentPage;
    private int hoveringButton;
    private float buttonTimer;
    private readonly List<UnitController> tavernUnits = new List<UnitController>();

    public CustomInput sellInput;
    public GameObject sellingWindow;
    public Image sellUnitBorder;
    public Image sellUnitImage;
    private int sellingPrice;
    private UnitInfo trashingUnit;
    private bool isTrashing;
    private UnitInfo sellingUnit;
    private bool isSelling;

    public void Setup(bool keepPage = false, bool updateUnits = true)
    {
        BaseUtils.onTavern = true;
        mapController.LoadMap(MapType.Tavern);
        mapController.placingUnits = true;
        mapController.gameLocked = false;
        if (updateUnits)
        {
            mapController.ClearUnits();
            tavernUnits.Clear();
            for (int i = 0; i < Database.maxUnits; i++)
            {
                int unitID = Database.GetUnitFromVisualIndex(i);
                int databaseIndex = Database.GetUnitFromID(unitID);
                if (i < 12 && databaseIndex != -1 && Database.databaseStruct.ownedUnits[databaseIndex].price == 0)
                {
                    int posX = Mathf.RoundToInt(tavernTiles[i].position.x + 14);
                    int posY = Mathf.RoundToInt(tavernTiles[i].position.z + 7);
                    tavernUnits.Add(mapController.PlaceUnit(MapController.nodeGrid[posX, posY], Database.databaseStruct.ownedUnits[databaseIndex], false));
                }
            }
        }
        for (int i = 0; i < tavernUnits.Count; i++)
        {
            foreach (Image image in tavernUnits[i].healthbarController.GetComponentsInChildren<Image>())
            {
                image.color = Color.clear;
            }
        }
        UpdateRoster();
        SetInnerPage(keepPage ? currentPage : 0);
    }
    public void Dispose()
    {
        for (int i = 0; i < mapController.mapObjs.Length; i++)
        {
            mapController.mapObjs[i].SetActive(false);
            mapController.backgroundObjs[i].SetActive(false);
        }
    }
    public void UpdateRoster()
    {
        for (int i = 0; i < Database.maxUnits; i++)
        {
            int unitID = Database.GetUnitFromVisualIndex(i);
            int databaseIndex = Database.GetUnitFromID(unitID);
            unitCells[i].SetData(new UnitCellData(databaseIndex, i));
        }
    }
    public void LateUpdate()
    {
        if (hoveringButton != 0)
        {
            buttonTimer += Time.deltaTime;
            if (buttonTimer > .5f)
            {
                SetPage(hoveringButton == 1);
                buttonTimer = 0;
                hoveringButton = 0;
            }
        }
        else
        {
            buttonTimer = 0;
        }
        if (isTrashing && !Input.GetMouseButton(0))
        {
            isTrashing = false;
            if (trashingUnit.price != 0)
            {
                BaseUtils.ShowWarningMessage("Error", new string[1] { "You cannot fire units that are currently being sold" });
            }
            else
            {
                BaseUtils.ShowWarningMessage("Wait!", new string[3] { "Are you sure you want to trash", $"{BaseUtils.GetUnitName(trashingUnit)}?", "You cannot revert this action!" }, mainMenuController.unitPreviewImage, mainMenuController.unitPreviewEdge, OnAcceptTrashUnit);
            }
        }
        if (isSelling && !Input.GetMouseButton(0))
        {
            isSelling = false;
            if (sellingUnit.price != 0)
            {
                BaseUtils.ShowWarningMessage("Stop Selling", new string[2] { $"Do you wish to stop selling {BaseUtils.GetUnitName(sellingUnit)}?", "It will be removed from the market." }, mainMenuController.unitPreviewImage, mainMenuController.unitPreviewEdge, OnAcceptStopSell);
            }
            else
            {
                sellingWindow.SetActive(true);
                sellInput.Setup(true);
                sellUnitBorder.sprite = mainMenuController.unitPreviewEdge.sprite;
                sellUnitImage.sprite = mainMenuController.unitPreviewImage.sprite;
                sellUnitImage.material = mainMenuController.unitPreviewImage.material;
            }
        }
    }
    private void OnAcceptStopSell()
    {
        if (BaseUtils.offlineMode)
        {
            OnCancelUnitSell();
        }
        else
        {
            StartCoroutine(nearHelper.RequestCancelUnitSell(sellingUnit.unitID));
        }
    }
    public void OnReceiveNewPlayerData()
    {
        BaseUtils.ShowWarningMessage("Unit Fired", new string[2] { $"You have fired {BaseUtils.GetUnitName(trashingUnit)}", "it is no longer in your party." }, mainMenuController.unitPreviewImage, mainMenuController.unitPreviewEdge);
        Setup(true);
    }
    public void OnCancelUnitSell()
    {
        BaseUtils.ShowWarningMessage("Removed from Market", new string[2] { $"You have removed {BaseUtils.GetUnitName(sellingUnit)}", "that was listed in the market." }, mainMenuController.unitPreviewImage, mainMenuController.unitPreviewEdge);
        Database.SetPrice(sellingUnit.unitID, 0);
        Setup(true);
    }
    public void OnTypeSellPrice(string input)
    {
        int.TryParse(input, out sellingPrice);
    }
    private void OnAcceptTrashUnit()
    {
        if (BaseUtils.offlineMode)
        {
            StartCoroutine(WaitAndShowFire());
        }
        else
        {
            StartCoroutine(nearHelper.RequestTrashUnit(trashingUnit.unitID));
        }
    }
    private IEnumerator WaitAndShowFire()
    {
        BaseUtils.ShowLoading();
        yield return new WaitForSeconds(.5f);
        BaseUtils.HideLoading();
        //OnTrashUnitCallback();
    }
    /*public void OnTrashUnitCallback()
    {
        BaseUtils.ShowWarningMessage("Unit Fired", new string[2] { $"You have fired {BaseUtils.GetUnitName(trashingUnit)}", "it is no longer in your party." }, mainMenuController.unitPreviewImage, mainMenuController.unitPreviewEdge);
        Database.RemoveUnit(trashingUnit.unitID);
        Setup(true);
    }*/
    public void OnSellExitClick()
    {
        sellingWindow.SetActive(false);
    }
    public void OnSellConfirmClick()
    {
        if (sellingPrice == 0 || sellingPrice > 100000)
        {
            BaseUtils.ShowWarningMessage("Error!", new string[1] { "Cannot sell your unit for this price." });
            return;
        }
        if (BaseUtils.offlineMode)
        {
            OnAcceptUnitSell();
        }
        else
        {
            StartCoroutine(nearHelper.RequestSellUnit(sellingUnit.unitID, sellingPrice));
        }
    }
    public void OnAcceptUnitSell()
    {
        sellingWindow.SetActive(false);
        BaseUtils.ShowWarningMessage("Selling Unit!", new string[2] { $"You have put {BaseUtils.GetUnitName(sellingUnit)} on the market", $"for a total amount of {sellingPrice} pxt" }, mainMenuController.unitPreviewImage, mainMenuController.unitPreviewEdge);
        Database.SetPrice(sellingUnit.unitID, sellingPrice);
        sellingPrice = 0;
        Setup(true);
    }
    public void OnTrashClick()
    {
        BaseUtils.ShowWarningMessage("Firing Units", new string[2] { "In order to fire an unit", "drag it into this button." });
    }
    public void OnSellClick()
    {
        BaseUtils.ShowWarningMessage("Sellings Units", new string[2] { "In order to sell an unit", "drag it into this button." });
    }
    public void SetPage(bool goingLeft)
    {
        SetInnerPage(goingLeft ? currentPage - 1 : currentPage + 1);
    }
    private void SetInnerPage(int pageNumber)
    {
        pageCounter.SetString($"{pageNumber + 1}/3");
        currentPage = pageNumber;
        for (int i = 0; i < cellPages.Length; i++)
        {
            if (i == currentPage)
            {
                cellPages[currentPage].anchoredPosition = Vector2.zero;
            }
            else
            {
                cellPages[i].anchoredPosition = Vector2.right * 10000f;
            }
        }
        pageButtons[0].SetDeactivated(currentPage == 0);
        pageButtons[1].SetDeactivated(currentPage == 2);
    }
    public void OnPageButtonEnter(bool left)
    {
        if (mainMenuController.showingPreview)
        {
            hoveringButton = left ? 1 : 2;
        }
    }
    public void OnPageButtonExit()
    {
        hoveringButton = 0;
    }
    public void OnTrashButtonEnter()
    {
        if (mainMenuController.showingPreview)
        {
            trashingUnit = mainMenuController.draggingUnit;
            isTrashing = true;
        }
    }
    public void OnTrashButtonExit()
    {
        isTrashing = false;
    }
    public void OnSellButtonEnter()
    {
        if (mainMenuController.showingPreview)
        {
            sellingUnit = mainMenuController.draggingUnit;
            isSelling = true;
        }
    }
    public void OnSellButtonExit()
    {
        isSelling = false;
    }
}
