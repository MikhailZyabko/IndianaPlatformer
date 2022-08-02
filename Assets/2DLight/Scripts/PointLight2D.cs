using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointLight2D : MonoBehaviour
{
    public float radius;
    public Color color;
    public Texture cookie;

    void OnEnable()
    {
        Light2D.plList.Add(this);
    }

    void OnDisable()
    {
        Light2D.plList.Remove(this);
    }
}
