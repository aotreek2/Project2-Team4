using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBlinking : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("GroupFlipFlop", 0f, .75f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GroupFlipFlop()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
