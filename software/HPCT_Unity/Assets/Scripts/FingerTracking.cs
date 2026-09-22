using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTracking : MonoBehaviour
{
    private static FingerTracking _instance;
    public static FingerTracking Instance{
        get{
            if (_instance == null){
                Debug.LogError("FingerTracking instance is null");
            }
            return _instance;
        }
    }

    private void Awake(){
        _instance = this;
    }

    [Range(0.0f, 1.0f)] 
    public float flex = 0f;

    // Assign in the inspector
    public GameObject indexBase;
    public GameObject indexMid;
    public GameObject indexTip;

    private readonly float rotationLimit = 90f;

    // Start is called before the first frame update
    void Start(){
        indexBase = this.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        indexMid = indexBase.gameObject.transform.GetChild(0).gameObject;
        indexTip = indexMid.gameObject.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    public void Update()
    {
        indexBase.transform.localEulerAngles = Vector3.right * flex * rotationLimit / 2;
        indexMid.transform.localEulerAngles = Vector3.right * flex * rotationLimit;
        indexTip.transform.localEulerAngles = Vector3.right * flex * rotationLimit;
    }
    public void updatePose(float voltage)
    {
        flex = 1f - (voltage-1.4f)/1.9f;
    }
}