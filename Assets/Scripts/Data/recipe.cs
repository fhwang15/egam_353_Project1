using UnityEngine;
using System;

[Serializable]
public class recipe
{
    public int recipeID;
    public string name;
    public RecipeType type; // "pancake" or "noodle"

    // For Pancake (null if noodle)
    public int minFlips;
    public int maxFlips;

    // For Noodles (null if pancake)
    public int minTemp;
    public int maxTemp;
    public int minCookTime;
    public int maxCookTime;
}

[Serializable]
public class RecipeList
{
    public recipe[] recipes;
}

[Serializable]
public enum RecipeType
{
    pancake = 0,
    noodle = 1
}
