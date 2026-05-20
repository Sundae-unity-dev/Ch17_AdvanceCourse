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

### 3-3. 의존성 확인

Vivox 패키지가 다음을 자동으로 함께 들여온다:

- `com.unity.services.core` (UGS 코어) ✅

> ⚠️ **함정**: `com.unity.services.authentication` 은 Vivox 16.x 의존성에 들어 있지 않다. 별도로 직접 설치해야 한다 (다음 3-4).

Console 에 빨간 에러 없으면 OK.

📸 **L2_06.png** — Package Manager 에 `Vivox 16.x.x` 가 Installed 로 표시된 상태

### 3-4. Authentication 패키지 별도 설치 (필수)

> Vivox 가 UGS 인증 토큰을 사용하기 때문에 Authentication 패키지가 반드시 함께 깔려 있어야 한다. 안 깔면 Step 4-3 에서 Vivox 패널에 노란 경고:
> `The Authentication Package has not been imported.`
> 가 뜨고, 런타임에서도 `AuthenticationException` 으로 초기화가 실패한다.

Package Manager 좌측에서 `Unity Registry` 선택 → 검색창에 `auth` 입력 → **Authentication** (Technical Name: `com.unity.services.authentication`, 3.6.1 이상) 선택 → 우측 `Install`.

> 💡 또는 좌상단 `+` > `Install package by name...` 에 `com.unity.services.authentication` 직접 입력해도 같은 결과.

📸 **L2_06b.png** — Package Manager 의 Authentication 패키지 (3.6.1) 가 Install 또는 Installed 상태로 표시된 화면

---

## Step 4 — Unity Dashboard 에서 Vivox 서비스 활성화

> Vivox 는 UGS 의 일부다. Unity Dashboard 에서 프로젝트를 만들고, Vivox 의 **공식 설정 가이드 (3단계 마법사)** 를 그대로 따라가면 Unity 프로젝트 연결까지 자동으로 처리된다. Player Authentication 만 마법사 외부에서 별도로 활성화하면 끝.

### 4-1. Unity Dashboard 접속 + 프로젝트 생성

