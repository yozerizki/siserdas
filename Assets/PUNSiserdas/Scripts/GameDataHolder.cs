using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataHolder : MonoBehaviourPunCallbacks
{
    public static GameDataHolder Instance;

    public bool soundon = true;
    public string mySereal;
    private bool pendingMenuTransition = false;

    public static GameDataHolder GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameDataHolder found = FindObjectOfType<GameDataHolder>();
        if (found != null)
        {
            Instance = found;
            return found;
        }

        GameObject go = new GameObject("GameDataHolder");
        return go.AddComponent<GameDataHolder>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDisable();
    }

    public void CompleteCoinPickupAndExit(string coinName)
    {
        if (pendingMenuTransition) return; // Guard duplicate calls

        mySereal = coinName;
        pendingMenuTransition = true;
        PrepareForLocalMenu();

        Debug.Log("[GameDataHolder] CompleteCoinPickupAndExit: " + coinName);

        PhotonNetwork.AutomaticallySyncScene = false;
        StartCoroutine(ExitToMenuCoroutine());
    }

    private IEnumerator ExitToMenuCoroutine()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[GameDataHolder] LeaveRoom...");
            PhotonNetwork.LeaveRoom();

            // Tunggu sampai keluar dari room (max 5 detik)
            float timeout = 5f;
            while (PhotonNetwork.InRoom && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            Debug.Log("[GameDataHolder] Left room. Loading scenemenu...");
        }

        pendingMenuTransition = false;
        SceneManager.LoadScene("scenemenu");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (!pendingMenuTransition)
            return;

        pendingMenuTransition = false;
        PrepareForLocalMenu();
        SceneManager.LoadScene("scenemenu");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "scenemenu")
            return;

        StartCoroutine(EnsureMenuCameraAfterSceneReady(scene));
    }

    private IEnumerator EnsureMenuCameraAfterSceneReady(Scene scene)
    {
        // Wait until scene objects are fully initialized this frame.
        yield return null;
        yield return new WaitForEndOfFrame();

        if (!scene.isLoaded)
            yield break;

        PrepareForLocalMenu();
        EnsureMenuCamera(scene);
    }

    private void PrepareForLocalMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void EnsureMenuCamera(Scene activeScene)
    {
        Camera activeSceneCamera = null;
        var cameras = FindObjectsOfType<Camera>(true);

        foreach (var camera in cameras)
        {
            if (camera == null)
                continue;

            bool isInActiveScene = camera.gameObject.scene == activeScene;
            if (isInActiveScene && activeSceneCamera == null)
            {
                activeSceneCamera = camera;
            }

            if (!isInActiveScene)
            {
                camera.enabled = false;

                var listener = camera.GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = false;
            }
        }

        bool hasActiveHierarchyCamera = false;
        foreach (var camera in cameras)
        {
            if (camera != null && camera.gameObject.scene == activeScene && camera.gameObject.activeInHierarchy)
            {
                hasActiveHierarchyCamera = true;
                break;
            }
        }

        if (activeSceneCamera == null || !hasActiveHierarchyCamera)
        {
            GameObject fallbackCameraObject = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(fallbackCameraObject, activeScene);
            fallbackCameraObject.tag = "MainCamera";

            Camera fallbackCamera = fallbackCameraObject.AddComponent<Camera>();
            fallbackCamera.clearFlags = CameraClearFlags.Skybox;
            fallbackCamera.transform.position = new Vector3(0f, 1f, -10f);
            fallbackCamera.transform.rotation = Quaternion.identity;

            fallbackCameraObject.AddComponent<AudioListener>();
            fallbackCameraObject.AddComponent<AudioSource>();
            fallbackCameraObject.AddComponent<SoundManager>();
            return;
        }

        activeSceneCamera.gameObject.SetActive(true);
        activeSceneCamera.enabled = true;

        var activeListener = activeSceneCamera.GetComponent<AudioListener>();
        if (activeListener != null)
            activeListener.enabled = true;
    }

}