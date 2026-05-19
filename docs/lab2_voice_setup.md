# 실습 L2 — Stylized Astronaut 캐릭터 + Photon Voice SDK 셋업

> L1 에서 만든 빈 프로젝트에 **Stylized Astronaut (Asset Store, 무료)** 캐릭터를 가져오고, **Voice 전용 AppId** 를 발급해 연결한 뒤, **A 청각 검증 (DebugEcho) + B 시각 검증 (Volume Meter)** 으로 마이크가 제대로 작동하는지 확인한다.
>
> ⏱️ 예상 시간: 60분 · 📸 슬롯: L2_01 ~ L2_16
> 📁 산출물: Astronaut 가 씬에 존재 + Voice 연결 + 마이크 검증 도구 동작

---

## 학습 목표

1. Unity Asset Store 에서 **Stylized Astronaut** (Humanoid rig) 를 다운로드·임포트한다
2. FBX Rig 를 **Humanoid** 로 설정해 후속 IK·시선·풀바디 학습에 대비한다
3. Photon 대시보드에서 **Voice 전용 AppId** 를 발급하고 Quantum AppId 와 구분해 관리한다
4. **VoiceConnection · Recorder · Speaker** 의 역할을 구분해 설명할 수 있다
5. **DebugEchoMode** 로 자기 음성을 청각 검증한다 (방법 A)
6. **Volume Meter UI** 로 마이크 음량을 시각 검증한다 (방법 B)

## 사전 확인

- [ ] L1 체크리스트 8개 모두 통과 (Unity 6.3 LTS · URP 프로젝트 · 패키지 · 베이스 씬)
- [ ] Photon 계정 로그인 가능
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

`Assets/Stylized Astronaut/` 폴더 생성 확인. 안에 FBX (`Character.fbx` 또는 유사한 이름) + 텍스처 + 머티리얼.

📸 **L2_03.png** — Project 창의 Stylized Astronaut 폴더 트리

---

## Step 2 — Rig 설정 + 씬 배치

### 2-1. FBX Rig 를 Humanoid 로 변경

Project 창에서 Astronaut FBX 선택 → Inspector 의 `Rig` 탭 → **Animation Type: Humanoid** → `Apply`.

> 💡 **왜 Humanoid 인가?**
> L8 (IK · 시선) 에서 `Animation Rigging` 의 MultiAimConstraint 와 Foot IK 를 쓰려면 Humanoid Avatar 가 필요하다. 기본값 Generic 으로 두면 후속 단계가 막힌다.

📸 **L2_04.png** — FBX Inspector 의 Rig 탭이 Humanoid 로 설정된 모습

### 2-2. 씬에 배치 (PlayerCharacter 프리팹화)

1. Astronaut FBX 를 `Hierarchy` 의 `PlayerSpawnPoint` 위로 드래그 → 자식으로 들어감
2. 자식 GameObject 의 이름을 **`PlayerCharacter`** 로 변경
3. Position 을 `(0, 0, 0)` (SpawnPoint 기준 로컬)
4. **PlayerCharacter 를 SpawnPoint 의 자식에서 분리** → 씬의 최상위로 이동
5. PlayerCharacter 를 `Assets/3.Prefabs/PlayerCharacter.prefab` 으로 드래그 → 프리팹화

📸 **L2_05.png** — 씬에 Astronaut 가 PlayerCharacter 이름으로 배치된 상태

### 2-3. 씬 카메라로 외형 확인

Game 뷰에서 우주인이 보이는지 확인. 너무 멀거나 가까우면 Step 1 의 Main Camera Position 조정.

---

## Step 3 — Photon Voice SDK Realtime5 다운로드·임포트 ⚠️

> **Asset Store 의 "Photon Voice 2" (ID 130518) 가 아닙니다.**
> 그건 Photon Realtime 4 기반이라 Quantum 3 (Realtime 5) 와 호환되지 않아 컴파일 에러 100+ 폭발한다.
> 정확한 SDK = **Photon Voice SDK Realtime5** (Photon 공식 SDK 다운로드 페이지).

### 3-1. 공식 SDK 페이지에서 다운로드

