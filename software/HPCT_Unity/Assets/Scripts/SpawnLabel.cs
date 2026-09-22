using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnLabel : MonoBehaviour
{
    public GameObject labelPrefab; 
    public GameObject canvas;
    public Vector3 offset = new Vector3(0, 2f, 0); 

    void Start()
    {
        if (labelPrefab == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + offset);

        GameObject labelInstance = Instantiate(labelPrefab, screenPos, Quaternion.identity);

        labelInstance.transform.SetParent(canvas.transform, worldPositionStays: false);
    }
}
