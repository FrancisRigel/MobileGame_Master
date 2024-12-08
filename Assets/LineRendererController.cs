using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    public Transform pointA; // First point (Transform A)
    public Transform pointB; // Second point (Transform B)

    private LineRenderer lineRenderer1; // First LineRenderer (from A to last seen position)
    private LineRenderer lineRenderer2; // Second LineRenderer (from last seen position to B)

    private Vector3? lastSeenPosition = null; // Stores the last obstruction point

    void Start()
    {
        // Create and configure the first LineRenderer (from A to hit point)
        lineRenderer1 = gameObject.AddComponent<LineRenderer>();
        InitializeLineRenderer(lineRenderer1);
    }

    void Update()
    {
        if (pointA != null && pointB != null)
        {
            Vector3 start = pointA.position;
            Vector3 end = pointB.position;

            // Perform the first raycast from pointA to pointB
            RaycastHit hit;
            Ray ray = new Ray(start, (end - start).normalized);

            if (Physics.Raycast(ray, out hit, Vector3.Distance(start, end)))
            {
                Debug.Log("Raycast hit: " + hit.collider.name);
                lastSeenPosition = hit.point; // Store the hit point
                Debug.DrawLine(ray.origin, hit.point, Color.green);

                // Set the lineRenderer1 to go from pointA to the hit point
                lineRenderer1.positionCount = 2;
                lineRenderer1.SetPosition(0, start);
                lineRenderer1.SetPosition(1, hit.point);

                // If the second LineRenderer hasn't been created yet, create it
                if (lineRenderer2 == null)
                {
                    lineRenderer2 = gameObject.AddComponent<LineRenderer>();
                    InitializeLineRenderer(lineRenderer2);
                }

                // Perform a second raycast from the last seen position to pointB
                Ray secondRay = new Ray(hit.point, (end - hit.point).normalized);
                if (Physics.Raycast(secondRay, out RaycastHit secondHit, Vector3.Distance(hit.point, end)))
                {
                    // Set lineRenderer2 to stop at the second hit point
                    lineRenderer2.positionCount = 2;
                    lineRenderer2.SetPosition(0, hit.point);
                    lineRenderer2.SetPosition(1, secondHit.point);
                }
                else
                {
                    // If no obstruction, connect lineRenderer2 from the last seen position to pointB
                    lineRenderer2.positionCount = 2;
                    lineRenderer2.SetPosition(0, hit.point);
                    lineRenderer2.SetPosition(1, end);
                }
            }
            else
            {
                // If no hit, ensure the first LineRenderer connects pointA to pointB
                lastSeenPosition = null;
                lineRenderer1.positionCount = 2;
                lineRenderer1.SetPosition(0, start);
                lineRenderer1.SetPosition(1, end);

                // Destroy the second line renderer if it exists
                if (lineRenderer2 != null)
                {
                    Destroy(lineRenderer2);
                    lineRenderer2 = null;
                }
            }

            // Visualize the raycast in the Scene view
            Debug.DrawLine(start, end, Color.red);
        }
    }

    // Initialize the LineRenderer with some default settings
    private void InitializeLineRenderer(LineRenderer lr)
    {
        lr.positionCount = 2;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default")); // Default material
        lr.startColor = Color.white;
        lr.endColor = Color.white;
    }
}
