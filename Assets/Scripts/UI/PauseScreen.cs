using UnityEngine;
using UnityEditor;

public class PauseScreen : MonoBehaviour
{

    [SerializeField] Player player;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    public void onResume()
    {
        player.isPause = false;
        gameObject.SetActive(false);
    }

    public void onQuit()
    {
        Application.Quit();
    }
}
