using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningController : MonoBehaviour
{
    public CustomText messageTitle;
    public CustomText[] messageLines;
    public GameObject buttonsObj;
    public GameObject imageObj;
    public Image unitImage;
    public Image borderImage;
    private Action OnAcceptCallback;
    public void Setup(string title, string[] message, Image borderImage, Image unitImage, Action OnAcceptCallback)
    {
        this.OnAcceptCallback = OnAcceptCallback;
        gameObject.SetActive(true);
        buttonsObj.SetActive(true);
        imageObj.SetActive(true);
        this.unitImage.sprite = unitImage.sprite;
        this.unitImage.material = unitImage.material;
        this.borderImage.sprite = borderImage.sprite;
        SetMessage(title, message);
    }
    public void Setup(string title, string[] message, Image borderImage, Image unitImage)
    {
        gameObject.SetActive(true);
        buttonsObj.SetActive(false);
        imageObj.SetActive(true);
        this.unitImage.sprite = unitImage.sprite;
        this.unitImage.material = unitImage.material;
        this.borderImage.sprite = borderImage.sprite;
        SetMessage(title, message);
    }
    public void Setup(string title, string[] message, Action OnAcceptCallback, bool removeNo)
    {
        this.OnAcceptCallback = OnAcceptCallback;
        gameObject.SetActive(true);
        buttonsObj.SetActive(true);
        buttonsObj.transform.GetChild(1).gameObject.SetActive(!removeNo);
        imageObj.SetActive(false);
        SetMessage(title, message);
    }
    public void Setup(string title, string[] message, UnitInfo unitInfo)
    {
        gameObject.SetActive(true);
        buttonsObj.SetActive(false);
        imageObj.SetActive(true);
        ScriptableUnit scriptableUnit = BaseUtils.unitDict[unitInfo.unitType];
        borderImage.sprite = BaseUtils.tierSlots[scriptableUnit.unitTier - 1];
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
        SetMessage(title, message);
    }
    public void Setup(string title, string[] message)
    {
        gameObject.SetActive(true);
        buttonsObj.SetActive(false);
        imageObj.SetActive(false);
        SetMessage(title, message);
    }
    private void SetMessage(string title, string[] message)
    {
        messageTitle.SetString(title);
        for (int i = 0; i < messageLines.Length; i++)
        {
            if (message.Length > i)
            {
                messageLines[i].gameObject.SetActive(true);
                messageLines[i].SetString(message[i]);
            }
            else
            {
                messageLines[i].gameObject.SetActive(false);
            }
        }
    }
    public void OnAcceptClick()
    {
        OnAcceptCallback.Invoke();
        gameObject.SetActive(false);
        BaseUtils.showingWarn = false;
    }
    public void OnRefuseClick()
    {
        gameObject.SetActive(false);
        BaseUtils.showingWarn = false;
    }
    public void OnExitClick()
    {
        gameObject.SetActive(false);
    }
}
