using UnityEngine;

public class RoadDetector : MonoBehaviour
{
    public LayerMask roadLayer;
    public Transform[] checkPoints; // точки проверки (углы машины)
    public float checkRadius = 0.2f;

    public bool isOnRoad;
    private bool wasOnRoad = true;

    void Update()
    {
        int onRoadCount = 0;

        foreach (var point in checkPoints)
        {
            Collider2D hit = Physics2D.OverlapCircle(point.position, checkRadius, roadLayer);
            if (hit != null)
                onRoadCount++;
        }

        isOnRoad = onRoadCount == 4;

        if (isOnRoad != wasOnRoad)
        {
            if (!isOnRoad)
                Debug.Log("Машина выехала за пределы дороги!");
            else
                Debug.Log("Машина вернулась на дорогу");
            wasOnRoad = isOnRoad;
        }
    }
}