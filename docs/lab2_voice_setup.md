# 실습 L2 — Stylized Astronaut 캐릭터 + Vivox 음성 SDK 셋업

> L1 에서 만든 빈 프로젝트에 **Stylized Astronaut (Asset Store, 무료)** 캐릭터를 가져오고, **Unity Gaming Services (UGS) 의 Vivox** 를 활성화해 연결한 뒤, **A 청각 검증 (Echo 채널) + B 시각 검증 (Volume Meter)** 으로 마이크가 제대로 작동하는지 확인한다.
>
> ⏱️ 예상 시간: 70분 · 📸 슬롯: L2_01 ~ L2_16
> 📁 산출물: Astronaut 가 씬에 존재 + Vivox 연결 + 마이크 검증 도구 동작

> ℹ️ **왜 Photon Voice 가 아닌 Vivox?**
> Quantum 3 와 호환되는 **Photon Voice Realtime 5** 는 Photon Industries Circle ($250/월) 멤버십이 필요해 학생 강의 환경에는 부적합. **Vivox (Unity 공식 UGS 일부)** 는 매월 5,000 PCU 무료 한도를 제공하고 Quantum 3 와 네트워크 충돌 없이 공존한다. (자세한 비교는 reference 문서 참조)

---

## 학습 목표

1. Unity Asset Store 에서 **Stylized Astronaut** (Generic rig) 를 다운로드·임포트한다
2. FBX Rig 를 **Generic** 으로 유지하고, Animation Rigging 패키지가 본 transform 기반이라 Humanoid 비의존임을 이해한다
3. **Unity Gaming Services (UGS)** 의 프로젝트 연결 흐름과 Vivox 활성화 절차를 안다
4. `UnityServices` · `AuthenticationService` · `VivoxService` 의 초기화 순서를 구분해 설명할 수 있다
5. **Echo 채널** (`JoinEchoChannelAsync`) 로 자기 음성을 청각 검증한다 (방법 A)
6. **VivoxParticipant.AudioEnergy** 로 마이크 음량을 시각 검증한다 (방법 B)

## 사전 확인

- [ ] L1 체크리스트 8개 모두 통과 (Unity 6.3 LTS · URP 프로젝트 · 패키지 · 베이스 씬)
- [ ] Unity ID 로그인 가능 (Unity Hub 좌상단 계정 확인)
- [ ] 마이크 (헤드셋·내장 모두 가능) 작동

---

## Step 1 — Stylized Astronaut 다운로드

> Asset Store ID 114298 / Publisher: PULSAR BYTES / 무료 / 871KB / URP 호환

### 1-1. Asset Store 페이지 접속

