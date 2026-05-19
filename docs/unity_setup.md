# Unity 프로젝트 베이스 셋업

> 심화 모듈 "상호작용·소통·캐릭터 표현 동기화" 실습을 시작하기 전에, 모든 학생이 같은 출발점에 서도록 준비하는 문서.
>
> 실습 L1 ~ L10 은 이 베이스 위에서 진행한다.

---

## 0. 사전 준비물 체크리스트

| 항목 | 요구 사양 | 확인 |
|---|---|---|
| **Unity Editor** | **Unity 6.3 LTS** (`6000.3.x`, 2025-12 출시, 지원 2027-12 까지) | □ |
| **렌더 파이프라인** | **URP** 권장 (Unity 6 신규 프로젝트는 URP 기본, BIRP 는 legacy) | □ |
| .NET Scripting Backend | Mono (당분간), 6.7 부터 CoreCLR 데스크톱 실험판 — 이번 모듈은 Mono | □ |
| Photon Engine 계정 | photonengine.com 가입 + 로그인 | □ |
| 기본 과정 챕터 05 결과물 | Quantum 3 + PlayerCharacter 빌드 동작 상태 (Unity 6.3 환경) | □ |
| 마이크 (헤드셋·내장 모두 가능) | 음성 통신 테스트용 | □ |
| 디스크 여유 | 약 10 GB (Unity 6 + 캐시 + 빌드) | □ |
| Windows 환경 | 마이크 권한 ON (설정 > 개인 정보 > 마이크) | □ |

## 1. 베이스 프로젝트 — 두 갈래

### 옵션 A. 본인이 만든 챕터 05 프로젝트 이어가기 (권장)

기본 과정 챕터 05 "Quantum 충돌 처리하기" 까지 완료된 학생용. 자기 프로젝트에서 그대로 이어 받는다.

확인할 것:
- `Assets/QuantumUser/` 폴더 존재
- `PlayerCharacter` EntityPrototype 동작
- 두 명 빌드 후 같은 룸에 입장해 캐릭터가 보임
- `MovementSystem` 코드 정상 작동

### 옵션 B. 베이스 프로젝트 zip 받기

챕터 05 를 완전히 마치지 못한 학생용. 강사가 제공하는 `base_project.zip` 을 풀고 시작.

`base_project.zip` 안에 들어 있는 것:
- 빈 Unity 프로젝트 (Quantum 3 SDK 임포트됨)
- `PlayerCharacter` 프리팹 + EntityPrototype
- `MovementSystem`·`PlayerSpawnSystem`·`PlayerCollisionSystem` 스크립트
- 두 명 빌드 가능한 `QuantumSampleMenu` 셋업

학생은 압축 풀고 Unity 에서 `Open Project` → 폴더 선택 → 임포트 완료 후 위 옵션 A 와 동일 상태가 된다.

## 2. 필요한 추가 패키지

이 모듈에서 새로 추가하는 패키지 목록.

| 패키지 | 버전 (Unity 6.3 호환) | 용도 | 설치 방법 |
|---|---|---|---|
| **Photon Voice 2** | 2.55 이상 (Unity 6 지원판) | 음성 통신 SDK | Unity Asset Store → "Photon Voice 2" → My Assets → Import (Unity 6 호환 명시된 최신 버전 선택) |
| **Animation Rigging** | Unity 6.3 빌트인 (Package Manager 에서 1.3+) | Footstep IK · LookAt MultiAimConstraint | Package Manager → Unity Registry → "Animation Rigging" → Install |
| **TextMeshPro** | (Unity 6 기본 포함, 일부 UI Toolkit 으로 이전 중) | 채팅 UI · 닉네임 표시 | 첫 사용 시 `Window > TextMeshPro > Import TMP Essential Resources` |
| **Input System** | 1.8+ (Unity 6 기본) | 키 바인딩 (V 키 PTT, 이모트 핫키) | 이미 설치되어 있을 가능성 높음 — Project Settings 에서 Active Input Handling 확인 |
| Photon Chat (선택) | 2.3+ | 별도 채팅 채널 사용 시 | Asset Store (이번 모듈은 RPC 기반 채팅이라 미사용 가능) |

> **호환성 메모**: Photon Voice 2 의 Unity 6 정식 지원은 2.55 부터. Asset Store 에서 받기 전 패키지 페이지의 "Compatible Unity Versions" 에 `Unity 6` 또는 `6000.x` 가 포함되어 있는지 확인. 만약 옛 버전이라면 Photon 포럼의 최신 빌드 다운로드 링크를 사용.

**Photon Voice 2 AppId 발급**
1. https://dashboard.photonengine.com/ 접속
2. CREATE A NEW APP 클릭
3. Photon Type: **Voice** 선택
4. 이름 입력 (예: "Elice_Quantum_Voice") → CREATE
5. 생성된 앱의 AppId 복사 → Unity 의 `PhotonServerSettings` (Voice 용) 에 붙여넣기

> 게임 시뮬레이션용 Quantum 3 AppId 와 **별도** 의 Voice AppId 가 필요하다. 두 AppId 가 같은 영역(Region)을 쓰도록 설정.

## 3. 폴더 구조 권장

이 모듈에서 추가될 스크립트·프리팹·머티리얼이 챕터 05 결과물과 섞이지 않도록 정리.

