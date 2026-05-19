# Ch17 — 상호작용·소통·캐릭터 표현 동기화 (심화 과정)

> 기본 과정(Ch01~16) 학습을 마친 학생이 이어서 진행하는 **메타버스 심화 모듈**.
> Quantum 3 멀티플레이어 캐릭터 위에 **음성 · 채팅 · 이모트 · 풀바디 포즈 · 이동 보간 · IK 시선 · 소셜 제스처**를 차례로 붙여 "사람이 만나면 자연스럽게 소통하는 공간" 을 완성한다.

---

## 🎯 학습 결과물

이 모듈을 끝낸 학생은 다음을 갖춘 멀티플레이 공간을 만들 수 있다.

- 🎙️ **음성 통신** — Push-to-Talk + 거리 기반 3D 음향 (Spatial Audio)
- 💬 **실시간 채팅** — RPC 기반, 입장·퇴장 알림, 닉네임·타임스탬프
- 😀 **이모트 + 풀바디 포즈** — 손 흔들기·춤·박수, 앉기·기대기·눕기 동기화
- 🏃 **자연스러운 움직임** — 보간(Lerp/Slerp) + 외삽 + Blend Tree (정지→걷기→달리기)
- 👣 **Footstep IK + LookAt** — 경사면 발 자동 정렬 + 가까운 플레이어 시선 추적
- 🤝 **소셜 제스처** — 두 명이 협동하는 악수·하이파이브 (위치 자동 정렬)
- 🖥️ **통합 HUD** — 채팅창 · 이모트 바 · 음성 인디케이터 · 닉네임 · 컨텍스트 힌트

---

## 🛠️ 학습 환경

| 항목 | 버전 |
|---|---|
| Unity Editor | **Unity 6.3 LTS** (`6000.3.x`) |
| 렌더 파이프라인 | **URP** (Universal Render Pipeline) |
| 멀티플레이 — 게임 시뮬레이션 | **Photon Quantum 3** |
| 멀티플레이 — 음성·채팅 | **Photon Voice 2** + Photon Realtime 5 |
| 애니메이션 보조 | **Animation Rigging** (Unity 6.3 빌트인) |
| 텍스트·UI | TextMeshPro · Input System |

> ⚠️ 게임 시뮬레이션용 AppId 와 **별도로** Voice 전용 AppId 가 필요합니다 — Photon 대시보드에서 두 종류 따로 발급 (L2 에서 상세 안내).

---

## 📚 모듈 구조

### 시놉시스 · 사전 준비

| 문서 | 내용 |
|---|---|
| [📖 시놉시스](./docs/synopsis.md) | 학습 목표·평가 기준·강사 메모·응용 질문 패턴 |
| [⚙️ Unity 사전 셋업](./docs/unity_setup.md) | 강사 사전 점검 체크리스트·트러블 슈팅 |

### 실습 가이드 (L1 ~ L11)

| # | 실습 | 핵심 학습 |
|---|---|---|
| L1 | [Unity 6.3 LTS 프로젝트 셋업](./docs/lab1_project_setup.md) | 새 프로젝트 (URP) · 패키지 임포트 · 베이스 씬 |
| L2 | [PlayerCharacter + Photon Voice 2 셋업](./docs/lab2_voice_setup.md) | 챕터 05 PlayerCharacter 이식 · Voice AppId · 마이크 검증 (A: DebugEcho · B: Volume Meter) |
| L3 | [Push-to-Talk 구현](./docs/lab3_push_to_talk.md) | Recorder · Speaker · V 키 바인딩 · 두 명 통신 |
| L4 | [Spatial Audio (3D 음향)](./docs/lab4_spatial_audio.md) | AudioSource 3D · 거리별 감쇠 |
| L5 | [RPC 기반 텍스트 채팅](./docs/lab5_chat.md) | RaiseEvent · ChatManager · UI 입력창 |
| L6 | [이모트 애니메이션 동기화](./docs/lab6_emote.md) | Animator Trigger · 키 입력 RPC · 1byte 이모트 ID |
| L7 | [풀바디 포즈 (앉기·기대기·눕기)](./docs/lab7_pose.md) | Animator Layer · Avatar Mask · 의자 자동 정렬 |
| L8 | [이동 보간 + Blend Tree](./docs/lab8_movement_blend.md) | Lerp/Slerp 보간 · 외삽 폴백 · 속도별 Blend |
| L9 | [Footstep IK + LookAt 시선](./docs/lab9_ik_lookat.md) | Animation Rigging · TwoBoneIK · MultiAimConstraint |
| L10 | [소셜 제스처 (악수·하이파이브)](./docs/lab10_social_gesture.md) | 두 캐릭터 위치 자동 정렬 · 동기 애니메이션 |
| L11 | [UI 통합](./docs/lab11_ui_integration.md) | HUD 일원화 · 채팅 토글 · 인디케이터 · 토스트 |

