using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMovement : MonoBehaviour
{
    private float length;
    private float StartPos;
    public GameObject Camera;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        StartPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        float temp = (Camera.transform.position.x * (1 - speed));
        float distance = (Camera.transform.position.x * speed);

        transform.position = new Vector3(StartPos + distance, transform.position.y, transform.position.z);
        if (temp > StartPos + length)
            StartPos += length;
        else if (temp < StartPos - length)
            StartPos -= length;
    }
}
