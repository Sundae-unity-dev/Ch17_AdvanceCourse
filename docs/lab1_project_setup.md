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

## Step 4 — Photon Voice 2 임포트

### 4-1. Asset Store 에서 받기

`Window > Asset Store` → Asset Store 웹페이지 열림 → **"Photon Voice 2"** 검색 → 카드 클릭.

📸 **L1_06.png** — Asset Store 의 Photon Voice 2 페이지 + Compatible Unity Versions 영역 (Unity 6 또는 6000.x 표시)

> ⚠️ Compatible Unity Versions 에 `Unity 6` 또는 `6000.x` 포함되어 있는지 반드시 확인. 옛 버전이면 Photon 포럼에서 최신 빌드 확인.

### 4-2. My Assets > Import

`Add to My Assets` → 자동으로 Unity 의 Package Manager 가 열림 (또는 Unity 메뉴 `Window > Package Manager > My Assets`) → **`Photon Voice 2` > Import**.

📸 **L1_07.png** — Package Manager > My Assets > Photon Voice 2 > Import 버튼

### 4-3. Import 다이얼로그

전체 체크 상태로 `Import` 클릭. 2~3분 대기.

> 🔧 임포트 직후 `TextMeshPro Essentials` 임포트 팝업이 뜨면 `Import TMP Essentials` 클릭.

---

## Step 5 — Quantum 3 SDK 임포트

### 5-1. 기본 과정 챕터 04~05 의 방식과 동일

Asset Store 에서 **Photon Quantum** 검색 → My Assets → Import.

📸 **L1_08.png** — Photon Quantum 임포트 화면

### 5-2. Quantum Hub 첫 실행

임포트 후 `Tools > Quantum > Quantum Hub` 실행 → 4단계 셋업 진행 (Install / Samples Game / Account / Final Setup).

> 기본 과정에서 이미 해 본 작업. 자세한 단계는 [실습 1] Quantum 3 프로젝트 기본 설정 PDF 참고.

### 5-3. ⚠️ 트러블슈팅 — PhotonRealtime 어셈블리 정의 중복

Quantum 3 임포트 후 Console 에 다음 에러가 뜨면:

> `Folder 'Assets/Photon/PhotonRealtime/Code/' contains multiple assembly definition files`
> `(Photon.Realtime.asmdef, PhotonRealtime.asmdef)`

**원인**: Photon Voice 2 (Step 4) 와 Quantum 3 (Step 5) 가 같은 `PhotonRealtime` 폴더에 각자 이름이 다른 `.asmdef` 를 넣어 충돌. Unity 는 한 폴더에 하나의 `.asmdef` 만 허용.

**해결**: 점 없는 옛 컨벤션 `PhotonRealtime.asmdef` 와 그 `.meta` 두 파일 삭제 — 점 있는 표준 `Photon.Realtime.asmdef` 만 유지.

**삭제 방법 (탐색기)**:
1. `Assets/Photon/PhotonRealtime/Code/` 폴더 열기
2. `PhotonRealtime.asmdef` + `PhotonRealtime.asmdef.meta` 두 파일 선택
3. Delete
4. Unity 로 돌아오면 자동 재컴파일

**삭제 방법 (PowerShell)**:
```powershell
Remove-Item "Assets\Photon\PhotonRealtime\Code\PhotonRealtime.asmdef*" -Force
```

📸 **L1_08b.png** — 두 .asmdef 파일이 같이 보이는 Project 창 (삭제 전, 함정 시각화)

> 💡 **예방 팁**: Photon Voice 2 임포트 시 Import 다이얼로그에서 `PhotonRealtime` 폴더 체크를 미리 해제하면 이 충돌이 발생하지 않는다 (Quantum 3 의 Realtime 만 사용). 이미 임포트했으면 위 사후 해결로 처리.

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