---

## ⏱️ 분량 · 학습 시간

- **총 분량**: 2주
- **수업자료(이론)**: 1회 · 50~58p
- **실습**: 11회 · 약 12시간 (강의실 진행 기준)
- **응용 질문 + 자율 확장**: 별도 (각 실습마다 🚀 도전 과제 3개씩)

---

## 🚀 응용 질문 — 학생 상상력 자극

각 실습 마지막에 **"🚀 응용 질문 (도전 과제)"** 1페이지가 있습니다. 정답이 정해지지 않은 탐색형 질문으로, 학생이 "여기서 이렇게 하면 어떻게 구현해요?" 형태로 자기 식으로 풀어보도록 유도합니다.

**예시**

- L4 Spatial Audio → *"벽 너머 음성을 자연스럽게 막히게 (오클루전) 하려면?"*
- L7 풀바디 포즈 → *"두 명이 같은 의자에 앉으려 하면 어떻게 처리할까?"*
- L8 이동 보간 → *"패킷 손실 환경에서도 안 끊기게 하려면?"*

산출물 평가에 **"응용 도전 1개 이상 구현 (10%)"** 포함.

---

## ✅ 평가 기준 (요약)

| 영역 | 배점 |
|---|---|
| 소통 (Voice · Spatial · 채팅) | 25% |
| 캐릭터 표현 (이모트 · 풀바디 포즈) | 15% |
| 자연스러운 움직임 (보간 · Blend · IK · 시선) | 25% |
| 소셜 제스처 | 10% |
| UI 통합 완성도 | 10% |
| 응용 도전 1개 이상 구현 | 10% |
| 코드 품질 · 주석 | 5% |

> 상세 평가 기준 → [synopsis.md § 평가 기준](./docs/synopsis.md#6-평가-기준)

---

## 📦 산출물 (학생 제출용)

- Unity 프로젝트 (zip 또는 GitHub repo)
- 빌드 파일 (Windows `.exe`)
- 시연 영상 60초 (음성 · 악수 · 시선 · 앉기 · 달리기 등 다양한 표현)
- 이동 보간 **Before/After** 비교 영상 10초
- 응용 도전 1개 이상 구현 결과물 + 짧은 설명 문서
- 자기 평가 1페이지

---

## 🏁 시작하기

1. **사전 준비** — Unity Hub 에서 6.3 LTS 설치, Photon 계정 보유, 챕터 05 결과물 준비
2. **시놉시스 한 번 훑기** → [synopsis.md](./docs/synopsis.md)
3. **L1 부터 차례대로** → [lab1_project_setup.md](./docs/lab1_project_setup.md)
4. 각 실습 끝에 **응용 질문 1개 이상** 자기 식으로 풀어 보기

> 💡 강사 안내: 모든 실습은 PDF 형태로도 제공됩니다. 마크다운 가이드는 작업 흐름·메모 기록용, PDF 는 학생 배포용입니다.

---

## 📝 라이선스

[MIT](./LICENSE)

---

## 🔗 관련

- **기본 과정 (Ch01~16)**: 사용자 메타버스 멀티플레이 강의 시리즈
- **다음 모듈 후보**: 영구 세계(DB 연동) · 대규모 동기화(AOI · LOD) · 매치메이킹 · 운영·배포