```
Assets/
  QuantumUser/                  # 기본 과정 산출물 (그대로 둠)
    Simulation/
    View/
  Module06_Interaction/         # ★ 이 모듈에서 새로 만드는 것 다 여기로
    Scripts/
      Voice/                    # L1, L2, L3 음성 관련
      Chat/                     # L4 채팅
      Emote/                    # L5 이모트
      Pose/                     # L6 풀바디 포즈
      Movement/                 # L7 보간·Blend, L8 IK·시선
      Social/                   # L9 소셜 제스처
    Prefabs/
      VoiceConnection.prefab
      ChatPanel.prefab
      EmotePanel.prefab
    Materials/
    Animations/
      Emotes/                   # 이모트 클립
      Poses/                    # 앉기·기대기 클립
      Social/                   # 악수·하이파이브 클립
    UI/
```

`Module06_Interaction` 안에 모든 산출물이 모이게 하면, 학생이 자기 작업물을 강사에게 제출할 때 이 폴더만 zip 으로 묶으면 된다.

## 4. 첫 동작 확인 (Sanity Check)

본격 실습 시작 전 다음 넷이 동작해야 한다.

1. **Unity 6.3 LTS Editor 가 정상 실행** — 상단 메뉴에 `Help > About Unity` 에서 `6000.3.x` 확인
2. **씬 실행 → 캐릭터가 나타나는지** (챕터 05 검증)
3. **두 명 빌드 후 같은 룸 입장 → 서로 보이고 이동 보이는지**
4. **마이크 인식 테스트** — 빈 씬에 `Debug.Log(string.Join(", ", Microphone.devices))` 임시 스크립트로 마이크 이름이 나오면 OK

이 넷이 동작 안 하면 L1 시작 전에 환경 문제 해결 우선.

## 5. 강사 사전 점검

수업 시작 전 강사가 미리 해 둘 것:

- [ ] **모든 강의실 PC 에 Unity 6.3 LTS 설치** (Unity Hub 에서 6000.3.x 다운로드)
- [ ] Voice AppId 발급한 계정 정보를 학생들에게 어떻게 줄지 결정 (각자 만들기 vs 공용 AppId)
- [ ] `base_project.zip` 준비 (옵션 B 학생용, **Unity 6.3 로 한 번 열어 캐시 생성된 상태 권장**)
- [ ] 강의실 PC 마이크 권한 확인 (학교 보안 정책으로 막힌 경우 자주 있음)
- [ ] 강의실 네트워크가 Photon Cloud 와 통신 가능한지 점검 (방화벽이 5055~5058 UDP 포트 막는 경우 있음)
- [ ] 두 명 동시 접속 테스트 (강사 자신이 빌드 + Editor 동시 실행)
- [ ] **Photon Voice 2 패키지 호환성** — Asset Store 에서 받기 전 Unity 6 호환 표시 확인

## 6. 트러블 슈팅 — 자주 발생하는 문제

| 증상 | 원인 | 해결 |
|---|---|---|
| Voice 패키지 임포트 후 컴파일 에러 (Unity 6) | Voice 2 옛 버전이라 Unity 6 API 비호환 | Photon Voice 2.55+ (Unity 6 호환판) 사용 |
| **`PhotonRealtime/Code/` 코드 중복** + `'ExitGames' could not be found` + `'Realtime' does not exist` (수십~수백 개) | Asset Store 의 `Photon Voice 2` (ID 130518) 는 **Realtime 4 기반** — Quantum 3 (Realtime 5) 와 호환 안 됨. PUN2/Chat 도 같이 들어와 충돌 폭발 | **정답**: Asset Store 의 Voice 2 가 아니라 **Photon 공식 SDK 페이지** (`photonengine.com/sdks`) 의 **Photon Voice SDK Realtime5** 다운로드. 임포트 시 `PhotonRealtime/` + `Photon3Unity3D.*` 체크 해제. 자세한 절차는 L2 가이드 |
| Voice 패키지 임포트 후 TMP 관련 에러 | TextMeshPro Essential 미임포트 | `Window > TextMeshPro > Import TMP Essential Resources` |
| Animation Rigging Constraint 가 동작 안 함 | 캐릭터 GameObject 에 `Rig Builder` 누락 | Rig Builder 컴포넌트 추가 후 Rig 등록 |
| 마이크 인식 안 됨 | Windows 권한 차단 | `설정 > 개인 정보 > 마이크 > Unity 허용` |
| 빌드 후 다른 PC 와 연결 안 됨 | Voice AppId 빈 칸 또는 다른 Region | `PhotonServerSettings` 에서 AppId·Region 확인 |
| Quantum 3 와 Voice 동시 사용 시 AppId 혼동 | 두 AppId 가 같은 PhotonServerSettings 에 들어가 충돌 | Voice 용은 별도 설정 파일 (`PhotonAppSettings` Voice 인스턴스) 분리 |
| Editor 에서는 들리는데 빌드에서 안 들림 | 빌드 후 마이크 권한 재요청 누락 | `Application.RequestUserAuthorization(UserAuthorization.Microphone)` 호출 |
| URP 머티리얼이 핑크색으로 표시 | BIRP 머티리얼이 섞여 들어옴 | `Edit > Rendering > Materials > Convert All Built-in Materials to URP` |

## 7. 다음 단계

베이스 셋업이 끝났으면 **실습 L1 — Photon Voice 2 셋업** 으로 진행.

L1 에서는 위 2번 단계의 SDK 임포트와 AppId 발급을 학생이 **직접 따라하면서 스크린샷 캡처** 한다. 강사는 이 `unity_setup.md` 를 사전 안내 자료로 한 번 훑고, 실제 작업은 L1 PDF 따라 진행.