브라우저로 [Stylized Astronaut](https://assetstore.unity.com/packages/3d/characters/humanoids/sci-fi/stylized-astronaut-114298) 접속 → 로그인 → `Add to My Assets`.

📸 **L2_01.png** — Asset Store 의 Stylized Astronaut 페이지 + Add to My Assets 클릭

### 1-2. Unity 에서 Download · Import

Unity 의 `Window > Package Manager > My Assets` 에서 `Stylized Astronaut` 찾기 → `Download` → `Import`.

Import 다이얼로그는 전체 체크 상태로 `Import` (FBX·텍스처·머티리얼 모두 필요).

📸 **L2_02.png** — Import 다이얼로그 (전체 체크된 상태)

### 1-3. 임포트 결과 확인

`Assets/Stylized_Astronaut/` 폴더 생성 확인. 안에 `Character/Astronaut.fbx` + 텍스처 + 머티리얼.

📸 **L2_03.png** — Project 창의 Stylized Astronaut 폴더 트리

---

## Step 2 — Rig 설정 + 씬 배치

### 2-1. FBX Rig 확인 (Generic 유지)

Project 창에서 `Astronaut.fbx` 선택 → Inspector 의 `Rig` 탭 → **Animation Type: Generic** 그대로 유지.

> ⚠️ **Humanoid 로 바꾸면 안 되는 이유**
> Stylized Astronaut 의 본 이름이 Unity 표준 (`LeftFoot`) 이 아니라 `Foot_1_Left` / `Foot_2_Left` 형식이라 자동 매핑이 실패한다 (`Required human bone 'LeftFoot' not found` 에러).
>
> 💡 **Generic 이라도 후속 IK 학습은 가능**
> L8 에서 쓸 `Animation Rigging` 의 `TwoBoneIKConstraint`·`MultiAimConstraint`·`RigBuilder` 모두 **본 transform 을 직접 지정**하므로 Humanoid 의존이 없다. Humanoid 가 진짜 필요한 건 Mecanim Retargeting (다른 휴머노이드 anim 끌어다 쓰기) 뿐인데, 본 모듈은 자체 anim 클립이라 무관.

📸 **L2_04.png** — FBX Inspector 의 Rig 탭이 Generic 유지된 모습

### 2-2. 머티리얼 URP 변환 + 씬 배치 (PlayerCharacter 프리팹화)

1. `Assets/Stylized_Astronaut/Materials/` 의 머티리얼들을 모두 선택 → 메뉴 `Edit > Rendering > Materials > Convert Selected Built-in Materials to Current SRP` (= URP)
2. `Astronaut.fbx` 원본을 `Hierarchy` 의 씬 루트로 드래그 (에셋에 포함된 데모 prefab 이 아니라 **FBX 원본** 사용 — 데모 prefab 의 자체 스크립트 회피 목적)
3. 씬에 들어간 GameObject 의 이름을 **`PlayerCharacter`** 로 변경
4. Position 을 `(0, 0, 0)`
5. PlayerCharacter 를 `Assets/3.Prefabs/PlayerCharacter.prefab` 으로 드래그 → 프리팹화

📸 **L2_05.png** — 씬에 Astronaut 가 PlayerCharacter 이름으로 배치되고 URP 머티리얼 적용된 상태

### 2-3. 씬 카메라로 외형 확인

Game 뷰에서 우주인이 보이는지 확인. 너무 멀거나 가까우면 Main Camera Position 조정.

---

## Step 3 — Vivox 패키지 설치

> Photon Voice SDK 와 달리 `.unitypackage` 다운로드가 필요 없다. Unity Package Manager 에서 바로 설치한다.

### 3-1. Package Manager 열기

메뉴 `Window > Package Manager` → 좌상단 `+` 버튼 → **Add package by name...**

### 3-2. Vivox 패키지 추가

다음 정확히 입력 후 `Add`:

```
com.unity.services.vivox
```

> 💡 **버전 메모**: 16.x 가 UGS Dashboard 연동을 지원하는 라인 (이전 16.0.0 미만은 Vivox Developer Portal 기반 deprecated). `Add by name` 만으로 최신 16.x 가 자동 설치된다.

### 3-3. 의존성 자동 설치 확인

Vivox 패키지가 다음 의존성을 함께 들여온다:

- `com.unity.services.core` (UGS 코어)
- `com.unity.services.authentication` (UGS 인증)

Console 에 빨간 에러 없으면 OK.

📸 **L2_06.png** — Package Manager 에 `Vivox 16.x.x` 가 Installed 로 표시된 상태

---

## Step 4 — Unity Dashboard 에서 Vivox 서비스 활성화

> Vivox 는 UGS 의 일부 → Unity Dashboard 에서 프로젝트 ID 발급 + Vivox 활성화 흐름.

### 4-1. Unity Dashboard 접속

브라우저로 [cloud.unity.com](https://cloud.unity.com) 로그인 → 좌측 `Projects` → `Create project` (또는 기존 프로젝트 선택) → 이름: `Ch17_AdvanceCourse` → `Create`.

생성된 프로젝트의 **Project ID** 를 메모해 둔다 (UUID 형식).

📸 **L2_07.png** — Unity Dashboard 의 Ch17_AdvanceCourse 프로젝트 + Project ID 표시

### 4-2. Vivox + Authentication 서비스 활성화

Dashboard 좌측 메뉴 `Products`:

1. **Vivox** → `Activate` 또는 `Get started`
2. **Authentication** → `Activate` (Anonymous sign-in 사용)

> ⚠️ **Authentication 도 반드시 활성화**. Vivox 는 UGS 인증 토큰 기반이라 Authentication 을 같이 켜야 정상 로그인된다.

📸 **L2_08.png** — Dashboard 에서 Vivox + Authentication 둘 다 Active 표시된 상태

### 4-3. Unity 에디터에서 프로젝트 연결

Unity 메뉴 `Edit > Project Settings > Services` → `Link to existing Unity Cloud Project` → Organization · Project 선택 (4-1 에서 만든 `Ch17_AdvanceCourse`) → `Link`.

📸 **L2_09.png** — Project Settings > Services 에 Project ID 가 연결된 모습

---

## Step 5 — Vivox 초기화 매니저 스크립트

### 5-1. VivoxManager.cs 작성

`Assets/2.Scripts/1.Voice/VivoxManager.cs` 생성:

```csharp
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class VivoxManager : MonoBehaviour
{
    public static VivoxManager Instance { get; private set; }

    [SerializeField] string defaultChannelName = "Lobby";
    [SerializeField] string displayName = "Player";

    public bool IsLoggedIn => VivoxService.Instance != null && VivoxService.Instance.IsLoggedIn;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        await InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            await VivoxService.Instance.InitializeAsync();

            Debug.Log($"[Vivox] Initialized. PlayerId={AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Vivox] Init failed: {e.Message}");
        }
    }

    public async Task LoginAsync()
    {
        var options = new LoginOptions
        {
            DisplayName = displayName + "_" + UnityEngine.Random.Range(1000, 9999),
            EnableTTS = false,
        };
        await VivoxService.Instance.LoginAsync(options);
        Debug.Log($"[Vivox] Logged in as {options.DisplayName}");
    }

    public async Task JoinDefaultChannelAsync()
    {
        await VivoxService.Instance.JoinGroupChannelAsync(
            defaultChannelName,
            ChatCapability.AudioOnly);
        Debug.Log($"[Vivox] Joined channel: {defaultChannelName}");
    }
}
```

### 5-2. VivoxManager 빈 GameObject 생성

씬에 `VivoxManager` 라는 빈 GameObject 생성 → 위 스크립트 부착.

Inspector 에서:
- **Default Channel Name**: `Lobby`
- **Display Name**: `Astronaut`

📸 **L2_10.png** — VivoxManager Inspector (DefaultChannelName · DisplayName)

### 5-3. 초기화 검증

Play → Console 에 `[Vivox] Initialized. PlayerId=...` 출력 확인.

> 💡 **자주 나는 에러**:
> - `Vivox service is not active in your project` → Step 4-2 에서 Vivox 활성화 안 됨
> - `AuthenticationException` → Step 4-2 에서 Authentication 활성화 안 됨
> - `Cannot link cloud project` → Step 4-3 에서 프로젝트 연결 안 됨

---

## Step 6 — 로그인 + 채널 입장 트리거

### 6-1. 간단한 디버그 키 바인딩

`VivoxManager` 에 메서드 추가:

```csharp
async void Update()
{
    if (Input.GetKeyDown(KeyCode.L)) await LoginAsync();
    if (Input.GetKeyDown(KeyCode.J)) await JoinDefaultChannelAsync();
}
```

> L = Login, J = Join. 실전에서는 UI 버튼으로 빼지만 검증 단계는 키로 충분.

### 6-2. Play 후 검증

1. Play
2. `L` 키 → Console 에 `Logged in as Astronaut_xxxx`
3. `J` 키 → Console 에 `Joined channel: Lobby`

📸 **L2_11.png** — Console 에 Logged in + Joined channel 두 줄 출력된 상태

---

## Step 7 — 마이크 인식 확인 (방법 A 의 사전 단계)

### 7-1. VivoxManager 에 마이크 목록 출력 추가

`Update()` 에 추가:

```csharp
if (Input.GetKeyDown(KeyCode.M))
{
    Debug.Log("[Vivox] 입력 장치 목록:");
    foreach (var dev in VivoxService.Instance.AvailableInputDevices)
        Debug.Log($"  - {dev.DeviceName} (Active={dev == VivoxService.Instance.ActiveInputDevice})");
}
```

### 7-2. Play 후 M 키

`L` → `J` → `M` 순서로 누르면 Console 에 마이크 목록 + 현재 Active 표시.

📸 **L2_12.png** — Console 에 마이크 목록 출력된 상태

> 💡 **장치 변경 API**: `await VivoxService.Instance.SetActiveInputDeviceAsync(device);` — 응용 질문에서 다룬다.

---

## Step 8 — 방법 A: Echo 채널 (청각 검증)

> Vivox 의 `JoinEchoChannelAsync` 는 자기 마이크 입력을 자기 헤드폰으로 즉시 재생해 주는 전용 채널. Photon Voice 의 `DebugEchoMode` 와 같은 역할.

### 8-1. VivoxManager 에 Echo 메서드 추가

```csharp
public async Task JoinEchoChannelAsync()
{
    await VivoxService.Instance.JoinEchoChannelAsync(
        "EchoTest",
        ChatCapability.AudioOnly);
    Debug.Log("[Vivox] Echo channel joined.");
}

public async Task LeaveEchoChannelAsync()
{
    await VivoxService.Instance.LeaveChannelAsync("EchoTest");
    Debug.Log("[Vivox] Echo channel left.");
}
```

키 바인딩 추가:

```csharp
if (Input.GetKeyDown(KeyCode.E)) await JoinEchoChannelAsync();
if (Input.GetKeyDown(KeyCode.X)) await LeaveEchoChannelAsync();
```

### 8-2. Play → 헤드폰으로 자기 목소리 확인

- 헤드폰 착용
- `L` → `E` → 잠시 후 말하기
- 자기 음성이 자기 헤드폰에 들리면 OK

> ⚠️ 스피커로 들으면 하울링(피드백 루프) 위험. 반드시 헤드폰.

📸 **L2_13.png** — Echo channel joined 로그 + 헤드폰으로 자기 음성 확인 중인 화면

### 8-3. 검증 완료 후 X 키

실제 배포 시 자기 음성은 자기에게 안 들리는 게 자연스러움. 검증 끝나면 `X` 키로 Echo 채널 나감.

---

## Step 9 — 방법 B: Volume Meter UI (시각 검증)

> Vivox 의 `VivoxParticipant.AudioEnergy` (0.0 ~ 1.0) 는 해당 참가자가 내는 음성 에너지. 자기 자신은 `IsSelf == true` 인 참가자로 표현되어, 자기 마이크 음량을 그대로 시각화할 수 있다.

### 9-1. UI 준비

`Canvas` 하위에 다음 구조 (없으면 `GameObject > UI > Canvas` 부터):

```
Canvas (HUD)
└─ VolumeMeter (Panel, 좌상단)
   ├─ Background (Image, 어두운 회색, 폭 200·높이 30)
   └─ Fill (Image, 초록색, 동일 위치)
```

📸 **L2_14.png** — VolumeMeter UI 의 Hierarchy + 좌상단 위치

### 9-2. Fill Image 설정

- **Image Type**: `Filled`
- **Fill Method**: `Horizontal`
- **Fill Origin**: `Left`
- **Fill Amount**: `0` (초기)

### 9-3. MicVolumeMeter 스크립트

`Assets/2.Scripts/1.Voice/MicVolumeMeter.cs`:

```csharp
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class MicVolumeMeter : MonoBehaviour
{
    [SerializeField] Image fillBar;
    [SerializeField] float boost = 3f;

    VivoxParticipant selfParticipant;

    void OnEnable()
    {
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;
    }

    void OnDisable()
    {
        if (VivoxService.Instance == null) return;
        VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
    }

    void OnParticipantAdded(VivoxParticipant p)
    {
        if (p.IsSelf) selfParticipant = p;
    }

    void OnParticipantRemoved(VivoxParticipant p)
    {
        if (p.IsSelf) selfParticipant = null;
    }

    void Update()
    {
        if (selfParticipant == null) { fillBar.fillAmount = 0f; return; }
        float energy = (float)selfParticipant.AudioEnergy;
        fillBar.fillAmount = Mathf.Clamp01(energy * boost);
    }
}
```

### 9-4. 컴포넌트 부착

`VolumeMeter` GameObject 에 `MicVolumeMeter` 추가, Inspector 에서:
- **Fill Bar** ← Fill Image

📸 **L2_15.png** — MicVolumeMeter Inspector 필드 입력 완료

### 9-5. 실행 + 말하기

`L` → `E` (Echo 채널) → 말하면 막대가 차오르고, 입 다물면 0 으로 돌아옴.

📸 **L2_16.png** — Echo 채널 입장 + 말하는 동안 막대가 차오른 화면 캡처

> 💡 **막대가 0 에서 안 움직이면**:
> - 채널에 입장하지 않은 상태 (먼저 `L` → `E` 또는 `J`)
> - Windows 마이크 권한 / 장치 연결 / `VivoxService.Instance.AvailableInputDevices` 비어 있지 않은지 확인
> - `selfParticipant == null` — Console 로그 추가해 `OnParticipantAdded` 가 호출됐는지 확인

---

## 정상 동작 체크리스트

- [ ] Stylized Astronaut 가 씬에 PlayerCharacter 이름으로 존재 (Generic rig, URP 머티리얼)
- [ ] Unity Dashboard 에 `Ch17_AdvanceCourse` 프로젝트 존재 + **Vivox · Authentication 둘 다 Active**
- [ ] Project Settings > Services 에서 프로젝트가 Unity Cloud 와 연결됨
- [ ] Play 시 Console 에 `[Vivox] Initialized. PlayerId=...` 출력
- [ ] `L` 키 → Vivox 로그인 성공 로그
- [ ] `J` 키 → `Lobby` 채널 입장 성공 로그
- [ ] `M` 키 → 마이크 입력 장치 목록 출력
- [ ] **A. Echo 채널 (`E` 키)** 입장 후 헤드폰으로 자기 음성이 들림
- [ ] **B. Volume Meter** 가 말할 때마다 막대로 시각 반응 (Echo 또는 Lobby 채널 입장 상태에서)

다음 실습 **L3 — Push-to-Talk** 에서는 V 키 바인딩으로 `VivoxService.Instance.SetChannelTransmissionModeAsync` 를 토글해 음성 송신을 제어한다.

---

## 🚀 응용 질문

### Q1. 마이크 장치가 여러 개일 때 드롭다운으로 선택하게 하려면?

힌트:
- `VivoxService.Instance.AvailableInputDevices` 를 TMP_Dropdown 의 옵션으로 매핑
- 선택 시 `await VivoxService.Instance.SetActiveInputDeviceAsync(device);`
- `AvailableInputDevicesChanged` 이벤트로 USB 마이크 hot-plug 반영

### Q2. Volume Meter 가 너무 작은 소리도 잡아서 노이즈처럼 보일 때 어떻게 정제할까?

힌트:
- 임계값 (threshold) 아래는 0 으로 처리 (`energy < 0.05 ? 0 : energy`)
- 시간 평활화 (Smoothing) — `Mathf.Lerp(prev, target, Time.deltaTime * 8)` 로 점진 변화
- Vivox 자체의 `VivoxService.Instance.InputDeviceMuted` 토글로 노이즈 게이트 흉내

### Q3. 다중 룸별로 Voice 채널을 분리하려면?

힌트:
- Quantum 룸 이름과 동일한 문자열을 Vivox 채널명으로 사용 (`JoinGroupChannelAsync(roomName, ...)`)
- 룸 이동 시 `await VivoxService.Instance.LeaveChannelAsync(oldRoom)` 후 새 룸 Join
- `ActiveChannels` 딕셔너리로 현재 입장한 채널 확인

---

> 응용 질문 중 **하나 이상** 구현한 결과는 최종 산출물에 포함.
