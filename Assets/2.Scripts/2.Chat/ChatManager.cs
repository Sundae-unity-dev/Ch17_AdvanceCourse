// =============================================================================
// ChatManager.cs — Vivox 텍스트 채널 위에서 동작하는 채팅 매니저.
//                  음성과 같은 Lobby 채널에 흐르는 텍스트 메시지를 송수신한다.
// -----------------------------------------------------------------------------
// 학습 포인트:
// 1) Vivox 는 음성 SDK 지만 텍스트 메시지도 같은 채널 위에서 함께 흐른다.
//    채널 입장 시 ChatCapability.TextAndAudio 로 들어가면 한 채널에 두 종류
//    데이터가 동시에 오갈 수 있다.
// 2) 송신: VivoxService.Instance.SendChannelTextMessageAsync(channel, text)
//    수신: VivoxService.Instance.ChannelMessageReceived += OnMessage
// 3) 수신 핸들러는 VivoxMessage 한 객체로 ChannelName/SenderDisplayName/
//    MessageText/ReceivedTime/FromSelf/MessageId 를 한꺼번에 받는다.
// 4) RaiseEvent 같은 별도 패킷 코드 정의 없이, 채널과 텍스트만 알면 끝이다.
//    음성 인프라(L2~L4) 를 그대로 재활용하므로 새 의존성이 없다.
// =============================================================================

