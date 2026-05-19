# 실습 L11 — UI 통합 (채팅·이모트·음성·시선·제스처)

> 지금까지 만든 모든 요소를 하나의 통합 UI 로 묶는다. 채팅창 토글, 이모트 단축 버튼, 음성 인디케이터, 시선 디버그, 제스처 요청 알림이 한 화면에서 자연스럽게 동작하도록.
>
> ⏱️ 예상 시간: 70분 · 📸 슬롯: __L11___TMP ~ __L11___TMP

---

## 학습 목표

1. 산재한 UI 요소를 **HUD Canvas 한 곳** 으로 모아 관리한다
2. **채팅창 토글** (Tab/Enter 키) 로 게임 입력과 채팅 입력 분리
3. **이모트 단축 버튼** (1~3 + UI) 으로 통합 인터페이스
4. **음성 인디케이터** 로 누가 말하는지 시각적 표시
5. **제스처 요청 알림** 토스트 UI 로 사용자 응답 받기
6. **플레이어 닉네임** 머리 위 표시 + 거리별 페이드

## 사전 확인

- [ ] L10 까지 모든 실습 완료 (Voice·채팅·이모트·포즈·이동 보간·IK·제스처)

---

## Step 1 — HUD Canvas 구조 정리

### 1-1. 통합 Canvas

`Canvas` (Screen Space - Overlay) 하나에 모든 HUD 자식 배치.

```
Canvas (HUD)
├─ TopLeft
│  └─ MicStatusIcon (말하는 중 아이콘)
├─ BottomLeft
│  └─ ChatPanel (L5)
├─ BottomCenter
│  └─ EmoteBar (이모트 버튼 3개)
├─ BottomRight
│  └─ GestureHint ("E: 악수" 같은 컨텍스트 힌트)
├─ Center
│  └─ ToastContainer (제스처 요청·시스템 알림)
└─ World UI Layer
   └─ PlayerNameTags (월드 좌표 따라가는 닉네임)
```

📸 **__L11___TMP.png** — Hierarchy 의 HUD 구조

### 1-2. CanvasGroup 으로 영역별 페이드

전투 중에는 채팅창 반투명, 채팅 입력 중에는 이모트 바 비활성 등 상황별 표시.

---

## Step 2 — 채팅 입력 중 게임 입력 차단

### 2-1. InputBlocker

채팅 InputField 가 포커스되어 있을 때 캐릭터 이동/이모트/제스처 입력 막기.

```csharp
public static class InputContext
{
    public static bool IsChatFocused => EventSystem.current != null
        && EventSystem.current.currentSelectedGameObject != null
        && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
}
```

각 컨트롤러의 `Update` 시작 부분에 `if (InputContext.IsChatFocused) return;` 추가.

📸 **__L11___TMP.png** — InputContext 코드 + 각 Controller 에 적용

### 2-2. Enter 로 채팅 열기, Esc 로 닫기

```csharp
if (Keyboard.current.enterKey.wasPressedThisFrame && !InputContext.IsChatFocused)
    chatInput.ActivateInputField();
if (Keyboard.current.escapeKey.wasPressedThisFrame && InputContext.IsChatFocused)
    EventSystem.current.SetSelectedGameObject(null);
```

📸 **__L11___TMP.png** — 채팅 토글 코드

---

## Step 3 — 이모트 바 UI

### 3-1. 가로 3 버튼 (Wave / Dance / Clap)

각 버튼:
- 아이콘 이미지
- 단축키 표시 ("1", "2", "3")
- OnClick → EmoteController.Trigger

📸 **__L11___TMP.png** — EmoteBar UI 디자인

### 3-2. 호버 시 미리보기 (선택)

마우스 호버 시 작은 GIF 또는 정지 이미지로 어떤 이모트인지 보여줌.

📸 **__L11___TMP.png** — 이모트 버튼 호버 시 툴팁

---

## Step 4 — 음성 인디케이터

### 4-1. 본인이 말하는 중 (좌상단 아이콘)

```csharp
void Update()
{
    micStatusIcon.SetActive(localRecorder.IsCurrentlyTransmitting);
}
```

### 4-2. 다른 플레이어 말하는 중 (닉네임 강조)

채팅 친구 목록 또는 플레이어 네임태그가 말하는 중일 때 노란색으로 글로우.

```csharp
nameTagText.color = speaker.IsPlaying ? Color.yellow : Color.white;
```

📸 **__L11___TMP.png** — 좌상단 마이크 아이콘 + 닉네임 강조

---

## Step 5 — 플레이어 닉네임 (World UI)

### 5-1. NameTag 프리팹

캐릭터 머리 위에 작은 Canvas (World Space) + TMP Text. 항상 카메라 향하기 (`Billboard`).

