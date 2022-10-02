using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogController : MonoBehaviour, IEnhancedScrollerDelegate
{
    private readonly SmallList<string> elementData = new SmallList<string>();
    public EnhancedScroller scroller;
    public EnhancedScrollerCellView cellViewPrefab;
    private readonly Queue<string> logsToAdd = new Queue<string>();
    public void Setup()
    {
        BaseUtils.battleKey = "";
        //scroller.Delegate = this;
    }
    private void Update()
    {
        if (logsToAdd.Count > 0)
        {
            elementData.Add(logsToAdd.Dequeue());
            scroller.ReloadData(scroller.NormalizedScrollPosition);
        }
    }
    public void AddLog(string text)
    {
        text = text.ToLower();
        if (text.Contains("purple"))
        {
            BaseUtils.battleKey += "p";
        }
        if (text.Contains("blue"))
        {
            BaseUtils.battleKey += "b";
        }
        if (text.Contains("damages"))
        {
            BaseUtils.battleKey += "d";
        }
        if (text.Contains("heals"))
        {
            BaseUtils.battleKey += "h";
        }
        if (text.Contains("dies"))
        {
            BaseUtils.battleKey += "z";
        }
        if (text.Contains("revives"))
        {
            BaseUtils.battleKey += "r";
        }
        if (text.Contains("exhumes"))
        {
            BaseUtils.battleKey += "x";
        }
        if (text.Contains("buffs"))
        {
            BaseUtils.battleKey += "f";
        }
        if (text.Contains("skeleton"))
        {
            BaseUtils.battleKey += "t";
        }
        if (text.Contains("wolf"))
        {
            BaseUtils.battleKey += "w";
        }
        if (text.Contains("necromancer"))
        {
            BaseUtils.battleKey += "n";
        }
        if (text.Contains("priest"))
        {
            BaseUtils.battleKey += "i";
        }
        if (text.Contains("squire"))
        {
            BaseUtils.battleKey += "q";
        }
        if (text.Contains("executioner"))
        {
            BaseUtils.battleKey += "c";
        }
        if (text.Contains("marksman"))
        {
            BaseUtils.battleKey += "m";
        }
        if (text.Contains("elementalist"))
        {
            BaseUtils.battleKey += "l";
        }
        if (text.Contains("bard"))
        {
            BaseUtils.battleKey += "a";
        }
        if (text.Contains("assassin"))
        {
            BaseUtils.battleKey += "s";
        }
        if (text.Contains("chemist"))
        {
            BaseUtils.battleKey += "e";
        }
        if (text.Contains("druid"))
        {
            BaseUtils.battleKey += "u";
        }
        if (text.Contains("knight"))
        {
            BaseUtils.battleKey += "k";
        }
        if (text.Contains("mage"))
        {
            BaseUtils.battleKey += "2";
        }
        if (text.Contains("paladin"))
        {
            BaseUtils.battleKey += "3";
        }
        if (text.Contains("timebender"))
        {
            BaseUtils.battleKey += "4";
        }
        if (text.Contains("warlock"))
        {
            BaseUtils.battleKey += "5";
        }
        //logsToAdd.Enqueue(text);
    }
    #region EnhancedScroller Callbacks
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return elementData.Count;
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 7f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        LogCell cellView = scroller.GetCellView(cellViewPrefab) as LogCell;
        cellView.SetData(this, dataIndex, elementData[dataIndex]);
        return cellView;
    }
    #endregion
}
