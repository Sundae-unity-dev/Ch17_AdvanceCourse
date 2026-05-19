# 실습 L1 — Unity 6.3 LTS 프로젝트 셋업

> 심화 모듈 전체의 출발점. **Unity 6.3 LTS** 환경에서 **새 프로젝트 생성**, **필요한 패키지 임포트** (Photon Voice 2 · Quantum 3 · Animation Rigging), **베이스 씬 구성** 까지 한 번에 마친다.
>
> ⏱️ 예상 시간: 50분 · 📸 슬롯: L1_01 ~ L1_12
> 📁 산출물: Unity 6.3 LTS · URP 프로젝트 + 패키지 임포트 완료 + 베이스 씬(`MainScene.unity`) 실행 가능 상태

---

## 학습 목표

1. **Unity Hub** 에서 Unity 6.3 LTS 설치를 확인하고, 새 프로젝트를 **URP 템플릿**으로 생성한다
2. 심화 모듈 진행에 필요한 패키지 4종을 임포트한다 — Photon Voice 2 · Quantum 3 · Animation Rigging · (Input System / TMP 자동 포함)
3. 빈 씬에 바닥·조명·카메라·SpawnPoint 를 배치해 캐릭터가 등장할 준비를 한다
4. 씬을 실행해 빈 상태에서 정상 동작함을 검증한다

## 사전 확인

- [ ] Unity Hub 가 설치되어 있다
- [ ] Photon Engine 계정 보유
- [ ] 디스크 여유 약 10 GB

---

## Step 1 — Unity 6.3 LTS 설치 확인

### 1-1. Unity Hub 의 Installs 탭 열기

Unity Hub 실행 → 좌측 메뉴 `Installs`.
`6000.3.x` (Unity 6.3 LTS) 가 보이면 OK.

📸 **L1_01.png** — Unity Hub > Installs 탭, `6000.3.x` 항목

### 1-2. 만약 6.3 LTS 미설치라면

`Install Editor` 클릭 → Official releases 에서 **Unity 6.3 LTS** 선택 → Modules: `Windows Build Support (IL2CPP)` 체크 → Install.
(설치는 약 10~20분 소요)

---

## Step 2 — 새 프로젝트 생성 (URP)

### 2-1. New Project 다이얼로그 열기

Unity Hub > `Projects` 탭 > **`New project`** 클릭.

📸 **L1_02.png** — Unity Hub > Projects > New project 다이얼로그 (템플릿 목록)

### 2-2. URP 템플릿 선택

좌측 **Editor Version**: `6000.3.x` 확인.
중앙 템플릿 목록에서 **Universal 3D** (URP) 선택.

> ⚠️ 3D (BIRP) 가 아닌 **Universal 3D** 골라야 한다. 메타버스 컨셉에 적합한 모던 렌더 파이프라인.

### 2-3. 프로젝트 이름·경로

