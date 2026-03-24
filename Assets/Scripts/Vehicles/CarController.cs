using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет движением автомобиля в 2D.
/// Обрабатывает ввод, применяет ускорение, поворот и подавляет боковое скольжение.
/// </summary>
public class CarController : MonoBehaviour
{
    [Header("Параметры движения")]

    [Tooltip("Ускорение автомобиля")]
    public float acceleration = 8f;

    [Tooltip("Скорость поворота (град/с)")]
    public float steering = 240f;

    [Tooltip("Максимальная скорость")]
    public float maxSpeed = 60f;

    [Tooltip("Линейное затухание (сопротивление движению)")]
    public float damping = 1f;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = damping;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    /// <summary>
    /// FixedUpdate вызывается с фиксированной частотой (по умолчанию 50 раз в секунду).
    /// Используется для работы с физикой, в отличие от Update (который зависит от FPS).
    /// </summary>
    /// <summary>
    /// Основной цикл физики
    /// </summary>
    void FixedUpdate()
    {
        Vector2 input = ReadInput();

        ApplyMovement(input.y);
        ApplySteering(input.x);
        ApplyDriftCorrection();
    }

    /// <summary>
    /// Считывает ввод игрока
    /// </summary>
    /// <returns>Вектор (X — поворот, Y — газ)</returns>
    private Vector2 ReadInput()
    {
        return inputActions.Player.Move.ReadValue<Vector2>();
    }

    /// <summary>
    /// Применяет ускорение и ограничивает максимальную скорость
    /// </summary>
    /// <param name="move">Газ (-1..1)</param>
    private void ApplyMovement(float move)
    {
        rb.AddForce(transform.up * move * acceleration);

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    /// <summary>
    /// Управляет поворотом автомобиля
    /// </summary>
    /// <param name="turn">Поворот (-1..1)</param>
    private void ApplySteering(float turn)
    {
        float forwardSpeed = GetForwardSpeed();

        if (Mathf.Abs(forwardSpeed) < 0.3f)
        {
            rb.angularVelocity = 0f;
            return;
        }

        float direction = forwardSpeed > 0 ? 1f : -1f;
        float steeringFactor = CalculateSteeringFactor();

        rb.angularVelocity = -turn * steering * steeringFactor * direction;
    }

    /// <summary>
    /// Возвращает скорость вдоль направления машины
    /// </summary>
    private float GetForwardSpeed()
    {
        return Vector2.Dot(rb.linearVelocity, transform.up);
    }

    /// <summary>
    /// Рассчитывает коэффициент поворота в зависимости от скорости
    /// </summary>
    private float CalculateSteeringFactor()
    {
        return Mathf.Pow(rb.linearVelocity.magnitude / maxSpeed, 0.5f);
    }

    /// <summary>
    /// Уменьшает боковое скольжение автомобиля
    /// </summary>
    private void ApplyDriftCorrection()
    {
        Vector2 forward = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 sideways = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);

        rb.linearVelocity = forward + sideways * 0.2f;
    }
}