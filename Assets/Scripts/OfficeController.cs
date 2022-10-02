using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfficeController : MonoBehaviour
{
    public NearHelper nearHelper;
    public MainMenuController mainMenuController;
    public UnitBuyAnimator unitBuyAnimator;
    public GameObject officeWindow;
    public Image unitImage;
    public CustomText supplyText;
    private UnitInfo hiredUnit;
    public void Setup()
    {
        officeWindow.SetActive(true);
        int currentSupply = 200;
        supplyText.SetString($"supply: {currentSupply}/cost: {200}PXT");
        unitImage.sprite = BaseUtils.unitDict[(UnitType)BaseUtils.RandomInt(1, 16)].defaultImage;
    }
    public void Dispose()
    {
        officeWindow.SetActive(false);
    }
    public void OnPurchaseClick()
    {
        if (Database.databaseStruct.pixelTokens < 200)
        {
            BaseUtils.ShowWarningMessage("out of balance", new string[2] { "you do not have enough pixel tokens for this purchase", "would you like to acquire more balance?" }, OnAcceptBalance);
            return;
        }
        if (Database.databaseStruct.ownedUnits.Count >= Database.maxUnits)
        {
            BaseUtils.ShowWarningMessage("arsenal full", new string[2] { "you cannot purchase any more units", "please sell or fire one of your units" });
            return;
        }
        BaseUtils.ShowWarningMessage("buying unit", new string[2] { "would you like to purchase an unit for 200 pxt?", "be aware that the unit will be completely random!" }, OnAcceptPurchase);
    }
    private void OnAcceptBalance()
    {
        Application.OpenURL("https://app.ref.finance/#wrap.near%7Cpixeltoken.near");
    }
    private void OnAcceptPurchase()
    {
        if (BaseUtils.offlineMode)
        {
            StartCoroutine(WaitAndShowPurchase());
        }
        else
        {
            StartCoroutine(nearHelper.RequestHireUnit());
        }
    }
    private IEnumerator WaitAndShowPurchase()
    {
        BaseUtils.ShowLoading();
        yield return new WaitForSeconds(.5f);
        BaseUtils.HideLoading();
        Database.databaseStruct.pixelTokens -= 200;
        UnitInfo unitInfo = BaseUtils.GenerateRandomUnit();
        Database.AddUnit(unitInfo);
        BaseUtils.ShowWarningMessage("unit bought!", new string[2] { $"you hired {BaseUtils.GetUnitName(unitInfo)}", "it is now part of your party." }, unitInfo);
        unitBuyAnimator.Setup(unitInfo);
    }
    public void OnReceiveNewPlayerData()
    {
        unitBuyAnimator.Setup(hiredUnit);
        mainMenuController.Setup(true);
    }
    public void OnHireCallBack(UnitToken unitToken)
    {
        hiredUnit = new UnitInfo(unitToken);
        nearHelper.dataGetState = DataGetState.AfterHire;
        StartCoroutine(nearHelper.GetPlayerData());
    }
}
