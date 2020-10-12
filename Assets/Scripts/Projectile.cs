using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float expiryTime = 3f;
    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Expire());
    }

    private IEnumerator Expire()
    {
        yield return new WaitForSeconds(expiryTime);
        Destroy(gameObject);
    }
}
