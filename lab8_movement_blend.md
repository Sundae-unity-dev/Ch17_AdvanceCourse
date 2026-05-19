# 실습 L8 — 부드러운 이동 보간 + Blend Tree

> 네트워크 캐릭터가 끊겨 보이는 문제를 **보간(Lerp/Slerp)** 과 **외삽(Extrapolation)** 으로 해결하고, 속도에 따라 **정지 → 걷기 → 달리기** 가 자연스럽게 전환되는 **Blend Tree** 를 적용한다.
>
> ⏱️ 예상 시간: 90분 · 📸 슬롯: __L8___TMP ~ __L8___TMP (코드 슬라이드 포함)

---

## 학습 목표

1. **왜 네트워크 캐릭터가 끊겨 보이는지** 패킷 주기와 프레임 주기 차이로 설명한다
2. **NetworkTransform** 의 원격 캐릭터 위치 수신을 **Lerp/Slerp** 로 보간한다
3. 패킷이 늦거나 끊겼을 때 **외삽 (Extrapolation)** 으로 폴백한다
4. **Blend Tree** (1D, Speed 파라미터) 로 Idle ↔ Walk ↔ Run 자연 전환
5. **Before/After 비교** 영상을 캡처해 차이를 시각적으로 증명한다

## 사전 확인

- [ ] L7 완료 (포즈 동기화)
- [ ] 원격 캐릭터가 이동하는 모습이 현재 끊겨 보임 (이게 출발점)

---

## Step 1 — 현재 상태 캡처 (Before)

### 1-1. 두 명 접속 후 한쪽이 빠르게 움직임

상대방 화면에서 캐릭터가 **딱딱 끊겨** 보이는지 확인.

📸 **__L8___TMP.png** — Before: 끊겨 보이는 원격 캐릭터 (연속 프레임 캡처 또는 영상 캡처)

### 1-2. 영상 캡처 시작

Unity Recorder 또는 OBS 로 10초 정도 캡처. 나중에 After 영상과 비교.

---

## Step 2 — NetworkTransform Receiver 분석

### 2-1. 현재 동기화 방식

Photon 기본 `PhotonTransformView` 는 위치 데이터를 받아 즉시 `transform.position = received`.
→ 패킷 간격 (예: 100ms) 동안은 정지, 패킷 도착 순간 점프 → 끊겨 보임.

📸 **__L8___TMP.png** — PhotonTransformView 의 기본 Inspector

### 2-2. 직접 보간 컴포넌트로 교체

`PhotonTransformView` 비활성화 또는 제거하고, 자작 `SmoothTransformSync` 사용.

---

## Step 3 — SmoothTransformSync 스크립트

### 3-1. 코드 작성

`Assets/Interaction/Scripts/Movement/SmoothTransformSync.cs`:

```csharp
using Photon.Pun;
using UnityEngine;

public class SmoothTransformSync : MonoBehaviourPun, IPunObservable
{
    [SerializeField] float lerpRate = 15f;        // 보간 속도
    [SerializeField] float extrapolateLimit = 0.3f; // 외삽 허용 시간(s)

    Vector3 targetPos;
    Quaternion targetRot;
    Vector3 velocity;
    double lastReceivedTime;

    void Awake()
    {
        targetPos = transform.position;
        targetRot = transform.rotation;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(velocity);
        }
        else
        {
            targetPos = (Vector3)stream.ReceiveNext();
            targetRot = (Quaternion)stream.ReceiveNext();
            velocity  = (Vector3)stream.ReceiveNext();
            lastReceivedTime = PhotonNetwork.Time;
        }
    }

    void Update()
    {
        if (photonView.IsMine) return;

        double elapsed = PhotonNetwork.Time - lastReceivedTime;
        Vector3 predicted = targetPos;

        // 외삽: 패킷이 늦어지면 마지막 속도로 미래 위치 예측
        if (elapsed < extrapolateLimit)
            predicted = targetPos + velocity * (float)elapsed;

        // 보간: 부드럽게 따라가기
        transform.position = Vector3.Lerp(transform.position, predicted, Time.deltaTime * lerpRate);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lerpRate);
    }
}
```

📸 **__L8___TMP.png** — SmoothTransformSync.cs 코드 슬라이드 (OnPhotonSerializeView)

📸 **__L8___TMP.png** — SmoothTransformSync.cs 코드 슬라이드 (Update + 외삽 로직)

### 3-2. 캐릭터 자기 자신은 velocity 직접 전송

자기 캐릭터의 CharacterController.velocity 를 매 프레임 캐싱해서 위 `velocity` 에 넣어 SendNext.

```csharp
void LateUpdate()
{
    if (!photonView.IsMine) return;
    velocity = (transform.position - prevPos) / Time.deltaTime;
    prevPos = transform.position;
}
```

📸 **__L8___TMP.png** — velocity 캐싱 코드

### 3-3. 프리팹 부착

`PlayerCharacter` 에 `SmoothTransformSync` 추가, `PhotonView` 의 Observed Components 리스트에 등록.

📸 **__L8___TMP.png** — PhotonView Observed Components 에 SmoothTransformSync

---

## Step 4 — After 캡처

### 4-1. 두 명 다시 빌드 + 접속

상대방이 빠르게 움직여도 부드럽게 보임.

📸 **__L8___TMP.png** — After: 부드러운 원격 캐릭터 이동

### 4-2. 영상 비교

Before · After 영상을 좌우 분할 화면으로 합성. 학생 제출 산출물.

