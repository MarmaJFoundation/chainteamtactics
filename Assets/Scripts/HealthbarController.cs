using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarController : MonoBehaviour
{
    public RectTransform rectTransform;
    public CustomText hpText;
    public CustomText nameText;
    public Image unitImage;
    public Image hpBar;
    private Transform followTransform;
    private int maxHealth;
    private int currentHealth;
    public bool staticBar;
    public void Setup(UnitController unitController)
    {
        ScriptableUnit scriptableUnit = BaseUtils.unitDict[unitController.unitInfo.unitType];
        unitImage.sprite = scriptableUnit.defaultImage;
        unitImage.transform.localScale = unitController.isPurple ? new Vector3(-2, 2, 2) : Vector3.one * 2;
        Material goMaterial = new Material(unitController.isPurple ? BaseUtils.purpleMaterialUI : BaseUtils.yellowMaterialUI);
        goMaterial.SetColor("_HairColor", unitController.unitInfo.unitColorInfo.hairColor);
        goMaterial.SetColor("_SkinColor", unitController.unitInfo.unitColorInfo.skinColor);
        goMaterial.SetColor("_RarityOneColor", unitController.unitInfo.unitColorInfo.rarityColors[0]);
        goMaterial.SetColor("_RarityTwoColor", unitController.unitInfo.unitColorInfo.rarityColors[1]);
        goMaterial.SetColor("_RarityThreeColor", unitController.unitInfo.unitColorInfo.rarityColors[2]);
        goMaterial.SetColor("_RarityFourColor", unitController.unitInfo.unitColorInfo.rarityColors[3]);
        goMaterial.SetFloat("_HasRarity", unitController.unitInfo.unitColorInfo.hasRarity ? 1f : 0f);
        unitImage.material = goMaterial;
        unitImage.color = Color.white;
        maxHealth = unitController.unitInfo.health;
        currentHealth = maxHealth;
        hpText.SetString($"HP:{Mathf.Clamp(currentHealth, 0, Mathf.Infinity)}/{maxHealth}", Color.white);
        nameText.SetString(unitController.scriptableUnit.abrevName, Color.white);
        currentHealth = unitController.unitInfo.health;
        hpBar.transform.localScale = new Vector3(Mathf.Clamp01(currentHealth / maxHealth), 1, 1);
    }
    public void Setup(Transform followTransform, int maxHealth)
    {
        this.followTransform = followTransform;
        this.maxHealth = maxHealth;
        gameObject.SetActive(true);
        currentHealth = maxHealth;
        hpBar.transform.localScale = new Vector3(Mathf.Clamp01(currentHealth / maxHealth), 1, 1);
        rectTransform.SetAsFirstSibling();
    }
    private void LateUpdate()
    {
        //rectTransform.position = BaseUtils.mainCam.WorldToScreenPoint(followTransform.position + Vector3.up * 4 + Vector3.forward * 10);
        if (!staticBar)
        {
            rectTransform.position = followTransform.position + Vector3.up * 3f;
        }
    }
    public void UpdateHealth(int currentHealth)
    {
        int fromHealth = this.currentHealth;
        this.currentHealth = currentHealth;
        if (staticBar)
        {
            hpText.SetString($"HP:{fromHealth}/{maxHealth}");
        }
        hpBar.transform.localScale = new Vector3(1, Mathf.Clamp01(fromHealth / (float)maxHealth), 1);
        StartCoroutine(AnimateHPBar(fromHealth));
    }
    private IEnumerator AnimateHPBar(float fromHealth)
    {
        float timer = 0;
        while (timer <= 1)
        {
            int lerpHP = Mathf.RoundToInt(Mathf.Lerp(fromHealth, currentHealth, timer.Evaluate(CurveType.EaseOut)));
            if (staticBar)
            {
                hpText.SetString($"HP:{Mathf.Clamp(lerpHP, 0, Mathf.Infinity)}/{maxHealth}");
            }
            hpBar.transform.localScale = new Vector3(Mathf.Clamp01(lerpHP / (float)maxHealth), 1, 1);
            timer += Time.deltaTime * 3;
            yield return null;
        }
        if (staticBar)
        {
            if (currentHealth <= 0)
            {
                hpText.SetString("dead", Color.gray);
                nameText.SetString(Color.gray);
                unitImage.color = BaseUtils.damageColor;
            }
            else
            {
                hpText.SetString($"HP:{Mathf.Clamp(currentHealth, 0, Mathf.Infinity)}/{maxHealth}");
            }
        }
        hpBar.transform.localScale = new Vector3(Mathf.Clamp01(currentHealth / (float)maxHealth), 1, 1);
    }
}