브라우저로 [https://www.photonengine.com/sdks](https://www.photonengine.com/sdks) 접속 → 로그인.

페이지에서 **Voice** 섹션 찾기 → **Realtime 5 호환 Unity SDK** (또는 "Photon Voice SDK Realtime5") 다운로드.

📸 **L2_06a.png** — Photon Engine SDKs 페이지의 Voice 섹션 (Realtime 5 버전 다운로드 버튼)

> 💡 **버전 식별 팁**: 다운로드 페이지에서 "Realtime 5" / "for Quantum 3" / "v2.5x+" 같은 표시 확인. "Realtime 4" 표시된 것은 Quantum 3 와 호환 안 됨.

### 3-2. Unity 에 임포트

다운로드한 `.unitypackage` 파일을 더블클릭 또는 Unity 메뉴 `Assets > Import Package > Custom Package` 로 임포트.

**Import 다이얼로그에서 반드시 체크 해제** (Quantum 과 중복 방지):

| 위치 | 항목 | 이유 |
|---|---|---|
| `Assets/Photon/PhotonRealtime/` | **폴더 전체** | Quantum 의 Realtime 5 와 중복 |
| `Assets/Photon/PhotonLibs/` | ⚠️ 노란 경고 있는 모든 파일 | dll 버전 충돌 방지 |

> 💡 **간단 규칙**: 다이얼로그 좌측 위에서 아래로 훑으며 ⚠️ 노란 경고 아이콘 보이면 다 체크 해제.

📸 **L2_06b.png** — Import 다이얼로그에서 PhotonRealtime · Photon3Unity3D.* 체크 해제된 상태 ⭐ 핵심

`Import` 클릭 → 2~3분 대기.

### 3-3. 임포트 결과 검증

- `Assets/Photon/PhotonVoice/` 폴더 존재 ✅
- `Assets/Photon/PhotonRealtime/` 폴더 **없음** (체크 해제했으므로) ✅
- Console 에 빨간 에러 없음 ✅

📸 **L2_06c.png** — Project 창의 Photon 폴더 구조 (Voice + Quantum 공존, PhotonRealtime 단일)

---

## Step 4 — Voice 전용 AppId 발급

> 게임용 (Quantum 3) AppId 와 **별도** 의 Voice AppId 가 필요. 같은 PhotonAppSettings 에 둘 다 따로 저장된다.

### 4-1. Photon 대시보드 접속

[dashboard.photonengine.com](https://dashboard.photonengine.com) 로그인.

### 4-2. 새 앱 만들기 — Voice 선택

`CREATE A NEW APP` 클릭 → **Photon Type: `Voice`** (⚠️ Realtime · Quantum 과 헷갈리지 말 것) → 이름: `Ch17_AdvanceCourse_Voice` → `CREATE`.

📸 **L2_07.png** — Voice 선택된 생성 폼

### 4-3. AppId 복사

생성된 앱 카드의 AppId 해시 문자열 복사. 메모장에 잠깐 붙여 두면 다음 단계가 편함.

📸 **L2_08.png** — AppId 표시된 카드 (보안상 앞 8자리만 보이게 마스킹 권장)

---

## Step 5 — PhotonAppSettings 에 AppIdVoice 입력

### 5-1. PhotonAppSettings 찾기

Project 창 검색: `t:PhotonAppSettings`. 또는 `Assets/Photon/PhotonVoice/Resources/PhotonAppSettings.asset`.

### 5-2. AppIdVoice 필드에 붙여넣기

Inspector 의 **App Id Voice** 필드에 AppId 붙여넣기.

> ⚠️ App Id Quantum 과는 **다른 필드** 다. Quantum 은 L1 에서 입력한 값이 있을 것 — 건드리지 말 것.

📸 **L2_09.png** — AppIdVoice 입력 완료된 PhotonAppSettings Inspector

### 5-3. Fixed Region (선택)

지연 줄이려면 `Fixed Region`: `asia` 또는 `kr`.

---

## Step 6 — VoiceManager + PhotonVoiceNetwork

### 6-1. 빈 GameObject 생성

씬에 `VoiceManager` 라는 빈 GameObject 생성.

### 6-2. PhotonVoiceNetwork 컴포넌트 추가

`VoiceManager` 에 `PhotonVoiceNetwork` 컴포넌트 추가.

### 6-3. Auto Connect 설정

Inspector 에서 `Connect on Start` 체크.

📸 **L2_10.png** — VoiceManager + PhotonVoiceNetwork Inspector

---

## Step 7 — 마이크 인식 테스트

### 7-1. 임시 검증 스크립트

`Assets/2.Scripts/1.Voice/MicTester.cs`:

```csharp
using UnityEngine;

public class MicTester : MonoBehaviour
{
    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[MicTester] 인식된 마이크가 없습니다!");
            return;
        }
        Debug.Log("[MicTester] 인식된 마이크:");
        foreach (var n in Microphone.devices) Debug.Log($"  - {n}");
    }
}
```

### 7-2. VoiceManager 에 부착 후 Play

`VoiceManager` 에 `MicTester` 추가 → Play.

📸 **L2_11.png** — Console 에 마이크 이름 출력된 상태

---

## Step 8 — 방법 A: DebugEchoMode (청각 검증)

> 자기 마이크 입력이 자기 헤드폰으로 즉시 재생됨. "마이크가 인식되는가" 가장 빠른 청각 검증.

### 8-1. PlayerCharacter 의 Recorder 추가

PlayerCharacter prefab 더블클릭으로 편집 모드 → `Add Component > Photon Voice > Recorder`.

### 8-2. DebugEchoMode 활성화

Recorder Inspector 에서 `Debug Echo Mode` 체크.

📸 **L2_12.png** — Recorder Inspector 의 Debug Echo Mode 체크된 상태

### 8-3. Play 후 헤드폰으로 자기 목소리 확인

- 헤드폰 착용
- Play → 잠시 후 자기 목소리 말하기
- 자기 음성이 자기 헤드폰에 들리면 OK

> ⚠️ 스피커로 들으면 하울링(피드백 루프) 위험. 반드시 헤드폰.

📸 **L2_13.png** — Play 모드 + DebugEchoMode 체크된 Recorder Inspector (실시간 확인)

### 8-4. 검증 완료 후 OFF

실제 배포 시 자기 음성은 자기에게 안 들리는 게 자연스러움. 검증 끝나면 `Debug Echo Mode` 해제.

---

## Step 9 — 방법 B: Volume Meter UI (시각 검증)

> 화면 좌상단에 막대 그래프 — 말하는 동안 막대가 차오르며 마이크 음량 시각화. 청각 검증과 별도로 항상 살아 있는 디버그 도구.

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
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

public class MicVolumeMeter : MonoBehaviour
{
    [SerializeField] Recorder recorder;
    [SerializeField] Image fillBar;
    [SerializeField] float boost = 5f;

    void Update()
    {
        if (recorder == null || recorder.LevelMeter == null) { fillBar.fillAmount = 0f; return; }
        float level = recorder.LevelMeter.CurrentAvgAmp;
        fillBar.fillAmount = Mathf.Clamp01(level * boost);
    }
}
```

### 9-4. 컴포넌트 부착

`VolumeMeter` GameObject 에 `MicVolumeMeter` 추가, Inspector 에서:
- **Recorder** ← 씬의 PlayerCharacter 의 Recorder
- **Fill Bar** ← Fill Image

📸 **L2_15.png** — MicVolumeMeter Inspector 필드 입력 완료

### 9-5. 실행 + 말하기

Play → 말하면 막대가 차오르고, 입 다물면 0 으로 돌아옴.

📸 **L2_16.png** — 말하는 동안 막대가 차오른 화면 캡처

> 💡 막대가 0 에서 안 움직이면 마이크가 안 잡힌 것. Windows 마이크 권한 / 장치 연결 / Recorder Inspector 의 `Recording Enabled` 모두 확인.

---

## 정상 동작 체크리스트

- [ ] Stylized Astronaut 가 씬에 PlayerCharacter 이름으로 존재 (Humanoid rig)
- [ ] `PhotonAppSettings` 의 **App Id Voice** 입력됨 (App Id Quantum 과 분리)
- [ ] `VoiceManager + PhotonVoiceNetwork` 가 씬에 존재
- [ ] Play 시 Console 에 마이크 이름 출력
- [ ] **A. DebugEchoMode** 켜면 자기 음성이 자기 헤드폰에 들림
- [ ] **B. Volume Meter** 가 말할 때마다 막대로 시각 반응

다음 실습 **L3 — Push-to-Talk** 에서는 V 키 바인딩으로 음성 송신을 제어하고 두 명이 통신하도록 만든다.

---

## 🚀 응용 질문

### Q1. 마이크 장치가 여러 개일 때 드롭다운으로 선택하게 하려면?

힌트:
- `Microphone.devices` 배열을 TMP_Dropdown 에 바인딩
- `Recorder.MicrophoneDevice` 프로퍼티 동적 변경
- 변경 시 `Recorder.Reset()` 또는 재시작

### Q2. Volume Meter 가 너무 작은 소리도 잡아서 노이즈처럼 보일 때 어떻게 정제할까?

힌트:
- 임계값 (threshold) 아래는 0 으로 처리 (`level < 0.05 ? 0 : level`)
- 평균이 아닌 최댓값 (`LevelMeter.CurrentPeakAmp`) 사용
- 시간 평활화 (Smoothing) — `Mathf.Lerp` 로 점진 변화

### Q3. 다중 룸별로 Voice 채널을 분리하려면?

힌트:
- `PhotonVoiceNetwork.Client.OpJoinRoom()` 으로 Voice Room 분리
- 게임 룸 이름과 Voice 룸 이름을 같이 묶기
- 방 이동 시 Voice 룸도 같이 이동

---

> 응용 질문 중 **하나 이상** 구현한 결과는 최종 산출물에 포함.