using System.Collections.Generic;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    // ---------------------------------------------------------------------
    // 싱글톤 — 외부에서 ChatManager.Instance.Send(text) 로 호출 가능
    // ---------------------------------------------------------------------
    public static ChatManager Instance { get; private set; }

    // ---------------------------------------------------------------------
    // UI 참조 — Inspector 에서 연결
    //   input        : 메시지 입력칸 (TMP_InputField)
    //   messageRoot  : 메시지 한 줄씩 쌓이는 부모 (ScrollView 의 Content)
    //   messageItem  : 한 줄짜리 메시지 프리팹 (TMP_Text 컴포넌트 포함)
    //   scrollRect   : 새 메시지 도착 시 자동 하단 스크롤용
    // ---------------------------------------------------------------------
    [Header("UI References")]
    [SerializeField] TMP_InputField input;
    [SerializeField] Transform messageRoot;
    [SerializeField] GameObject messageItemPrefab;
    [SerializeField] ScrollRect scrollRect;

    // ---------------------------------------------------------------------
    // 채널 이름 — 비우면 VivoxManager.DefaultChannelName 사용
    // 한 씬에 채팅 전용 별도 채널을 두고 싶을 때만 직접 지정한다.
    // ---------------------------------------------------------------------
    [Header("Channel")]
    [SerializeField] string channelName = "";

    // ---------------------------------------------------------------------
    // 메시지 버퍼 — 최대 개수 초과 시 가장 오래된 줄을 Destroy 해 메모리 보존
    // ---------------------------------------------------------------------
    [Header("Buffer")]
    [SerializeField] int maxMessages = 50;
    readonly Queue<GameObject> spawned = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        // VivoxService 가 아직 초기화 전이면 이벤트 등록만 미뤄둔다.
        // 늦게 들어와도 ChannelMessageReceived 는 정적 이벤트가 아니므로
        // VivoxService.Instance 가 준비된 이후에 다시 시도해야 한다.
        TrySubscribe();

        if (input != null)
        {
            input.onSubmit.RemoveListener(OnSubmit);
            input.onSubmit.AddListener(OnSubmit);
        }
    }

    void OnDisable()
    {
        if (VivoxService.Instance != null)
        {
            VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
        }
        if (input != null) input.onSubmit.RemoveListener(OnSubmit);
    }

    // ---------------------------------------------------------------------
    // TrySubscribe — VivoxService.Instance 준비 여부 확인 후 이벤트 구독.
    // 아직이라면 다음 프레임에 다시 시도하도록 코루틴 처리.
    // ---------------------------------------------------------------------
    void TrySubscribe()
    {
        if (VivoxService.Instance == null)
        {
            StartCoroutine(WaitAndSubscribe());
            return;
        }
        VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
        VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
    }

    System.Collections.IEnumerator WaitAndSubscribe()
    {
        while (VivoxService.Instance == null) yield return null;
        VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
        VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
        AppendSystem("채팅 준비 완료 — 잠시 후 자동 접속됩니다. Enter 로 메시지 전송");
    }

    // ---------------------------------------------------------------------
    // OnSubmit — InputField 에서 Enter 가 눌렸을 때 호출되는 콜백.
    // 빈 문자열은 무시, 비동기 전송 후 입력칸 비우고 다시 포커스.
    // ---------------------------------------------------------------------
    async void OnSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        await SendAsync(text);
        if (input != null)
        {
            input.text = "";
            input.ActivateInputField();
        }
    }

    // ---------------------------------------------------------------------
    // SendAsync — Vivox 채널로 텍스트 한 줄 송신.
    // VivoxService 가 준비 안 됐거나 채널에 입장 전이면 시스템 메시지로 알림.
    //
    // 사전 체크 3단계:
    //   1) 채널 이름이 비어있지 않은가
    //   2) Vivox 로그인 상태인가
    //   3) 해당 채널에 실제로 입장해 있는가 (ActiveChannels 확인)
    //
    // 3) 가 없으면 Vivox 가 InvalidOperationException 을 던지면서 Console 에
    // 빨간 스택트레이스가 뜨므로, 미리 막아서 회색 시스템 메시지로 안내한다.
    // ---------------------------------------------------------------------
    public async System.Threading.Tasks.Task SendAsync(string text)
    {
        var ch = EffectiveChannel;
        if (string.IsNullOrEmpty(ch))
        {
            AppendSystem("채널 이름이 비어있어요 — VivoxManager 의 defaultChannelName 을 확인해 주세요");
            return;
        }
        if (VivoxService.Instance == null || !VivoxService.Instance.IsLoggedIn)
        {
            AppendSystem("Vivox 로그인 대기 중이에요 — 잠시 후 다시 시도해 주세요");
            return;
        }

        // 사전 체크 — 해당 채널에 실제로 입장해 있는지
        var active = VivoxService.Instance.ActiveChannels;
        if (active == null || !active.ContainsKey(ch))
        {
            AppendSystem($"{ch} 채널 입장 대기 중이에요 — 잠시 후 다시 시도해 주세요");
            return;
        }

        try
        {
            await VivoxService.Instance.SendChannelTextMessageAsync(ch, text);
        }
        catch (System.Exception e)
        {
            AppendSystem($"전송 실패: {e.Message}");
        }
    }

    // ---------------------------------------------------------------------
    // OnChannelMessageReceived — 채널에 메시지가 도착했을 때 Vivox 가 호출.
    // VivoxMessage 한 객체에 발신자/내용/채널/시간/자기여부가 모두 들어있다.
    //
    // FromSelf 가 true 인 메시지는 자기 자신이 보낸 메시지의 echo 인데,
    // Vivox 가 송신 직후 자동으로 같이 보내주기 때문에 여기서 그대로 표시하면
    // 자기 메시지가 한 번만 화면에 뜬다. (별도 처리 불필요)
    // ---------------------------------------------------------------------
    void OnChannelMessageReceived(VivoxMessage msg)
    {
        string time = msg.ReceivedTime.ToLocalTime().ToString("HH:mm:ss");
        string name = msg.SenderDisplayName;
        string body = msg.MessageText;
        string colored = msg.FromSelf ? "#9be1ff" : "#ffe27a";  // 자기/타인 색 구분

        Append($"<color=#999>[{time}]</color> <color={colored}><b>{name}</b></color>: {body}");
    }

    // ---------------------------------------------------------------------
    // AppendSystem — 시스템 안내 메시지. 일반 메시지와 구분되는 회색 톤.
    // ---------------------------------------------------------------------
    public void AppendSystem(string text)
    {
        string time = System.DateTime.Now.ToString("HH:mm:ss");
        Append($"<color=#888>[{time}] [시스템] {text}</color>");
    }

    // ---------------------------------------------------------------------
    // Append — 메시지 한 줄을 messageRoot 아래에 인스턴스화하고
    // maxMessages 를 넘으면 가장 오래된 항목 Destroy. 자동 하단 스크롤.
    // ---------------------------------------------------------------------
    void Append(string richText)
    {
        if (messageItemPrefab == null || messageRoot == null) return;

        var item = Instantiate(messageItemPrefab, messageRoot);
        var label = item.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = richText;
        spawned.Enqueue(item);

        while (spawned.Count > maxMessages)
        {
            var old = spawned.Dequeue();
            if (old != null) Destroy(old);
        }

        // 다음 프레임에 레이아웃 갱신 후 스크롤 — 즉시 호출하면 Content 의
        // 새 항목 높이 반영 전이라 스크롤이 한 박자 늦게 적용된다.
        if (scrollRect != null) StartCoroutine(ScrollToBottomNextFrame());
    }

    System.Collections.IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;            // 한 프레임 대기 — Layout Group 갱신 완료
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // ---------------------------------------------------------------------
    // EffectiveChannel — 직접 지정한 channelName 이 있으면 그걸,
    // 없으면 VivoxManager 의 DefaultChannelName 을 자동 사용.
    // ---------------------------------------------------------------------
    string EffectiveChannel
    {
        get
        {
            if (!string.IsNullOrEmpty(channelName)) return channelName;
            if (VivoxManager.Instance != null) return VivoxManager.Instance.DefaultChannelName;
            return null;
        }
    }

    // ---------------------------------------------------------------------
    // Clear — UI 버튼 등에서 호출 가능. 모든 메시지 GameObject 제거.
    // ---------------------------------------------------------------------
    public void Clear()
    {
        while (spawned.Count > 0)
        {
            var go = spawned.Dequeue();
            if (go != null) Destroy(go);
        }
    }
}
