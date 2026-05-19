# 실습 L10 — 소셜 제스처 (악수·하이파이브)

> 두 플레이어가 협동하는 **소셜 제스처** 를 구현한다. 한쪽이 트리거하면 상대방에게 RPC 신호를 보내고, **두 캐릭터가 자동으로 위치를 정렬한 뒤 동시에 애니메이션을 실행**한다.
>
> ⏱️ 예상 시간: 80분 · 📸 슬롯: __L10___TMP ~ __L10___TMP

---

## 학습 목표

1. 거리·각도 기준으로 **악수 가능한 상대** 를 자동 탐지한다
2. **RPC 핸드셰이크** (요청 → 수락 → 실행) 로 두 명 동기화 시작점을 맞춘다
3. 두 캐릭터의 **위치·회전을 서로 마주보도록 자동 정렬** 한다
4. 동시 트리거된 애니메이션이 양쪽 클라이언트에서 **같은 타이밍** 으로 보이게 한다
5. 거절·취소·취소 시 복원 흐름을 처리한다

## 사전 확인

- [ ] L9 완료 (IK + LookAt)
- [ ] 악수·하이파이브 애니메이션 클립 (Mixamo 등) 임포트

---

## Step 1 — Animator 에 제스처 State 추가

### 1-1. Handshake, HighFive State

Base Layer 또는 Pose Layer 에 추가.
- `Handshake` (Trigger: `DoHandshake`)
- `HighFive` (Trigger: `DoHighFive`)

📸 **__L10___TMP.png** — Animator 의 Handshake / HighFive State

### 1-2. 전이 조건

`Any State → Handshake` (Trigger: DoHandshake)
종료 후 Exit Time 0.95 로 Locomotion 복귀.

📸 **__L10___TMP.png** — 전이 조건 설정

---

## Step 2 — GestureController 스크립트

### 2-1. 코드 작성 (요청·수락 흐름)

`Assets/Interaction/Scripts/Social/GestureController.cs`:

```csharp
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class GestureController : MonoBehaviourPun
{
    [SerializeField] Animator animator;
    [SerializeField] float gestureRange = 2f;
    [SerializeField] float faceAngle = 60f;

    void Update()
    {
        if (!photonView.IsMine) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.eKey.wasPressedThisFrame) RequestGesture("Handshake");
        else if (kb.rKey.wasPressedThisFrame) RequestGesture("HighFive");
    }

    void RequestGesture(string type)
    {
        var partner = FindPartner();
        if (partner == null) return;

        // 상대에게 RPC 요청
        partner.photonView.RPC(nameof(OnGestureRequest), partner.photonView.Owner,
            type, photonView.ViewID);
    }

    GestureController FindPartner()
    {
        var all = FindObjectsByType<GestureController>(FindObjectsSortMode.None);
        foreach (var g in all)
        {
            if (g == this) continue;
            float d = Vector3.Distance(transform.position, g.transform.position);
            float a = Vector3.Angle(transform.forward, g.transform.position - transform.position);
            if (d < gestureRange && a < faceAngle * 0.5f) return g;
        }
        return null;
    }

    [PunRPC]
    void OnGestureRequest(string type, int requesterViewId)
    {
        // 간단 구현: 자동 수락. 실제는 UI 토스트 + 수락/거절 버튼
        var requester = PhotonView.Find(requesterViewId)?.GetComponent<GestureController>();
        if (requester == null) return;

        // 양쪽 정렬 후 동시 실행
        AlignAndPlay(requester, type);
    }

    void AlignAndPlay(GestureController requester, string type)
    {
        // 두 캐릭터 사이 중간 지점으로 약간씩 이동, 서로 마주보게
        Vector3 mid = (transform.position + requester.transform.position) * 0.5f;
        Vector3 toReq = (requester.transform.position - transform.position).normalized;
        transform.position = mid - toReq * 0.7f;        // 살짝 떨어진 위치
        transform.rotation = Quaternion.LookRotation(toReq);

        // 요청자 측도 같은 방식으로 RPC 호출
        photonView.RPC(nameof(PlayGesture), RpcTarget.All, type);
        requester.photonView.RPC(nameof(PlayGesture), RpcTarget.All, type);
    }

    [PunRPC]
    void PlayGesture(string type)
    {
        animator.SetTrigger("Do" + type);
    }
}
```

📸 **__L10___TMP.png** — GestureController.cs 코드 (FindPartner + Update)

📸 **__L10___TMP.png** — GestureController.cs 코드 (RPC 핸드셰이크 흐름)

### 2-2. 프리팹 부착

`PlayerCharacter` 프리팹에 `GestureController` 추가.

📸 **__L10___TMP.png** — GestureController Inspector

---

## Step 3 — 자동 위치 정렬 보간

### 3-1. 즉시 이동 → 0.3초 보간

