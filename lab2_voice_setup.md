# 실습 L2 — PlayerCharacter 가져오기 + Photon Voice 2 셋업

> L1 에서 만든 빈 프로젝트에 **기본 과정 챕터 05 의 PlayerCharacter** 를 가져오고, **Voice 전용 AppId** 를 발급해 연결한 뒤, **A 청각 검증 (DebugEcho) + B 시각 검증 (Volume Meter)** 으로 마이크가 제대로 작동하는지 확인한다.
>
> ⏱️ 예상 시간: 60분 · 📸 슬롯: L2_01 ~ L2_15
> 📁 산출물: PlayerCharacter 가 씬에 존재 + Voice 연결 + 마이크 검증 도구 동작

---

## 학습 목표

1. 기본 과정 챕터 05 의 **PlayerCharacter prefab** 을 새 프로젝트에 가져온다 (`.unitypackage` 방식)
2. Photon 대시보드에서 **Voice 전용 AppId** 를 발급하고 게임용 AppId 와 구분해 관리한다
3. **VoiceConnection · Recorder · Speaker** 의 역할을 구분해 설명할 수 있다
4. **DebugEchoMode** 로 자기 음성을 청각 검증한다 (방법 A)
5. **Volume Meter UI** 로 마이크 음량을 시각 검증한다 (방법 B)

## 사전 확인

- [ ] L1 체크리스트 6개 모두 통과 (Unity 6.3 LTS · URP 프로젝트 · 패키지 4종 · 베이스 씬)
- [ ] 기본 과정 챕터 05 결과물 프로젝트 보유 (또는 강사 제공 `base_project.zip`)
- [ ] Photon 계정 로그인 가능

---

## Step 1 — PlayerCharacter `.unitypackage` 익스포트

> 챕터 05 프로젝트가 그대로 있는 학생용. 만약 챕터 05 결과물이 없으면 Step 1-Alt 의 강사 제공 zip 사용.

### 1-1. 챕터 05 프로젝트 열기

Unity Hub 에서 챕터 05 프로젝트 (Quantum 3 충돌 처리까지 완료) 열기.

### 1-2. PlayerCharacter 관련 에셋 선택

Project 창에서 다음을 `Ctrl+클릭` 으로 모두 선택:
- `Assets/QuantumUser/View/PlayerCharacter.prefab`
- `Assets/QuantumUser/View/PlayerCharacter EntityPrototype.asset`
- `Assets/QuantumUser/Simulation/Movement/` 폴더 (MovementSystem, Input.qtn 등)

📸 **L2_01.png** — 챕터 05 Project 창에서 선택된 에셋들

### 1-3. Export Package

선택된 상태에서 우클릭 → `Export Package` → **Include Dependencies** 체크 → `Export...` → 바탕화면에 `PlayerCharacter_FromCh05.unitypackage` 저장.

📸 **L2_02.png** — Export Package 다이얼로그 (체크된 항목 + 파일명)

### 1-1-Alt. 강사 제공 zip 사용하는 경우

`base_project.zip` 풀면 `PlayerCharacter_Base.unitypackage` 파일이 있다. 이걸 사용.

---

## Step 2 — 새 프로젝트에 임포트

### 2-1. L1 의 새 프로젝트 열기

Unity Hub > L1 에서 만든 `Module_Interaction` 프로젝트 열기.

### 2-2. Package 임포트

`Assets > Import Package > Custom Package` → 위에서 만든 `.unitypackage` 선택.
다이얼로그에서 전체 체크 → `Import`.

📸 **L2_03.png** — Custom Package 임포트 다이얼로그

### 2-3. 임포트 결과 확인

Project 창에 PlayerCharacter prefab 이 보이는지 확인.

📸 **L2_04.png** — Project 창의 PlayerCharacter prefab

### 2-4. 씬에 배치

`Hierarchy` 의 `PlayerSpawnPoint` 위치에 PlayerCharacter prefab 끌어다 놓기.
Position 을 `(0, 1, 0)` 으로 (지면 약간 위) 설정.

📸 **L2_05.png** — 씬에 PlayerCharacter 가 배치된 상태

---

## Step 3 — Voice 전용 AppId 발급

> 게임용 (Quantum 3) AppId 와 **별도** 의 Voice AppId 가 필요. 같은 PhotonAppSettings 에 둘 다 따로 저장된다.

### 3-1. Photon 대시보드 접속