📸 **__L8___TMP.png** — Before / After 비교 합성 화면

---

## Step 5 — Blend Tree 추가

### 5-1. Animator Controller 에 Blend Tree 노드

Base Layer 의 `Locomotion` 이라는 새 State → Motion 슬롯에 `Blend Tree` 더블클릭.

- Blend Type: `1D`
- Parameter: `Speed` (Float, Parameters 탭에서 추가)

### 5-2. Motion 3개 추가

- Idle (Threshold 0)
- Walk (Threshold 2)
- Run (Threshold 5)

📸 **__L8___TMP.png** — Blend Tree 1D, Idle/Walk/Run 3개 Motion

### 5-3. Threshold 자동 계산

`Compute Thresholds > Speed` 클릭.

---

## Step 6 — MovementSystem 에 Speed 전달

### 6-1. 현재 캐릭터 속도 → Animator Speed

`MovementSystem` 또는 `PlayerMove` 에 추가:

```csharp
void UpdateAnimator()
{
    float speed = new Vector2(velocity.x, velocity.z).magnitude;
    animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime); // 보간 0.1초
}
```

`SetFloat` 의 dampTime (0.1) → Idle ↔ Walk ↔ Run 전환이 즉시가 아니라 0.1초 보간.

📸 **__L8___TMP.png** — UpdateAnimator 코드

### 6-2. 원격 캐릭터의 Speed 도 동기화

OnPhotonSerializeView 에 speed 추가 전송:

```csharp
if (stream.IsWriting) stream.SendNext(currentSpeed);
else animator.SetFloat("Speed", (float)stream.ReceiveNext(), 0.1f, Time.deltaTime);
```

📸 **__L8___TMP.png** — Speed 동기화 코드

---

## Step 7 — Blend Tree 테스트

### 7-1. 정지 → 천천히 움직임 → 빠르게 이동

매끄럽게 Idle → Walk → Run 으로 전환되는지 시각 확인.

📸 **__L8___TMP.png** — 캐릭터가 Walk 애니메이션 중 (Speed 약 2)

📸 **__L8___TMP.png** — 캐릭터가 Run 애니메이션 중 (Speed 약 5)

### 7-2. 원격 캐릭터에서도 같은 전환

빌드와 Editor 양쪽에서 같은 Speed 로 같은 Blend.

📸 **__L8___TMP.png** — 양쪽 화면 동시 캡처

---

## Step 8 — 트레이드오프 비교 표 정리

### 8-1. 강의용 비교 표

| 방식 | 부드러움 | 지연 | 패킷 끊김 견딤 | 오차 위험 |
|---|---|---|---|---|
| 즉시 적용 (기본) | ❌ 끊김 | 0 | ❌ | ❌ 없음 |
| Lerp 보간만 | ✅ 부드러움 | 1~2 tick | ❌ | ❌ |
| 외삽만 | ✅ | 0 | ✅ | ⚠️ 큼 |
| **Lerp + 외삽 폴백** | ✅✅ | 매우 작음 | ✅ | 작음 |

📸 **__L8___TMP.png** — 비교 표 다이어그램

### 8-2. Network Settings 의 SendRate 영향

`PhotonNetwork.SendRate` 를 20Hz → 30Hz 로 늘리면 보간 부담 줄음. 반대로 10Hz 면 외삽 의존도 커짐.

📸 **__L8___TMP.png** — SendRate 변경 후 차이

---

## 정상 동작 체크리스트

- [ ] 원격 캐릭터가 부드럽게 이동 (Before 영상과 명확히 차이)
- [ ] 패킷 끊김 (네트워크 일시 단절) 시에도 짧은 시간 외삽으로 부드러움 유지
- [ ] Idle ↔ Walk ↔ Run 자동 전환 (Speed 파라미터)
- [ ] 양쪽 클라이언트에서 같은 Blend 결과
- [ ] Before/After 비교 영상 캡처 완료 (산출물)

다음 실습 **L8 — Footstep IK + LookAt** 에서는 발이 지면에 자연스럽게 닿고, 캐릭터가 다른 플레이어를 자연스럽게 바라보도록 만든다.

---

## 🚀 응용 질문

### Q1. 패킷 손실(Packet Loss) 환경에서도 안 끊기게 하려면?

극심한 패킷 손실 (20%+) 환경에서 보간만으로 부족. 어떻게 개선?

힌트:
- 외삽 시간을 더 길게 (`extrapolateLimit` 0.5s 등)
- Kalman Filter 또는 Spline 보간 (3차 곡선)
- 손실률을 측정해서 동적으로 보간/외삽 비율 조정

### Q2. 캐릭터마다 다른 걷기 스타일을 동기화하려면?

무거운 캐릭터·가벼운 캐릭터·바쁜 캐릭터 등 같은 입력에 다른 애니메이션.

힌트:
- 캐릭터 데이터에 `walkStyle: enum` 추가
- AnimatorOverrideController 로 Blend Tree 의 Clip 만 교체
- 캐릭터 스폰 시 Style 정보도 함께 동기화

### Q3. 매우 빠른 캐릭터 (순간이동·점프) 가 있을 때 보간이 잘 동작하게 하려면?

순간이동의 경우 보간하면 캐릭터가 "끌려가는" 것처럼 보임.

힌트:
- "텔레포트 플래그" 전송 → 받으면 보간 스킵하고 즉시 적용
- 위치 차이가 임계값 초과하면 자동으로 텔레포트로 간주