`AlignAndPlay` 의 즉시 텔레포트 부분을 코루틴으로 보간 (L7 의 MoveToSeat 패턴 재사용).

```csharp
StartCoroutine(MoveTo(mid - toReq * 0.7f, Quaternion.LookRotation(toReq), 0.3f));
```

📸 **__L10___TMP.png** — MoveTo 코루틴 코드

### 3-2. 두 명 보간 + 애니메이션은 보간 끝나고 실행

`yield return new WaitForSeconds(0.3f); animator.SetTrigger(...)`.

---

## Step 4 — 거절·취소 UI

### 4-1. GestureRequestPopup

받은 쪽 화면에 "[OOO 님이 악수를 청합니다] [수락] [거절]" 토스트 UI.

```csharp
[PunRPC]
void OnGestureRequest(string type, int requesterViewId)
{
    GestureRequestPopup.Instance.Show(type, requesterViewId,
        onAccept: () => AlignAndPlay(...),
        onReject: () => SendReject(requesterViewId));
}
```

📸 **__L10___TMP.png** — GestureRequestPopup UI

### 4-2. 거절 응답

거절 시 요청자에게 알림. 5초 안에 응답 없으면 자동 만료.

📸 **__L10___TMP.png** — 거절 후 알림 토스트

---

## Step 5 — 2명 테스트

### 5-1. 가까이 마주보고 E 누르기

악수 자동 진행.

📸 **__L10___TMP.png** — 두 캐릭터 악수 중 (Editor + 빌드)

### 5-2. 멀리서·등 돌리고 E

조건 불만족 → 아무 일 안 일어남.

📸 **__L10___TMP.png** — 거리·각도 조건 불만족 시

### 5-3. R 키로 하이파이브

📸 **__L10___TMP.png** — 하이파이브 중

---

## Step 6 — 시각·청각 피드백

### 6-1. 악수 가능 인디케이터

상대 캐릭터가 악수 가능 범위에 들어오면 머리 위 [E] 아이콘 표시.

```csharp
void Update()
{
    var partner = FindPartner();
    handshakeIcon.SetActive(partner != null);
}
```

📸 **__L10___TMP.png** — 악수 가능 인디케이터

### 6-2. 효과음

악수 시작 시 손바닥 부딪히는 소리. AudioSource.PlayOneShot.

📸 **__L10___TMP.png** — 효과음 AudioClip 슬롯

---

## Step 7 — 자세 보정 (선택)

### 7-1. 두 캐릭터 키 차이로 손 위치 조정

캐릭터 키가 다르면 손이 어긋남. IK 로 보정:

```csharp
// 악수 중 IK target = 상대 캐릭터의 손 위치
rightHandIK.data.target = partner.rightHand;
rightHandIK.weight = 1f;
```

📸 **__L10___TMP.png** — Hand IK 가 적용된 악수 (손이 정확히 만남)

---

## 정상 동작 체크리스트

- [ ] E 키 → 가까이 마주보는 상대와 자동 악수
- [ ] R 키 → 자동 하이파이브
- [ ] 두 캐릭터가 0.3초 보간으로 자동 정렬
- [ ] 양쪽 클라이언트에서 같은 타이밍에 애니메이션
- [ ] 거절 시 요청자에게 알림
- [ ] 범위·각도 밖이면 트리거 안 됨

다음 실습 **L11 — UI 통합** 에서는 모든 요소(채팅·이모트·음성·시선·제스처) 를 통합 UI 로 묶는다.

---

## 🚀 응용 질문

### Q1. 포옹 (Hug) 처럼 두 명이 한 몸짓에 정확히 정렬해야 하는 제스처를 추가하려면?

포옹은 손 위치·머리 위치까지 정확해야 어색하지 않음.

힌트:
- 두 캐릭터에 Hug Snap Point (가슴 위치) 정의
- 양쪽 Snap Point 가 정확히 맞도록 위치 보정
- IK 로 손 위치 미세 조정

### Q2. 거리·각도 조건이 아니라 "서로 손가락질" 같은 동기화를 하려면?

서로 떨어져 있어도 가리키는 방향으로 손을 들 수 있게.

힌트:
- IK 로 손가락 방향 = 상대 위치
- 양쪽 클라이언트에서 같은 타깃 본 사용
- 거리 멀어도 가능하지만 너무 멀면 weight 자연스럽게 감소

### Q3. 그룹 제스처 (3명 이상이 같이 박수) 를 만들려면?

여러 명이 동시에 동기화된 동작.

힌트:
- 마스터 클라이언트가 "그룹 박수 시작" RPC 브로드캐스트
- 모든 참가자가 동시에 위치 정렬
- 시간 동기화: `PhotonNetwork.Time` 기준으로 정확히 같은 시점 시작
