# 실습 L5 — RPC 기반 텍스트 채팅

> 음성에 이어 **텍스트 채팅** 채널을 만든다. Photon Realtime 의 `RaiseEvent` 를 활용해 모든 클라이언트에 메시지를 동기화한다.
>
> ⏱️ 예상 시간: 80분 · 📸 슬롯: __L5___TMP ~ __L5___TMP (코드 슬라이드 포함)

---

## 학습 목표

1. **RaiseEvent / OnEvent** 패턴으로 임의 페이로드를 모든 클라이언트에 동기화한다
2. **ChatManager** 싱글톤으로 채팅 흐름을 한 곳에서 관리한다
3. UI Toolkit 또는 uGUI 의 **InputField + ScrollView + Text** 로 채팅창을 만든다
4. 닉네임·타임스탬프·시스템 메시지를 분리해 표시한다
5. 채팅 내용을 일정 개수만 유지하는 **버퍼 관리** 를 구현한다

## 사전 확인

- [ ] L4 완료 (Spatial Audio 동작)
- [ ] TextMeshPro Essential 임포트 완료

---

## Step 1 — Chat UI 만들기

### 1-1. Canvas + ChatPanel 생성

씬에 `Canvas` 가 없다면 추가. `Canvas` 아래에 `ChatPanel` (Panel) 생성.

```
Canvas
└─ ChatPanel
   ├─ MessageScroll (ScrollView)
   │  └─ Content (Vertical Layout Group)
   └─ InputField (TMP_InputField)
```

📸 **__L5___TMP.png** — Hierarchy 의 ChatPanel 구조

### 1-2. 위치·크기 (앵커)

- 좌하단 앵커 (Anchors: BottomLeft)
- 크기: 가로 500 · 세로 300
- ScrollView 의 Vertical Layout Group + Content Size Fitter

📸 **__L5___TMP.png** — ChatPanel 의 RectTransform 설정

### 1-3. 메시지 1줄 프리팹

`Assets/Interaction/Prefabs/ChatMessageItem.prefab`:
- TextMeshPro - Text 한 줄, 좌측 정렬
- 자동 줄바꿈 (Word Wrapping)

📸 **__L5___TMP.png** — ChatMessageItem 프리팹

---

## Step 2 — ChatManager 스크립트

### 2-1. 코드 작성

`Assets/Interaction/Scripts/Chat/ChatManager.cs`:

```csharp
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviour, IOnEventCallback
{
    public static ChatManager Instance { get; private set; }
    const byte CHAT_EVENT_CODE = 1;
    const int MAX_MESSAGES = 50;

    [SerializeField] TMP_InputField input;
    [SerializeField] Transform messageRoot;
    [SerializeField] GameObject messageItemPrefab;

    readonly Queue<GameObject> messages = new();

    void OnEnable()
    {
        Instance = this;
        PhotonNetwork.AddCallbackTarget(this);
        input.onSubmit.AddListener(Send);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void Send(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var payload = new object[] { PhotonNetwork.NickName, text };
        PhotonNetwork.RaiseEvent(CHAT_EVENT_CODE, payload,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable);
        input.text = "";
        input.ActivateInputField();
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != CHAT_EVENT_CODE) return;
        var data = (object[])photonEvent.CustomData;
        var nick = (string)data[0];
        var msg  = (string)data[1];
        Append($"<b>{nick}</b>: {msg}");
    }

    void Append(string line)
    {
        var item = Instantiate(messageItemPrefab, messageRoot);
        item.GetComponentInChildren<TMP_Text>().text = line;
        messages.Enqueue(item);
        if (messages.Count > MAX_MESSAGES) Destroy(messages.Dequeue());
    }
}
```

📸 **__L5___TMP.png** — ChatManager.cs 코드 슬라이드 (스크립트 에디터)

### 2-2. 씬에 부착

`ChatPanel` 에 `ChatManager` 추가. Inspector 에서:
- **Input** ← `InputField (TMP)`
- **Message Root** ← `Content (Vertical Layout)`
- **Message Item Prefab** ← `ChatMessageItem.prefab`

📸 **__L5___TMP.png** — ChatManager Inspector 필드 입력 완료

---

## Step 3 — Enter 키로 전송, 채팅창 토글

### 3-1. Enter 전송

`TMP_InputField` 의 `On Submit` 이벤트가 위 `Send(text)` 호출하도록 연결.

📸 **__L5___TMP.png** — InputField OnSubmit 이벤트 연결

### 3-2. Tab/Enter 로 채팅창 포커스 토글

```csharp
public class ChatToggle : MonoBehaviour
{
    [SerializeField] TMP_InputField input;
    [SerializeField] Key toggleKey = Key.Enter;
    void Update()
    {
        if (Keyboard.current[toggleKey].wasPressedThisFrame
            && !input.isFocused)
        {
            input.ActivateInputField();
        }
    }
}
```

