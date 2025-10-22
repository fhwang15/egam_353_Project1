using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using System.Collections.Generic;

public class GamePlayManager : MonoBehaviour
{

    public RecipeManager recipeManager;
    public TemperatureControll temperatureController;

    // UI
    public Transform orderContainer;
    public GameObject orderCardPrefab;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalTimerText; //Total Time of the game
    public TextMeshProUGUI timerText; //Order Timer

    private List<Order> activeOrders = new List<Order>();
    private int maxOrders = 5;
    private int totalOrdersCreated = 0;
    private int totalOrdersCompleted = 0;
    private int currentScore = 0;

    public float totalTimer;
    public float orderSpawnInterval; // create order per sec
    private float nextOrderTime;

    private bool gameIsOver;
    
    //pancake animation shit
    public RectTransform pancakeImage;
    private Vector2 pancakeOriginalPos; 
    public float pancakeJumpHeight = 100f; 
    public float pancakeJumpSpeed = 10f; 

    private bool isPancakeFlipping = false;
    private Vector2 pancakeTargetPos;

    //noodle shit
    public Image noodleImage;
    public Image noodleOverlay;
    public Color noodleCookColor = new Color();

    void Start()
    {

        gameIsOver = false;

        //Original Color of the burners
        nextOrderTime = orderSpawnInterval;

        scoreText.text = "Score: 0";

        // first order
        CreateNewOrder();

        if (pancakeImage != null)
        {
            pancakeOriginalPos = pancakeImage.anchoredPosition;
            pancakeTargetPos = pancakeOriginalPos;
        }

        if (noodleOverlay != null)
        {
            Color overlayColor = noodleCookColor;
            overlayColor.a = 0f;
            noodleOverlay.color = overlayColor;
        }

    }

    void Update()
    {
        if (gameIsOver) return;

        //Timer of the game
        totalTimer -= Time.deltaTime;
        totalTimerText.text = $"Time: {Mathf.Max(0, totalTimer):F1}s";

        if (totalTimer <= 0)
        {
            GameOver();
            return;
        }

        if (pancakeImage != null)
        {
            pancakeImage.anchoredPosition = Vector2.Lerp(
                pancakeImage.anchoredPosition,
                pancakeTargetPos,
                Time.deltaTime * pancakeJumpSpeed
            );
        }

        // Time left for next order
        nextOrderTime -= Time.deltaTime;
        timerText.text = $"Next Order Coming in... {Mathf.Max(0, nextOrderTime):F1}s";

        // Create new orders when less than 5 orders are on the screen
        if (nextOrderTime <= 0 && activeOrders.Count < maxOrders)
        {
            CreateNewOrder();
            nextOrderTime = orderSpawnInterval;
        }

        UpdateNoodleCookingVisual();
        UpdateAllNoodleUI();

        // Flipping pancake input
        if (Input.GetKeyDown(KeyCode.T))
        {
            // small animation
            if (pancakeImage != null)
            {
                pancakeTargetPos = pancakeOriginalPos + new Vector2(0, pancakeJumpHeight);
            }

            ProcessPancakeFlip();
        }
        else if (Input.GetKeyUp(KeyCode.T))
        { 

            // Back to its position
            if (pancakeImage != null)
            {
                pancakeTargetPos = pancakeOriginalPos;
            }
        }

        //Cooking the noodle input
        if (Input.GetKey(KeyCode.U))
        {
            ProcessNoodleCooking();
        }
        else
        {
        }
    }

    void CreateNewOrder()
    {
        if (activeOrders.Count >= maxOrders) return; //Will not create if there are 5 orders

        Order newOrder = new Order();
        newOrder.recipe = recipeManager.GetRandomRecipe();

        if (newOrder.recipe.type == RecipeType.pancake)
        {
            newOrder.requiredFlips = recipeManager.GetRandomFlips(newOrder.recipe);
        }
        else
        {
            newOrder.requiredTemp = recipeManager.GetRandomTemp(newOrder.recipe);
            newOrder.requiredTime = recipeManager.GetRandomCookTime(newOrder.recipe);
        }

        // Creatnig ui
        GameObject orderCard = Instantiate(orderCardPrefab, orderContainer);
        newOrder.orderUI = orderCard;
        newOrder.uiTexts = orderCard.GetComponentsInChildren<TextMeshProUGUI>();

        UpdateOrderUI(newOrder);

        activeOrders.Add(newOrder);
    }

