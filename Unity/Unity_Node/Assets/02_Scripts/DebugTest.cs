using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTest : MonoBehaviour
{
    public int Health = 0;

    private void Start()
    {
        for(int i =0; i<10; i++)
        {
            HealCounter();
        }
    }

    public void HealCounter()
    {
        Health += 10;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Health -= 5;
        }
        
    }
}  
