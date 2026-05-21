using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class scenemanager : MonoBehaviour
{
    private const string GameplaySceneName = "GameScene";
    private const string ExitPanelTag = "panelexit";

    AudioSource sfx;
    GameObject panelexit;
    private void Start()
    {
        sfx = this.gameObject.GetComponent<AudioSource>();
        RefreshExitPanel();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshExitPanel();
    }

    private void RefreshExitPanel()
    {
        TryResolveExitPanel();

        if (panelexit != null)
            panelexit.SetActive(false);

        if (ShouldManageCursorForGameplay())
            LockCursorForGameplay();
    }

    private void Update()
    {
        if (IsEscapeReleasedThisFrame()) {
            exitpressed();
        }
    }

    private bool IsEscapeReleasedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasReleasedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyUp(KeyCode.Escape);
#else
        return false;
#endif
    }

    public void restartscene() {
        StartCoroutine(waitandchangescene(SceneManager.GetActiveScene().name));
    }

    public void gotokuis() {
        StartCoroutine(waitandchangescene("scenekuis"));
    }
    public void gotomenu() {
        StartCoroutine(waitandchangescene("scenemenu"));
    }

    public void gotolauncher() {
        StartCoroutine(waitandchangescene("Launcher"));
    }



    public void exitpressed() {
        TryResolveExitPanel();

        if (panelexit != null)
            panelexit.SetActive(true);

        if (ShouldManageCursorForGameplay())
            UnlockCursorForUI();
    }

    public void cancelexitgame()
    {
        TryResolveExitPanel();

        if (panelexit != null)
            panelexit.SetActive(false);

        if (ShouldManageCursorForGameplay())
            LockCursorForGameplay();
    }

    public void exitgame()
    {
        Application.Quit();
    }

    public string getThissceneName() {
        string a = SceneManager.GetActiveScene().name;
        return a;
    }

    IEnumerator waitandchangescene(string namascene)
    {

        //Wait Until Sound has finished playing
        while (sfx.isPlaying)
        {
            yield return null;
        }

        // Saat keluar dari gameplay ke menu/launcher, pastikan client tidak tertinggal di room lama.
        if ((namascene == "scenemenu" || namascene == "Launcher") && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();

            float leaveTimeout = 5f;
            while (PhotonNetwork.InRoom && leaveTimeout > 0f)
            {
                leaveTimeout -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        //Audio has finished playing, disable GameObject
        SceneManager.LoadScene(namascene);
    }

    private void UnlockCursorForUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockCursorForGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool ShouldManageCursorForGameplay()
    {
        return IsGameplaySceneActive() && IsDesktopOrWebPlatform() && IsFpsControllerActive();
    }

    private bool IsGameplaySceneActive()
    {
        return SceneManager.GetActiveScene().name == GameplaySceneName;
    }

    private bool IsDesktopOrWebPlatform()
    {
        RuntimePlatform platform = Application.platform;
        return platform == RuntimePlatform.WindowsPlayer
               || platform == RuntimePlatform.OSXPlayer
               || platform == RuntimePlatform.LinuxPlayer
               || platform == RuntimePlatform.WindowsEditor
               || platform == RuntimePlatform.OSXEditor
               || platform == RuntimePlatform.LinuxEditor
               || platform == RuntimePlatform.WebGLPlayer
               || Application.isEditor;
    }

    private bool IsFpsControllerActive()
    {
        var fpsController = FindObjectOfType<EasyPeasyFirstPersonController.FirstPersonController>();
        return fpsController != null && fpsController.isActiveAndEnabled;
    }

    private bool TryResolveExitPanel()
    {
        if (panelexit != null)
            return true;

        panelexit = GameObject.FindGameObjectWithTag(ExitPanelTag);
        if (panelexit != null)
            return true;

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
            return false;

        GameObject[] roots = activeScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] children = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < children.Length; j++)
            {
                if (children[j] == null)
                    continue;

                if (children[j].CompareTag(ExitPanelTag))
                {
                    panelexit = children[j].gameObject;
                    return true;
                }
            }
        }

        return false;
    }

}
