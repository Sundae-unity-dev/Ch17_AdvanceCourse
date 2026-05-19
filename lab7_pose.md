# 실습 L7 — 풀바디 포즈 (앉기·기대기·눕기)

> 단발성 이모트 트리거를 넘어, **앉기·기대기·눕기** 같은 **지속 상태 포즈** 를 만든다. **Animator Layer + Avatar Mask** 로 상체·하체를 분리 동기화하고, 의자·벽 등 환경 오브젝트와 자동 정렬한다.
>
> ⏱️ 예상 시간: 80분 · 📸 슬롯: __L7___TMP ~ __L7___TMP

---

## 학습 목표

1. **Animator Layer** 를 추가해 기존 이동 애니메이션 위에 포즈를 덮어쓴다
2. **Avatar Mask** 로 상체만 / 하체만 적용되는 애니메이션을 만든다
3. **Bool 파라미터** 로 포즈 진입·종료를 동기화한다
4. 의자·벤치 같은 SeatPoint 트리거에 접근 → 캐릭터 위치·회전 자동 정렬
5. 두 명이 같은 의자에 앉으려 할 때 충돌 처리

## 사전 확인

- [ ] L6 완료 (이모트 트리거 동기화)
- [ ] 앉기·기대기·눕기 애니메이션 클립 3개 (Mixamo · Asset Store 등)

---

## Step 1 — Animator Layer 추가

### 1-1. PoseLayer 생성

Animator Window 좌측 Layers 탭 → `+` → 새 Layer `Pose Layer`.

- Weight: 1
- Blending: Override

📸 **__L7___TMP.png** — Animator 의 두 번째 Layer (Pose Layer)

### 1-2. PoseLayer 에 상태 추가

- `PoseIdle` (Empty State, 기본)
- `Sit`, `Lean`, `LieDown`

📸 **__L7___TMP.png** — Pose Layer 의 State 4개

### 1-3. Bool 파라미터

좌측 Parameters 탭에 추가:
- `IsSitting` (Bool)
- `IsLeaning` (Bool)
- `IsLyingDown` (Bool)

전이 조건: 각 Bool ↔ true ↔ false.

📸 **__L7___TMP.png** — Bool 파라미터 3개

---

## Step 2 — Avatar Mask

### 2-1. UpperBodyMask 만들기

`Create > Avatar Mask` → `UpperBodyMask.asset`.
Humanoid → 머리·팔만 체크, 다리는 해제.

📸 **__L7___TMP.png** — Avatar Mask 의 Humanoid 영역 (상체만 활성화)

### 2-2. PoseLayer 에 적용

Pose Layer 의 Mask 슬롯에 `UpperBodyMask` 끌어다 놓기.

> 이제 앉기 애니메이션은 상체만 적용되고, 하체는 베이스 레이어(걷기·정지) 그대로 → 의자에 앉아서도 발은 자연스럽게.

📸 **__L7___TMP.png** — Pose Layer Mask 슬롯에 UpperBodyMask 적용

---

## Step 3 — SeatPoint 컴포넌트

### 3-1. 의자 GameObject 만들기

씬에 `Chair` GameObject + 자식 `SeatPoint` (Empty, 앉을 위치).

📸 **__L7___TMP.png** — Chair 의 Hierarchy

### 3-2. SeatPoint.cs 스크립트

```csharp
using UnityEngine;

public class SeatPoint : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    PoseController currentSitter;

    public bool TrySit(PoseController sitter)
    {
        if (IsOccupied) return false;
        IsOccupied = true;
        currentSitter = sitter;
        return true;
    }

    public void Leave(PoseController sitter)
    {
        if (currentSitter == sitter)
        {
            IsOccupied = false;
            currentSitter = null;
        }
    }
}
```

📸 **__L7___TMP.png** — SeatPoint 스크립트 코드

### 3-3. Trigger Collider

`Chair` 에 BoxCollider, `Is Trigger` 체크.

---

## Step 4 — PoseController

