using TMPro;
using UnityEngine;

public class Order
{
    public recipe recipe;

    public int requiredFlips;
    public int currentProgress = 0;

    public int requiredTemp;
    public int requiredTime;
    public float cookingProgress = 0f;

    public CookingQuality cookingQuality = CookingQuality.Perfect;

    public GameObject orderUI;
    public TextMeshProUGUI[] uiTexts;
}

public enum CookingQuality
{
    Perfect,  
    TooHot,   
    TooCold  
}
