// =============================================================================
// VivoxManager.cs — Vivox 음성 SDK 의 초기화 / 로그인 / 채널 입장 / 마이크 인식 /
//                   Echo 검증 / Push-to-Talk 를 한 곳에서 담당하는 매니저.
// -----------------------------------------------------------------------------
// 학습 포인트:
// 1) Unity Gaming Services 의 초기화는 반드시 정해진 순서를 따라야 한다.
//    UnityServices  →  AuthenticationService  →  VivoxService
// 2) 세 단계 모두 await 가 필요한 비동기 작업이라 async/await 패턴을 쓴다.
// 3) 어디서든 VivoxManager.Instance.XXX 로 접근할 수 있도록 싱글톤으로 만든다.
// 4) 검증 단계에서는 UI 대신 디버그 키 (L/J/M/E/X/V) 로 트리거한다.
// 5) 채널에 들어갔다고 곧바로 송신되지 않는다. SetChannelTransmissionModeAsync 로
//    None / All 송신 상태를 토글한다 — 이것이 Push-to-Talk 의 핵심 원리다.
// =============================================================================

using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;   // 익명 로그인 (SignInAnonymouslyAsync)
using Unity.Services.Core;             // UnityServices.InitializeAsync()
using Unity.Services.Vivox;            // VivoxService, LoginOptions, ChatCapability
using UnityEngine;

public class VivoxManager : MonoBehaviour
{
    // ---------------------------------------------------------------------
    // 싱글톤 — 다른 스크립트에서 VivoxManager.Instance 로 접근
    // ---------------------------------------------------------------------
    public static VivoxManager Instance { get; private set; }

    // ---------------------------------------------------------------------
    // Inspector 에서 편집할 수 있도록 SerializeField 로 노출하는 설정 값
    // ---------------------------------------------------------------------
    [SerializeField] string defaultChannelName = "Lobby";  // 기본 입장 채널 이름
    [SerializeField] string displayName = "Player";        // Vivox 표시 이름 (접두어로 사용)
    [SerializeField] string echoChannelName = "EchoTest";  // Echo 검증 채널 이름
    [SerializeField] KeyCode pushToTalkKey = KeyCode.V;    // L3 — Push-to-Talk 키

    // ---------------------------------------------------------------------
    // 외부에서 "지금 로그인되어 있나?" 를 확인할 때 쓰는 읽기 전용 프로퍼티
    // ---------------------------------------------------------------------
    public bool IsLoggedIn =>
        VivoxService.Instance != null && VivoxService.Instance.IsLoggedIn;

