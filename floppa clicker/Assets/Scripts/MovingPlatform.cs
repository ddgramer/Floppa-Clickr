using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private bool hit = false;

    public bool moving;

    public float timeToTake;

    public Vector3 movingRange;
    private Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (moving)
        {
            Vector3 target = originalPos + movingRange;

            if (hit)
            {
                target = originalPos - movingRange;
            }

            transform.position = Vector3.MoveTowards(transform.position, target, (2 * movingRange.magnitude) * (Time.fixedDeltaTime / timeToTake));

            if (transform.position == target)
            {
                hit = !hit;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.transform.SetParent(transform);
    }

    private void OnCollisionExit(Collision collision)
    {
        collision.transform.SetParent(null);
    }
}