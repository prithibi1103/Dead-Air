
using UnityEngine;
using UnityEngine.UI;

public class MenuClickSounds : MonoBehaviour
{
    public Transform mainMenuPanel;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (mainMenuPanel == null || audioSource == null)
        {
            Debug.LogWarning("Menu audio setup is incomplete!");
            return;
        }

        Button[] buttons = mainMenuPanel.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    void PlayClickSound()
    {
        if (audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}

