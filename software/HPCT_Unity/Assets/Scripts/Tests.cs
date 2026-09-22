using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Tests : MonoBehaviour
{   
    [SerializeField] GameObject parentObject;
    [SerializeField] GameObject[] tests;
    [SerializeField] Vector3[] spawnPositions;
     [SerializeField] Quaternion[] spawnRotations = new Quaternion[]
    {
        Quaternion.Euler(0.0f, 0.0f, 0.0f),
        Quaternion.Euler(0.0f, 0.0f, 0.0f),
        Quaternion.Euler(0.0f, 0.0f, 0.0f),
        Quaternion.Euler(0.0f, 90.0f, 0.0f),
        Quaternion.Euler(0.0f, 90.0f, 0.0f),
        Quaternion.Euler(0.0f, 90.0f, 0.0f),
        Quaternion.Euler(0.0f, 180.0f, 0.0f),
        Quaternion.Euler(0.0f, 180.0f, 0.0f),
        Quaternion.Euler(0.0f, 180.0f, 0.0f),
        Quaternion.Euler(0.0f, 0.0f, 0.0f),
        Quaternion.Euler(0.0f, 0.0f, 0.0f),
        Quaternion.Euler(0.0f, 0.0f, 0.0f),
        Quaternion.Euler(0.0f, 0.0f, 0.0f),
        Quaternion.Euler(0.0f, 90.0f, 0.0f),
    };
    
    // Start is called before the first frame update
    void Start()
    {
        spawnTests();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // fills the list of integers that represent each test, and shuffles it for randomized order
    private int[] fillAndShuffleList(){
        
        int[] temp = new int[tests.Length];
        for (int i = 0; i < tests.Length; i++)
        {
            temp[i] = i;
        }
        return ShuffleArray(temp);
    }

    // Shuffle method from https://thomassteffen.medium.com/super-simple-array-shuffle-with-linq-167b317ba035
    static T[] ShuffleArray<T>(T[] array)
    {
        System.Random random = new System.Random();
        return array.OrderBy(x => random.Next()).ToArray();
    }

    private void spawnTests(){
        int[] testsOrder =  fillAndShuffleList();

        foreach(int i in testsOrder){
            Debug.Log(i);
        }
        for (int i = 0; i < tests.Length; i++){
        Debug.Log("at index "+ i);
            GameObject test = Instantiate(tests[testsOrder[i]], this.transform);
            test.transform.localPosition = spawnPositions[i];
            test.transform.localRotation = spawnRotations[i];
        }
    }

}
