﻿using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour
{
    public static GameObject instance;
    public TransformAnimator animator;
    float animationDelay = 0.5f;

    void Awake()
    {
        instance = gameObject;
        animator = GetComponent<TransformAnimator>();
    }

    public void MoveTo(Transform t) {
        transform.SetParent(t.transform, worldPositionStays: true);
        animator.Animate(new TimedValue<TransformState>(new TransformState(Vector3.zero, Quaternion.identity, Vector3.one), TimeManager.GameTime + animationDelay));
    }

    public void MoveToInstant(Transform t) {
        transform.SetParent(t.transform, worldPositionStays: false);
    }
}