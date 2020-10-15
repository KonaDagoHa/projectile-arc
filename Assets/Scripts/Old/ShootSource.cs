using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootSource : MonoBehaviour
{
    public GameObject projectile;
    [Range(0, 100)]
    public float shootVelocity = 5f;
    [Range(0, 80)]
    public float shootAngle = 0f;

    private bool isCharged = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            ChargeShot();
        }
        else if (!Input.GetButton("Fire1"))
        {
            Shoot();
        }

    }

    private void ChargeShot()
    {
        // max angle of 80
        if (shootAngle < 80)
        {
            // Angle increases by n per second as player holds down button
            shootAngle += 40f * Time.deltaTime;
            shootVelocity += 0.1f * Time.deltaTime;
            isCharged = true;
        }
    }

    private void Shoot()
    {
        if (isCharged)
        {
            GameObject proj = Instantiate(projectile, transform.position, Quaternion.LookRotation(transform.forward)); // Create projectile
            // Find local velocity of projectile relative to shootSource
            Vector3 localVel = new Vector3(0, shootVelocity * Mathf.Sin(Mathf.Deg2Rad * shootAngle), shootVelocity * Mathf.Cos(Mathf.Deg2Rad * shootAngle));
            // Convert local velocity to world velocity
            proj.GetComponent<Rigidbody>().velocity = transform.TransformVector(localVel);

            shootAngle = 0f;
            isCharged = false;
        }
    }
}
