# 실습 L5 — Vivox 텍스트 채팅

> L2~L4 에서 깔아둔 Vivox 채널 위에 **텍스트 채팅** 을 얹어요.
> 새 SDK 없이 같은 Lobby 채널에서 음성·텍스트가 동시에 흐르도록 만듭니다.
>
> ⏱️ 예상 시간: 60분 · 📸 슬롯: L5_01 ~ L5_10

---

## 학습 목표

1. **ChatCapability.TextAndAudio** 의 의미 — 한 채널 위에서 음성·텍스트가 동시에 흐르는 구조 이해
2. **SendChannelTextMessageAsync** 로 메시지 송신, **ChannelMessageReceived** 이벤트로 수신
3. **VivoxMessage** 객체에 담긴 발신자/내용/시간/자기여부 활용
4. ChatManager 싱글톤과 UI 결합 (TMP_InputField + ScrollRect + Vertical Layout Group)
5. 메시지 버퍼·자동 스크롤·시스템 메시지·타임스탬프 등 UX 디테일

## 사전 확인

- [ ] L4 완료 (Spatial Audio 동작)
- [ ] TextMeshPro Essential Resources 임포트 완료
- [ ] J 키로 Lobby 채널 입장이 정상 동작

---

## Step 1 — VivoxManager 의 채널 capability 변경 + 자동 접속

### 1-1. TextAndAudio 전환

음성 전용으로 들어가던 Lobby 채널을 **TextAndAudio** 로 바꿔서 한 채널에서 둘 다 흐르게 합니다.

```csharp
// VivoxManager.cs - JoinDefaultChannelAsync 안
await VivoxService.Instance.JoinGroupChannelAsync(
    defaultChannelName,
    ChatCapability.TextAndAudio);   // ← AudioOnly 였던 부분
```

> **왜 같은 채널에 얹나요?** Vivox 채널은 capability 만 바꾸면 음성과 텍스트가 한 파이프 위에서 흐릅니다. 별도 채널을 더 만들 필요가 없어서 코드도 단순하고, 음성으로 대화 중인 사람들과 그대로 같은 그룹에서 채팅이 가능해요.

ChatManager 가 어느 채널로 보낼지 알 수 있도록 `DefaultChannelName` 프로퍼티도 노출해 둡니다:

```csharp
public string DefaultChannelName => defaultChannelName;
```

### 1-2. 자동 접속 옵션 추가

L2~L4 에서는 학생이 L (로그인) → J (채널 입장) 키를 직접 눌러 검증했어요. 학습 단계에서는 의미 있는 흐름이지만, 채팅을 쓰는 단계에서는 **게임 시작과 동시에 자동으로 서버 접속이 끝나 있어야** 자연스러워요.

VivoxManager 에 자동 접속 옵션 두 개를 추가합니다:

```csharp
[Header("Auto Connect (L5)")]
[SerializeField] bool autoLogin = true;
[SerializeField] bool autoJoinDefault = true;

async void Start()
{
    await InitializeAsync();
    if (autoLogin) await LoginAsync();
    if (autoLogin && autoJoinDefault) await JoinDefaultChannelAsync();
}
```

그리고 두 메서드를 **멱등하게** — 이미 처리된 경우 다시 호출돼도 안전하게 — 만들어요:

```csharp
public async Task LoginAsync()
{
    if (VivoxService.Instance != null && VivoxService.Instance.IsLoggedIn)
    {
        Debug.Log("[Vivox] 이미 로그인된 상태 — LoginAsync skip");
        return;
    }
    // ... 기존 코드
}

public async Task JoinDefaultChannelAsync()
{
    var active = VivoxService.Instance?.ActiveChannels;
    if (active != null && active.ContainsKey(defaultChannelName))
    {
        Debug.Log($"[Vivox] 이미 {defaultChannelName} 입장 상태 — JoinDefault skip");
        return;
    }
    // ... 기존 코드
}
```

> **L/J 키는 그대로 살려둬요.** Inspector 에서 자동 접속을 꺼두면 학생이 단계별로 키를 눌러가며 학습할 수 있어요. 멱등성 덕분에 자동 접속과 키를 같이 써도 충돌이 없습니다.

📸 **L5_01** — VivoxManager TextAndAudio 전환 + 자동 접속 옵션

---

## Step 2 — Chat UI 만들기

### 2-1. Hierarchy 구조 (최종 형태)

Canvas 안에 ChatPanel 하나 두고, 그 자식으로 MessageScroll 과 ChatField 를 **나란히** 둡니다.