### 4-1. PoseController.cs

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PoseController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] PhotonView photonView;
    [SerializeField] float seatTransitionTime = 0.3f;

    SeatPoint nearbySeat;
    bool isSitting;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SeatPoint seat) && !isSitting)
            nearbySeat = seat;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out SeatPoint seat) && nearbySeat == seat)
            nearbySeat = null;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (!Keyboard.current.fKey.wasPressedThisFrame) return;

        if (!isSitting && nearbySeat != null && nearbySeat.TrySit(this))
            StartSit(nearbySeat);
        else if (isSitting)
            StopSit();
    }

    void StartSit(SeatPoint seat)
    {
        isSitting = true;
        animator.SetBool("IsSitting", true);
        // 위치·회전 정렬 (간단히 즉시 - 보간은 응용 질문)
        transform.position = seat.transform.position;
        transform.rotation = seat.transform.rotation;
        photonView.RPC(nameof(SyncSitState), RpcTarget.Others, true);
    }

    void StopSit()
    {
        isSitting = false;
        animator.SetBool("IsSitting", false);
        if (nearbySeat != null) nearbySeat.Leave(this);
        photonView.RPC(nameof(SyncSitState), RpcTarget.Others, false);
    }

    [PunRPC]
    void SyncSitState(bool sitting)
    {
        animator.SetBool("IsSitting", sitting);
    }
}
```

📸 **__L7___TMP.png** — PoseController.cs 코드 (Update + StartSit 부분)

### 4-2. 프리팹 부착

`PlayerCharacter` 프리팹에 `PoseController` 추가.
캐릭터에 Trigger Collider (Capsule) 가 필요 — 보통 캐릭터 컨트롤러가 이미 가지고 있음.

📸 **__L7___TMP.png** — PoseController 부착된 Inspector

---

## Step 5 — Lean·LieDown 동일 패턴 적용

### 5-1. LeanPoint, LyingArea 같은 트리거 영역

같은 패턴으로 `LeanPoint` (벽 트리거), `LyingArea` (바닥 트리거) 추가.
PoseController 의 `nearbySeat` 같은 `nearbyLean`, `nearbyLyingArea` 추가.

📸 **__L7___TMP.png** — Lean 트리거 영역 (벽)

### 5-2. 키 바인딩

- F: 앉기/일어나기
- G: 기대기/멈춤
- H: 눕기/일어나기

---

## Step 6 — 2명 테스트

### 6-1. 두 캐릭터가 다른 의자에 앉기

📸 **__L7___TMP.png** — 두 캐릭터가 의자에 앉아 있는 상태 (Editor + 빌드)

### 6-2. 한 의자에 동시 시도

먼저 앉은 사람이 우선, 늦은 사람은 `TrySit` 가 false → 앉기 못함.

📸 **__L7___TMP.png** — 한 명만 앉아 있고 다른 한 명이 시도하는 상태

---

## Step 7 — 정렬 자연스럽게 (보간)

### 7-1. 즉시 텔레포트 대신 0.3초 보간

`StartSit` 의 `transform.position = ...` 부분을 코루틴으로 보간:

```csharp
System.Collections.IEnumerator MoveToSeat(Vector3 to, Quaternion rot)
{
    Vector3 from = transform.position;
    Quaternion fromRot = transform.rotation;
    float t = 0;
    while (t < 1)
    {
        t += Time.deltaTime / seatTransitionTime;
        transform.position = Vector3.Lerp(from, to, t);
        transform.rotation = Quaternion.Slerp(fromRot, rot, t);
        yield return null;
    }
}
```

📸 **__L7___TMP.png** — MoveToSeat 코루틴 코드

---

## Step 8 — 시각 디버그

### 8-1. SeatPoint Gizmo

```csharp
void OnDrawGizmos()
{
    Gizmos.color = IsOccupied ? Color.red : Color.green;
    Gizmos.DrawWireSphere(transform.position, 0.3f);
    Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
}
```

📸 **__L7___TMP.png** — 씬뷰의 SeatPoint Gizmo (점유 여부 색상)

---

## 정상 동작 체크리스트

- [ ] F 키 → 가까운 의자에 앉기, 다시 F → 일어나기
- [ ] 앉아 있는 동안 상체는 앉은 자세, 발은 자연스러움 (Avatar Mask)
- [ ] 두 명 동시 같은 의자 시도 → 먼저 누른 사람만 앉음
- [ ] 다른 클라이언트에서도 앉는 모습 보임 (RPC 동기화)
- [ ] 의자 이동 시 위치·회전 0.3초 보간

다음 실습 **L8 — 이동 보간 + Blend Tree** 에서는 원격 캐릭터 움직임을 더 부드럽게 만든다.

---

## 🚀 응용 질문

### Q1. 의자 종류별로 다른 앉기 포즈를 자동 매칭하려면?

식탁 의자·소파·바닥쿠션 등 의자가 다양하면, 각각 다른 애니메이션을 자동 선택.

힌트:
- `SeatPoint` 에 enum `SeatType` 추가
- Animator 에 SeatType 파라미터 추가
- Sit_Normal · Sit_Sofa · Sit_Floor 같은 sub-state 분기

### Q2. 두 명이 같은 의자에 앉으려 할 때 "양보 / 거절 / 같이 끼어 앉기" 같은 상호작용 흐름을 만들려면?

힌트:
- 첫 시도자에게 RPC 로 "앉을래?" 알림
- UI 토스트로 응답 받기 (수락/거절)
- 같이 앉는 경우 SeatPoint 의 capacity = 2 + 위치 살짝 옆으로 분산

### Q3. 누워있는 동안 카메라가 위에서 비추는 탑다운 뷰로 자동 전환하려면?

힌트:
- Pose 상태 enum → 카메라 프로파일 매핑
- Cinemachine VirtualCamera Priority 동적 변경
- 일어날 때 원래 카메라로 복원
