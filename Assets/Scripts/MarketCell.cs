using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;

public class MarketCell : EnhancedScrollerCellView
{
    public int DataIndex { get; private set; }
    private MarketController marketController;
    private MarketData marketData;
    public CustomText powerText;
    public CustomText speedText;
    public CustomText healthText;
    public CustomText unitName;
    public CustomText priceText;
    public Image unitImage;
    public GameObject buyButton;
    public GameObject yoursButton;
    public GameObject pivotObj;
    public UnitCell unitCell;
    public void SetData(int dataIndex, MarketController marketController, MarketData marketData)
    {
        DataIndex = dataIndex;
        if (marketData == null)
        {
            pivotObj.SetActive(false);
            return;
        }
        pivotObj.SetActive(true);
        this.marketController = marketController;
        this.marketData = marketData;
        if (marketData.owner == Database.databaseStruct.playerAccount)
        {
            buyButton.SetActive(false);
            yoursButton.SetActive(true);
        }
        else
        {
            buyButton.SetActive(true);
            yoursButton.SetActive(false);
        }
        powerText.SetString($"power: {marketData.unitInfo.damage}");
        speedText.SetString($"speed: {marketData.unitInfo.speed}");
        healthText.SetString($"health: {marketData.unitInfo.health}");
        unitName.SetString(BaseUtils.GetUnitName(marketData.unitInfo));
        priceText.SetString($"{marketData.unitInfo.price} PXT");
        unitCell.SetData(marketController.mainMenuController, marketData.unitInfo);
        unitImage.sprite = unitCell.unitImage.sprite;
        unitImage.material = unitCell.unitImage.material;
    }
    public void OnBuyClick()
    {
        marketController.OnBuyClick(this, marketData);
    }
}