    void UpdateOrderUI(Order order)
    {
        if (order.uiTexts == null || order.uiTexts.Length < 3) return;

        if (order.recipe.type == RecipeType.pancake)
        {
            order.uiTexts[0].text = order.recipe.name;
            order.uiTexts[1].text = $"Flip: {order.currentProgress}/{order.requiredFlips}";
            order.uiTexts[2].text = "Press A when pan touches!";
        }
        else  // Noodle recipe
        {
            order.uiTexts[0].text = order.recipe.name;

            int tempDiff = temperatureController.currentTemp - order.requiredTemp;
            Debug.Log($"temperatureController.currentTemp = {temperatureController.currentTemp}");


            order.uiTexts[1].text = $"Temp: {order.requiredTemp}°C (Now: {temperatureController.currentTemp}°C)";

            // color of the text
            if (Mathf.Abs(tempDiff) <= 0)
            {
                // Perfect:Green
                order.uiTexts[1].color = Color.green;
            }
            else if (tempDiff > 1)
            {
                // Too high: Red
                order.uiTexts[1].color = Color.red;
            }
            else // tempDiff < -1
            {
                // Too low: Cyan
                order.uiTexts[1].color = Color.cyan;
            }

            order.uiTexts[2].text = $"Hold D: {order.cookingProgress:F1}s / {order.requiredTime}s";
        }
    }

    void ProcessPancakeFlip()
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            Order order = activeOrders[i];
        }

        Order pancakeOrder = activeOrders.Find(o =>
        {
            return o.recipe.type == RecipeType.pancake;
        });

        if (pancakeOrder != null)
        {
            pancakeOrder.currentProgress++;
            UpdateOrderUI(pancakeOrder);

            Debug.Log($"Flip! {pancakeOrder.currentProgress}/{pancakeOrder.requiredFlips}");

            if (pancakeOrder.currentProgress >= pancakeOrder.requiredFlips)
            {
                CompleteOrder(pancakeOrder);
            }
        }

    }


    void ProcessNoodleCooking()
    {
        Order noodleOrder = activeOrders.Find(o => o.recipe.type == RecipeType.noodle);

        if (noodleOrder != null)
        {
            int currentTemp = temperatureController.currentTemp;
            int tempDiff = currentTemp - noodleOrder.requiredTemp;

            
            if (Mathf.Abs(tempDiff) <= 0)
            {
                noodleOrder.cookingQuality = CookingQuality.Perfect;
            }
            else if (tempDiff > 1)
            {
                noodleOrder.cookingQuality = CookingQuality.TooHot;
            }
            else
            {
                noodleOrder.cookingQuality = CookingQuality.TooCold;
            }

            noodleOrder.cookingProgress += Time.deltaTime;
            UpdateOrderUI(noodleOrder);

            if (noodleOrder.cookingProgress >= noodleOrder.requiredTime)
            {
                CompleteOrder(noodleOrder);
            }
        }
    }


    void UpdateAllNoodleUI()
    {
        // Update all noodle related UI
        foreach (Order order in activeOrders)
        {
            if (order.recipe.type == RecipeType.noodle)
            {
                UpdateOrderUI(order);
            }
        }
    }

    void UpdateNoodleCookingVisual()
    {
        if (noodleOverlay == null) return;

        Order noodleOrder = activeOrders.Find(o => o.recipe.type == RecipeType.noodle);

        if (noodleOrder != null)
        {
            float cookProgress = Mathf.Clamp01(noodleOrder.cookingProgress / noodleOrder.requiredTime);

            Color overlayColor = noodleCookColor;
            overlayColor.a = cookProgress * 0.3f;
            noodleOverlay.color = overlayColor;
        }
        else
        {
            Color overlayColor = noodleCookColor;
            overlayColor.a = 0f;
            noodleOverlay.color = overlayColor;
        }
    }

    void CompleteOrder(Order order)
    {
        int completeScore = 100; // 기본 점수

        // if its noodle, it will depend on the quality (enum) of your cooking. 
        if (order.recipe.type == RecipeType.noodle)
        {
            switch (order.cookingQuality)
            {
                case CookingQuality.Perfect:
                    completeScore = 100;
                    break;
                case CookingQuality.TooHot:
                    completeScore = 50;
                    break;
                case CookingQuality.TooCold:
                    completeScore = 50;
                    break;
            }
        }

        currentScore += completeScore;
        totalOrdersCompleted++;
        scoreText.text = $"Score: {currentScore}";

        activeOrders.Remove(order);
        Destroy(order.orderUI);
    }

    void GameOver()
    {
        gameIsOver = true;

        totalTimerText.text = "TIME'S UP!";
        timerText.text = $"Orders Completed: {totalOrdersCompleted}";

        Debug.Log($"=== GAME OVER ===");
        Debug.Log($"Final Score: {currentScore}");
        Debug.Log($"Orders Completed: {totalOrdersCompleted}");

        // Get rid of all 
        foreach (Order order in activeOrders)
        {
            Destroy(order.orderUI);
        }
        activeOrders.Clear();

    }
}