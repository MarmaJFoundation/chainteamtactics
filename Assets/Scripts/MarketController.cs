using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;

public class MarketController : MonoBehaviour, IEnhancedScrollerDelegate
{
    private SmallList<MarketData> elementData = new SmallList<MarketData>();
    public NearHelper nearHelper;
    public MainMenuController mainMenuController;
    public UnitBuyAnimator unitBuyAnimator;
    public EnhancedScroller scroller;
    public EnhancedScrollerCellView cellViewPrefab;
    public int numberOfCellsPerRow = 3;
    public GameObject marketWindow;
    public GameObject[] sortButtons;
    public CustomInput classInput;
    public CustomInput sortInput;
    private string sortString;
    private string classString;
    private bool sortingUpwards;
    private MarketCell marketCell;
    private MarketData marketData;
    public void Setup()
    {
        marketWindow.SetActive(true);
        scroller.Delegate = this;
        elementData.Clear();
        classInput.Setup(false);
        sortInput.Setup(false);
        sortString = "";
    }
    public void Dispose()
    {
        marketWindow.SetActive(false);
    }
    public void OnSearchClick()
    {
        if (BaseUtils.offlineMode)
        {
            for (int i = 0; i < 100; i++)
            {
                elementData.Add(new MarketData() { owner = "offlineplayer" + i, unitInfo = BaseUtils.GenerateRandomUnit(BaseUtils.RandomInt(1, 2000)) });
            }
            scroller.ReloadData();
        }
        else
        {
            if (classString != null && classString.Length > 0)
            {
                string firstLetter = classString[0].ToString();
                classString = firstLetter.ToUpper() + classString.Substring(1);
                if (!System.Enum.TryParse(classString, out UnitType unitType))
                {
                    unitType = UnitType.None;
                }
                StartCoroutine(nearHelper.RequestMarketData(unitType));
            }
            else
            {
                StartCoroutine(nearHelper.RequestMarketData(UnitType.None));
            }
        }
    }
    public void OnReceiveUnits(List<MarketWrapper> unitWrappers)
    {
        elementData.Clear();
        for (int i = 0; i < unitWrappers.Count; i++)
        {
            elementData.Add(new MarketData()
            {
                owner = unitWrappers[i].unit_data.owner,
                unitInfo = new UnitInfo(unitWrappers[i].unit_data)
            });
        }
        SortData();
    }

    private void SortData()
    {
        List<MarketData> listToSort = new List<MarketData>();
        for (int i = 0; i < elementData.Count; i++)
        {
            listToSort.Add(elementData[i]);
        }
        if (sortString.Length > 0)
        {
            if ("price".Contains(sortString) || sortString.Contains("price"))
            {
                if (sortingUpwards)
                {
                    listToSort.Sort((x, y) => x.unitInfo.price.CompareTo(y.unitInfo.price));
                }
                else
                {
                    listToSort.Sort((x, y) => y.unitInfo.price.CompareTo(x.unitInfo.price));
                }
            }
            else if ("stats".Contains(sortString) || sortString.Contains("stats"))
            {
                if (sortingUpwards)
                {
                    listToSort.Sort((x, y) => x.Stats.CompareTo(y.Stats));
                }
                else
                {
                    listToSort.Sort((x, y) => y.Stats.CompareTo(x.Stats));
                }
            }
            else if ("rarity".Contains(sortString) || sortString.Contains("rarity"))
            {
                if (sortingUpwards)
                {
                    listToSort.Sort((x, y) => x.Rarity.CompareTo(y.Rarity));
                }
                else
                {
                    listToSort.Sort((x, y) => y.Rarity.CompareTo(x.Rarity));
                }
            }
        }
        elementData.Clear();
        for (int i = 0; i < listToSort.Count; i++)
        {
            elementData.Add(listToSort[i]);
        }
        scroller.ReloadData();
    }

