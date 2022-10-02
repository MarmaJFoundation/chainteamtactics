using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;

public class MarketRow : EnhancedScrollerCellView
{
    public MarketCell[] rowCellViews;

    public void SetData(ref SmallList<MarketData> data, MarketController marketController, int startingIndex)
    {
        // loop through the sub cells to display their data (or disable them if they are outside the bounds of the data)
        for (var i = 0; i < rowCellViews.Length; i++)
        {
            var dataIndex = startingIndex + i;

            // if the sub cell is outside the bounds of the data, we pass null to the sub cell
            rowCellViews[i].SetData(dataIndex, marketController, dataIndex < data.Count ? data[dataIndex] : null);
        }
    }
}
