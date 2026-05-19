# 실습 L6 — 이모트 애니메이션 동기화

> 키 입력(1, 2, 3) 또는 UI 버튼으로 **춤·인사·박수** 같은 이모트 애니메이션을 트리거하고, 모든 클라이언트에서 동시에 실행되도록 동기화한다.
>
> ⏱️ 예상 시간: 60분 · 📸 슬롯: L6_01 ~ L6_12

---

## 학습 목표

1. Animator 의 **Trigger** 파라미터로 이모트 애니메이션을 실행한다
2. **RPC** 또는 **RaiseEvent** 로 이모트를 모든 클라이언트에 동기화한다
3. 이모트 ID 를 정수 1byte 로 전송하는 **네트워크 비용 절감** 패턴을 적용한다
4. UI 버튼 / 핫키 두 방식으로 이모트를 호출한다

## 사전 확인

- [ ] L5 완료 (채팅 동작)
- [ ] 캐릭터 프리팹의 `Animator` 컴포넌트가 동작 중 (걷기·달리기 애니메이션)
- [ ] 이모트용 애니메이션 클립 3개 (Wave, Dance, Clap) — Asset Store Free 또는 Mixamo 에서 받기

---

## Step 1 — 이모트 클립 임포트

### 1-1. 클립 다운로드

Mixamo (mixamo.com) 에서 무료 다운로드:
- `Wave Hip Hop Dance` 또는 `Dancing` (춤)
- `Waving` (인사)
- `Clapping` (박수)

다운로드 옵션: **FBX for Unity** · **In Place** 체크.

📸 **L6_01** — Mixamo 에서 클립 다운로드

### 1-2. Unity 임포트

`Assets/Interaction/Animations/Emotes/` 에 끌어다 놓기.
각 FBX 의 Rig 설정:
- **Animation Type**: Humanoid
- **Avatar Definition**: Copy From Other Avatar → PlayerCharacter 의 Avatar 선택

📸 **L6_02** — Mixamo 클립 임포트 설정

---

## Step 2 — Animator Controller 에 State 추가

### 2-1. PlayerCharacter 의 Animator Controller 열기

기존 `PlayerAnimator.controller` 더블클릭.

### 2-2. Emote State 3개 추가

`Any State` 에서 각 이모트 State 로 전이.
State 이름: `Emote_Wave`, `Emote_Dance`, `Emote_Clap`.

📸 **L6_03** — Animator Window 에 Emote State 3개 추가된 상태

### 2-3. Trigger 파라미터

좌측 Parameters 탭에서 추가:
- `Wave` (Trigger)
- `Dance` (Trigger)
- `Clap` (Trigger)

각 전이 조건: 해당 Trigger.

📸 **L6_04** — Trigger 파라미터 3개 추가

### 2-4. 이모트 종료 후 Idle 로 돌아오기

각 Emote State 에서 Exit 또는 Idle State 로 `Has Exit Time = true` 로 전이.
Exit Time: 0.95 정도 (애니메이션 거의 끝나면 자동 복귀).

📸 **L6_05** — Emote_Wave → Idle 전이의 Exit Time 설정

---

## Step 3 — EmoteController 스크립트

### 3-1. 이모트 ID 정의

`Assets/Interaction/Scripts/Emote/EmoteId.cs`:

```csharp
public enum EmoteId : byte
{
    None = 0,
    Wave = 1,
    Dance = 2,
    Clap = 3,
}
```

> byte (1바이트) 로 정의 — 네트워크 트래픽 최소화.

### 3-2. EmoteController.cs

`Assets/Interaction/Scripts/Emote/EmoteController.cs`:

```csharp
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class EmoteController : MonoBehaviour, IOnEventCallback
{
    const byte EMOTE_EVENT_CODE = 2;

    [SerializeField] Animator animator;
    [SerializeField] PhotonView photonView;  // 자기 캐릭터 식별용

    void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
    void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    void Update()
    {
        if (!photonView.IsMine) return;  // 자기 캐릭터만 입력 받음

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) Trigger(EmoteId.Wave);
        else if (kb.digit2Key.wasPressedThisFrame) Trigger(EmoteId.Dance);
        else if (kb.digit3Key.wasPressedThisFrame) Trigger(EmoteId.Clap);
    }

    public void Trigger(EmoteId id)
    {
        // 자기 자신 즉시 실행
        PlayLocal(id);

        // 다른 클라이언트에 전파
        var payload = new object[] { photonView.ViewID, (byte)id };
        PhotonNetwork.RaiseEvent(EMOTE_EVENT_CODE, payload,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable);
    }

    public void OnEvent(EventData e)
    {
        if (e.Code != EMOTE_EVENT_CODE) return;
        var data = (object[])e.CustomData;
        int viewId = (int)data[0];
        EmoteId id = (EmoteId)(byte)data[1];

        // 해당 ViewID 의 캐릭터 찾아 PlayLocal
        if (viewId == photonView.ViewID) PlayLocal(id);
    }

    void PlayLocal(EmoteId id)
    {
        switch (id)
        {
            case EmoteId.Wave:  animator.SetTrigger("Wave");  break;
            case EmoteId.Dance: animator.SetTrigger("Dance"); break;
            case EmoteId.Clap:  animator.SetTrigger("Clap");  break;
        }
    }
}
```

