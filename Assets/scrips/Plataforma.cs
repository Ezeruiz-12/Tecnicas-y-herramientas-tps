using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plataforma : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform platform;
    public float speed = 1.0f;

    private Vector3 target;

    void Start()
    {
        target = pointB.position;
    }

    // Update is called once per frame
    void Update()
    {
        platform.position = Vector3.MoveTowards(platform.position, target, speed * Time.deltaTime);

        if (platform.position == target)
        {
            target = target == pointB.position ? pointA.position : pointB.position;
        }
    }
}