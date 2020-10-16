using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ProjectileLauncher : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform target;
    public Transform camera;

    public float maxDisplacementY = 25;

    private LineRenderer projectilePath;
    private int resolution = 10; // there will be (resolution + 1) number of vertices in linerenderer
    private float gravityY;
    private float launchCooldown = 1f;
    private bool canLaunch = true; // used for cooldown
    private bool isCharging = false; // used to raycast targetDestination before charging
    private Vector3 targetDestination; // where target will end up if player holds button long enough
    private Rigidbody targetRB;

    private void Awake()
    {
        projectilePath = GetComponent<LineRenderer>();
        projectilePath.startWidth = 0.1f;
        projectilePath.endWidth = 0.1f;
        gravityY = Physics.gravity.y;
        targetRB = target.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(Cooldown());
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") && canLaunch)
        {
            ChargeLaunch();
        } else if (Input.GetButtonUp("Fire1"))
        {
            Launch();
        }
    }

    // Cooldown to restrict fire rate of projectiles
    private IEnumerator Cooldown()
    {
        while (true)
        {
            if (!canLaunch)
            {
                yield return new WaitForSeconds(launchCooldown);
                canLaunch = true;
                target.transform.localPosition = new Vector3(0, 0, 0); // reset target's position
            }
            yield return null;
        }
    }

    // The launch the button is held down, the farther away the projectile goes
    private void ChargeLaunch()
    {
        if (canLaunch)
        {
            if (!isCharging)
            {
                isCharging = true;
                // Raycast where the target will end up
                RaycastHit hit;
                if (Physics.Raycast(camera.position, camera.forward, out hit))
                {
                    targetDestination = hit.point;
                }
            }

            // Move projectile forward
            target.transform.position = Vector3.MoveTowards(target.transform.position, targetDestination, 4 * Time.deltaTime);

            // Update projectile arc
            LaunchData launchData = CalculateLaunchData(transform);
            DrawPath(transform, launchData); // Draw predicted path

        }
    }

    // Launches the projectile on the predicted path
    private void Launch()
    {
        if (canLaunch)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, transform.rotation); // Create projectile
            // TODO: reduce duplicate code by sending launchData from ChargeLaunch() to Launch()
            LaunchData launchData = CalculateLaunchData(projectile.transform);
            projectile.GetComponent<Rigidbody>().velocity = launchData.initialVelocity; // Launch projectile
            DrawPath(projectile.transform, launchData); // Draw predicted path
            canLaunch = false;
            isCharging = false;
        }
    }

    // Calculates the predicted path of the projectile
    private LaunchData CalculateLaunchData(Transform projectile)
    {
        float displacementY = target.position.y - projectile.position.y;
        if (maxDisplacementY < displacementY) // prevent error in case displacementY > maxDisplacementY
        {
            maxDisplacementY = displacementY;
        }
        if (maxDisplacementY <= 0) // prevent error that occurs when maxDisplacement <= 0
        {
            maxDisplacementY = 0.01f;
        }

        Vector3 displacementXZ = new Vector3(target.position.x - projectile.position.x, 0, target.position.z - projectile.position.z);

        float time = Mathf.Sqrt(-2 * maxDisplacementY / gravityY) + Mathf.Sqrt(2 * (displacementY - maxDisplacementY) / gravityY);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravityY * maxDisplacementY);
        Vector3 velocityXZ = displacementXZ / time;

        // "Mathf.Sign(gravityY)" allows for reverse gravity arcs (for funsies)
        return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(gravityY), time);
    }

    // Draws the predicted path of the projectile
    private void DrawPath(Transform projectile, LaunchData launchData)
    {
        Vector3[] positions = new Vector3[resolution + 1];

        for (int i = 0; i < resolution + 1; i++)
        {
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * gravityY * simulationTime * simulationTime / 2f;
            Vector3 drawPoint = projectile.position + displacement;
            positions[i] = drawPoint;
        }
        projectilePath.positionCount = resolution + 1;
        projectilePath.SetPositions(positions);
    }

    // Simple struct used to store projectile's initial velocity and time to target calculated from CalculateLaunchData()
    private struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }
    }
}
