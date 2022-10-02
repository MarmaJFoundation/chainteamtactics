using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class RosterButton : CustomButton
{
    public UnityEvent enterEvent;
    public UnityEvent exitEvent;
    private UnitController unitController;
    private MainMenuController mainMenuController;
    public void Setup(MainMenuController mainMenuController, UnitController unitController)
    {
        this.unitController = unitController;
        this.mainMenuController = mainMenuController;
    }
    public void OnBattleHover()
    {
        mainMenuController.ShowTooltip(buttonImage.rectTransform, unitController.unitInfo, unitController.currentHealth, unitController.isPurple);
    }
    public void OnBattleExit()
    {
        mainMenuController.HideTooltip();
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (deactivated)
        {
            buttonImage.material = BaseUtils.graylightUI;
        }
        else
        {
            buttonImage.material = OutlineMaterial() ? BaseUtils.highLineUI : BaseUtils.highlightUI;
        }
        enterEvent.Invoke();
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (deactivated)
        {
            buttonImage.material = BaseUtils.grayscaleUI;
        }
        else
        {
            buttonImage.material = OutlineMaterial() ? BaseUtils.outlineUI : BaseUtils.normalUI;
        }
        exitEvent.Invoke();
    }
}
