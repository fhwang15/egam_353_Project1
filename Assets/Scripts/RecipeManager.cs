using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public class RecipeManager : MonoBehaviour
{
    private RecipeList recipeList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        LoadRecipes();
        
    }

    void Start()
    {
    }

    void LoadRecipes()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("recipes");
        recipeList = JsonUtility.FromJson<RecipeList>(jsonFile.text);

        Debug.Log($"JSON 파일 내용: {jsonFile.text}");

        if (recipeList == null || recipeList.recipes == null)
        {
            Debug.LogError("JSON 파싱 실패!");
            return;
        }

        Debug.Log($"레시피 {recipeList.recipes.Length}개 로드 완료!");
    }

    public recipe GetRandomRecipe()
    {
        return recipeList.recipes[Random.Range(0, recipeList.recipes.Length)];
    }

    public int GetRandomFlips(recipe recipe)
    {
        if (recipe.type == RecipeType.pancake)
        {
            return Random.Range(recipe.minFlips, recipe.maxFlips + 1);
        }
        return 0;
    }

    public int GetRandomTemp(recipe recipe)
    {
        if (recipe.type == RecipeType.noodle)
        {
            return Random.Range(recipe.minTemp, recipe.maxTemp + 1);
        }
        return 0;
    }

    public int GetRandomCookTime(recipe recipe)
    {
        if (recipe.type == RecipeType.noodle)
        {
            return Random.Range(recipe.minCookTime, recipe.maxCookTime + 1);
        }
        return 0;
    }

}
