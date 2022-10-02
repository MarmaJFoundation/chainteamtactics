using UnityEngine;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DropdownController : MonoBehaviour, IEnhancedScrollerDelegate
{
    private SmallList<DropdownData> elementData;
    public EnhancedScroller scroller;
    public EnhancedScrollerCellView cellViewPrefab;
    public MainMenuController mainMenuController;
    public string prefix;
    public string[] elements;
    public CustomInput customInput;
    public void Setup()
    {
        gameObject.SetActive(true);
        scroller.Delegate = this;
        elementData = new SmallList<DropdownData>();
        for (int i = 0; i < elements.Length; i++)
        {
            elementData.Add(new DropdownData() { elementString = elements[i] });
        }
        scroller.ReloadData();
    }
    public void OnDropButtonClick()
    {
        if (gameObject.activeSelf)
        {
            customInput.ResetInput(true);
            gameObject.SetActive(false);
            return;
        }
        Setup();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool casted = mainMenuController.GetCanvasCasts(out List<RaycastResult> raycastResults);
            if (casted && !raycastResults[0].gameObject.name.Contains(prefix) && !raycastResults[0].gameObject.name.Contains("char"))
            {
                Dispose();
            }
        }
    }
    public void Dispose()
    {
        gameObject.SetActive(false);
    }
    public void OnDropdownClick(string creatureName)
    {
        customInput.SetInputText(creatureName);
        gameObject.SetActive(false);
    }
    #region EnhancedScroller Callbacks
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return elementData.Count;
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 12f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        DropdownCell cellView = scroller.GetCellView(cellViewPrefab) as DropdownCell;
        cellView.name = prefix;
        cellView.SetData(this, dataIndex, elementData[dataIndex]);
        return cellView;
    }
    #endregion
}
public class DropdownData
{
    public string elementString;
}