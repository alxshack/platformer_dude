using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] private float speed = 2.0f;
    [SerializeField] private Transform target;

    private void Awake()
    {
        if (!target)
        {
            target = FindObjectOfType<Character>().transform;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = target.position;
        position.z = -50;
        transform.position = Vector3.Lerp(transform.position, position, speed * Time.deltaTime);
    }
}