📸 **L6_06** — EmoteController.cs 코드 슬라이드

### 3-3. 프리팹에 컴포넌트 부착

`PlayerCharacter` 프리팹에 `EmoteController` 추가, Inspector 에서:
- **Animator** ← 자기 자신
- **Photon View** ← 자기 자신

📸 **L6_07** — EmoteController Inspector 필드 연결

---

## Step 4 — UI 버튼 (선택)

### 4-1. EmotePanel 만들기

`Canvas` 아래 `EmotePanel` (Panel) + 3개 버튼 (Wave / Dance / Clap).

📸 **L6_08** — EmotePanel UI

### 4-2. OnClick 으로 Trigger 호출

각 버튼의 OnClick → `EmoteController.Trigger(EmoteId.Wave)` (또는 Dance/Clap).
주의: 자기 EmoteController 만 참조해야 함 — 시작 시 로컬 PhotonView.IsMine 캐릭터 찾기.

📸 **L6_09** — 버튼 OnClick 이벤트 연결

---

## Step 5 — 2명 빌드 + 동기화 테스트

### 5-1. 1번 키 → 양쪽에서 같은 캐릭터가 손 흔듦

📸 **L6_10** — 빌드와 Editor 양쪽에서 같은 캐릭터가 Wave 애니메이션

### 5-2. 동시 이모트 (1·2 동시)

빠르게 1 → 2 → 3 누르면 마지막 트리거가 우선 (이전 이모트 중단). Has Exit Time 으로 처리.

📸 **L6_11** — 이모트 연속 입력 시 동작

---

## Step 6 — 시각 피드백

### 6-1. 이모트 실행 중 머리 위 아이콘

`PlayLocal` 호출 시 1초간 이모트 아이콘 표시 (`Coroutine` 으로 처리).

📸 **L6_12** — 이모트 아이콘 표시

---

## 정상 동작 체크리스트

- [ ] 1·2·3 키 → 자기 캐릭터가 Wave/Dance/Clap 실행
- [ ] 다른 클라이언트에서도 같은 캐릭터가 같은 애니메이션
- [ ] 이모트 종료 후 자동으로 Idle 로 복귀
- [ ] UI 버튼 클릭으로도 동일 동작
- [ ] PhotonView.IsMine 으로 자기/원격 캐릭터 구분

다음 실습 **L7 — 풀바디 포즈 (앉기·기대기·눕기)** 에서는 Animator Layer 와 Avatar Mask 로 더 정교한 캐릭터 표현을 만든다.

---

## 🚀 응용 질문

### Q1. 이모트마다 다른 효과음·이펙트를 추가하려면?

박수 칠 때 박수 소리, 춤출 때 음악 일부 재생 같은.

힌트:
- AudioClip 을 EmoteId 별로 배열로 관리
- ParticleSystem 으로 파티클 (꽃잎·하트) 트리거
- 이모트 실행 시 같이 PlayOneShot · Particle.Play

### Q2. 이모트 트리거 시 카메라 줌인·줌아웃을 자동으로 하려면?

박수 같은 짧은 이모트는 그대로, 춤 같은 긴 이모트는 카메라가 살짝 줌아웃해서 전체 모습 보이게.

힌트:
- Cinemachine FreeLook + Camera Distance 변경
- 이모트별 카메라 프로파일 (Field of View, Distance)
- 이모트 종료 시점에 원래 거리로 복원

### Q3. 이모트 종류를 사용자가 자기 슬롯에 매핑할 수 있게 하려면?

지금은 1=Wave 고정. 사용자가 "내 1번 키는 Dance, 2번은 Clap" 식으로 자유 매핑하려면?

힌트:
- 이모트 슬롯 (1~9) → EmoteId 매핑을 ScriptableObject 또는 PlayerPrefs 로 저장
- 옵션 UI 에서 슬롯별 드롭다운으로 이모트 선택
- 인 게임에서 슬롯 입력 → 매핑된 EmoteId 호출
