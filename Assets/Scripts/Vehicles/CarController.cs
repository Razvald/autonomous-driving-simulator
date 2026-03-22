using UnityEngine;
using UnityEngine.InputSystem; // Новая система ввода Unity (замена устаревшего Input Manager)

/// <summary>
/// Управляет физикой движения автомобиля в 2D пространстве.
/// Реализует реалистичное управление с учетом ограничения скорости, поворота и бокового скольжения.
/// </summary>
public class CarController : MonoBehaviour
{
    [Header("Параметры движения")]
    [Tooltip("Сила ускорения. Чем выше значение, тем быстрее машина набирает скорость")]
    public float acceleration = 8f;

    [Tooltip("Скорость поворота (градусов в секунду). Влияет на то, насколько резко машина поворачивает")]
    public float steering = 120f;

    [Tooltip("Максимальная скорость в единицах в секунду. Ограничивает разгон")]
    public float maxSpeed = 30f;

    [Tooltip("Коэффициент торможения. Чем выше, тем быстрее машина теряет скорость при отсутствии газа")]
    public float damping = 1f;

    private Rigidbody2D rb;                      // Компонент физики для управления движением
    private InputSystem_Actions inputActions;    // Объект для работы с новой системой ввода

    void Awake()
    {
        // Создаем экземпляр Input Actions (настройки управления из файла InputSystem_Actions.inputactions)
        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        // Получаем компонент Rigidbody2D, прикрепленный к этому объекту
        rb = GetComponent<Rigidbody2D>();
        
        // Устанавливаем линейное торможение. Без нажатия на газ машина будет замедляться
        rb.linearDamping = damping;
    }

    void OnEnable()
    {
        // Активируем систему ввода при включении объекта
        // Без этого вызов inputActions.Player.Move.ReadValue() не будет работать
        inputActions.Enable();
    }

    void OnDisable()
    {
        // Отключаем систему ввода при выключении объекта (например, при уничтожении машины)
        // Это хорошая практика для предотвращения утечек памяти
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

        // Проверяем текущую скорость. transform.up - это направление "вперед" для машины
        // rb.linearVelocity.magnitude - длина вектора скорости (скалярная величина скорости)
        // Пример: magnitude = √(x² + y²)
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            // Умножаем на move (1 = вперед, -1 = назад) и на силу ускорения
            // AddForce добавляет силу в физическую симуляцию
            rb.AddForce(transform.up * move * acceleration);
        }

        // Vector2.Dot - скалярное произведение векторов.
        // Если машина движется вперед (transform.up = (0,1)), 
        // а ее скорость rb.linearVelocity = (0,10), то Dot = 0*0 + 1*10 = 10
        // Если скорость = (0,-5) (движется назад), то Dot = 0*0 + 1*(-5) = -5
        // forwardSpeed — скорость только вдоль направления машины (может быть отрицательной)
        // magnitude — общая скорость (всегда положительная)
        float forwardSpeed = Vector2.Dot(rb.linearVelocity, transform.up);

        // Поворачиваем машину только если она движется (скорость > 0.1)
        if (Mathf.Abs(forwardSpeed) > 0.1f)
        {
            // Определяем направление движения: 1 = вперед, -1 = назад
            float direction = forwardSpeed > 0 ? 1f : -1f;
            
            // steeringFactor - коэффициент поворота, зависящий от скорости.
            // Раньше использовали Mathf.Clamp01, так как он ограничивает 
            // значения между 0 и 1, однако т.к. magnitude является
            // общей скоростью, то не может быть меньше 0, а скорость 
            // не может превысить максимальную
            float steeringFactor = rb.linearVelocity.magnitude / maxSpeed;
            
            // Формула поворота:
            // rb.rotation - текущий угол поворота машины в градусах
            // turn - ввод игрока (-1 до 1)
            // steering - базовая скорость поворота (градусов/секунду)
            // steeringFactor - влияние скорости на поворот
            // direction - направление движения (для заднего хода)
            // Time.fixedDeltaTime - время между кадрами физики (для плавности)
            //
            // Пример: при speed=15, maxSpeed=30, steering=120, turn=1 (поворот вправо)
            // steeringFactor = 0.5, direction = 1 (едем вперед)
            // Изменение угла = 1 * 120 * 0.5 * 1 * 0.02 = 1.2 градуса за кадр
            rb.MoveRotation(rb.rotation - turn * steering * steeringFactor * direction * Time.fixedDeltaTime);
        }

        // Разделяем вектор скорости на две составляющие:
        // 1. Продольная (вдоль направления машины) - туда, куда смотрит машина
        // 2. Поперечная (боковая) - перпендикулярно направлению машины
        
        // Продольная скорость: проекция скорости на направление "вперед"
        // transform.up - это направление "вперед"
        // Vector2.Dot дает величину проекции
        // transform.up * проекция = вектор продольной скорости
        Vector2 forward = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        
        // Поперечная скорость: проекция скорости на направление "вправо"
        // transform.right - это направление "вправо" (перпендикулярно вперед)
        Vector2 sideways = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        
        // Собираем финальную скорость:
        // - Продольная составляющая остается полностью (машина едет вперед/назад)
        // - Поперечная составляющая уменьшается в 10 раз (умножаем на 0.1)
        // Пример: машина движется под углом 45 градусов со скоростью 10
        // Без коррекции: forward = (7,7), sideways = (7,-7) -> итоговая скорость = (14,0)
        // С коррекцией: forward = (7,7), sideways * 0.1 = (0.7,-0.7) -> итог = (7.7,6.3)
        // Машина продолжает двигаться преимущественно вперед, а боковое скольжение минимально
        rb.linearVelocity = forward + sideways * 0.1f;
    }
}