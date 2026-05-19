# 실습 L9 — Footstep IK + LookAt 시선 처리

> 캐릭터가 계단·경사면에서 발이 미끄러지지 않도록 **Footstep IK**, 다른 플레이어를 자연스럽게 바라보도록 **LookAt MultiAimConstraint** 를 적용한다. **Animation Rigging** 패키지를 사용.
>
> ⏱️ 예상 시간: 80분 · 📸 슬롯: __L9___TMP ~ __L9___TMP (코드 슬라이드 포함)

---

## 학습 목표

1. **Animation Rigging** 패키지의 `RigBuilder` · `Rig` 구조를 이해한다
2. **TwoBoneIKConstraint** 로 양 발에 IK 를 적용한다
3. `Physics.Raycast` 로 발 아래 지면 위치를 찾아 IK Target 동적 이동
4. **MultiAimConstraint** 로 머리·상체가 가까운 플레이어를 바라보게 한다
5. 시선·IK 가중치를 거리·각도에 따라 자연스럽게 페이드

## 사전 확인

- [ ] L8 완료 (이동 보간 + Blend Tree)
- [ ] Animation Rigging 패키지 설치 (`unity_setup.md` Step 2 참고)

---

## Step 1 — Animation Rigging 셋업

### 1-1. RigBuilder 추가

`PlayerCharacter` 프리팹에 `Rig Builder` 컴포넌트 추가.

📸 **__L9___TMP.png** — RigBuilder 컴포넌트 추가

### 1-2. Rig GameObject

`PlayerCharacter` 자식으로 `Rig` Empty 생성, `Rig` 컴포넌트 추가.

📸 **__L9___TMP.png** — Rig GameObject + Rig 컴포넌트

### 1-3. RigBuilder 의 Layers 에 Rig 등록

`Rig Builder` 의 Layers → `+` → 위에서 만든 Rig 끌어다 놓기.

📸 **__L9___TMP.png** — RigBuilder Layers 에 Rig 등록

---

## Step 2 — Footstep IK (양 발)

### 2-1. LeftFootIK, RightFootIK 자식 만들기

`Rig` 아래에 `LeftFootIK`, `RightFootIK` 2개 GameObject.
각각 `Two Bone IK Constraint` 컴포넌트.

📸 **__L9___TMP.png** — LeftFootIK GameObject + TwoBoneIK 컴포넌트

### 2-2. TwoBoneIK 설정

LeftFootIK:
- **Root**: LeftUpperLeg
- **Mid**: LeftLowerLeg
- **Tip**: LeftFoot
- **Source Object**: LeftFootIK_Target (자식으로 빈 GameObject 만들어 등록)
- **Hint**: LeftKnee_Hint (무릎이 휘는 방향)

오른쪽도 동일.

📸 **__L9___TMP.png** — Two Bone IK Inspector (Root/Mid/Tip 채워짐)

---

## Step 3 — FootIKController 스크립트

### 3-1. 코드 작성

`Assets/Interaction/Scripts/Movement/FootIKController.cs`:

```csharp
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FootIKController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Transform leftFoot;
    [SerializeField] Transform rightFoot;
    [SerializeField] Transform leftFootTarget;
    [SerializeField] Transform rightFootTarget;
    [SerializeField] TwoBoneIKConstraint leftIK;
    [SerializeField] TwoBoneIKConstraint rightIK;
    [SerializeField] float footHeight = 0.1f;
    [SerializeField] LayerMask groundLayer = ~0;

    void LateUpdate()
    {
        UpdateFootTarget(leftFoot, leftFootTarget, leftIK);
        UpdateFootTarget(rightFoot, rightFootTarget, rightIK);
    }

    void UpdateFootTarget(Transform foot, Transform target, TwoBoneIKConstraint ik)
    {
        Vector3 origin = foot.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, Vector3.down, out var hit, 1.5f, groundLayer))
        {
            target.position = hit.point + Vector3.up * footHeight;
            target.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * foot.rotation;
            ik.weight = 1f;
        }
        else
        {
            ik.weight = 0f;
        }
    }
}
```

📸 **__L9___TMP.png** — FootIKController.cs 코드 (UpdateFootTarget)

### 3-2. 프리팹 부착

PlayerCharacter 에 `FootIKController` 추가, Inspector 에 모든 슬롯 연결.

📸 **__L9___TMP.png** — FootIKController Inspector 필드 입력

### 3-3. 경사면 테스트

씬에 경사진 평면 만들기 (Plane 회전 15도) → 캐릭터가 경사 위에서 발이 평면에 붙음.

📸 **__L9___TMP.png** — 경사면 위 캐릭터, 발이 자연스럽게 평면에 닿음

---

## Step 4 — LookAt MultiAimConstraint

### 4-1. HeadAim GameObject

`Rig` 아래 `HeadAim` 자식 + `Multi Aim Constraint` 컴포넌트.

📸 **__L9___TMP.png** — HeadAim + MultiAimConstraint 컴포넌트

