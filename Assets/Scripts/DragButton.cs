using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public Image buttonImage;
    public UnitCell unitCell;
    public Image edgeImage;
    public Image unitImage;
    [HideInInspector]
    public MainMenuController mainMenuController;
    public static UnitCell hoveringCell;
    [HideInInspector]
    public bool startedDragging;
    [HideInInspector]
    public Vector3 dragStartPos;
    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        mainMenuController = FindObjectOfType<MainMenuController>();
    }
    private void Update()
    {
        if (startedDragging && Vector3.Distance(Input.mousePosition, dragStartPos) > .3f)
        {
            mainMenuController.BeginDrag(unitCell);
            startedDragging = false;
        }
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        buttonImage.material = BaseUtils.highlightUI;
        if (unitCell.unitInfo.price == 0 || unitCell.marketCell)
        {
            unitImage.color = BaseUtils.healingColor;
            unitImage.material.SetFloat("_UseOutline", 0);
        }
        startedDragging = false;
        mainMenuController.EndDrag();
        if (!unitCell.emptyCell)
        {
            mainMenuController.SwitchUnits(unitCell, null);
        }
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        mainMenuController.HideTooltip();
        buttonImage.material = BaseUtils.normalUI;
        if (unitCell.unitInfo.price == 0 || unitCell.marketCell)
        {
            unitImage.color = Color.white;
            unitImage.material.SetFloat("_UseOutline", 0);
        }
        if (unitCell.emptyCell)
        {
            return;
        }
        startedDragging = true;
        dragStartPos = Input.mousePosition;
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        hoveringCell = unitCell;
        if (mainMenuController.showingPreview)
        {
            if (edgeImage != null)
            {
                edgeImage.material = BaseUtils.highLineUI;
            }
            buttonImage.material = BaseUtils.highLineUI;
            if (unitCell.unitInfo.price == 0 || unitCell.marketCell)
            {
                unitImage.color = BaseUtils.healingColor;
                unitImage.material.SetFloat("_UseOutline", 1);
                unitImage.material.SetColor("_OutlineColor", Color.white);
            }
        }
        else
        {
            buttonImage.material = BaseUtils.highlightUI;
            if (unitCell.unitInfo.price == 0 || unitCell.marketCell)
            {
                unitImage.color = BaseUtils.healingColor;
                unitImage.material.SetFloat("_UseOutline", 0);
            }
            if (!unitCell.emptyCell)
            {
                mainMenuController.ShowTooltip(buttonImage.rectTransform, unitCell.unitInfo, unitCell.unitInfo.health, false);
            }
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hoveringCell = null;
        if (edgeImage != null)
        {
            edgeImage.material = BaseUtils.normalUI;
        }
        buttonImage.material = BaseUtils.normalUI;
        if (unitCell.unitInfo.price == 0 || unitCell.marketCell)
        {
            unitImage.color = Color.white;
            unitImage.material.SetFloat("_UseOutline", 0);
        }
        mainMenuController.HideTooltip();
    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (unitCell.emptyCell)
        {
            return;
        }
        if (hoveringCell != null)
        {
            mainMenuController.SwitchUnits(unitCell, hoveringCell);
        }
        hoveringCell = null;
        mainMenuController.EndDrag();
    }
    private void OnDisable()
    {
        buttonImage.material = BaseUtils.normalUI;
        if (unitCell.unitInfo.price == 0 || unitCell.marketCell)
        {
            unitImage.color = Color.white;
            unitImage.material.SetFloat("_UseOutline", 0);
        }
        if (edgeImage != null)
        {
            edgeImage.material = BaseUtils.normalUI;
        }
    }
}
