using UnityEngine;
using UnityEngine.InputSystem; // Новая система ввода Unity (замена устаревшего Input Manager)

/// <summary>
/// Управляет движением автомобиля в 2D.
/// Машина получает ввод через New Input System, разгоняется силой,
/// поворачивает через angularVelocity и частично гасит боковое скольжение.
/// </summary>
public class CarController : MonoBehaviour
{
    [Header("Параметры движения")]
    [Tooltip("Сила ускорения. Чем выше значение, тем быстрее машина набирает скорость")]
    public float acceleration = 8f;

    [Tooltip("Скорость поворота. Чем выше значение, тем быстрее машина вращается")]
    public float steering = 240f;

    [Tooltip("Максимальная скорость машины")]
    public float maxSpeed = 60f;

    [Tooltip("Линейное затухание Rigidbody2D. Уменьшает скольжение и плавно замедляет машину")]
    public float damping = 1f;

    private Rigidbody2D rb;                      // Компонент физики для управления движением
    private InputSystem_Actions inputActions;    // Объект для работы с новой системой ввода

    void Awake()
    {
        // Создаём объект с настройками ввода из Input Actions asset
        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        // Получаем Rigidbody2D машины
        rb = GetComponent<Rigidbody2D>();

        // Задаём сопротивление движению
        rb.linearDamping = damping;
    }

    void OnEnable()
    {
        // Включаем ввод, когда объект активен
        inputActions.Enable();
    }

    void OnDisable()
    {
        // Выключаем ввод, когда объект отключается
        inputActions.Disable();
    }

    /// <summary>
    /// FixedUpdate вызывается с фиксированной частотой (по умолчанию 50 раз в секунду).
    /// Используется для работы с физикой, в отличие от Update (который зависит от FPS).
    /// </summary>
    void FixedUpdate()
    {
        // Получаем вектор управления: (X = горизонталь, Y = вертикаль)
        Vector2 moveVector = inputActions.Player.Move.ReadValue<Vector2>();
        
        // Отделяем горизонтальную и вертикальную составляющие
        float move = moveVector.y;   // Газ/тормоз (W/S или стрелки вверх/вниз)
        float turn = moveVector.x;   // Поворот (A/D или стрелки влево/вправо)

        // Разгоняем машину вперёд или назад вдоль её локальной оси Y
        rb.AddForce(transform.up * move * acceleration);

        // Ограничиваем общую скорость
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // Vector2.Dot - скалярное произведение векторов.
        // Если машина движется вперед (transform.up = (0,1)), 
        // а ее скорость rb.linearVelocity = (0,10), то Dot = 0*0 + 1*10 = 10
        // Если скорость = (0,-5) (движется назад), то Dot = 0*0 + 1*(-5) = -5
        // forwardSpeed — скорость только вдоль направления машины (может быть отрицательной)
        float forwardSpeed = Vector2.Dot(rb.linearVelocity, transform.up);

        // Поворачиваем машину только если она движется
        if (Mathf.Abs(forwardSpeed) > 0.3f)
        {
            // Для движения назад руль работает в обратную сторону
            float direction = forwardSpeed > 0 ? 1f : -1f;
            
            // Коэффициент поворота зависит от текущей скорости
            // rb.linearVelocity.magnitude - длина вектора скорости (скалярная величина скорости)
            // Пример: magnitude = √(x² + y²)
            float steeringFactor = Mathf.Pow(rb.linearVelocity.magnitude / maxSpeed, 0.5f);
            // При скорости 15 (половина от max): 0.5^0.5 = 0.71
            
            // Формула поворота:
            // turn - ввод игрока (-1 до 1)
            // steering - базовая скорость поворота (градусов/секунду)
            // steeringFactor - влияние скорости на поворот
            // direction - направление движения (для заднего хода)
            //
            // Пример: при speed=15, maxSpeed=30, steering=120, turn=1 (поворот вправо)
            // steeringFactor = 0.5, direction = 1 (едем вперед)
            // Изменение угла = 1 * 120 * 0.5 * 1 * 0.02 = 1.2 градуса за кадр
            rb.angularVelocity = -turn * steering * steeringFactor * direction;
        }
        else
        {
            // Если машина почти стоит, не даём ей крутиться
            rb.angularVelocity = 0f;
        }
        // Разделяем скорость на две части:
        // forward — вдоль машины
        // sideways — поперёк машины
        Vector2 forward = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 sideways = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        
        // Оставляем почти всю продольную скорость,
        // а боковую заметно уменьшаем, чтобы машина не скользила как лёд
        rb.linearVelocity = forward + sideways * 0.2f;
    }
}