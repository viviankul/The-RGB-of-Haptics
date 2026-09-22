using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Xml;
using System.Xml.Linq;

public class SaveData : MonoBehaviour
{
    public Button quitButton;
    private bool incomplete = false;
    public GameObject warningText;
    public bool test1 = true;
    private string xmlFilePath;
    private XDocument xmlDoc;
    int userID = 1;
    bool gotFile1 = false;
    bool gotFile2 = false;

    void Start()
    {
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(FinishTest);
        }
        else
        {
            Debug.LogError("Quit button not assigned");
        }
    }

    void FinishTest()
    {
        if (test1)
        {
            SaveData1();
            if (!incomplete)
                {
                    SceneManager.LoadScene("MaterialTests");
                }
        }
        else
        {
            SaveData2();
            if (!incomplete)
                {
                    QuitGame();
                }
        }
    }

    void SaveData1()
    {
        List<string> testNames = new List<string>
        {
            "Test1A_1(Clone)",
            "Test1A_2(Clone)",
            "Test1B_1(Clone)",
            "Test1B_2(Clone)",
            "Test1C_1(Clone)",
            "Test1C_2(Clone)",
            "Test1D_1(Clone)",
            "Test1D_2(Clone)"
        };

        foreach (string testName in testNames)
        {
            GameObject testObject = GameObject.Find(testName);
            if (testObject != null)
            {
                Debug.Log("Saving results of " + testName);
                GameObject content = testObject.gameObject.transform.GetChild(1).GetChild(1).GetChild(3).
                GetChild(0).GetChild(0).gameObject;

                List<int> testValues = new List<int>();
                for (int i = 0; i < content.transform.childCount; i++)
                {
                    GameObject question = content.transform.GetChild(i).GetChild(1).gameObject;
                    int testValue = GetDropdownValue(question);
                    if (testValue == -1)
                    {
                        incomplete = true;
                        break;
                    }
                    else
                    {
                        testValues.Add(testValue);
                    }
                }

                if (!incomplete)
                {
                    SaveTestResultsToXml(testName, testValues);
                }
                else
                {
                    StartCoroutine(ShowWarningText());
                }
            }
            else
            {
                Debug.LogWarning("GameObject not found: " + testName);
            }
        }
    }

    void SaveData2()
    {
        List<string> testNames = new List<string>
        {
            "Water Bowl Test 1",
            "Water Bowl Test 2",
            "Metal Test 1",
            "Metal Test 2",
            "Wood Test 1",
            "Wood Test 2",
            "Wood Test 3"
        };

        foreach (string testName in testNames)
        {
            GameObject testObject = GameObject.Find(testName);
            if (testObject != null)
            {
                Debug.Log("Saving results of " + testName);
                GameObject panels = testObject.gameObject.transform.GetChild(0).gameObject;

                List<List<int>> testValues = new List<List<int>>();
                for (int i = 0; i < panels.transform.childCount; i++)
                {
                    if (!incomplete)
                    {
                        GameObject panel = panels.transform.GetChild(i).gameObject;
                        GameObject content = panel.gameObject.transform.GetChild(1).GetChild(3).
                        GetChild(0).GetChild(0).gameObject;

                        List<int> panelValues = new List<int>();
                        for (int j = 0; j < content.transform.childCount; j++)
                        {
                            GameObject question = content.transform.GetChild(j).GetChild(1).gameObject;
                            int testValue = GetDropdownValue(question);
                            if (testValue == -1)
                            {
                                incomplete = true;
                                break;
                            }
                            else
                            {
                                panelValues.Add(testValue);
                            }
                        }
                        if (!incomplete)
                        {
                            testValues.Add(panelValues);
                        }
                    }


                }

                if (!incomplete)
                {
                    SaveTestResultsToXml(testName, testValues);
                }
                else
                {
                    StartCoroutine(ShowWarningText());
                }
            }
            else
            {
                Debug.LogWarning("GameObject not found: " + testName);
            }
        }
    }

    int GetDropdownValue(GameObject dropdown)
    {
        int value = dropdown.GetComponent<TMPro.TMP_Dropdown>().value;
        print("Dropdown value: " + value);
        return value;
    }

    void QuitGame()
    {
        Debug.Log("Exiting Game Automatically");

#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SaveTestResultsToXml(string testName, List<int> testValues)
    {
        // Save data for test 1
        while (!gotFile1)
        {
            xmlFilePath = "Test1.xml";

            if (System.IO.File.Exists("TestResults/" + userID + "_" + xmlFilePath))
            {
                userID++;
            }
            else
            {
                xmlDoc = new XDocument(new XElement("TestResults"));
                xmlFilePath = "TestResults/" + userID + "_" + xmlFilePath;
                gotFile1 = true;
                break;
            }
        }

        XElement testElement = new XElement("Test",
            new XAttribute("name", testName),
            ArrayToElements(testValues)
        );

        xmlDoc.Root.Add(testElement);
        xmlDoc.Save(xmlFilePath);
    }

    void SaveTestResultsToXml(string testName, List<List<int>> testValues)
    {
        // Save data for test 2
        while (!gotFile2)
        {
            xmlFilePath = "Test2.xml";

            if (System.IO.File.Exists("TestResults/" + userID + "_" + xmlFilePath))
            {
                userID++;
            }
            else
            {
                xmlDoc = new XDocument(new XElement("TestResults"));
                xmlFilePath = "TestResults/" + userID + "_" + xmlFilePath;
                gotFile2 = true;
                break;
            }
        }

        XElement testElement = new XElement("Test",
            new XAttribute("name", testName),
            ArrayToElements(testValues)
        );

        xmlDoc.Root.Add(testElement);
        xmlDoc.Save(xmlFilePath);
    }

    XElement ArrayToElements(List<int> testValues)
    {
        XElement values = new XElement("Values");
        foreach (int value in testValues)
        {
            values.Add(new XElement("Value", value));
        }
        return values;
    }

    XElement ArrayToElements(List<List<int>> testValues)
    {
        XElement values = new XElement("Values");
        foreach (List<int> panel in testValues)
        {
            foreach (int value in panel)
            {
                values.Add(new XElement("Value", value));
            }
        }
        return values;
    }

    private IEnumerator ShowWarningText()
    {
        warningText.SetActive(true);
        yield return new WaitForSeconds(3f);
        warningText.SetActive(false);
        incomplete = false;
    }
}
