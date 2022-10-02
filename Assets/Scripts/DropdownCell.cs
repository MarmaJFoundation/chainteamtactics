using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EnhancedUI.EnhancedScroller;

public class DropdownCell : EnhancedScrollerCellView, IPointerEnterHandler, IPointerExitHandler
{
    public CustomText dropdownText;
    public int DataIndex { get; private set; }
    private DropdownData data;
    private DropdownController dropdownController;
    public void SetData(DropdownController dropdownController, int dataIndex, DropdownData data)
    {
        this.dropdownController = dropdownController;
        this.data = data;
        DataIndex = dataIndex;
        dropdownText.SetString(data.elementString);
    }
    public void OnClick()
    {
        dropdownController.OnDropdownClick(data.elementString);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CustomInput.hoveringDropdownElement = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CustomInput.hoveringDropdownElement = false;
    }
}
