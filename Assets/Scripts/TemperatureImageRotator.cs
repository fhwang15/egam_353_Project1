using UnityEngine;

/// <summary>
/// 当温度改变时旋转图片特定角度
/// </summary>
public class TemperatureImageRotator : MonoBehaviour
{
    [Header("旋转设置")]
    public Transform targetImage; // 要旋转的图片Transform
    public float rotationAngle = 15f; // 每次温度改变时的旋转角度（度）
    public float rotationSpeed = 5f; // 旋转速度
    
    [Header("温度控制引用")]
    public TemperatureControll temperatureController; // 温度控制脚本引用
    
    private int lastTemperature; // 记录上一帧的温度
    private bool isRotating = false; // 是否正在旋转
    private float targetRotation; // 目标旋转角度
    private float currentRotation; // 当前旋转角度
    
    void Start()
    {
        // 如果没有指定targetImage，使用当前物体
        if (targetImage == null)
        {
            targetImage = transform;
        }
        
        // 记录初始温度
        if (temperatureController != null)
        {
            lastTemperature = temperatureController.currentTemp;
        }
        
        // 初始化旋转角度
        currentRotation = targetImage.eulerAngles.z;
        targetRotation = currentRotation;
    }
    
    void Update()
    {
        if (temperatureController == null || targetImage == null) return;
        
        // 检测温度是否改变
        if (temperatureController.currentTemp != lastTemperature)
        {
            OnTemperatureChanged();
            lastTemperature = temperatureController.currentTemp;
        }
        
        // 执行旋转动画
        if (isRotating)
        {
            RotateToTarget();
        }
    }
    
    void OnTemperatureChanged()
    {
        // 计算新的目标旋转角度
        targetRotation += rotationAngle;
        
        // 开始旋转
        isRotating = true;
        
        Debug.Log($"温度改变: {lastTemperature} -> {temperatureController.currentTemp}, 旋转角度: {rotationAngle}°");
    }
    
    void RotateToTarget()
    {
        // 平滑旋转到目标角度
        currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * rotationSpeed);
        targetImage.rotation = Quaternion.Euler(0, 0, currentRotation);
        
        // 检查是否接近目标角度
        if (Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation)) < 0.1f)
        {
            currentRotation = targetRotation;
            targetImage.rotation = Quaternion.Euler(0, 0, currentRotation);
            isRotating = false;
        }
    }
    
    // 公共方法：手动触发旋转
    public void TriggerRotation()
    {
        OnTemperatureChanged();
    }
    
    // 公共方法：重置旋转角度
    public void ResetRotation()
    {
        targetRotation = 0f;
        currentRotation = 0f;
        targetImage.rotation = Quaternion.identity;
        isRotating = false;
    }
    
    // 公共方法：设置旋转角度
    public void SetRotationAngle(float angle)
    {
        rotationAngle = angle;
    }
    
    // 公共方法：设置旋转速度
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
}