브라우저로 [cloud.unity.com](https://cloud.unity.com) 로그인 → 좌측 `Projects` → `Create project` (또는 기존 프로젝트 선택) → 이름: `Ch17_AdvanceCourse` → `Create`.

생성된 프로젝트를 클릭해 상세 페이지로 들어간다. `설정(Settings)` 탭의 **Project ID** (UUID) 는 한 번 확인해두면 좋다 (필수는 아님 — Unity 에디터가 자동 매칭).

📸 **L2_07.png** — Unity Dashboard 의 Ch17_AdvanceCourse 프로젝트 상세 페이지

### 4-2. Vivox 진입 + 설정 가이드 시작

Dashboard 상단 탭 `서비스(Services)` → **Vivox Voice and Text Chat** 카드 클릭 → 좌측 메뉴 **`설정 가이드`** 선택.

마법사 3단계가 차례대로 진행된다.

📸 **L2_08a.png** — Vivox 설정 가이드 1단계 (엔진 및 서비스 선택) 진입 화면

### 4-3. 마법사 1단계 — 엔진 및 서비스 선택

- **게임 엔진**: **`Unity Build Automation`** 클릭 (Unreal / 커스텀 무시)
- **필수 서비스**: **`Voice and Text Chat`** 이 이미 선택됨 — 그대로 둔다
- **선택 서비스 (Safe Text)**: 강의 범위 밖이라 **선택하지 않음**

하단 `다음` 클릭.

### 4-4. 마법사 2단계 — Unity 프로젝트 연결

마법사가 자동으로 "Edit > Project Settings > Services" 의 Link 절차를 안내한다.

1. Unity 에디터 열고 `Edit > Project Settings > Services`
2. `Use an existing Unity project ID` 섹션의 Organizations / Projects 드롭다운:
   - **Organization**: 본인 조직 (예: `bjclove15`)
   - **Project**: `Ch17_AdvanceCourse`
3. `Link project ID` 클릭

연결되면 우측 패널에 다음이 표시된다.
- **Project Name**: `Ch17_AdvanceCourse`
- **Unity Project ID**: UUID 표시 + `Unlink project` 버튼
- 좌측 트리에 `Services > Vivox` 항목 자동 추가

Dashboard 마법사 화면으로 돌아와 하단 `다음` 클릭.

> 💡 **이미 연결돼 있다면 그냥 `다음`**: 어제·이전에 다른 흐름으로 같은 프로젝트를 Link 했다면 `Unlink project` 버튼이 이미 보이는 상태다. 이 단계는 추가 작업 없이 `다음` 만 누르면 통과된다. ("Project was linked successfully" 배너는 최초 Link 1회만 표시되므로 안 보여도 정상.)

📸 **L2_09.png** — Project Settings > Services 에 Ch17_AdvanceCourse 가 연결된 화면

### 4-5. 마법사 3단계 — 서비스 구성

마법사가 “구성 완료” 같은 마지막 안내 화면을 보여준다. 추가 작업이 없으면 `완료(Finish)` 클릭.

> 💡 **Dashboard 의 `패키지 및 SDK` 페이지는 무시해도 된다**
> 그 페이지에 보이는 **Unity Windows SDK / Unity Android SDK / Core Android SDK / Core Windows SDK** 등은 `com.unity.services.vivox` 16.x 패키지 안에 이미 네이티브 플러그인 (`VivoxNative.dll`·`libVivoxNative.so`) 으로 포함되어 있어 별도로 받지 않아도 된다.
> `패키지 설치` 버튼은 Unity Editor 로 Deep Link 를 보내지만 자주 전달이 안 된다. 눌렀을 때 아무 반응 없어도 무시. **Unreal 엔진이나 Switch/PS 콘솔 빌드** 같은 특수 경우에만 의미가 있다.

#### 직접 확인하고 싶다면 (검증)

Unity 의 Project 창 상단 검색창에 정확히 입력:
```
VivoxNative
```
검색 범위를 **`In Packages`** 로 바꾸면 다음 두 파일이 검색된다.

- `VivoxNative.dll` (Windows 용)
- `libVivoxNative.so` (Android 용)

둘 다 `com.unity.services.vivox` 패키지의 `Runtime/Plugins/` 하위에 이미 존재. Dashboard 에서 받는 것과 동일한 내용물이다.

### 4-6. Player Authentication 활성화 (마법사 외 별도)

Vivox 설정 가이드 마법사는 Vivox 활성화까지만 처리한다. **Player Authentication 은 별도로 활성화** 해야 한다.

1. Dashboard 의 `Ch17_AdvanceCourse` → `서비스` 탭
2. 목록을 아래로 스크롤해 **Player Authentication** 카드 찾기 (설명: "익명, 플랫폼별 또는 커스텀 플레이어 인증")
3. 카드 우측 `실행` 클릭 → Authentication 상세 페이지의 `Identity Providers` 화면 진입
4. 화면이 열리면 **활성 상태로 간주**. Identity Provider 등록은 0개 그대로 두어도 OK — 익명 로그인 (`SignInAnonymouslyAsync`) 은 Provider 없이 동작.

> 💡 한국어 라벨 함정: 서비스 탭의 **"활성화된 Game Overrides"** 섹션은 사실 "최근 30일간 트래픽이 있었던 서비스" 라는 뜻이다. 트래픽이 없는 신규 서비스는 비활성/설정 중 섹션에 머문다. **이게 “비활성”을 의미하지는 않는다**.

📸 **L2_08b.png** — Player Authentication 의 Identity Providers 페이지

### 4-7. Vivox 패널 정상 상태 확인

다시 Unity 에디터 `Edit > Project Settings > Services` → 좌측 트리 `Services > Vivox` 클릭 → 우측 `Environment Configuration` 섹션:

- **Server / Domain / Token Issuer / Token Key**: 모두 비어 있음 ✅ (UGS Dashboard 자동 인증 방식에서는 빈 칸이 정상. Test Mode 켜는 학습 경로에서만 채움)
- **Test Mode**: OFF ✅
- 페이지 하단에 **노란 경고가 없어야 한다**.

> ⚠️ 만약 `The Authentication Package has not been imported.` 노란 경고가 보이면 Step 3-4 의 Authentication 패키지 설치가 빠진 상태다. 돌아가서 설치 후 (필요하면 Unity 재시작) 다시 확인.

📸 **L2_09b.png** — Vivox 패널의 Environment Configuration 섹션 (빈 칸 + 노란 경고 없음)

---

## Step 5 — Vivox 매니저 스크립트 (통합본)

> 이 한 파일에 **초기화(Step 5) · 로그인+채널(Step 6) · 마이크 인식(Step 7) · Echo 채널(Step 8)** 의 모든 메서드와 디버그 키 바인딩을 한꺼번에 넣는다. 학생은 한 번의 복사 붙여넣기로 모든 검증 단계에 필요한 코드를 확보한다. 이후 Step 6~8 에서는 코드 추가 없이 **Play 모드에서 키만 눌러 검증**한다.

### 5-1. VivoxManager.cs 작성

`Assets/2.Scripts/1.Voice/VivoxManager.cs` 생성 후 아래 코드 전체를 통째로 붙여넣기.

```csharp
// =============================================================================
// VivoxManager.cs — Vivox 음성 SDK 의 초기화 / 로그인 / 채널 입장 / 마이크 인식 /
//                   Echo 검증을 한 곳에서 담당하는 매니저.
// -----------------------------------------------------------------------------
// 학습 포인트:
// 1) Unity Gaming Services 의 초기화는 반드시 정해진 순서를 따라야 한다.
//    UnityServices  →  AuthenticationService  →  VivoxService
// 2) 세 단계 모두 await 가 필요한 비동기 작업이라 async/await 패턴을 쓴다.
// 3) 어디서든 VivoxManager.Instance.XXX 로 접근할 수 있도록 싱글톤으로 만든다.
// 4) 검증 단계에서는 UI 대신 디버그 키 (L/J/M/E/X) 로 트리거한다.
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
    //   L 키 → Vivox 로그인              (LoginAsync)
    //   J 키 → 기본 그룹 채널 입장        (JoinDefaultChannelAsync)
    //   M 키 → 인식된 마이크 목록 출력    (LogInputDevices)
    //   E 키 → Echo 채널 입장 (청각 검증) (JoinEchoChannelAsync)
    //   X 키 → Echo 채널 나가기           (LeaveEchoChannelAsync)
    // 실제 게임에서는 UI 버튼으로 대체하지만, 검증 단계는 키가 가장 빠르다.
    // ---------------------------------------------------------------------
    async void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) await LoginAsync();
        if (Input.GetKeyDown(KeyCode.J)) await JoinDefaultChannelAsync();
        if (Input.GetKeyDown(KeyCode.M)) LogInputDevices();
        if (Input.GetKeyDown(KeyCode.E)) await JoinEchoChannelAsync();
        if (Input.GetKeyDown(KeyCode.X)) await LeaveEchoChannelAsync();
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

        Debug.Log($"[Vivox] Joined channel: {defaultChannelName}");
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

        Debug.Log($"[Vivox] Echo channel joined: {echoChannelName}");
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
}
```

### 5-2. VivoxManager 빈 GameObject 생성

씬에 `VivoxManager` 라는 빈 GameObject 생성 → 위 스크립트 부착.

Inspector 에서:
- **Default Channel Name**: `Lobby`
- **Display Name**: `Astronaut`
- **Echo Channel Name**: `EchoTest` (기본값 그대로)

📸 **L2_10.png** — VivoxManager Inspector (DefaultChannelName · DisplayName · EchoChannelName)

### 5-3. 초기화 검증

Play → Console 에 `[Vivox] Initialized. PlayerId=...` 출력 확인.

> 💡 **자주 나는 에러**:
> - `Vivox service is not active in your project` → Dashboard 의 Vivox 활성화 또는 자격 증명 발급 누락
> - `AuthenticationException` → Authentication 패키지 미설치 또는 Player Authentication 미활성
> - `Cannot link cloud project` → Project Settings > Services 의 프로젝트 연결 누락

---

## Step 6 — 로그인 + 채널 입장 검증 (코드 추가 없음)

> Step 5 의 통합 코드에 `L` / `J` 키 바인딩이 이미 포함되어 있다. 이번 단계는 **Play 모드에서 키를 누르고 Console 로그만 확인**한다.

### 6-1. 검증 절차

1. Unity 에디터에서 **Play ▶️** 클릭
2. 잠시 후 Console 에 `[Vivox] Initialized. PlayerId=...` 가 한 번 뜨는지 확인
3. **Game 뷰를 한 번 클릭** (포커스 잡기) — 안 하면 키 입력이 안 잡힘
4. **`L` 키** 누르기 → Console 에 다음 줄이 떠야 정상:
   ```
   [Vivox] Logged in as Astronaut_xxxx
   ```
5. **`J` 키** 누르기 → Console 에 다음 줄이 떠야 정상:
   ```
   [Vivox] Joined channel: Lobby
   ```

📸 **L2_11.png** — Console 에 `Initialized` + `Logged in` + `Joined channel` 세 줄이 차례로 출력된 상태

> ⚠️ Scene 뷰가 활성화된 상태에서는 키 입력이 들어오지 않는다. 반드시 Game 뷰를 클릭한 다음 키를 누른다.

---

## Step 7 — 마이크 인식 확인 (코드 추가 없음)

> Step 5 의 통합 코드에 `M` 키 바인딩 + `LogInputDevices()` 가 이미 포함되어 있다.

### 7-1. 검증 절차

`L` → `J` 이후 또는 그냥 `M` 키 단독으로:

1. **`M` 키** 누르기 → Console 에 마이크 목록 + 현재 Active 표시:
   ```
   [Vivox] 입력 장치 목록:
     - Default System Device (Active=True)
     - Microphone (Realtek(R) Audio) (Active=False)
     - ...
   ```

📸 **L2_12.png** — Console 에 마이크 목록 출력된 상태

> 💡 **장치 변경 API**: `await VivoxService.Instance.SetActiveInputDeviceAsync(device);` — 응용 질문 Q1 에서 다룬다.

---

## Step 8 — 방법 A: Echo 채널 (청각 검증, 코드 추가 없음)

> Step 5 의 통합 코드에 `E` / `X` 키 바인딩 + `JoinEchoChannelAsync()` · `LeaveEchoChannelAsync()` 가 이미 포함되어 있다. Vivox 의 `JoinEchoChannelAsync` 는 자기 마이크 입력을 자기 헤드폰으로 즉시 재생해 주는 전용 채널 (Photon Voice 의 `DebugEchoMode` 와 같은 역할).

### 8-1. 검증 절차

1. **헤드폰 착용** (반드시. 스피커로 들으면 하울링/피드백 루프 위험)
2. Play → `L` 키 (로그인)
3. **`E` 키** 누르기 → Console 에 `[Vivox] Echo channel joined: EchoTest` 출력
4. 잠시 후 자기 목소리로 말하기 → **헤드폰에 자기 음성이 들리면 OK**

📸 **L2_13.png** — Echo channel joined 로그 + 헤드폰으로 자기 음성 확인 중인 화면

### 8-2. 검증 완료 후 X 키

검증이 끝나면 `X` 키로 Echo 채널 나가기. Console 에 `[Vivox] Echo channel left: EchoTest` 출력.

> 실제 배포 시 자기 음성은 자기에게 안 들리는 게 자연스럽다. Echo 는 검증용으로만 쓰고 평소엔 끄는 습관을 들인다.

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
- [ ] **`com.unity.services.vivox` (16.x) + `com.unity.services.authentication` (3.6.1+) 두 패키지 모두 설치됨** (Package Manager > In Project)
- [ ] Unity Dashboard 에 `Ch17_AdvanceCourse` 프로젝트 존재 + **Vivox · Player Authentication 둘 다 Active**
- [ ] Project Settings > Services 에서 프로젝트가 Unity Cloud 와 연결됨
- [ ] Project Settings > Services > Vivox 패널에 노란 “Authentication Package not imported” 경고 없음
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