```csharp
public class Billboard : MonoBehaviour
{
    Camera cam;
    void Awake() => cam = Camera.main;
    void LateUpdate() => transform.rotation = cam.transform.rotation;
}
```

📸 **__L11___TMP.png** — NameTag World UI

### 5-2. 거리별 페이드

```csharp
float distance = Vector3.Distance(cam.transform.position, transform.position);
canvasGroup.alpha = Mathf.Clamp01(1f - (distance - fadeStart) / fadeRange);
```

`fadeStart = 5m`, `fadeRange = 10m` 정도.

📸 **__L11___TMP.png** — 가까이/멀리 닉네임 페이드 비교

---

## Step 6 — 제스처 요청 토스트

### 6-1. ToastUI 컴포넌트

```csharp
public class ToastUI : MonoBehaviour
{
    public static ToastUI Instance;
    [SerializeField] GameObject template;
    [SerializeField] Transform container;

    void Awake() => Instance = this;

    public void Show(string message, float duration = 5f, System.Action onClick = null)
    {
        var item = Instantiate(template, container);
        item.GetComponentInChildren<TMP_Text>().text = message;
        item.GetComponent<Button>().onClick.AddListener(() => { onClick?.Invoke(); Destroy(item); });
        Destroy(item, duration);
    }
}
```

📸 **__L11___TMP.png** — Toast 컴포넌트 코드

### 6-2. L10 GestureRequest 연결

`OnGestureRequest` 에서 ToastUI.Show("악수 요청") 호출.

---

## Step 7 — 컨텍스트 힌트 (우하단)

### 7-1. GestureHint

가까이 마주보는 상대가 있으면 "E: 악수 / R: 하이파이브" 자동 표시.

```csharp
void Update()
{
    var partner = gestureController.FindPartner();
    hintPanel.SetActive(partner != null);
}
```

📸 **__L11___TMP.png** — 컨텍스트 힌트 표시

---

## Step 8 — 최종 통합 영상 캡처

### 8-1. 60초 데모 시나리오

1. 입장 (0~5초) — 닉네임 표시
2. 채팅 (5~15초) — "안녕하세요!"
3. 음성 통신 (15~25초) — V 키, 좌상단 아이콘
4. 이동·달리기 (25~35초) — Blend Tree
5. 이모트 (35~45초) — 1·2·3 키
6. 악수·앉기 (45~60초) — E 키 후 의자에 같이 앉음

📸 **__L11___TMP.png** — 통합 데모 스틸 컷

### 8-2. 평가 산출물 영상

이 60초 영상을 학생이 제출. 채점 기준의 시각 평가에 사용.

📸 **__L11___TMP.png** — 영상 캡처 도구 (Unity Recorder) 설정

---

## 정상 동작 체크리스트

- [ ] 채팅 입력 중에 캐릭터가 안 움직임 (W 눌러도 채팅창에 W 입력됨)
- [ ] Esc → 채팅창 닫힘 → 게임 입력 복귀
- [ ] 이모트 단축키 (1·2·3) 와 UI 버튼 둘 다 동작
- [ ] 말하는 중 좌상단 아이콘 + 닉네임 노란색
- [ ] 캐릭터 머리 위 닉네임이 거리별로 자연스럽게 페이드
- [ ] 제스처 요청 시 토스트로 알림
- [ ] 60초 통합 데모 영상 캡처 완료

이 실습으로 심화 모듈의 모든 학습 항목이 통합됩니다. 학생 산출물 제출 — Unity 프로젝트 + 빌드 + 60초 데모 영상 + Before/After 보간 영상 + 응용 도전 1개 + 자기 평가.

---

## 🚀 응용 질문

### Q1. 미니맵에 다른 플레이어 위치·상태(말하는 중·이모트 중) 표시?

힌트:
- 별도 Render Texture 카메라 (탑다운 뷰)
- 플레이어 마커 (작은 점 또는 화살표)
- 상태별 아이콘 (마이크·이모트·앉기)

### Q2. 사용자가 자기 HUD 레이아웃을 자유롭게 옮길 수 있게 하려면?

힌트:
- DraggableUIElement 컴포넌트 (마우스 드래그)
- 옵션 모드에서만 활성 (잠금/해제 토글)
- 위치를 PlayerPrefs 또는 ScriptableObject 에 저장

### Q3. 닉네임 위에 추가 정보 (레벨·국적·길드) 를 한 줄 더 표시하려면?

힌트:
- NameTag UI 에 두 번째 Text 줄 추가
- 플레이어 데이터 모델 확장 (PlayerInfo struct)
- 룸 입장 시 모든 클라이언트에 PlayerInfo 동기화 (CustomProperties)
