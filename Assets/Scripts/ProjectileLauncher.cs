using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ProjectileLauncher : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform target;

    public float maxDisplacementY = 25;

    private LineRenderer projectilePath;
    private int resolution = 30;
    private float gravityY;

    private void Awake()
    {
        projectilePath = GetComponent<LineRenderer>();
        projectilePath.startWidth = 0.2f;
        projectilePath.endWidth = 0.2f;
        gravityY = Physics.gravity.y;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Launch();
        }
    }

    // Launches the projectile on the predicted path
    private void Launch()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(transform.forward)); // Create projectile
        Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();
        LaunchData launchData = CalculateLaunchData(projectileRB);
        projectileRB.velocity = launchData.initialVelocity; // Launch projectile
        DrawPath(projectileRB, launchData); // Draw predicted path
    }

    // Calculates the predicted path of the projectile
    private LaunchData CalculateLaunchData(Rigidbody projectileRB)
    {
        float displacementY = target.position.y - projectileRB.position.y;
        if (maxDisplacementY < displacementY) // prevent error in case displacementY > maxDisplacementY
        {
            maxDisplacementY = displacementY;
        }
        Vector3 displacementXZ = new Vector3(target.position.x - projectileRB.position.x, 0, target.position.z - projectileRB.position.z);
        float time = Mathf.Sqrt(-2 * maxDisplacementY / gravityY) + Mathf.Sqrt(2 * (displacementY - maxDisplacementY) / gravityY);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravityY * maxDisplacementY);
        Vector3 velocityXZ = displacementXZ / time;

        // "Mathf.Sign(gravityY)" allows for reverse gravity arcs (for funsies)
        return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(gravityY), time);
    }

    // Draws the predicted path of the projectile
    private void DrawPath(Rigidbody projectileRB, LaunchData launchData)
    {
        Vector3[] positions = new Vector3[resolution + 1];
        //Vector3 previousDrawPoint = projectileRB.position;

        for (int i = 0; i < resolution + 1; i++)
        {
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * gravityY * simulationTime * simulationTime / 2f;
            Vector3 drawPoint = projectileRB.position + displacement;
            positions[i] = drawPoint;
            //Debug.DrawLine(previousDrawPoint, drawPoint, Color.green);
            //previousDrawPoint = drawPoint;
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
