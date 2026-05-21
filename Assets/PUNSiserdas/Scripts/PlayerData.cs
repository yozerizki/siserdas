using UnityEngine;
using Photon.Pun;
using EasyPeasyFirstPersonController;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerData : MonoBehaviourPunCallbacks, IPunObservable
{
    public string myCoin = "";
    public bool hasCoin = false;

    [SerializeField] private float remoteLerpSpeed = 12f;
    [SerializeField] private float idleKickSeconds = 30f;
    [SerializeField] private float idlePositionThreshold = 0.05f;
    [SerializeField] private float idleRotationThreshold = 1f;

    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 lastOwnedPosition;
    private Quaternion lastOwnedRotation;
    private float lastActivityTime;
    private bool afkKickInProgress;

    private FirstPersonController firstPersonController;
    private InputManagerOld inputManagerOld;
    private MobileInputManager mobileInputManager;
    private CharacterController characterController;
    private Camera[] childCameras;
    private AudioListener[] childAudioListeners;

    private void Awake()
    {
        firstPersonController = GetComponent<FirstPersonController>();
        inputManagerOld = GetComponent<InputManagerOld>();
        mobileInputManager = GetComponent<MobileInputManager>();
        characterController = GetComponent<CharacterController>();
        childCameras = GetComponentsInChildren<Camera>(true);
        childAudioListeners = GetComponentsInChildren<AudioListener>(true);

        EnsureObservedByPhotonView();
        ConfigureOwnershipComponents();

        networkPosition = transform.position;
        networkRotation = transform.rotation;

        lastOwnedPosition = transform.position;
        lastOwnedRotation = transform.rotation;
        lastActivityTime = Time.unscaledTime;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            TrackOwnedPlayerActivity();
            return;
        }

        transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * remoteLerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, networkRotation, Time.deltaTime * remoteLerpSpeed);
    }

    private void TrackOwnedPlayerActivity()
    {
        if (afkKickInProgress || hasCoin)
            return;

        float movedDistance = Vector3.Distance(transform.position, lastOwnedPosition);
        float rotatedAngle = Quaternion.Angle(transform.rotation, lastOwnedRotation);

        if (movedDistance >= idlePositionThreshold || rotatedAngle >= idleRotationThreshold)
            lastActivityTime = Time.unscaledTime;

        lastOwnedPosition = transform.position;
        lastOwnedRotation = transform.rotation;

        if (!PhotonNetwork.InRoom)
        {
            lastActivityTime = Time.unscaledTime;
            return;
        }

        if (Time.unscaledTime - lastActivityTime < idleKickSeconds)
            return;

        StartAfkKickFlow();
    }

    private void StartAfkKickFlow()
    {
        if (afkKickInProgress)
            return;

        afkKickInProgress = true;

        if (firstPersonController != null)
            firstPersonController.enabled = false;
        if (inputManagerOld != null)
            inputManagerOld.enabled = false;
        if (mobileInputManager != null)
            mobileInputManager.enabled = false;
        if (characterController != null)
            characterController.enabled = false;

        StartCoroutine(LeaveRoomAndLoadLauncherCoroutine());
    }

    private IEnumerator LeaveRoomAndLoadLauncherCoroutine()
    {
        PhotonNetwork.AutomaticallySyncScene = false;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();

            float leaveTimeout = 5f;
            while (PhotonNetwork.InRoom && leaveTimeout > 0f)
            {
                leaveTimeout -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        SceneManager.LoadScene("Launcher");
    }

    private void EnsureObservedByPhotonView()
    {
        if (photonView == null)
            return;

        if (!photonView.ObservedComponents.Contains(this))
            photonView.ObservedComponents.Add(this);

        photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
    }

    private void ConfigureOwnershipComponents()
    {
        bool isMine = photonView.IsMine;

        if (firstPersonController != null)
            firstPersonController.enabled = isMine;

        if (inputManagerOld != null)
            inputManagerOld.enabled = isMine;

        if (mobileInputManager != null)
            mobileInputManager.enabled = isMine;

        if (characterController != null)
            characterController.enabled = isMine;

        for (int i = 0; i < childCameras.Length; i++)
        {
            if (childCameras[i] != null)
                childCameras[i].enabled = isMine;
        }

        for (int i = 0; i < childAudioListeners.Length; i++)
        {
            if (childAudioListeners[i] != null)
                childAudioListeners[i].enabled = isMine;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void ReceiveCoin(string coinName)
    {
        if (!photonView.IsMine || hasCoin)
            return;

        myCoin = coinName;
        hasCoin = true;

        Debug.Log("[PlayerData] ReceiveCoin: " + coinName);

        // Pastikan GameDataHolder ada dan siap SEBELUM destroy player
        GameDataHolder holder = GameDataHolder.GetOrCreate();

        // Destroy player object di semua screen
        PhotonNetwork.Destroy(gameObject);

        // Trigger scene transition - coroutine jalan di GameDataHolder (DontDestroyOnLoad)
        holder.CompleteCoinPickupAndExit(coinName);
    }
}