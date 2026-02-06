using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;

    [Header("Audio")]
    [SerializeField] private AudioSource musicAudioSource;

    private void Start()
    {
        // Pastikan settings panel tidak aktif di awal
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Setup button listeners
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        // Setup volume slider
        if (volumeSlider != null)
        {
            // Load saved volume atau set default
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            volumeSlider.value = savedVolume;
            
            if (musicAudioSource != null)
            {
                musicAudioSource.volume = savedVolume;
            }

            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    private void Update()
    {
        // Cek jika Escape ditekan untuk menutup settings panel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettingsPanel();
            }
        }
    }

    private void OnStartButtonClicked()
    {
        // Pindah ke scene SampeScene
        SceneManager.LoadScene("SampleScene");
    }

    private void OnSettingsButtonClicked()
    {
        // Tampilkan settings panel
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    private void OnExitButtonClicked()
    {
        // Keluar dari game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnVolumeChanged(float value)
    {
        // Update volume musik
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = value;
        }

        // Simpan volume setting
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    private void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Cleanup listeners untuk menghindari memory leak
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}
