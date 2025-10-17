using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
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
    private int currentScore = 0;

    public float totalTimer;
    public float orderSpawnInterval; // create order per sec
    private float nextOrderTime;

    public MeshRenderer PanBurner;
    public MeshRenderer PotBurner;

    //Materials
    public Material PancakePressed;
    public Material PancakeReleased;


    public Material NoodleConnected;
    public Material NoodleReleased;


    Keyboard kbd;

    void Start()
    {
        //Original Color of the burners
        PancakeReleased = PanBurner.material;
        NoodleReleased = PotBurner.material;

        kbd = Keyboard.current;
        nextOrderTime = orderSpawnInterval;

        scoreText.text = "Score: 0";

        // first order
        CreateNewOrder();
    }

    void Update()
    {

        totalTimer -= Time.deltaTime;
        totalTimerText.text = totalTimer.ToString();

        // time left for Next order
        if (totalOrdersCreated < maxOrders)
        {
            nextOrderTime -= Time.deltaTime;
            timerText.text = $"Next Order Coming in... {Mathf.Max(0, nextOrderTime):F1}s";

            // new order
            if (nextOrderTime <= 0)
            {
                CreateNewOrder();
                nextOrderTime = orderSpawnInterval;
            }
        }
        else
        {
            timerText.text = "Order Full!";
        }

        // Flipping pancake input
        if (Input.GetKeyDown(KeyCode.A))
        {
            PanBurner.material = PancakePressed;
            ProcessPancakeFlip();

        } else if (Input.GetKeyUp(KeyCode.A))
        {
            PanBurner.material = PancakeReleased;
        }

        //Cooking the noodle input
        if (Input.GetKey(KeyCode.D))
        {
            PotBurner.material = NoodleConnected;
            ProcessNoodleCooking();
        }
        else
        {
            PotBurner.material = NoodleReleased;
        }
    }

    void CreateNewOrder()
    {
        if (totalOrdersCreated >= maxOrders) return;

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

        // UI 생성
        GameObject orderCard = Instantiate(orderCardPrefab, orderContainer);
        newOrder.orderUI = orderCard;
        newOrder.uiTexts = orderCard.GetComponentsInChildren<TextMeshProUGUI>();

        UpdateOrderUI(newOrder);

        activeOrders.Add(newOrder);
        totalOrdersCreated++;

        Debug.Log($"new Order: {newOrder.recipe.name} ({totalOrdersCreated}/{maxOrders})");
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
        else
        {
            order.uiTexts[0].text = order.recipe.name;
            order.uiTexts[1].text = $"Temp: {order.requiredTemp}°C (Now: {temperatureController.currentTemp}°C)";  // ← 현재 온도 표시
            order.uiTexts[2].text = $"Hold D: {order.cookingProgress:F1}s / {order.requiredTime}s";  // ← 진행도 표시
        }
    }

    void ProcessPancakeFlip()
    {

        // 모든 주문 출력
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
        // 첫 번째 누들 주문 찾기
        Order noodleOrder = activeOrders.Find(o => o.recipe.type == RecipeType.noodle);

        if (noodleOrder != null)
        {
            int currentTemp = temperatureController.currentTemp;

            // 온도가 맞는지 체크 (±1 허용)
            if (Mathf.Abs(currentTemp - noodleOrder.requiredTemp) <= 1)
            {
                // 온도가 맞으면 조리 시간 증가
                noodleOrder.cookingProgress += Time.deltaTime;
                UpdateOrderUI(noodleOrder);

                // 목표 시간 도달하면 완료
                if (noodleOrder.cookingProgress >= noodleOrder.requiredTime)
                {
                    CompleteOrder(noodleOrder);
                }
            }
            else
            {
                // 온도가 안 맞으면 UI만 업데이트 (경고 표시용)
                UpdateOrderUI(noodleOrder);
            }
        }
    }

    void CompleteOrder(Order order)
    {
        currentScore += 100;
        scoreText.text = $"Score: {currentScore}";

        activeOrders.Remove(order);
        Destroy(order.orderUI);

        Debug.Log($"Order Complete/ # of left orders: {activeOrders.Count}");

        // Check for game over
        if (totalOrdersCreated >= maxOrders && activeOrders.Count == 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log($"Game Over! Final Score: {currentScore}");
        timerText.text = "GAME OVER!";


    }
}