# 실습 L3 — Push-to-Talk 구현

> L2 에서 마이크 인식까지 됐다. 이번에는 **V 키를 누르는 동안** 마이크가 켜져 다른 플레이어에게 음성이 전달되도록 구현한다.
>
> ⏱️ 예상 시간: 60분 · 📸 슬롯: __L3___TMP ~ __L3___TMP

---

## 학습 목표

1. **Recorder** 컴포넌트를 캐릭터 프리팹에 배치하고 마이크 입력을 음성 스트림으로 만든다
2. **Speaker** 컴포넌트가 원격 플레이어의 음성을 재생하도록 자동 배치한다
3. **Input System** 으로 V 키 Hold 바인딩을 만들고 `Recorder.TransmitEnabled` 토글
4. 두 명 빌드 후 V 키 누르는 동안만 음성이 들리는 것을 검증

## 사전 확인

- [ ] L2 체크리스트 5개 모두 통과
- [ ] 캐릭터 프리팹 (`PlayerCharacter`) 이 챕터 05 결과물에 존재

---

## Step 1 — Recorder 컴포넌트 추가

### 1-1. 캐릭터 프리팹 열기

`Assets/QuantumUser/View/PlayerCharacter.prefab` 더블클릭.

📸 **__L3___TMP.png** — 프리팹 편집 모드 진입

### 1-2. Recorder 컴포넌트 추가

`Add Component > Photon Voice > Recorder`.

주요 필드:
- **Source Type**: `Microphone`
- **Microphone Type**: `Unity`
- **Transmit Enabled**: OFF (시작은 OFF, V 키로 켤 것)
- **Voice Detection**: OFF (PTT 방식이므로)

📸 **__L3___TMP.png** — Recorder Inspector 설정

### 1-3. PhotonVoiceView 컴포넌트 추가

`Add Component > Photon Voice > PhotonVoiceView`.
이 컴포넌트가 Recorder 와 Speaker 를 묶어 네트워크상 식별자 역할.

📸 **__L3___TMP.png** — PhotonVoiceView 추가된 상태

---

## Step 2 — Speaker 자동 배치

### 2-1. Speaker 컴포넌트 추가

같은 프리팹에 `Add Component > Photon Voice > Speaker`.

📸 **__L3___TMP.png** — Speaker 컴포넌트 추가

### 2-2. AudioSource 자동 생성 확인

Speaker 추가 시 `AudioSource` 가 자동으로 같이 붙는다. 이게 다른 플레이어의 음성을 재생할 출력 채널.

📸 **__L3___TMP.png** — AudioSource 자동 생성된 상태

### 2-3. 프리팹 저장

`Save` 또는 `Ctrl+S`.

---

## Step 3 — V 키 입력 바인딩

### 3-1. PTT 컨트롤러 스크립트

`Assets/Interaction/Scripts/Voice/PushToTalkController.cs` 생성:

```csharp
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Recorder))]
public class PushToTalkController : MonoBehaviour
{
    [SerializeField] Key talkKey = Key.V;  // 변경 가능
    Recorder recorder;

    void Awake()
    {
        recorder = GetComponent<Recorder>();
        recorder.TransmitEnabled = false;  // 시작은 OFF
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // V 키 누르면 ON, 떼면 OFF
        recorder.TransmitEnabled = kb[talkKey].isPressed;
    }
}
```

### 3-2. 컴포넌트 부착

`PlayerCharacter` 프리팹에 `PushToTalkController` 추가.

📸 **__L3___TMP.png** — PushToTalkController 추가된 Inspector

---

## Step 4 — VoiceManager 연결

### 4-1. PhotonVoiceNetwork 자동 발견 확인

씬의 `VoiceManager` (L2 에서 만든 것) 의 `PhotonVoiceNetwork` 가 활성화되어 있어야 한다.

### 4-2. UsePrimaryRecorder

PhotonVoiceView 의 `Use Primary Recorder` 체크 — 같은 GameObject 의 Recorder 를 자동으로 연결.

📸 **__L3___TMP.png** — Use Primary Recorder 체크된 상태

---

## Step 5 — 1차 테스트 (Editor + 빌드)

### 5-1. 빌드 한 번 만들기

`File > Build And Run` 으로 `Build/PTT_Test.exe` 생성.

📸 **__L3___TMP.png** — Build Settings 창

### 5-2. Editor 와 빌드 동시 실행

- 빌드된 .exe 실행 → 같은 룸 입장
- Unity Editor 에서도 Play → 같은 룸 입장
- 두 인스턴스에서 서로의 캐릭터 보이는지 확인

