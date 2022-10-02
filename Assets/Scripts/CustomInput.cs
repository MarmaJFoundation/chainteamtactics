using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomInput : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public EventSystem eventSystem;
    public CustomText inputText;
    public DropdownController dropdownController;
    public Color holderColor;
    public Image pulseChar;
    public UnityEvent<string> unityEvent;
    public UnityEvent enterEvent;
    public string holderString;
    public bool numbersOnly;
    [HideInInspector]
    public string typeString;
    private bool listening;
    private bool pressedKey;
    private KeyCode lastKey;
    private float pressTime;
    private float pressInterval;
    private float pulseTimer;
    [HideInInspector]
    public Image buttonImage;

    public static bool hoveringDropdownElement;

    public void Setup(bool focus = false)
    {
        buttonImage = GetComponent<Image>();
        inputText.SetString(holderString, holderColor);
        pulseChar.gameObject.SetActive(false);
        if (focus)
        {
            OnPointerClick(null);
            eventSystem.SetSelectedGameObject(gameObject);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        inputText.SetString("", Color.white);
        pulseChar.gameObject.SetActive(false);
        typeString = "";
        listening = true;
        pressTime = 0;
        pressInterval = .25f;
        eventSystem.SetSelectedGameObject(gameObject);
        buttonImage.material = BaseUtils.highlightUI;
        if (dropdownController != null)
        {
            dropdownController.Setup();
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.material = BaseUtils.normalUI;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.material = BaseUtils.highlightUI;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.material = BaseUtils.normalUI;
    }
    public void ResetInput(bool fromButton = false, bool focus = false)
    {
        inputText.SetString(holderString, holderColor);
        pulseChar.gameObject.SetActive(false);
        listening = false;
        eventSystem.SetSelectedGameObject(null);
        if (unityEvent != null)
        {
            unityEvent.Invoke("");
        }
        if (fromButton && dropdownController != null)
        {
            dropdownController.Dispose();
        }
        if (focus)
        {
            OnPointerClick(null);
            eventSystem.SetSelectedGameObject(gameObject);
        }
    }
    private void Update()
    {
        if (!listening)
        {
            pulseChar.gameObject.SetActive(false);
            pulseTimer = 0;
            return;
        }
        if (pulseTimer <= .8f)
        {
            pulseTimer += Time.deltaTime;
        }
        else
        {
            pulseChar.gameObject.SetActive(!pulseChar.gameObject.activeSelf);
            pulseTimer = 0;
            UpdatePulseChar();
        }
        pressedKey = false;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                typeString += GUIUtility.systemCopyBuffer;
                inputText.SetString(typeString, Color.white);
                UpdatePulseChar();
            }
        }
        else if (eventSystem.currentSelectedGameObject == gameObject)
        {
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Array.IndexOf(BaseUtils.modifierKeys, kcode) == -1)
                {
                    if (Input.GetKey(kcode))
                    {
                        if (kcode != lastKey)
                        {
                            pressTime = 0;
                        }
                        lastKey = kcode;
                        if (pressTime < pressInterval)
                        {
                            pressTime += Time.deltaTime;
                            pressedKey = true;
                        }
                        else
                        {
                            pressInterval = .1f;
                            pressTime = 0;
                            OnPressKey(kcode);
                        }
                    }
                    if (Input.GetKeyDown(kcode))
                    {
                        OnPressKey(kcode);
                    }
                }
                /*else if (kcode == KeyCode.CapsLock)
                {
                    Extensions.capsLockOn = !Extensions.capsLockOn;
                }*/
            }
        }
        else
        {
            listening = false;
            if (typeString == "")
            {
                ResetInput();
            }
            if (unityEvent != null)
            {
                unityEvent.Invoke(typeString);
            }
        }
        if (!pressedKey)
        {
            pressTime = 0;
            pressInterval = .25f;
        }
    }

    private void UpdatePulseChar()
    {
        pulseChar.color = inputText.lastColor;
        pulseChar.transform.SetAsLastSibling();
    }

    public void OnPressKey(KeyCode kcode)
    {
        if (kcode == KeyCode.KeypadEnter || kcode == KeyCode.Escape || kcode == KeyCode.Return)
        {
            if (enterEvent != null && (kcode == KeyCode.KeypadEnter || kcode == KeyCode.Return))
            {
                enterEvent.Invoke();
            }
            listening = false;
            if (typeString == "")
            {
                ResetInput();
            }
        }
        else
        {
            if (kcode == KeyCode.Delete)
            {
                if (typeString.Length > 0)
                {
                    typeString = typeString.Remove(0, 1);
                    inputText.SetString(typeString, Color.white);
                    UpdatePulseChar();
                }
            }
            else if (kcode == KeyCode.Backspace)
            {
                if (typeString.Length > 0)
                {
                    typeString = typeString.Remove(typeString.Length - 1);
                    inputText.SetString(typeString, Color.white);
                    UpdatePulseChar();
                }
            }
            else
            {
                char addChar = kcode.ToCorrectChar();
                if (!numbersOnly || int.TryParse(addChar.ToString(), out _))
                {
                    typeString += kcode.ToCorrectChar();
                    inputText.SetString(typeString, Color.white);
                    UpdatePulseChar();
                }
            }
            if (dropdownController != null)
            {
                dropdownController.Dispose();
            }
        }
        if (unityEvent != null)
        {
            unityEvent.Invoke(typeString);
        }
        pressedKey = true;
    }
    public void SetInputText(string text)
    {
        typeString = text;
        inputText.SetString(typeString, Color.white);
        UpdatePulseChar();
        if (unityEvent != null)
        {
            unityEvent.Invoke(typeString);
        }
    }
}
