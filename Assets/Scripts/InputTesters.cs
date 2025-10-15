using UnityEngine;

public class InputTesters : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Pan is on the burner");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Pancake is on the Pan");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Pot is on the burner");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Noodle is in the Pot");
        }
    }
}