- Project name: `Module_Interaction` (또는 원하는 이름)
- Location: `C:\Users\<사용자명>\UnityProjects\` 같은 짧고 한글·공백 없는 경로 권장
- Connect to Unity Cloud: 체크 해제 (선택)

`Create project` 클릭.

📸 **L1_03.png** — Universal 3D 선택 + 이름·경로 입력된 상태

### 2-4. 프로젝트 첫 로딩 대기

처음 로딩은 2~5분 소요. 패키지 다운로드 + Library 캐시 생성.

📸 **L1_04.png** — 새 프로젝트가 열린 직후 Editor 첫 화면

> 💡 검증: `Help > About Unity` 에서 버전이 `6000.3.x` 인지 한 번 더 확인.

---

## Step 3 — Animation Rigging 패키지 설치

### 3-1. Package Manager 열기

`Window > Package Manager`.
좌측 상단 드롭다운에서 **Unity Registry** 선택.
검색창에 **Animation Rigging** 입력.

📸 **L1_05.png** — Package Manager > Unity Registry > Animation Rigging > **Install** 버튼

### 3-2. 설치 완료 확인

설치 후 좌측 패널에서 `Animation Rigging` 옆에 ✅ 표시.

---

## Step 4 — Quantum 3 SDK 임포트 (⚠️ Voice 보다 먼저)

> Photon Voice 2 와 Quantum 3 는 둘 다 자체적인 `PhotonRealtime` 코드를 가지고 있다.
> **반드시 Quantum 을 먼저 임포트** 한 뒤 Voice 를 임포트해야 충돌이 안 난다 (Step 5 참고).

### 4-1. Asset Store 에서 받기

`Window > Asset Store` → **"Photon Quantum"** 검색 → 카드 클릭 → `Add to My Assets`.

📸 **L1_06.png** — Asset Store 의 Photon Quantum 페이지

### 4-2. Package Manager 에서 Import

`Window > Package Manager` > **My Assets** > **Photon Quantum** > Download > Import.
전체 체크 상태로 `Import`. 2~5분 대기.

📸 **L1_07.png** — Package Manager 의 Photon Quantum Import 버튼

### 4-3. Quantum Hub 첫 실행 (Welcome 화면 4단계)

Quantum 임포트가 끝나면 **Photon Quantum Hub 가 자동으로 열린다** (안 열리면 `Tools > Quantum > Quantum Hub` 직접 실행).

좌측 메뉴: `Welcome` 탭. 우측에 4단계 셋업이 보인다.

| 단계 | 내용 | 필수 / 선택 | 학생 액션 |
|---|---|---|---|
| **Step 1** | Complete the installation of Quantum Unity SDK | **자동 ✅** | 임포트 완료 시 이미 체크됨 (학생이 따로 누를 것 없음) |
| **Step 2** | Install the asteroids Quantum game sample | **선택** | "Skip this step" 클릭. 심화 모듈에선 샘플 게임 불필요 |
| **Step 3** | Register Photon account + **Quantum AppId 입력** | **필수** | 아래 **Step 3 상세 (Photon Dashboard)** 참고 |
| **Step 4** | Install the menu package + Unity build (asteroids) | **선택** | "Skip this step" 또는 패스. 심화 모듈에선 별도 메뉴 시스템 사용 |

📸 **L1_08.png** — Quantum Hub Welcome 화면 (Step 1 자동 ✅ 표시 + Step 3 AppId 입력란)

> ⭐ **AppId 두 가지 명확히**:
> - **Quantum AppId** ← 여기서 입력 (게임 시뮬레이션용)
> - **Voice AppId** ← 다음 실습 L2 에서 별도 발급 (음성 통신용)
> 두 개를 **반드시 분리** 해서 발급·관리한다.

> 💡 기본 과정 Ch04 에서 발급한 Quantum AppId 가 있다면 그대로 재사용 가능. Photon Dashboard 에서 기존 앱 확인.

#### Step 3 상세 — Photon Dashboard 에서 Quantum AppId 발급

1. Quantum Hub 의 **`Photon Dashboard (Opens Web Browser)`** 버튼 클릭 → 브라우저로 [dashboard.photonengine.com](https://dashboard.photonengine.com) 열림 (로그인 필요).

📸 **L1_08a.png** — Photon Dashboard 메인 화면 (`Create New App` 카드 위치)

2. **`Create New App`** 카드 클릭 → 새 앱 생성 폼 열림.

3. 폼 입력:

| 필드 | 입력값 | 비고 |
|---|---|---|
| **Select Photon SDK** | **`Quantum`** ⭐ | Voice / Realtime 과 헷갈리지 말 것 |
| **Select SDK Version** | `Quantum 3 (Recommended)` | 자동 선택됨 |
| **Application Name** | `Ch17_AdvanceCourse` 같은 식별 가능한 이름 | 본인 알아볼 이름 |
| **Description** | "심화 과정 Quantum" 같은 짧은 설명 | 선택 |
| **Url** | **비워두기** | 마케팅·랜딩 페이지용. 학습 시 불필요 |

📸 **L1_08b.png** — 새 앱 생성 폼 (**Photon SDK: Quantum** + 입력 완료 + URL 빈 상태) ⭐ 핵심 캡처

4. **`CREATE`** 클릭 → 앱 생성됨.

5. 생성된 앱 카드에서 **AppId** (긴 영숫자 해시) 복사.

📸 **L1_08c.png** — 생성된 앱 카드 + AppId 위치 (보안상 앞 8자리만 보이게 마스킹 권장)

6. Unity 의 Quantum Hub 로 돌아와 **`App Id:`** 입력란에 붙여넣기 → Step 3 완료.

> ⚠️ **URL 필드와 Webhook 의 차이**:
> - **URL 필드** (앱 생성 폼) = 마케팅·랜딩 페이지 외부 링크. 비워두기.
> - **Webhook** = 게임 이벤트 알림용 (룸 생성·플레이어 입장 등). 앱 생성 후 별도 `Webhooks` 탭에서 설정. 학습엔 불필요.

---

## Step 5 — Photon Voice 는 L2 에서 별도 통합 ⚠️

> **L1 에서는 Voice 를 임포트하지 않는다.** 이유:
>
> Asset Store 의 `Photon Voice 2` (ID 130518) 는 **Photon Realtime 4** 기반 (PUN2 의존) 인데,
> Quantum 3 는 **Photon Realtime 5** 를 사용한다. **두 SDK 의 Realtime 버전이 달라** 같이 임포트하면
> `Folder ... contains multiple assembly definition files` · `error CS0234: 'Realtime' does not exist` ·
> `error CS0101: already contains a definition for ...` 같은 컴파일 에러가 100건 이상 폭발한다.

#### 올바른 SDK — Photon Voice SDK Realtime5

Quantum 3 와 호환되는 정확한 SDK 는 **Photon Voice SDK Realtime5** (Photon Realtime 5 포함) 다.
다운로드 위치 → [photonengine.com/sdks](https://www.photonengine.com/sdks) (Unity Asset Store 가 아닌 **Photon 공식 SDK 페이지**).

이 SDK 의 설치·연결·검증은 학습 흐름상 자연스럽게 **L2 (PlayerCharacter + Voice 셋업)** 에서 다룬다.

#### L1 에서 할 일 — 패스

L1 단계에서는 Voice 임포트 **건너뛰고** Step 6 (`Interaction/` 폴더 구조) 으로 바로 진행.

> 💡 **이미 Voice 2 (Asset Store ID 130518) 를 임포트해서 에러가 폭발한 학생**:
> 1. Unity Editor 닫기
> 2. `Assets/Photon/` 폴더 **통째 삭제** (Quantum 도 같이)
> 3. (선택) `Library/` 도 삭제 (캐시 완전 초기화)
> 4. Unity 다시 열기 (에러 무시) → **Quantum 3 만 재임포트** (Step 4 다시)
> 5. Voice 는 L2 에서 정확한 SDK 로 임포트
>
> 📸 **L1_09.png** — `Assets/Photon/` 폴더 삭제 직전 탐색기 (재임포트 함정 시각화 — 선택 캡처)

---

## Step 6 — 폴더 구조 정리

### 6-1. Interaction 폴더 생성

`Assets/Interaction/` 폴더 생성. 하위에:

```
Assets/Interaction/
  Scripts/
    Voice/
    Chat/
    Emote/
    Pose/
    Movement/
    Social/
    UI/
  Prefabs/
  Materials/
  Animations/
    Emotes/
    Poses/
    Social/
  Scenes/
