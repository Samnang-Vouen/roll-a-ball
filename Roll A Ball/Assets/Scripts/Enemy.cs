using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 0;
    public List<Transform> wayPoints;

    private int wayPointIndex;
    private float range;
    // Start is called before the first frame update
    void Start()
    {
        wayPointIndex = 0;
        range = 1.0f;

        // Make enemy unaffected by physics pushes; it will move only via script
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Not moved by physics
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Keep upright/still on rotation
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    
    // Move towards the next waypoint without rotating the object
    void Move()
    {
        if (wayPoints == null || wayPoints.Count == 0) return;

        // Keep movement on the XZ plane at the current height
        Vector3 target = wayPoints[wayPointIndex].position;
        target.y = transform.position.y;

        // Move in world space directly towards the target (no rotation applied)
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < range)
        {
            wayPointIndex++;
            if (wayPointIndex >= wayPoints.Count)
            {
                wayPointIndex = 0;
            }
        }
    }
}
