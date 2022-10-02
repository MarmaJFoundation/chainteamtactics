using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CustomText : MonoBehaviour
{
    public string stringInput = "";
    public bool smallLetters;
    public bool outline;
    public Color textColor = Color.white;
    [HideInInspector]
    public Color lastColor = Color.white;
    [HideInInspector]
    public string lastString = "";

    private BaseUtils baseUtils;

    private void Start()
    {
        if (!Application.isPlaying)
        {
            baseUtils = FindObjectOfType<BaseUtils>();
            baseUtils.SetLetterDicts();
        }
        UpdateText();
    }
    public void SetString(Color goColor)
    {
        if (textColor == goColor)
        {
            return;
        }
        textColor = goColor;
        UpdateText();
    }
    public void SetString(string gotoString)
    {
        if (stringInput == gotoString)
        {
            return;
        }
        stringInput = gotoString;
        UpdateText();
    }
    public void SetString(string gotoString, bool outline)
    {
        if (stringInput == gotoString)
        {
            return;
        }
        this.outline = outline;
        stringInput = gotoString;
        UpdateText();
    }
    public void SetString(string gotoString, Color goColor)
    {
        if (stringInput == gotoString && textColor == goColor)
        {
            return;
        }
        stringInput = gotoString;
        textColor = goColor;
        UpdateText();
    }
    private void Update()
    {
        if (!Application.isPlaying && stringInput != "" && (stringInput != lastString || textColor != lastColor))
        {
            UpdateText();
        }
    }
    public void UpdateText()
    {
        if (baseUtils == null)
        {
            baseUtils = FindObjectOfType<BaseUtils>();
        }
        if (BaseUtils.smallLetterDict.Count == 0)
        {
            baseUtils.SetLetterDicts();
        }
        lastString = stringInput;
        lastColor = textColor;
        Image[] allChars = GetComponentsInChildren<Image>();
        for (int i = allChars.Length - 1; i >= 0; i--)
        {
            if (allChars[i].name.Contains("char"))
            {
                if (Application.isPlaying)
                {
                    Destroy(allChars[i].gameObject);
                }
                else
                {
                    DestroyImmediate(allChars[i].gameObject);
                }
            }
        }
        if (smallLetters)
        {
            AddCharImage(BaseUtils.smallLetterDict, stringInput.ToLower());
        }
        else
        {
            AddCharImage(BaseUtils.mediumLetterDict, stringInput.ToLower());
        }
    }

    private void AddCharImage(Dictionary<char, Sprite> letterDict, string finalString)
    {
        for (int i = 0; i < finalString.Length; i++)
        {
            char gotoChar = letterDict.ContainsKey(finalString[i]) ? finalString[i] : '?';
            Image letterImage;
            GameObject letterObj = new GameObject(gotoChar + "char");
            letterObj.transform.SetParent(transform);
            letterObj.transform.localScale = Vector3.one;
            letterObj.transform.localPosition = Vector3.zero;
            letterObj.transform.localRotation = Quaternion.identity;
            letterImage = letterObj.AddComponent<Image>();
            letterImage.sprite = letterDict[gotoChar];
            letterImage.raycastTarget = false;
            letterImage.material = outline ? baseUtils._outlineUI : baseUtils._normalUI;
            letterImage.color = textColor;
            letterImage.SetNativeSize();
        }
    }
}
