using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TouchProcessor : MonoBehaviour
{
    private Vector3 previousPosition;
    private Vector3 velocity;
    private Queue<float> velocitySamples = new Queue<float>();
    private const int sampleSize = 5;
    private int counter = 0;
    private float maxVelocity = 1.5f;
    private bool entered = false;

    // Solidity settings
    int[] hardnessSettings_0 = {150};  // Hard
    int[] hardnessSettings_1 = {105, 120, 138, 180 };  // Soft
    int[] hardnessSettings_2 = {100};  // No force/light tension

    // Vibration settings
    int maxBumps = 20;
    int maxRoughness = 150;
    int prevRoughness = 0;
    int prevBumps = 0; 
    int prevServo = 0;

    // Temperature variables
    int heatScale = 20;
    int freezeScale = 10;
    int roomTemp = 35;

    void Start()
    {
        previousPosition = transform.position;
    }

    /* The finger's velocity is smoothed by averaging the latest values
     */
    void Update()
    {
        velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;

        velocitySamples.Enqueue(velocity.magnitude);
        while (velocitySamples.Count > sampleSize)
        {
            velocitySamples.Dequeue();
        }
    }

    /* When the finger touches an object
     * gather haptic information from the object
     * send it over UDP to microcontroller
     */
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Touched object: " + other.gameObject.name);

        HapticInfo hapticInfo = other.gameObject.GetComponent<HapticInfo>();
        if (hapticInfo != null)
        {
            int roughness = (int)(hapticInfo.Roughness * 205 + 50);
            int bumpSize = (int)(hapticInfo.BumpSize * 200);
            int temperature = roomTemp;

            if (hapticInfo.Temperature > 0)
            {
                temperature = (int)(hapticInfo.Temperature * heatScale + roomTemp);
            }
            else if (hapticInfo.Temperature < 0)
            {
                temperature = (int)(hapticInfo.Temperature * freezeScale + roomTemp);
            }

                Debug.Log("Haptic Info of " + other.gameObject.name + ": " +
                    "Temperature " + temperature + ", " +
                    "Roughness " + roughness + ", " +
                    "BumpsSize " + bumpSize);
            
            if (velocitySamples.Count > sampleSize)
            {
                velocitySamples.Dequeue();
                velocitySamples.Enqueue(0);
            }

            int repeat = 1;
            while (repeat > 0)
            {
                CommunicationController.Instance.SendMsg("1," +
                    temperature.ToString() + "," +
                    roughness.ToString() + "," +
                    bumpSize.ToString());
                repeat -= 1;
            }
            entered = true;
            velocitySamples.Clear();
            velocitySamples.Enqueue(-0.5f);
        }
    }

    /* Send finger's calculated velocity 
     * over UDP to microcontroller
     */
    void OnTriggerStay(Collider other)
    {
        HapticInfo hapticInfo = other.gameObject.GetComponent<HapticInfo>();
        if ((hapticInfo != null) && entered)
        {
            while (counter < sampleSize)
            {
                counter += 1;
                return;
            }
            counter = 0;

            // Both the bump and roughness frequencies are calculated
            // based on the finger's velocity range and the haptic information (1/3, 2/3 or 3/3)
            // This scales roughness from 25-50, 50-100, 75-150
            float averageVelocity = CalculateAverageVelocity();

            // Bumpiness
            int bumpsFrequency = 0;

            // int value = UnityEngine.Random.Range(0, 3); //Irregular bumps
            int value = 0 // No irregularities 

            if (averageVelocity >= (maxVelocity * 1))
            {
                bumpsFrequency = (int)(hapticInfo.BumpDensity * maxBumps * 1) + value;
            }
            else if (averageVelocity > (maxVelocity * 0.65))
            {
                bumpsFrequency = (int)(hapticInfo.BumpDensity * maxBumps * 0.75) + value;
            }
            else if (averageVelocity > (maxVelocity * 0.20))
            {
                bumpsFrequency = (int)(hapticInfo.BumpDensity * maxBumps * 0.4) + value;
            }
            else
            {
                bumpsFrequency = 0;
            }

            // Roughness
            int roughnessFrequency = 0;
            if (averageVelocity >= (maxVelocity * 0.55))
            {
                roughnessFrequency = (int)(hapticInfo.RoughnessDensity * maxRoughness * 1);
            }
            else if (averageVelocity > (maxVelocity * 0.37))
            {
                roughnessFrequency = (int)(hapticInfo.RoughnessDensity * maxRoughness * 0.75);
            }
            else if (averageVelocity > (maxVelocity * 0.18))
            {
                roughnessFrequency = (int)(hapticInfo.RoughnessDensity * maxRoughness * 0.66);
            }
            else
            {
                roughnessFrequency = 0;
            }

            // P-controller to adjust the servo angle based on the depth of the finger
            /*
            float colliderDistance = Vector3.Distance(transform.position, 
                other.ClosestPoint(transform.position));
            float depth = 1 - (10000 * (colliderDistance)) / 237;

            int goalAngle = (int) (180 * depth);
            int currentAngle = hapticInfo.getActualAngle();
            int angleOffset = goalAngle - currentAngle;

            int servoAngle = (int)(currentAngle + angleOffset * ((hapticInfo.Hardness + 0.1) / 1.1)); */

            // Adjustable force range: Harder material = shorter range
            float colliderDistance = Vector3.Distance(transform.position,
                other.ClosestPoint(transform.position));
            float depth = 1 - (10000 * (colliderDistance)) / 237;
            float maxDepth = 1.001f - hapticInfo.Hardness;
            int servoAngle = 0;
            
            if (depth < 0)
            {
                depth = - depth;
            }

            if ((hapticInfo.Hardness == 1) && (depth > 0.35f)) // Hard
            { 
                servoAngle = hardnessSettings_0[0];
            }
            else if (hapticInfo.Hardness == 0.5) // Soft
            {
                
                if (depth < 0.33f)
                {
                    servoAngle = hardnessSettings_1[0]; 
                }
                else if (depth < 0.66f)
                {
                    servoAngle = hardnessSettings_1[1]; 
                }
                else
                {
                    servoAngle =  hardnessSettings_1[2]; 
                }

            }
            else if ((hapticInfo.Hardness == 0) && (depth > 0.35f)) // No force/light tension
            {
                servoAngle = hardnessSettings_2[0];
            }

            if ((servoAngle != prevServo) || (roughnessFrequency != prevRoughness) || (bumpsFrequency != prevBumps))
            {
                prevServo = servoAngle;
                prevBumps = bumpsFrequency;
                prevRoughness = roughnessFrequency;
                print("Updated values: 2," +
                    roughnessFrequency.ToString() + "," +
                    bumpsFrequency.ToString() + "," +
                    (180 - servoAngle).ToString());
                print("Speed = " + averageVelocity);

                CommunicationController.Instance.SendMsg("2," +
                roughnessFrequency.ToString() + "," +
                bumpsFrequency.ToString() + "," +
                (180 - servoAngle).ToString());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        HapticInfo hapticInfo = other.gameObject.GetComponent<HapticInfo>();
        if (hapticInfo != null)
        {
            int repeat = 3;
            while (repeat > 0)
            {
                CommunicationController.Instance.SendMsg("0,0,0,0");
                repeat -= 1;
            }
            velocitySamples.Clear();
            counter = 0;
            prevServo = 0;
            prevBumps = 0;
            prevRoughness = 0;
            entered = false;
        }
    }

    private float CalculateAverageVelocity()
    {
        float sum = 0f;
        foreach (float sample in velocitySamples)
        {
            sum += sample;
        }
        return sum;
    }
}
