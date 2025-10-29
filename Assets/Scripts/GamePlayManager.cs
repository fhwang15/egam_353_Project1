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
    public TextMeshProUGUI totalTimerText; // Total Time of the game
    public TextMeshProUGUI timerText;      // Order Timer

    private List<Order> activeOrders = new List<Order>();
    private int maxOrders = 4;
    private int totalOrdersCreated = 0;
    private int totalOrdersCompleted = 0;
    private int currentScore = 0;

    public float totalTimer;
    public float orderSpawnInterval; // create order per sec
    private float nextOrderTime;

    private bool gameIsOver;

    public bool noodleCanCook;

    // pancake animation
    public RectTransform pancakeImage;
    private Vector2 pancakeOriginalPos;
    public float pancakeJumpHeight = 100f;
    public float pancakeJumpSpeed = 10f;

    private bool isPancakeFlipping = false;
    private Vector2 pancakeTargetPos;
    private float pancakeRotation = 0f;
    private float pancakeRotationSpeed = 360f; // degrees per second

    // NEW: remember last-frame T state (for edge detection)
    private bool wasTPressed = false;

    // noodle
    public Image noodleImage;
    public Image noodleOverlay;
    public Color noodleCookColor = new Color();

    // order images
    public Sprite pancakeOrderSprite;
    public Sprite noodleOrderSprite;

    // audio
    public AudioSource audioSource;
    public AudioClip pancakeJumpSound;
    public AudioClip noodleCookingSound;
    public AudioClip orderCompleteSound;

    void Start()
    {
        gameIsOver = false;

        // next order timer
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

        // Game total timer
        totalTimer -= Time.deltaTime;
        totalTimerText.text = $" {Mathf.Max(0, totalTimer):F1}s";

        if (totalTimer <= 0)
        {
            GameOver();
            return;
        }

        // Pancake UI movement + rotation
        if (pancakeImage != null)
        {
            pancakeImage.anchoredPosition = Vector2.Lerp(
                pancakeImage.anchoredPosition,
                pancakeTargetPos,
                Time.deltaTime * pancakeJumpSpeed
            );

            if (isPancakeFlipping)
            {
                pancakeRotation += pancakeRotationSpeed * Time.deltaTime;
                pancakeImage.rotation = Quaternion.Euler(pancakeRotation, 0, 0);
            }
        }

        // next order countdown
        nextOrderTime -= Time.deltaTime;
        timerText.text = $"Next Order Coming in... {Mathf.Max(0, nextOrderTime):F1}s";

        // spawn new order if room
        if (nextOrderTime <= 0 && activeOrders.Count < maxOrders)
        {
            CreateNewOrder();
            nextOrderTime = orderSpawnInterval;
        }

        UpdateNoodleCookingVisual();
        UpdateAllNoodleUI();

        // ========= Inverted pancake input logic =========
        bool isTPressed = Input.GetKey(KeyCode.T);

        if (!isTPressed)
        {
            // No input -> pancake jumps up and spins
            if (pancakeImage != null)
            {
                pancakeTargetPos = pancakeOriginalPos + new Vector2(0, pancakeJumpHeight);
                isPancakeFlipping = true;
                // keep spinning; do not reset rotation each frame
            }

            // Only count a flip once when transitioning from pressed -> not pressed
            if (wasTPressed)
            {
                ProcessPancakeFlip();
                pancakeRotation = 0f; // optional: restart the spin cycle each counted flip

                // Play pancake jump sound
                PlayPancakeJumpSound();
            }
        }
        else
        {
            // Holding input -> return to rest and stop spinning
            if (pancakeImage != null)
            {
                pancakeTargetPos = pancakeOriginalPos;
                isPancakeFlipping = false;
                pancakeImage.rotation = Quaternion.identity;
            }
        }

        wasTPressed = isTPressed;
        // ========= End inverted logic =========

        // Noodle cooking input - requires both U and = keys

        if (Input.GetKey(KeyCode.Equals))
        {
            noodleCanCook = true;
        }
        else
        {
            noodleCanCook = false;
            audioSource.Stop();
        }

        if (noodleCanCook && Input.GetKey(KeyCode.U))
        {
            Debug.Log("lalal");
            ProcessNoodleCooking();
            PlayNoodleCookingSound();
        }
        else if (!Input.GetKey(KeyCode.U))
        {
            audioSource.Stop();
        }

        // Restart game with Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    void CreateNewOrder()
    {
        if (activeOrders.Count >= maxOrders) return; // do not create if already 5 orders

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

        // Create UI
        GameObject orderCard = Instantiate(orderCardPrefab, orderContainer);
        newOrder.orderUI = orderCard;
        newOrder.uiTexts = orderCard.GetComponentsInChildren<TextMeshProUGUI>();
        newOrder.orderImage = orderCard.GetComponentInChildren<Image>();

        UpdateOrderUI(newOrder);

        activeOrders.Add(newOrder);
        totalOrdersCreated++;
    }

    void UpdateOrderUI(Order order)
    {
        if (order.uiTexts == null || order.uiTexts.Length < 3) return;

        if (order.recipe.type == RecipeType.pancake)
        {
            order.uiTexts[0].text = order.recipe.name;
            order.uiTexts[1].text = $"Flip: {order.currentProgress}/{order.requiredFlips}";
            order.uiTexts[2].text = "Flip your pancake!!!";
            
            // Set pancake image
            if (order.orderImage != null && pancakeOrderSprite != null)
            {
                order.orderImage.sprite = pancakeOrderSprite;
            }
        }
        else // Noodle
        {
            order.uiTexts[0].text = order.recipe.name;

            int tempDiff = temperatureController.currentTemp - order.requiredTemp;
            Debug.Log($"temperatureController.currentTemp = {temperatureController.currentTemp}");

            order.uiTexts[1].text =
                $"{order.requiredTemp}\u00B0C";

            // color
            if (Mathf.Abs(tempDiff) <= 0)
            {
                // Perfect: Green
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

            order.uiTexts[2].text =
                $"Boil for: {order.cookingProgress:F1}s / {order.requiredTime}s";
            
            // Set noodle image
            if (order.orderImage != null && noodleOrderSprite != null)
            {
                order.orderImage.sprite = noodleOrderSprite;
            }
        }
    }

    void ProcessPancakeFlip()
    {
        // find first pancake order
        Order pancakeOrder = activeOrders.Find(o => o.recipe.type == RecipeType.pancake);

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

            // Make color change more visible
            Color overlayColor = noodleCookColor;
            overlayColor.a = cookProgress * 0.8f; // Increased from 0.3f to 0.8f for more visibility
            
            // Add color intensity based on cooking quality
            if (noodleOrder.cookingQuality == CookingQuality.Perfect)
            {
                overlayColor = Color.green;
                overlayColor.a = cookProgress * 0.6f;
            }
            else if (noodleOrder.cookingQuality == CookingQuality.TooHot)
            {
                overlayColor = Color.red;
                overlayColor.a = cookProgress * 0.7f;
            }
            else if (noodleOrder.cookingQuality == CookingQuality.TooCold)
            {
                overlayColor = Color.cyan;
                overlayColor.a = cookProgress * 0.7f;
            }
            
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
        int completeScore = 100; // base score

        // noodle score by quality
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

        // Play order complete sound
        PlayOrderCompleteSound();

        activeOrders.Remove(order);
        if (order.orderUI != null) Destroy(order.orderUI);
    }

    void GameOver()
    {
        gameIsOver = true;

        totalTimerText.text = "TIME'S UP!";
        timerText.text = $"Orders Completed: {totalOrdersCompleted}";

        Debug.Log("=== GAME OVER ===");
        Debug.Log($"Final Score: {currentScore}");
        Debug.Log($"Orders Completed: {totalOrdersCompleted}");

        // clear all order UIs
        foreach (Order order in activeOrders)
        {
            if (order.orderUI != null) Destroy(order.orderUI);
        }
        activeOrders.Clear();
    }

    void PlayPancakeJumpSound()
    {
        if (audioSource != null && pancakeJumpSound != null)
        {
            audioSource.PlayOneShot(pancakeJumpSound);
        }
    }

    void PlayNoodleCookingSound()
    {
        if (audioSource != null && noodleCookingSound != null)
        {
            if (audioSource.isPlaying != true)
            {
                audioSource.PlayOneShot(noodleCookingSound);
            }
        }
    }

    void PlayOrderCompleteSound()
    {
        if (audioSource != null && orderCompleteSound != null)
        {
            audioSource.PlayOneShot(orderCompleteSound);
        }
    }

    void RestartGame()
    {
        Debug.Log("Restarting game via Space key...");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Get current scene index
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        
        // Check if scene is in Build Settings
        if (currentSceneIndex < 0)
        {
            Debug.LogError("Current scene is not in Build Settings! Please add it to File > Build Settings > Scenes In Build");
            return;
        }
        
        // Reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
    }
}