[dashboard.photonengine.com](https://dashboard.photonengine.com) 로그인.

### 3-2. 새 앱 만들기 — Voice 선택

`CREATE A NEW APP` 클릭 → **Photon Type: `Voice`** (⚠️ Realtime·Quantum 과 헷갈리지 말 것) → 이름: `Elice_심화_Voice` → `CREATE`.

📸 **L2_06.png** — Voice 선택된 생성 폼

### 3-3. AppId 복사

생성된 앱 카드의 AppId 해시 문자열 복사. 메모장에 잠깐 붙여 두면 다음 단계가 편함.

📸 **L2_07.png** — AppId 표시된 카드 (보안상 앞 8자리만 보이게 마스킹 권장)

---

## Step 4 — PhotonAppSettings 에 AppIdVoice 입력

### 4-1. PhotonAppSettings 찾기

Project 창 검색: `t:PhotonAppSettings`. 또는 `Assets/Photon/PhotonVoice/Resources/PhotonAppSettings.asset`.

### 4-2. AppIdVoice 필드에 붙여넣기

Inspector 의 **App Id Voice** 필드에 AppId 붙여넣기.

> ⚠️ App Id Quantum 과는 **다른 필드** 다. Quantum 은 챕터 04 에서 입력한 값이 있을 것 — 건드리지 말 것.

📸 **L2_08.png** — AppIdVoice 입력 완료된 PhotonAppSettings Inspector

### 4-3. Fixed Region (선택)

지연 줄이려면 `Fixed Region`: `asia` 또는 `kr`.

---

## Step 5 — VoiceManager + PhotonVoiceNetwork

### 5-1. 빈 GameObject 생성

씬에 `VoiceManager` 라는 빈 GameObject 생성.

### 5-2. PhotonVoiceNetwork 컴포넌트 추가

`VoiceManager` 에 `PhotonVoiceNetwork` 컴포넌트 추가.

📸 **L2_09.png** — VoiceManager + PhotonVoiceNetwork Inspector

### 5-3. Auto Connect 설정

`Connect on Start` 체크.

---

## Step 6 — 마이크 인식 테스트

### 6-1. 임시 검증 스크립트

`Assets/Interaction/Scripts/Voice/MicTester.cs`:

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

`VoiceManager` 에 추가 후 Play.

📸 **L2_10.png** — Console 에 마이크 이름 출력된 상태

---

## Step 7 — 방법 A: DebugEchoMode (청각 검증)

> 자기 마이크 입력이 자기 헤드폰으로 즉시 재생됨. "마이크가 인식되는가" 가장 빠른 청각 검증.

### 7-1. PlayerCharacter 의 Recorder 추가

PlayerCharacter prefab 더블클릭으로 편집 모드 → `Add Component > Photon Voice > Recorder`.

### 7-2. DebugEchoMode 활성화

Recorder Inspector 에서 `Debug Echo Mode` 체크.

📸 **L2_11.png** — Recorder Inspector 의 Debug Echo Mode 체크된 상태

### 7-3. Play 후 헤드폰으로 자기 목소리 확인

- 헤드폰 착용
- Play → 잠시 후 자기 목소리 말하기
- 자기 음성이 자기 헤드폰에 들리면 OK

> ⚠️ 스피커로 들으면 하울링(피드백 루프) 위험. 반드시 헤드폰.

📸 **L2_12.png** — Play 모드 + DebugEchoMode 체크된 Recorder Inspector (실시간 확인)

### 7-4. 검증 완료 후 OFF

실제 배포 시 자기 음성은 자기에게 안 들리는 게 자연스러움. 검증 끝나면 `Debug Echo Mode` 해제.

---

## Step 8 — 방법 B: Volume Meter UI (시각 검증)

> 화면 좌상단에 막대 그래프 — 말하는 동안 막대가 차오르며 마이크 음량 시각화. 청각 검증과 별도로 항상 살아 있는 디버그 도구.

### 8-1. UI 준비

`Canvas` 하위에 다음 구조 (없으면 `GameObject > UI > Canvas` 부터):

```
Canvas (HUD)
└─ VolumeMeter (Panel, 좌상단)
   ├─ Background (Image, 어두운 회색, 폭 200·높이 30)
   └─ Fill (Image, 초록색, 동일 위치)
```

📸 **L2_13.png** — VolumeMeter UI 의 Hierarchy + 좌상단 위치

### 8-2. Fill Image 설정

- **Image Type**: `Filled`
- **Fill Method**: `Horizontal`
- **Fill Origin**: `Left`
- **Fill Amount**: `0` (초기)

### 8-3. MicVolumeMeter 스크립트

`Assets/Interaction/Scripts/Voice/MicVolumeMeter.cs`:

```csharp
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

public class MicVolumeMeter : MonoBehaviour
{
    [SerializeField] Recorder recorder;
    [SerializeField] Image fillBar;
    [SerializeField] float boost = 5f;  // 시각 가독성 보정

    void Update()
    {
        if (recorder == null || recorder.LevelMeter == null) { fillBar.fillAmount = 0f; return; }
        float level = recorder.LevelMeter.CurrentAvgAmp;
        fillBar.fillAmount = Mathf.Clamp01(level * boost);
    }
}
```

### 8-4. 컴포넌트 부착

`VolumeMeter` GameObject 에 `MicVolumeMeter` 추가, Inspector 에서:
- **Recorder** ← 씬의 PlayerCharacter 의 Recorder (또는 자기 자신의 Recorder)
- **Fill Bar** ← Fill Image

📸 **L2_14.png** — MicVolumeMeter Inspector 필드 입력 완료

### 8-5. 실행 + 말하기

Play → 말하면 막대가 차오르고, 입 다물면 0 으로 돌아옴.

📸 **L2_15.png** — 말하는 동안 막대가 차오른 화면 캡처

> 💡 막대가 0 에서 안 움직이면 마이크가 안 잡힌 것. Windows 마이크 권한 / 장치 연결 / Recorder Inspector 의 `Recording Enabled` 모두 확인.

---

## 정상 동작 체크리스트

- [ ] PlayerCharacter 가 씬에 존재 + 이동 가능
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
