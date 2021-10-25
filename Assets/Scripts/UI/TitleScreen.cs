using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleScreen : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject camera;
    [SerializeField] private TMP_Text splashText;
    [Header("Variables")]
    [SerializeField] private string[] splashTexts;

    private float y = 0;

    private void Start()
    {
        mainScreen = GameObject.Find("Canvas/MainScreen");
        settingsScreen = GameObject.Find("Canvas/SettingsScreen");
        splashText = GameObject.Find("Canvas/MainScreen/SplashText").GetComponent<TMP_Text>();
        System.Random r = new System.Random();
        int i = r.Next(0, splashTexts.Length);
        splashText.text = splashTexts[i];
    }

    private void Update()
    {
        Vector3 rotateValue = new Vector3(0f, y * -1, 0f);
        camera.transform.eulerAngles = transform.eulerAngles - rotateValue;
        y = y + 0.01F;
    }

    public void onClickSolo()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
    }

    public void onClickMultiplayer()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(2));
    }

    public void onClickSettings()
    {
        mainScreen.SetActive(!mainScreen.activeSelf);
        settingsScreen.SetActive(!settingsScreen.activeSelf);
    }

    public void onClickQuit()
    {
        Application.Quit();
    }
}
