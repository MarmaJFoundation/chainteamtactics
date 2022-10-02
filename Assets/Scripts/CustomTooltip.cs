using UnityEngine;
using UnityEngine.EventSystems;

public enum TooltipPosition
{
    BottomCenter = 0,
    BottomRight = 1,
    TopCenter = 2,
    TopRight = 3,
    TopLeft = 4,
    BottomLRight = 5,
    TopLRight = 6,
    Right = 7,
    BottomLeft = 8
}
public class CustomTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private MainMenuController mainMenuController;
    private RectTransform rectTransform;
    public TooltipPosition position;
    public string[] tooltipText;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainMenuController = FindObjectOfType<MainMenuController>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mainMenuController.ShowTextTooltip(rectTransform, tooltipText, position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mainMenuController.HideTextTooltip();
    }
}