```

심화 모듈에서 만드는 산출물이 모두 `Interaction/` 하위에 모이게 한다. 학생 제출 시 이 폴더 통째로 zip.

📸 **L1_09.png** — Project 창의 최종 폴더 트리 (Photon · Quantum · Animation Rigging · Interaction)

---

## Step 7 — 베이스 씬 만들기

### 7-1. 새 씬 생성

`File > New Scene` → 기본 템플릿(Basic (URP)) 선택 → Create.

📸 **L1_10.png** — File > New Scene 다이얼로그

### 7-2. Hierarchy 기본 GameObject 배치

- **Plane** — `GameObject > 3D Object > Plane`. Scale (5, 1, 5) 로 키워 바닥 역할
- **Directional Light** — 기본 포함
- **Main Camera** — 기본 포함. Position `(0, 5, -10)`, Rotation `(20, 0, 0)` 으로 약간 위에서 바라봄
- **PlayerSpawnPoint** — 빈 GameObject 생성, Position `(0, 0, 0)`, Rotation `(0, 0, 0)` (캐릭터 스폰 위치)

📸 **L1_11.png** — Hierarchy 의 Plane · Directional Light · Main Camera · PlayerSpawnPoint 배치

### 7-3. 씬 저장

`Ctrl+S` → `Assets/Interaction/Scenes/MainScene.unity` 로 저장.

---

## Step 8 — 정상 동작 검증

### 8-1. Play 버튼

상단 Play 버튼 클릭.

📸 **L1_12.png** — Play 모드에서 빈 씬 정상 실행 (Plane 위에 카메라가 바라보는 빈 공간)

### 8-2. Console 확인

빨간 에러가 없으면 OK. 일부 노란 경고(Photon SDK 의 무해한 경고)는 무시 가능.

> ❌ 에러 있으면:
> - "TMP_FontAsset 누락" → `Window > TextMeshPro > Import TMP Essential Resources`
> - "Animation Rigging API 미일치" → Package Manager 에서 Animation Rigging 최신 버전 업데이트
> - "Photon SDK 컴파일 에러" → 2~3분 더 기다리고 한 번 더 Play

---

## 정상 동작 체크리스트

이 실습을 마쳤다면 다음 6개가 모두 ✅ 이어야 한다.

- [ ] Unity 6.3 LTS (`6000.3.x`) 로 새 프로젝트 생성됨 (URP 템플릿)
- [ ] `Assets/Photon/PhotonVoice` 폴더 존재
- [ ] `Assets/Photon/Quantum` 폴더 존재 (Quantum SDK 임포트됨)
- [ ] `Assets/Interaction/` 하위에 폴더 구조 만들어짐
- [ ] `Assets/Interaction/Scenes/MainScene.unity` 가 빈 씬에서 정상 실행
- [ ] Console 에 빨간 에러 없음

다음 실습 **L2 — PlayerCharacter 가져오기 + Photon Voice 2 셋업** 에서는 기본 과정 챕터 05 의 `PlayerCharacter` 를 이 씬에 가져오고, Voice 전용 AppId 를 발급해 마이크 인식까지 확인한다.

---

## 🚀 응용 질문 (도전 과제)

### Q1. 프로젝트를 GitHub 에 올릴 때 큰 패키지(Photon, Quantum)를 제외하고 가볍게 공유하려면?

힌트:
- `.gitignore` 에 `Library/`, `Temp/`, `Logs/`, 그리고 큰 패키지 폴더 추가
- `Packages/manifest.json` 만 공유하면 다른 PC 에서 자동으로 패키지 복원
- Git LFS 로 큰 에셋 처리

### Q2. URP 가 아닌 HDRP 로 시작하면 어떻게 다를까?

힌트:
- HDRP 는 고품질 그래픽 (PC·콘솔 전용, 모바일 부적합)
- 메타버스에서 다중 사용자·고밀도 환경 → URP 가 일반적으로 더 적합
- 셰이더·머티리얼 호환 차이 (커스텀 셰이더 시 어느 쪽 기준으로 작성?)

### Q3. 프로젝트 이름이 한글이거나 경로에 공백이 있으면 어떤 문제가 생길까?

힌트:
- Unity 빌드 시 일부 플랫폼에서 한글 경로 에러
- Photon Voice 2 같은 네이티브 라이브러리는 영문 경로 권장
- 만약 발생하면 어떻게 진단할까? Editor.log 분석
