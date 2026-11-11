using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 10.0f;

    private void Update()
    {
        transform.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