> 채팅창이 포커스되어 있을 때 캐릭터 이동(WASD) 도 막아야 함 — `EventSystem.current.currentSelectedGameObject` 검사로 처리.

📸 **__L5___TMP.png** — ChatToggle 컴포넌트 추가

---

## Step 4 — 닉네임·타임스탬프 보강

### 4-1. 닉네임 설정

룸 입장 전 (또는 메뉴) 에서:

```csharp
PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
```

📸 **__L5___TMP.png** — 룸 입장 전 NickName 입력 UI

### 4-2. 타임스탬프 추가

ChatManager.Append 에 `[HH:mm:ss]` 접두 추가:

```csharp
string time = System.DateTime.Now.ToString("HH:mm:ss");
Append($"<color=#999>[{time}]</color> <b>{nick}</b>: {msg}");
```

📸 **__L5___TMP.png** — 타임스탬프 표시된 채팅 메시지

---

## Step 5 — 시스템 메시지 (입장·퇴장)

### 5-1. OnPlayerEnteredRoom·OnPlayerLeftRoom

`MonoBehaviourPunCallbacks` 상속해서:

```csharp
public override void OnPlayerEnteredRoom(Player player)
    => Append($"<color=#88f>[시스템] {player.NickName} 님이 입장했습니다</color>");

public override void OnPlayerLeftRoom(Player player)
    => Append($"<color=#f88>[시스템] {player.NickName} 님이 퇴장했습니다</color>");
```

📸 **__L5___TMP.png** — 시스템 메시지 출력 화면

---

## Step 6 — 2명 빌드 + 채팅 테스트

### 6-1. 빌드 후 양쪽에서 메시지 주고받기

📸 **__L5___TMP.png** — 두 인스턴스에서 채팅이 동기화된 상태 (좌우 화면)

### 6-2. 스크롤뷰 자동 하단 이동

새 메시지가 오면 ScrollRect.verticalNormalizedPosition = 0 으로 자동 스크롤.

```csharp
// Append 마지막에
Canvas.ForceUpdateCanvases();
scrollRect.verticalNormalizedPosition = 0f;
```

📸 **__L5___TMP.png** — 자동 스크롤 동작

---

## Step 7 — 버퍼·메모리 관리

### 7-1. 50개 초과 시 가장 오래된 메시지 삭제

(이미 코드에 포함됨 — `messages.Count > MAX_MESSAGES` 부분)

### 7-2. 채팅창 비우기 (Clear) 버튼

```csharp
public void Clear()
{
    while (messages.Count > 0) Destroy(messages.Dequeue());
}
```

UI 에 Clear 버튼 추가하고 OnClick 으로 연결.

📸 **__L5___TMP.png** — Clear 버튼 추가된 ChatPanel

---

## Step 8 — 데이터·코드 분리 정리

### 8-1. ChatMessage struct

문자열 결합 대신 struct 로:

```csharp
[System.Serializable]
public struct ChatMessage
{
    public string Sender;
    public string Text;
    public long TimestampTicks;
}
```

> 나중에 메시지 저장·필터링 시 유리. 지금은 간단히 진행해도 OK.

📸 **__L5___TMP.png** — ChatMessage struct 코드

---

## 정상 동작 체크리스트

- [ ] InputField 에 메시지 입력 후 Enter → 양쪽 모두에 표시
- [ ] 입장·퇴장 시 시스템 메시지 자동 표시
- [ ] 50개 초과 시 오래된 메시지 자동 삭제
- [ ] 자동 스크롤 동작
- [ ] 닉네임·타임스탬프 표시

다음 실습 **L6 — 이모트 애니메이션 동기화** 에서는 키 입력으로 캐릭터 애니메이션을 트리거하고 동기화한다.

---

## 🚀 응용 질문

### Q1. 1:1 귓속말 (DM) 을 구현하려면?

`@닉네임 안녕` 식으로 특정 사용자에게만 보내려면?

힌트:
- `RaiseEventOptions.TargetActors` 로 특정 ActorNumber 지정
- 닉네임 → ActorNumber 매핑 테이블 유지
- 받은 쪽 UI 에서 다른 색·아이콘으로 표시

### Q2. 이모지·이모트 단축어 (`:smile:` → 😊) 처리는?

힌트:
- 전송 전 정규식으로 `:키워드:` → 이모지 문자 치환
- 또는 Sprite Asset 으로 채팅 안에 이미지 표시 (TMP_SpriteAsset)
- 욕설 필터링도 같은 방식 (단어 매칭 → ***)

### Q3. 채팅 내용을 디스크에 저장해서 재접속 시 복원하려면?

힌트:
- ChatMessage 리스트를 JSON 으로 직렬화 → PlayerPrefs 또는 파일
- 룸 입장 시 마스터 클라이언트가 신규 입장자에게 RaiseEvent 로 과거 기록 전송
- 영구 저장은 백엔드 DB 필요 (PlayFab 등 — 추후 모듈)