```
Canvas
└─ ChatPanel  (Image, 반투명 배경)
   ├─ MessageScroll  (UI > Scroll View 로 생성)
   │  ├─ Viewport  (Mask)
   │  │  └─ Content  (Vert Layout + Content Size Fitter)
   │  ├─ Scrollbar Vertical
   │  └─ Scrollbar Horizontal  (사용 안 함 — 비활성/제거)
   └─ ChatField  (UI > Input Field - TextMeshPro 로 생성)
      └─ Text Area
         ├─ Placeholder
         └─ Text
```

📸 **L5_02** — Hierarchy 의 ChatPanel 구조

### 2-2. ChatPanel 생성

1. Canvas 우클릭 → **UI → Panel** → 이름 `ChatPanel`
2. RectTransform — 원하는 위치·크기 (예: 우하단 앵커, 약 340×260)
3. Image 컴포넌트의 알파를 100~150 정도로 낮춰 반투명 배경

### 2-3. MessageScroll 생성

1. ChatPanel 우클릭 → **UI → Scroll View** → 이름 `MessageScroll`
   - Unity 가 Viewport / Content / Scrollbar 까지 자동 생성
2. RectTransform 권장 — Anchor **center/middle**, Width **300**, Height **140**, Pos Y **20** (ChatField 위쪽으로 띄움)
3. **ScrollRect 옵션**:

   | 항목 | 값 |
   |---|---|
   | Horizontal | ☐ off |
   | Vertical | ✅ on |
   | Movement Type | **Clamped** |
   | Inertia | ✅ on |
   | Deceleration Rate | 0.135 (기본) |
   | Scroll Sensitivity | 1 (기본) |
   | Horizontal Scrollbar | **None** (슬롯 비우기) |

4. Content 에 컴포넌트 추가:
   - **Vertical Layout Group** — Child Force Expand: Width ✅, Height ☐, Spacing 4
   - **Content Size Fitter** — Vertical Fit: Preferred Size

📸 **L5_03** — MessageScroll Inspector + Content 컴포넌트

### 2-4. ChatField 생성

1. ChatPanel 우클릭 → **UI → Input Field - TextMeshPro** → 이름 `ChatField`
2. RectTransform — MessageScroll 아래쪽에 가로로 펼치기 (예: 하단 앵커, Height 36)
3. **Line Type**: **Single Line** ⚠ Enter 가 Submit 이벤트로 가는 핵심
4. Placeholder 텍스트 변경 — 예: "메시지를 입력하세요"

📸 **L5_04** — ChatField Inspector (Line Type Single Line)

### 2-5. ChatMessageItem 프리팹

`Assets/3.Prefabs/ChatMessageItem.prefab`:
- Hierarchy 빈 곳 우클릭 → **UI → Text - TextMeshPro**, 이름 `ChatMessageItem`
- Inspector 에서:
  - **Main Settings** — Font Size 18, Color White
  - **Wrapping & Overflow** 섹션 — **Wrapping** ✅ Enabled, Overflow: Overflow (기본)
  - Rich Text 는 TextMeshPro 에서 **기본 ON · UI 토글 없음** — 태그 (`<b>`, `<color>`) 가 자동으로 처리돼요
- Project 의 `Assets/3.Prefabs` 폴더로 드래그 → 프리팹화 → Hierarchy 원본 삭제

📸 **L5_05** — ChatMessageItem 프리팹 인스펙터

---

## Step 3 — ChatManager 스크립트

`Assets/2.Scripts/2.Chat/ChatManager.cs` 신규 작성.

핵심은 다음 두 줄입니다:

```csharp
// 송신
await VivoxService.Instance.SendChannelTextMessageAsync(channel, text);

// 수신 — 이벤트 구독
VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;

void OnChannelMessageReceived(VivoxMessage msg)
{
    // msg.SenderDisplayName, msg.MessageText, msg.ReceivedTime, msg.FromSelf
}
```

> **FromSelf 처리** — Vivox 는 자기가 보낸 메시지를 echo 로 같은 이벤트에 흘려보내요. 그래서 송신 측에서 별도로 "내 메시지 직접 그리기" 를 안 해도 자동으로 한 번 화면에 뜹니다. `msg.FromSelf` 로 색만 다르게 칠해서 구분해요.

전체 코드는 동봉된 `ChatManager.cs` 참고 (~200 줄, 학습 주석 포함).

📸 **L5_05** — ChatManager.cs 코드 스크린샷

### 3-1. 씬에 부착

`ChatPanel` 에 `ChatManager` 컴포넌트 추가, Inspector 에서:

| 필드 | 값 |
|---|---|
| Input | `ChatField` (TMP_InputField) |
| Message Root | `Content` (MessageScroll/Viewport/Content) |
| Message Item Prefab | `ChatMessageItem.prefab` |
| Scroll Rect | `MessageScroll` |
| Channel Name | (비워두면 VivoxManager 의 Lobby 자동 사용) |
| Max Messages | 50 |

