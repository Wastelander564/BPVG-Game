using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class screenUIs : MonoBehaviour
{
    public GameObject guidePanel; // Reference to the guide panel GameObject

    // Start is called before the first frame update
    void Start()
    {
        // Hide the guide panel initially
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SwitchToOverWorldScene()
    {
        SceneManager.LoadScene("OverWorld");
    }
    public void ToggleGuidePanel()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(true); // Toggle visibility
        }
    }
    public void CloseGuidePanel()
    {
        guidePanel.SetActive(false); // Hide the guide panel
    }
    public void ReturnToStartScreen()
    {
        SceneManager.LoadScene("StartScreen");
    }
}
