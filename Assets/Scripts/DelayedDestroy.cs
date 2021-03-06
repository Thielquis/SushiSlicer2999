﻿using UnityEngine;
using System.Collections;

public class DelayedDestroy : MonoBehaviour {

    [Range(0.01f, 10.0f)]
    public float delay;

    public void Start()
    {
        Destroy(gameObject, delay);
    }

}