    // ---------------------------------------------------------------------
    // Awake — 게임 오브젝트가 생성될 때 가장 먼저 호출되는 Unity 콜백
    // 싱글톤이 이미 있다면 자신을 파괴해서 중복을 막고,
    // 처음이라면 자신을 Instance 로 등록하고 씬 전환에도 살아남게 한다.
    // ---------------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬을 바꿔도 매니저가 사라지지 않게 보존
    }

    // ---------------------------------------------------------------------
    // Start — 첫 프레임 직전에 호출. async 로 선언해서 초기화 비동기 호출.
    // ---------------------------------------------------------------------
    async void Start()
    {
        await InitializeAsync();
    }

    // ---------------------------------------------------------------------
    // Update — 매 프레임 호출되는 Unity 콜백. 검증용 디버그 키 바인딩.
    //   L 키       → Vivox 로그인              (LoginAsync)
    //   J 키       → 기본 그룹 채널 입장        (JoinDefaultChannelAsync)
    //   M 키       → 인식된 마이크 목록 출력    (LogInputDevices)
    //   E 키       → Echo 채널 입장 (청각 검증) (JoinEchoChannelAsync)
    //   X 키       → Echo 채널 나가기           (LeaveEchoChannelAsync)
    //   V 키(눌림) → Push-to-Talk 송신 ON      (SetTransmissionAsync(true))
    //   V 키(뗌)   → Push-to-Talk 송신 OFF     (SetTransmissionAsync(false))
    // 실제 게임에서는 UI 버튼으로 대체하지만, 검증 단계는 키가 가장 빠르다.
    // ---------------------------------------------------------------------
    async void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) await LoginAsync();
        if (Input.GetKeyDown(KeyCode.J)) await JoinDefaultChannelAsync();
        if (Input.GetKeyDown(KeyCode.M)) LogInputDevices();
        if (Input.GetKeyDown(KeyCode.E)) await JoinEchoChannelAsync();
        if (Input.GetKeyDown(KeyCode.X)) await LeaveEchoChannelAsync();

        // L3 — V 키를 누른 순간 송신 시작, 떼는 순간 송신 차단
        if (Input.GetKeyDown(pushToTalkKey)) await SetTransmissionAsync(true);
        if (Input.GetKeyUp(pushToTalkKey))   await SetTransmissionAsync(false);
    }

    // =====================================================================
    // 1) 초기화
    // =====================================================================

    // ---------------------------------------------------------------------
    // InitializeAsync — UGS / Auth / Vivox 를 차례로 초기화
    // 순서를 반드시 지켜야 한다. 한 단계라도 어긋나면 다음 단계가 토큰을
    // 받지 못해 AuthenticationException 또는 Credentials 오류가 발생한다.
    // ---------------------------------------------------------------------
    public async Task InitializeAsync()
    {
        try
        {
            // 1) Unity Gaming Services 코어 초기화 (한 번만 필요)
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            // 2) 익명 로그인 — 별도 계정 없이 UGS 가 임시 PlayerId 발급
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // 3) Vivox 서비스 초기화 — 이때 Dashboard 의 Vivox 자격 증명을 자동으로 가져온다
            await VivoxService.Instance.InitializeAsync();

            // 정상이라면 PlayerId 까지 함께 출력 (Console 에서 식별용)
            Debug.Log($"[Vivox] Initialized. PlayerId={AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            // 실패 시 빨간 에러로 알려서 어디서 멈췄는지 추적 가능하게 한다
            Debug.LogError($"[Vivox] Init failed: {e.Message}");
        }
    }

    // =====================================================================
    // 2) 로그인 + 채널 입장
    // =====================================================================

    // ---------------------------------------------------------------------
    // LoginAsync — Vivox 에 표시 이름(DisplayName) 으로 로그인
    // 같은 PC 에서 여러 번 테스트해도 충돌이 없도록 4자리 난수를 붙인다.
    // ---------------------------------------------------------------------
    public async Task LoginAsync()
    {
        var options = new LoginOptions
        {
            // 예: "Astronaut_3742" 처럼 매번 다른 이름으로 로그인
            DisplayName = displayName + "_" + UnityEngine.Random.Range(1000, 9999),
            EnableTTS = false, // 텍스트→음성 합성 기능은 끔
        };

        await VivoxService.Instance.LoginAsync(options);
        Debug.Log($"[Vivox] Logged in as {options.DisplayName}");
    }

    // ---------------------------------------------------------------------
    // JoinDefaultChannelAsync — Inspector 에 설정한 채널에 음성 전용으로 입장
    // ChatCapability:
    //   AudioOnly     → 음성만
    //   TextOnly      → 텍스트만
    //   TextAndAudio  → 음성 + 텍스트
    // ---------------------------------------------------------------------
    public async Task JoinDefaultChannelAsync()
    {
        await VivoxService.Instance.JoinGroupChannelAsync(
            defaultChannelName,
            ChatCapability.AudioOnly);

        // L3 — 채널 입장 직후 송신을 차단해 V 키 PTT 기본 상태로 둔다
        await SetTransmissionAsync(false);

        Debug.Log($"[Vivox] Joined channel: {defaultChannelName} (PTT 대기)");
    }

    // =====================================================================
    // 3) 마이크 인식 확인 (Step 7)
    // =====================================================================

    // ---------------------------------------------------------------------
    // LogInputDevices — Vivox 가 인식한 마이크 입력 장치를 모두 Console 출력
    // 현재 활성(Active) 장치도 함께 표시해서 어떤 마이크가 사용 중인지 확인.
    // 비동기가 아니므로 async/await 불필요.
    // ---------------------------------------------------------------------
    public void LogInputDevices()
    {
        Debug.Log("[Vivox] 입력 장치 목록:");

        foreach (var dev in VivoxService.Instance.AvailableInputDevices)
        {
            bool isActive = (dev == VivoxService.Instance.ActiveInputDevice);
            Debug.Log($"  - {dev.DeviceName} (Active={isActive})");
        }
    }

    // =====================================================================
    // 4) Echo 채널 (Step 8 — 청각 검증)
    // =====================================================================

    // ---------------------------------------------------------------------
    // JoinEchoChannelAsync — 자기 마이크 입력이 자기 헤드폰으로 즉시 재생.
    // "마이크가 인식되는가" 가장 빠른 청각 검증. 스피커로 들으면 하울링
    // 위험하니 반드시 헤드폰 착용 상태로 테스트한다.
    // ---------------------------------------------------------------------
    public async Task JoinEchoChannelAsync()
    {
        await VivoxService.Instance.JoinEchoChannelAsync(
            echoChannelName,
            ChatCapability.AudioOnly);

        // L3 — Echo 채널에서도 동일하게 PTT 적용. V 눌러야 자기 목소리가 들린다.
        await SetTransmissionAsync(false);

        Debug.Log($"[Vivox] Echo channel joined: {echoChannelName} (PTT 대기)");
    }

    // ---------------------------------------------------------------------
    // LeaveEchoChannelAsync — Echo 채널 빠져나오기.
    // 실제 배포 시 자기 음성은 자기에게 안 들리는 게 자연스러우니
    // 검증이 끝나면 X 키로 나가 두는 습관을 들인다.
    // ---------------------------------------------------------------------
    public async Task LeaveEchoChannelAsync()
    {
        await VivoxService.Instance.LeaveChannelAsync(echoChannelName);
        Debug.Log($"[Vivox] Echo channel left: {echoChannelName}");
    }

    // =====================================================================
    // 5) Push-to-Talk (L3 — V 키)
    // =====================================================================

    // ---------------------------------------------------------------------
    // SetTransmissionAsync — 마이크 송신 ON/OFF 전환.
    //
    // Vivox 는 채널에 입장했다고 해서 곧바로 마이크 음성이 송출되지 않는다.
    // "지금 어떤 채널로 보낼지" 를 SetChannelTransmissionModeAsync 로 정한다:
    //
    //   TransmissionMode.None   → 어떤 채널에도 송신하지 않음 (마이크 차단)
    //   TransmissionMode.Single → 지정한 단일 채널 한 곳에만 송신 (channelName 인자 필수)
    //   TransmissionMode.All    → 입장해 있는 모든 채널에 송신
    //
    // Push-to-Talk 는 "키 누름 = 전체 송신 / 키 뗌 = 송신 차단" 두 상태만 쓰면 되므로
    // None ↔ All 만 사용한다.
    // ---------------------------------------------------------------------
    public async Task SetTransmissionAsync(bool transmitting)
    {
        var mode = transmitting ? TransmissionMode.All : TransmissionMode.None;
        await VivoxService.Instance.SetChannelTransmissionModeAsync(mode);
        Debug.Log($"[Vivox] Transmission: {mode}");
    }
}