### 4-2. MultiAimConstraint 설정

- **Constrained Object**: Head (머리 본)
- **Aim Axis**: Z
- **Up Axis**: Y
- **World Up Type**: SceneUp
- **Source Objects**: (코드로 동적 추가)

📸 **__L9___TMP.png** — MultiAimConstraint Inspector

---

## Step 5 — LookAtController 스크립트

### 5-1. 가장 가까운 다른 플레이어 찾기 + 시선 타깃

```csharp
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LookAtController : MonoBehaviour
{
    [SerializeField] Transform headAimTarget;
    [SerializeField] MultiAimConstraint headAim;
    [SerializeField] float lookRange = 5f;
    [SerializeField] float lookAngle = 90f;
    [SerializeField] float weightLerp = 5f;

    void Update()
    {
        var target = FindNearestPlayer();
        float desiredWeight = 0f;

        if (target != null)
        {
            Vector3 toTarget = target.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toTarget);
            if (angle < lookAngle * 0.5f)
            {
                headAimTarget.position = target.position + Vector3.up * 1.6f; // 머리 높이
                desiredWeight = 1f;
            }
        }

        headAim.weight = Mathf.Lerp(headAim.weight, desiredWeight, Time.deltaTime * weightLerp);
    }

    Transform FindNearestPlayer()
    {
        var all = GameObject.FindGameObjectsWithTag("Player");
        Transform nearest = null;
        float best = lookRange;
        foreach (var p in all)
        {
            if (p.transform == transform) continue;
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < best) { best = d; nearest = p.transform; }
        }
        return nearest;
    }
}
```

📸 **__L9___TMP.png** — LookAtController.cs 코드 (FindNearestPlayer + Update)

### 5-2. 프리팹 부착·테스트

부착 후 두 명 가까이 다가가면 머리만 자연스럽게 상대를 향함.

📸 **__L9___TMP.png** — 가까운 플레이어 바라보는 캐릭터

---

## Step 6 — 거리·각도 페이드

### 6-1. 거리 멀어지면 시선 weight 가 자연스럽게 0 으로

이미 코드의 `Mathf.Lerp(headAim.weight, desiredWeight, ...)` 가 처리.

### 6-2. 시야각 (lookAngle) 밖이면 안 바라봄

뒤쪽 사람은 안 돌아봄 — 사람다운 자연스러움.

📸 **__L9___TMP.png** — 뒤쪽 플레이어가 있어도 시선 weight = 0 (Inspector 확인)

---

## Step 7 — 디버그 시각화

### 7-1. 시선 Gizmo

```csharp
void OnDrawGizmos()
{
    Gizmos.color = Color.cyan;
    Gizmos.DrawLine(transform.position + Vector3.up * 1.6f, headAimTarget.position);
    Gizmos.DrawWireSphere(headAimTarget.position, 0.1f);
}
```

📸 **__L9___TMP.png** — Scene 뷰에서 시선 Gizmo 표시

---

## 정상 동작 체크리스트

- [ ] 평지·경사면·계단에서 발이 자연스럽게 지면에 닿음
- [ ] 가까운 플레이어 자동으로 머리·상체로 바라봄
- [ ] 거리 멀어지면 시선 weight 자연스럽게 0
- [ ] 시야각 밖이면 안 돌아봄
- [ ] 양쪽 클라이언트에서 같은 시선 동작 (Animator state 자동 동기화)

다음 실습 **L10 — 소셜 제스처 (악수·하이파이브)** 에서는 두 명이 협동하는 애니메이션을 만든다.

---

## 🚀 응용 질문

### Q1. 계단을 올라갈 때 발이 한 칸씩 정확히 디디게 하려면?

지금은 Raycast 가 발 바로 아래 한 점만 봄. 계단 모서리에서 어색.

힌트:
- Multi-point Raycast (발 앞·중간·뒤 3점)
- SphereCast 로 더 넓은 영역 검출
- 발 방향까지 step 모서리에 정렬

### Q2. 여러 명 동시에 바라보면 어떻게 처리할까?

3명이 같은 거리에 있을 때 머리가 중간 지점을 보거나, 가장 큰 음량으로 말하는 사람을 보거나.

힌트:
- 가장 가까운 1명 (현재) vs 평균 위치 vs 최대 음량 사용자
- Recorder.IsCurrentlyTransmitting 으로 말하는 사람 우선순위
- 짧은 시간마다 타깃 변경 (너무 자주 변경하면 어지러움)

### Q3. 시선이 어색하게 머리만 돌아가는 게 아니라 상체·발끝까지 자연스럽게 따라오려면?

힌트:
- MultiAimConstraint 를 머리·목·상체 3개 본에 가중치 분배
- 큰 각도면 발끝도 따라 돌도록 캐릭터 회전 자체를 부드럽게 보정
- IK 의 weight 를 본 별로 다르게 (머리 100% · 목 60% · 상체 30%)
