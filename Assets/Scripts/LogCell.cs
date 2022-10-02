using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LogCell : EnhancedScrollerCellView
{
    public int DataIndex { get; private set; }
    public CustomText customText;
    public void SetData(LogController logController, int dataIndex, string data)
    {
        this.dataIndex = dataIndex;
        customText.SetString(data, BaseUtils.grayColor);
    }
}
