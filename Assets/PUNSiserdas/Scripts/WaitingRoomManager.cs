using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomManager : MonoBehaviourPunCallbacks
{
    private const int RoomTimeoutSeconds = 30;
    private const int RoomMaxPlayers = 8;
    private const string RoomCountdownKey = "roomCountdownSec";
    private const string RoomCountdownRunningKey = "roomCountdownRunning";

    [Header("Input Panel")]
    public GameObject panelInputNama;
    public TMP_InputField inputNama;

    [Header("Waiting Room UI")]
    public GameObject panelWaitingRoom;
    public TMP_Text textPlayerCount;
    public TMP_Text textStatus;
    public TMP_Text textPlayerList;
    public TMP_Text textTimeoutRemaining;
    public Button startButton;

    private int syncedCountdownSeconds = -1;
    private bool syncedCountdownRunning = false;
    private bool timeoutTriggered = false;
    private float hostTickAccumulator = 0f;
    private float nextTimeoutTextResolveAt = 0f;
    private float nextCountdownPullAt = 0f;
    private int lastRenderedSecond = -1;
    private Coroutine roomActiveCountdownRoutine;
    private Coroutine timeoutMessageRoutine;
    private Coroutine disconnectMessageRoutine;
    private Coroutine connectWatchdogRoutine;
    private bool isHandlingTimeoutFlow;
    private bool isHandlingDisconnectFlow;
    private bool suppressNextDisconnectedUI;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (panelInputNama != null)
            panelInputNama.SetActive(true);
        if (panelWaitingRoom != null)
            panelWaitingRoom.SetActive(false);

        if (textStatus != null)
            textStatus.text = "Masukkan nama kelompok";
        if (textPlayerCount != null)
            textPlayerCount.text = "(0 / " + RoomMaxPlayers + ")";
        if (textPlayerList != null)
            textPlayerList.text = "";

        TryResolveStartButtonReference();
        TryResolveTimeoutTextReference(force: true);
        SetTimeoutText(string.Empty);
    }

    private void Update()
    {
        RefreshStartButtonVisibility();

        if (!PhotonNetwork.InRoom)
            return;

        if (textTimeoutRemaining == null && Time.unscaledTime >= nextTimeoutTextResolveAt)
        {
            TryResolveTimeoutTextReference(force: false);
            nextTimeoutTextResolveAt = Time.unscaledTime + 0.5f;
        }

        if (Time.unscaledTime >= nextCountdownPullAt)
        {
            PullCountdownFromRoomProperties();
            nextCountdownPullAt = Time.unscaledTime + 0.5f;
        }

        if (PhotonNetwork.IsMasterClient)
            TickHostCountdown();

        RenderCountdownAndHandleTimeout();
    }

    public void OnClickMasuk()
    {
        string nama = inputNama != null ? inputNama.text : string.Empty;
        if (string.IsNullOrEmpty(nama))
        {
            Debug.LogWarning("Nama tidak boleh kosong");
            return;
        }

        PhotonNetwork.NickName = nama;

        if (panelInputNama != null)
            panelInputNama.SetActive(false);
        if (panelWaitingRoom != null)
            panelWaitingRoom.SetActive(true);

        if (textStatus != null)
            textStatus.text = "Menghubungkan...\nPastikan perangkat terhubung ke Internet";

        if (textPlayerCount != null)
            textPlayerCount.text = "(0 / " + RoomMaxPlayers + ")";

        StartConnectWatchdog();

        if (PhotonNetwork.IsConnectedAndReady)
            JoinOrCreateWaitingRoom();
        else
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        JoinOrCreateWaitingRoom();
    }

    public override void OnJoinedRoom()
    {
        StopConnectWatchdog();

        timeoutTriggered = false;
        hostTickAccumulator = 0f;
        lastRenderedSecond = -1;
        nextTimeoutTextResolveAt = 0f;
        nextCountdownPullAt = 0f;

        if (panelWaitingRoom != null)
            panelWaitingRoom.SetActive(true);

        TryResolveTimeoutTextReference(force: false);
        EnsureCountdownInitializedByHost();
        PullCountdownFromRoomProperties();
        RenderCountdownAndHandleTimeout();
        UpdateUI();
        RefreshStartButtonVisibility();
    }

    public override void OnLeftRoom()
    {
        StopConnectWatchdog();
        ResetToInputState();
        RefreshStartButtonVisibility();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        StopConnectWatchdog();

        if (suppressNextDisconnectedUI)
        {
            suppressNextDisconnectedUI = false;
            ResetToInputState();
            RefreshStartButtonVisibility();
            return;
        }

        if (isHandlingTimeoutFlow)
        {
            isHandlingTimeoutFlow = false;
            RefreshStartButtonVisibility();
            return;
        }

        StartDisconnectedCountdown();
        RefreshStartButtonVisibility();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        StopConnectWatchdog();
        StartRoomActiveCountdown();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        StopConnectWatchdog();
        StartRoomActiveCountdown();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        hostTickAccumulator = 0f;
        UpdateUI();
        PullCountdownFromRoomProperties();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged == null)
            return;

        bool changed = false;

        if (propertiesThatChanged.TryGetValue(RoomCountdownKey, out object secObj) && secObj is int sec)
        {
            syncedCountdownSeconds = sec;
            changed = true;
        }

        if (propertiesThatChanged.TryGetValue(RoomCountdownRunningKey, out object runningObj) && runningObj is bool running)
        {
            syncedCountdownRunning = running;
            changed = true;
        }

        if (changed)
        {
            lastRenderedSecond = -1;
            RenderCountdownAndHandleTimeout();
        }
    }

    private void UpdateUI()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            RefreshStartButtonVisibility();
            return;
        }

        int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        int maxPlayers = RoomMaxPlayers;

        if (textPlayerCount != null)
            textPlayerCount.text = "Jumlah Kelompok: (" + currentPlayers + "/" + maxPlayers + ")";

        if (textPlayerList != null)
        {
            string joinedNames = string.Empty;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                Player p = PhotonNetwork.PlayerList[i];
                string nick = p != null ? p.NickName : string.Empty;

                if (string.IsNullOrEmpty(joinedNames))
                    joinedNames = nick;
                else
                    joinedNames += ", " + nick;
            }

            textPlayerList.text = "Daftar Kelompok: (" + joinedNames + ")";
        }

        if (textStatus != null)
            textStatus.text = "menunggu player join room";

        RefreshStartButtonVisibility();
    }

    private void JoinOrCreateWaitingRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(
            "Room_Siserdas",
            CreateWaitingRoomOptions(),
            TypedLobby.Default
        );
    }

    private RoomOptions CreateWaitingRoomOptions()
    {
        return new RoomOptions
        {
            MaxPlayers = RoomMaxPlayers,
            EmptyRoomTtl = 0,
            PlayerTtl = 0,
            CleanupCacheOnLeave = true
        };
    }

    private void EnsureCountdownInitializedByHost()
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
            return;

        PullCountdownFromRoomProperties();

        if (syncedCountdownRunning && syncedCountdownSeconds > 0)
            return;

        syncedCountdownSeconds = RoomTimeoutSeconds;
        syncedCountdownRunning = true;
        hostTickAccumulator = 0f;
        PushCountdownToRoomProperties();
    }

    private void PullCountdownFromRoomProperties()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;

        if (props.TryGetValue(RoomCountdownKey, out object secObj) && secObj is int sec)
            syncedCountdownSeconds = sec;

        if (props.TryGetValue(RoomCountdownRunningKey, out object runningObj) && runningObj is bool running)
            syncedCountdownRunning = running;
    }

    private void TickHostCountdown()
    {
        if (!syncedCountdownRunning || syncedCountdownSeconds <= 0)
            return;

        hostTickAccumulator += Time.unscaledDeltaTime;
        if (hostTickAccumulator < 1f)
            return;

        int elapsedWholeSeconds = Mathf.FloorToInt(hostTickAccumulator);
        hostTickAccumulator -= elapsedWholeSeconds;

        int newValue = Mathf.Max(0, syncedCountdownSeconds - elapsedWholeSeconds);
        if (newValue == syncedCountdownSeconds)
            return;

        syncedCountdownSeconds = newValue;
        if (syncedCountdownSeconds <= 0)
            syncedCountdownRunning = false;

        PushCountdownToRoomProperties();
    }

    private void PushCountdownToRoomProperties()
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
            return;

        Hashtable props = new Hashtable
        {
            { RoomCountdownKey, syncedCountdownSeconds },
            { RoomCountdownRunningKey, syncedCountdownRunning }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private void RenderCountdownAndHandleTimeout()
    {
        if (syncedCountdownSeconds < 0)
        {
            SetTimeoutText(string.Empty);
            return;
        }

        if (lastRenderedSecond != syncedCountdownSeconds)
        {
            UpdateTimeoutTextDisplay(syncedCountdownSeconds);
            lastRenderedSecond = syncedCountdownSeconds;
        }

        if (syncedCountdownSeconds > 0 || timeoutTriggered)
            return;

        timeoutTriggered = true;
        if (textStatus != null)
            textStatus.text = "Waktu room habis. Silakan masuk ulang.";

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        StartTimeoutMessageCountdown();
    }

    private void UpdateTimeoutTextDisplay(int remainingSeconds)
    {
        if (textTimeoutRemaining == null)
            TryResolveTimeoutTextReference(force: false);

        if (textTimeoutRemaining != null)
            textTimeoutRemaining.text = remainingSeconds.ToString();
    }

    private void ResetToInputState()
    {
        syncedCountdownSeconds = -1;
        syncedCountdownRunning = false;
        timeoutTriggered = false;
        hostTickAccumulator = 0f;
        nextTimeoutTextResolveAt = 0f;
        nextCountdownPullAt = 0f;
        lastRenderedSecond = -1;

        if (panelInputNama != null)
            panelInputNama.SetActive(true);

        if (panelWaitingRoom != null)
            panelWaitingRoom.SetActive(false);

        if (textPlayerCount != null)
            textPlayerCount.text = "(0 / " + RoomMaxPlayers + ")";

        if (textPlayerList != null)
            textPlayerList.text = "";

        SetTimeoutText(string.Empty);
    }

    private void TryResolveTimeoutTextReference(bool force)
    {
        if (textTimeoutRemaining != null)
            return;

        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
        for (int i = 0; i < allTexts.Length; i++)
        {
            TMP_Text t = allTexts[i];
            if (t == null)
                continue;

            string nameLower = t.gameObject.name.ToLowerInvariant();
            if (nameLower.Contains("timeout") || nameLower.Contains("countdown")
                || nameLower.Contains("sisa") || nameLower.Contains("timer"))
            {
                textTimeoutRemaining = t;
                return;
            }
        }

        if (force)
            Debug.LogWarning("WaitingRoomManager: textTimeoutRemaining tidak ditemukan.");
    }

    private void SetTimeoutText(string value)
    {
        if (textTimeoutRemaining == null)
            return;

        textTimeoutRemaining.text = value;
    }

    private void RefreshStartButtonVisibility()
    {
        if (startButton == null)
        {
            TryResolveStartButtonReference();
            if (startButton == null)
                return;
        }

        bool shouldHide = PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient;
        startButton.gameObject.SetActive(!shouldHide);

        if (!shouldHide)
            startButton.interactable = CanHostStartGameNow();
    }

    private bool CanHostStartGameNow()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
            return false;

        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
            return false;

        if (timeoutTriggered)
            return false;

        return syncedCountdownSeconds > 0;
    }

    private void TryResolveStartButtonReference()
    {
        if (startButton != null)
            return;

        Button[] allButtons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < allButtons.Length; i++)
        {
            Button btn = allButtons[i];
            if (btn == null)
                continue;

            string nameLower = btn.gameObject.name.ToLowerInvariant();
            if (nameLower.Contains("start") || nameLower.Contains("mulai") || nameLower.Contains("begin"))
            {
                startButton = btn;
                return;
            }
        }
    }

    public void OnClickStartGame()
    {
        if (timeoutTriggered)
        {
            ResetToInputState();
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();
            return;
        }

        if (!CanHostStartGameNow())
            return;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel("GameScene");
    }

    private void StartRoomActiveCountdown()
    {
        if (timeoutMessageRoutine != null)
        {
            StopCoroutine(timeoutMessageRoutine);
            timeoutMessageRoutine = null;
            isHandlingTimeoutFlow = false;
        }

        if (roomActiveCountdownRoutine != null)
            StopCoroutine(roomActiveCountdownRoutine);

        roomActiveCountdownRoutine = StartCoroutine(RoomActiveCountdownRoutine());
    }

    private System.Collections.IEnumerator RoomActiveCountdownRoutine()
    {
        const int countdownSeconds = 10;

        if (panelInputNama != null)
            panelInputNama.SetActive(false);
        if (panelWaitingRoom != null)
            panelWaitingRoom.SetActive(true);

        for (int remaining = countdownSeconds; remaining > 0; remaining--)
        {
            if (textStatus != null)
                textStatus.text = "sedang ada room yang aktif, cobalah beberapa menit lagi\nKembali dalam " + remaining + " detik";

            yield return new WaitForSecondsRealtime(1f);
        }

        roomActiveCountdownRoutine = null;
        ResetToInputState();
    }

    private void StartTimeoutMessageCountdown()
    {
        if (disconnectMessageRoutine != null)
        {
            StopCoroutine(disconnectMessageRoutine);
            disconnectMessageRoutine = null;
            isHandlingDisconnectFlow = false;
        }

        if (roomActiveCountdownRoutine != null)
        {
            StopCoroutine(roomActiveCountdownRoutine);
            roomActiveCountdownRoutine = null;
        }

        if (timeoutMessageRoutine != null)
            StopCoroutine(timeoutMessageRoutine);

        timeoutMessageRoutine = StartCoroutine(TimeoutMessageCountdownRoutine());
    }

    private System.Collections.IEnumerator TimeoutMessageCountdownRoutine()
    {
        const int countdownSeconds = 5;
        isHandlingTimeoutFlow = true;

        if (panelInputNama != null)
            panelInputNama.SetActive(false);
        if (panelWaitingRoom != null)
            panelWaitingRoom.SetActive(true);

        for (int remaining = countdownSeconds; remaining > 0; remaining--)
        {
            if (textStatus != null)
                textStatus.text = "Waktu room habis. Silakan masuk ulang.\nKembali dalam " + remaining + " detik";

            yield return new WaitForSecondsRealtime(1f);
        }

        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
        else
            isHandlingTimeoutFlow = false;

        timeoutMessageRoutine = null;
        ResetToInputState();
    }

    private void StartDisconnectedCountdown()
    {
        if (isHandlingDisconnectFlow)
            return;

        if (timeoutMessageRoutine != null)
        {
            StopCoroutine(timeoutMessageRoutine);
            timeoutMessageRoutine = null;
            isHandlingTimeoutFlow = false;
        }

        if (roomActiveCountdownRoutine != null)
        {
            StopCoroutine(roomActiveCountdownRoutine);
            roomActiveCountdownRoutine = null;
        }

        if (disconnectMessageRoutine != null)
            StopCoroutine(disconnectMessageRoutine);

        disconnectMessageRoutine = StartCoroutine(DisconnectedCountdownRoutine());
    }

    private System.Collections.IEnumerator DisconnectedCountdownRoutine()
    {
        const int countdownSeconds = 3;
        isHandlingDisconnectFlow = true;

        if (panelInputNama != null)
            panelInputNama.SetActive(false);
        if (panelWaitingRoom != null)
            panelWaitingRoom.SetActive(true);

        for (int remaining = countdownSeconds; remaining > 0; remaining--)
        {
            if (textStatus != null)
                textStatus.text = "koneksi terputus\nKembali dalam " + remaining + " detik";

            yield return new WaitForSecondsRealtime(1f);
        }

        isHandlingDisconnectFlow = false;
        disconnectMessageRoutine = null;
        ResetToInputState();
    }

    private void StartConnectWatchdog()
    {
        StopConnectWatchdog();
        connectWatchdogRoutine = StartCoroutine(ConnectWatchdogRoutine());
    }

    private void StopConnectWatchdog()
    {
        if (connectWatchdogRoutine == null)
            return;

        StopCoroutine(connectWatchdogRoutine);
        connectWatchdogRoutine = null;
    }

    private System.Collections.IEnumerator ConnectWatchdogRoutine()
    {
        yield return new WaitForSecondsRealtime(15f);

        connectWatchdogRoutine = null;

        if (PhotonNetwork.InRoom)
            yield break;

        suppressNextDisconnectedUI = true;
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        ResetToInputState();
    }
}
