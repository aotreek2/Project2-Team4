using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position += new Vector3(-1 * speed * Time.deltaTime, 0, 0);
        if (this.transform.position.x <= -200f)
        {
            Destroy(this.gameObject);
        } 
    }
}