📸 **__L3___TMP.png** — 두 인스턴스 동시 화면 (좌: 빌드, 우: Editor)

### 5-3. V 키 테스트

- 빌드에서 V 키 누르고 말함 → Editor 에서 들려야 함
- Editor 에서 V 키 누르고 말함 → 빌드에서 들려야 함

📸 **__L3___TMP.png** — V 키 눌렀을 때 Recorder.TransmitEnabled = true 로 변하는 Inspector

---

## Step 6 — 시각 피드백 추가 (선택)

### 6-1. 말하는 중일 때 캐릭터 위에 아이콘

`SpeakingIndicator.cs`:

```csharp
using Photon.Voice.Unity;
using UnityEngine;

public class SpeakingIndicator : MonoBehaviour
{
    [SerializeField] GameObject micIcon;  // 캐릭터 머리 위 아이콘
    Recorder recorder;
    Speaker speaker;

    void Awake()
    {
        recorder = GetComponent<Recorder>();
        speaker = GetComponent<Speaker>();
    }

    void Update()
    {
        // 자기 자신이 말하는 중 (로컬) 또는 다른 사람이 말하는 중 (원격)
        bool talking = (recorder != null && recorder.IsCurrentlyTransmitting)
                    || (speaker != null && speaker.IsPlaying);
        micIcon.SetActive(talking);
    }
}
```

📸 **__L3___TMP.png** — 캐릭터 머리 위 마이크 아이콘 GameObject

### 6-2. 아이콘 적용 후 테스트

V 누르는 동안 자기 + 상대 캐릭터 머리 위 아이콘 ON.

📸 **__L3___TMP.png** — 말하는 동안 아이콘 표시된 게임 화면

---

## Step 7 — 트러블 슈팅 시나리오

### 7-1. 음성은 안 들리는데 콘솔 에러는 없을 때

- Speaker 가 AudioSource 의 `Spatial Blend` 가 1.0 이면 거리 멀면 안 들림 → 0 으로 임시 변경해 테스트
- 마이크 권한 다시 확인

📸 **__L3___TMP.png** — AudioSource Spatial Blend = 0 으로 임시 변경

### 7-2. 자기 목소리가 자기에게 들릴 때

- Recorder 의 `Debug Echo Mode` 가 ON 인지 확인. 끄면 자기 음성은 자기에게 안 들림 (보통 원함)

📸 **__L3___TMP.png** — Recorder Debug Echo Mode 위치

---

## 정상 동작 체크리스트

- [ ] V 누를 때만 자기 캐릭터의 `Recorder.TransmitEnabled = true`
- [ ] 두 명 동시 접속 후 V 누르면 상대방 PC 에서 들림
- [ ] 키를 떼면 음성이 즉시 끊김 (지연 1초 이내)
- [ ] 머리 위 마이크 아이콘이 말하는 동안 표시 (선택 구현)
- [ ] 컴파일 에러·런타임 에러 없음

다음 실습 **L3 — Spatial Audio** 에서는 캐릭터 거리에 따라 음량이 변하도록 한다.

---

## 🚀 응용 질문

### Q1. PTT 키를 사용자가 자유롭게 변경하도록 하려면?

지금은 V 키 고정. 옵션 메뉴에서 변경 가능하게 만들려면?

힌트:
- Input System Action Asset 활용
- 사용자 설정을 PlayerPrefs 또는 JSON 으로 저장
- Rebinding API 사용 → InputAction.PerformInteractiveRebinding()

### Q2. PTT 가 아닌 "음성 활성화 (VAD)" 모드로 자동 송신하려면?

키를 누르지 않아도 사용자가 말하면 자동으로 마이크 켜지고, 일정 시간 무음이면 꺼지는 방식.

힌트:
- Recorder 의 `VoiceDetector` 활성화
- VAD 임계값 (dB) 조정 — 너무 낮으면 키보드 소리도 잡힘
- "PTT vs VAD" 모드 토글 UI 추가

### Q3. 음소거(Mute) 기능을 추가하려면?

마이크 자체를 끄거나, 특정 사용자의 소리만 안 듣게.

힌트:
- 자기 마이크 끄기: `Recorder.RecordingEnabled = false`
- 특정 사용자 음성 안 듣기: 그 사용자의 `Speaker.GetComponent<AudioSource>().mute = true`
- UI 에 다른 플레이어 목록 + 음소거 토글 버튼

---

> 응용 질문 중 **하나 이상** 구현한 결과는 최종 산출물에 포함.