    public void OnTypeClass(string inputText)
    {
        if (classString == inputText)
        {
            return;
        }
        classString = inputText;
    }
    public void OnTypeSort(string inputText)
    {
        if (sortString == inputText)
        {
            return;
        }
        sortString = inputText;
        SortData();
    }
    public void OnSortUpwardsClick()
    {
        sortingUpwards = !sortingUpwards;
        sortButtons[0].SetActive(sortingUpwards);
        sortButtons[1].SetActive(!sortingUpwards);
        SortData();
    }
    public void OnBuyClick(MarketCell marketCell, MarketData marketData)
    {
        if (Database.databaseStruct.pixelTokens < marketData.unitInfo.price)
        {
            BaseUtils.ShowWarningMessage("out of balance", new string[2] { "you do not have enough pixel tokens for this purchase", "would you like to acquire more balance?" }, OnAcceptBalance);
            return;
        }
        if (Database.databaseStruct.ownedUnits.Count >= Database.maxUnits)
        {
            BaseUtils.ShowWarningMessage("arsenal full", new string[2] { "you cannot purchase any more units", "please sell or fire one of your units" });
            return;
        }
        this.marketCell = marketCell;
        this.marketData = marketData;
        BaseUtils.ShowWarningMessage("buying unit", new string[4] { $"would you like to purchase", $"{BaseUtils.GetUnitName(marketData.unitInfo)}", $"for {marketData.unitInfo.price} pxt?", "this purchase cannot be refunded!" }, marketCell.unitImage, marketCell.unitCell.edgeImage, OnAcceptPurchase);
    }
    private void OnAcceptPurchase()
    {
        if (BaseUtils.offlineMode)
        {
            Database.databaseStruct.pixelTokens -= marketData.unitInfo.price;
            Database.AddUnit(marketData.unitInfo);
            Database.SetPrice(marketData.unitInfo.unitID, 0);
            mainMenuController.Setup(true);
            unitBuyAnimator.Setup(marketData.unitInfo);
        }
        else
        {
            StartCoroutine(nearHelper.RequestBuyUnit(marketData.unitInfo.unitID));
        }
    }
    public void ShowBuyAuthorize()
    {
        mainMenuController.authWindow.SetActive(true);
        nearHelper.dataGetState = DataGetState.MarketPurchase;
    }
    public void OnAuthorizedClick()
    {
        mainMenuController.authWindow.SetActive(false);
        StartCoroutine(nearHelper.GetPlayerData());
    }
    public void OnReceiveNewPlayerData()
    {
        BaseUtils.HideLoading();
        int itemIndex = -1;
        for (int i = 0; i < Database.databaseStruct.ownedUnits.Count; i++)
        {
            if (Database.databaseStruct.ownedUnits[i].unitID == marketData.unitInfo.unitID)
            {
                itemIndex = i;
                break;
            }
        }
        if (itemIndex != -1)
        {
            unitBuyAnimator.Setup(marketData.unitInfo);
        }
        else
        {
            BaseUtils.ShowWarningMessage($"Error on purchase", new string[2] { $"The purchase for the unit {marketData.unitInfo.unitID} failed.", $"Please try again!" });
        }
        mainMenuController.Setup(true);
        Database.SaveDatabase();
    }
    private void OnAcceptBalance()
    {
        Application.OpenURL("https://app.ref.finance/#wrap.near%7Cpixeltoken.near");
    }
    #region EnhancedScroller Handlers
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return Mathf.CeilToInt((float)elementData.Count / (float)numberOfCellsPerRow);
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 70f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        MarketRow cellView = scroller.GetCellView(cellViewPrefab) as MarketRow;
        var di = dataIndex * numberOfCellsPerRow;
        cellView.SetData(ref elementData, this, di);
        return cellView;
    }

    #endregion
}
public class MarketData
{
    public string owner;
    public UnitInfo unitInfo;

    public int Stats
    {
        get
        {
            return unitInfo.speed + unitInfo.health + unitInfo.damage;
        }
    }
    public int Rarity
    {
        get
        {
            return BaseUtils.unitDict[unitInfo.unitType].unitTier;
        }
    }
}
