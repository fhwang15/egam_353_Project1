using UnityEngine;
using TMPro;

public class TemperatureControll : MonoBehaviour
{
    public int maxTemp;
    public int currentTemp;

    bool isTurned;

    public TextMeshProUGUI temperatureText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTemp = 1;
        isTurned = false;
    }

    // Update is called once per frame
    void Update()
    {
        temperatureText.text = currentTemp.ToString();

        if (Input.GetKeyDown(KeyCode.W))
        {
            isTurned = true;

            if(currentTemp >= maxTemp)
            {
                currentTemp = 1;
            }
            else if(currentTemp < maxTemp && isTurned)
            {         
                currentTemp += 1;
                isTurned=false;
                return;
            }
        }
    }
}
