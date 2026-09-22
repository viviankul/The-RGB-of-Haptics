using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;

[CustomPropertyDrawer(typeof(SnappedRangeAttribute))]
public class SnappedRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Float)
        {
            SnappedRangeAttribute range = (SnappedRangeAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            // Draw the slider
            float value = EditorGUI.Slider(position, label, property.floatValue, range.Min, range.Max);

            // Snap to nearest step based on resolution
            float step = (range.Max - range.Min) / range.Resolution;
            property.floatValue = Mathf.Round(value / step) * step;

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Not a float value.");
        }
    }
}

public class SnappedRangeAttribute : PropertyAttribute
{
    public float Min { get; }
    public float Max { get; }
    public int Resolution { get; }

    public SnappedRangeAttribute(float min, float max, int resolution)
    {
        Min = min;
        Max = max;
        Resolution = resolution;
    }
}

[CustomEditor(typeof(HapticInfo))]
public class DropdownEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        HapticInfo script = (HapticInfo)target;

        GUIContent arrayLabel = new GUIContent("Presets");
        int newArrayIdx = EditorGUILayout.Popup(arrayLabel, script.arrayIdx, script.Presets);

        if (newArrayIdx != script.arrayIdx)
        {
            script.arrayIdx = newArrayIdx;
            UpdatePresetValues(script);
        }
    }

    private void UpdatePresetValues(HapticInfo script)
    {
        switch (script.arrayIdx)
        {
            case 0:
                // Custom
                script.Roughness = 0f;
                script.RoughnessDensity = 0f;
                script.BumpSize = 0f;
                script.BumpDensity = 0f;
                script.Hardness = 0;
                script.Temperature = 0f;
                break;
            case 1:
                // Wood
                script.Roughness = 0.66f;
                script.RoughnessDensity = 0.80f;
                script.BumpSize = 0.5f;
                script.BumpDensity = 0.20f;
                script.Hardness = 1;
                script.Temperature = 0.1f;
                break;
            case 2:
                // Steel
                script.Roughness = 0.33f;
                script.RoughnessDensity = 0.80f;
                script.BumpSize = 0f;
                script.BumpDensity = 0f;
                script.Hardness = 1;
                script.Temperature = -0.7f;
                break;
        }
    }
}

public class HapticInfo : MonoBehaviour
{
    [HideInInspector]
    public int arrayIdx = 0;
    [HideInInspector]
    public string[] Presets = new string[] { "Custom", "Wood", "Steel" };

    [SnappedRange(0.0f, 1.0f, 2)] public float Roughness = 0f;
    [SnappedRange(0.0f, 1.0f, 2)] public float RoughnessDensity = 0f;
    [SnappedRange(0.0f, 1.0f, 2)] public float BumpSize = 0f;
    [SnappedRange(0.0f, 1.0f, 2)] public float BumpDensity = 0f;
    [SnappedRange(0.0f, 1.0f, 2)] public float Hardness = 0;
    [SnappedRange(-1.0f, 1.0f, 4)] public float Temperature = 0f;

    public Boolean sendData = false;
    public Boolean activate = false;
    public Boolean deactivate = false;

    void Update()
    {
        // Buttons for testing purposes
        if (sendData){
            CommunicationController.Instance.SendMsg("1," +
                (Temperature * 10 + 25).ToString()+ "," +
                (Roughness * 255).ToString() + "," +
                (BumpSize * 255).ToString());
            sendData = false;
            CommunicationController.Instance.SendMsg("2," +
                "0," +
                "0" +
                "130");
            activate = false;
        }
        if (activate){
            CommunicationController.Instance.SendMsg("2," +
                "100," +
                "6," +
                "130");
            activate = false;
        }
        if (deactivate){
            CommunicationController.Instance.SendMsg("0,0,0,0");
            deactivate = false;
        }
    }
}