📸 **L5_06** — ChatManager Inspector 필드 입력 완료

---

## Step 4 — Enter 키 송신 연결

`ChatField` (TMP_InputField) 의 **Line Type** 을 **Single Line** 으로 두면 Enter 키 입력 시 `onSubmit` 이벤트가 발생해요. ChatManager 가 `OnEnable` 에서 자동으로 구독하므로 별도 설정 불필요.

```csharp
input.onSubmit.AddListener(OnSubmit);   // ChatManager.OnEnable 안
                                        // input 필드에는 ChatField 가 드래그돼 들어가요
```

📸 **L5_07** — ChatField Inspector 의 Line Type Single Line 확인

---

## Step 5 — 2명 검증

### 5-1. 빌드 + 동시 실행

L5 부터는 자동 접속 덕분에 흐름이 단순해요:

1. **빌드 산출물 1개** + **에디터** 동시 실행
2. 양쪽 모두 Play 후 잠시 대기 → Console 에 `Joined channel: Lobby (Text+Audio, PTT 대기)` 가 자동으로 뜨면 준비 완료
3. 한쪽에서 InputField 클릭 → 메시지 입력 → Enter
4. 양쪽 화면에 같은 메시지가 동시에 나타나는지 확인

📸 **L5_08** — 두 인스턴스가 같은 메시지를 표시한 상태

### 5-2. 시스템 메시지 확인

자동 접속이 완료되면 회색 시스템 메시지로:

- `채팅 준비 완료 — 잠시 후 자동 접속됩니다. Enter 로 메시지 전송`

자동 접속이 늦거나 잠시 끊겼을 때는:

- `Vivox 로그인 대기 중이에요 — 잠시 후 다시 시도해 주세요`
- `Lobby 채널 입장 대기 중이에요 — 잠시 후 다시 시도해 주세요`

📸 **L5_09** — 시스템 메시지 (회색 톤) 표시 확인

---

## Step 6 — 자동 스크롤 검증

50개 넘게 메시지를 쌓아본 후:
- 가장 오래된 메시지가 자동으로 사라지는지 (Destroy)
- 새 메시지가 도착하면 스크롤이 자동으로 하단으로 이동하는지

📸 **L5_10** — 50개 초과 후 오래된 메시지 자동 삭제 확인

---

## 정상 동작 체크리스트

- [ ] J 키로 Lobby 채널 입장 시 "Text+Audio, PTT 대기" 로그
- [ ] InputField 에 메시지 입력 후 Enter → 양쪽 모두 표시
- [ ] 자기 메시지(파랑) 와 타인 메시지(노랑) 색상 구분
- [ ] `[HH:mm:ss]` 타임스탬프 표시
- [ ] 50개 초과 시 오래된 메시지 자동 삭제
- [ ] 새 메시지 도착 시 자동 하단 스크롤
- [ ] 로그인/채널 입장 전 송신 시 시스템 메시지 안내

---

## 🚀 응용 질문

### Q1. 음성과 텍스트를 다른 채널로 분리하려면?

같은 채널 위에 두 종류가 흐르는 게 단순하지만, 길드 채팅 vs 파티 음성처럼 분리하고 싶을 수 있어요.

힌트:
- VivoxManager 에 `chatChannelName = "Chat"` 별도 필드 추가
- `JoinGroupChannelAsync(chatChannelName, ChatCapability.TextOnly)` 로 별도 입장
- ChatManager 의 `channelName` 에 "Chat" 명시
- 음성 채널은 그대로 AudioOnly 유지

### Q2. 1:1 귓속말(DM) 을 구현하려면?

힌트:
- Vivox 의 directed messages API — `SendDirectTextMessageAsync(playerId, text)`
- 받은 측은 `DirectedMessageReceived` 이벤트로 별도 처리
- 채팅 UI 에 `/w 닉네임 메시지` 또는 `@닉네임` 파서 추가

### Q3. 이모지·욕설 필터를 적용하려면?

힌트:
- 송신 전 정규식으로 `:smile:` → 😊 치환
- TMP Sprite Asset 으로 채팅 내 이미지 표시
- 욕설 단어 집합으로 매칭 → ***
- 서버 단 필터링은 클라이언트 우회 가능하니, 클라이언트는 표시 단계에서만 처리

---

다음 실습 **L6 — 이모트 애니메이션 동기화** 에서는 채팅 단축어로 캐릭터 애니메이션을 트리거해서 모든 참가자 화면에 동시에 표시해요.
