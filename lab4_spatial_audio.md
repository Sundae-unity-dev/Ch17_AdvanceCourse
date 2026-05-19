# 실습 L4 — Spatial Audio (3D 거리 음향)

> L3 에서 음성이 들리긴 하지만 거리 무관하게 항상 같은 음량. 이번엔 캐릭터가 **가까이 가면 잘 들리고, 멀어지면 작아지는** 자연스러운 음향을 구현한다.
>
> ⏱️ 예상 시간: 50분 · 📸 슬롯: __L4___TMP ~ __L4___TMP

---

## 학습 목표

1. **AudioSource 의 Spatial Blend** 가 2D/3D 음향을 결정하는 원리를 이해한다
2. **Min Distance / Max Distance / Rolloff Mode** 로 거리별 감쇠 곡선을 설계한다
3. 두 캐릭터 사이 거리를 바꿔가며 음량 변화를 청각적으로 검증한다
4. 가청 거리 밖에서는 패킷 처리를 줄이는 **컬링 최적화** 개념을 적용한다

## 사전 확인

- [ ] L3 완료 (V 키로 음성 통신 동작)

---

## Step 1 — Speaker 의 AudioSource 를 3D 음향으로

### 1-1. 캐릭터 프리팹의 AudioSource Inspector 열기

`PlayerCharacter.prefab` 의 Speaker 가 자동 생성한 AudioSource 클릭.

📸 **__L4___TMP.png** — AudioSource 컴포넌트 Inspector

### 1-2. Spatial Blend 슬라이더

**2D ←→ 3D** 슬라이더를 **1.0 (3D)** 로 이동.

> 0 이면 평면 사운드 (모든 위치에서 같은 음량), 1 이면 완전 3D 음향. 중간값은 보간.

📸 **__L4___TMP.png** — Spatial Blend = 1.0

### 1-3. 3D Sound Settings 펼치기

`3D Sound Settings` 섹션 확장.

---

## Step 2 — 거리·감쇠 곡선 설정

### 2-1. Min Distance / Max Distance

- **Min Distance**: `2` (이 거리 안에서는 최대 음량)
- **Max Distance**: `20` (이 거리 넘으면 거의 안 들림)

📸 **__L4___TMP.png** — Min/Max Distance 입력

### 2-2. Volume Rolloff (감쇠 곡선)

`Volume Rolloff: Custom Rolloff` 선택 → 그래프에서 곡선 모양 조정.

권장 모양:
- 2m 까지 100%
- 5m 에서 70%
- 10m 에서 30%
- 20m 에서 0%

📸 **__L4___TMP.png** — Volume Rolloff 그래프 (사용자 정의 곡선)

### 2-3. Spread (스피커 폭)

`Spread: 60` 정도 — 너무 좁으면 머리 약간만 돌려도 갑자기 한쪽 귀로만 들림.

---

## Step 3 — 청취자 (AudioListener) 위치 확인

### 3-1. AudioListener 는 1개만

3D 음향은 **AudioListener** 위치 기준으로 계산. 보통 메인 카메라에 붙어 있음.
씬에 AudioListener 가 **딱 1개** 만 있어야 한다 (여러 개면 경고).

📸 **__L4___TMP.png** — Main Camera 의 AudioListener 컴포넌트

### 3-2. AudioListener 가 카메라에 붙어있는지 확인

3인칭 카메라 기준이면 카메라가 따라다닐 때 청취자 위치도 같이 이동.

---

## Step 4 — 두 명 빌드 + 거리 테스트

### 4-1. 같은 룸 입장 + V 키 통신

L3 처럼 빌드 + Editor 동시 실행.

📸 **__L4___TMP.png** — 두 캐릭터가 가까이 있는 상태 (큰 음량)

### 4-2. 거리 멀어지면서 청취

- 0~2m: 또렷하게 들림
- 5m: 약간 작아짐
- 15m: 거의 안 들림
- 20m+: 무음

📸 **__L4___TMP.png** — 캐릭터 사이 거리 15m, 음량 줄어든 상태

### 4-3. Editor 에서 AudioSource 의 실시간 음량 확인

Recorder.IsCurrentlyTransmitting 상태에서, Editor 의 Audio Mixer 또는 AudioSource Inspector 의 실시간 값을 보면 거리 따라 변함.

📸 **__L4___TMP.png** — 거리별 음량 변화 그래프 (Audio Mixer)

---

## Step 5 — 좌우 분배 (Pan) 확인

### 5-1. 헤드폰 끼고 캐릭터 좌우로 이동

상대 캐릭터를 자기 캐릭터의 왼쪽으로 옮김 → 왼쪽 귀로 더 크게 들림.
오른쪽으로 옮기면 반대.

📸 **__L4___TMP.png** — 상대 캐릭터가 좌측에 있는 상태

> 이건 Stereo Pan 이 AudioListener-AudioSource 의 방향 벡터에 따라 자동 계산됨.

---

## Step 6 — Quality of Life 보완

### 6-1. Doppler Level

`Doppler Level: 0` (캐릭터 이동 속도가 빠르지 않으면 도플러 효과 없게).
빠른 차량이라면 0.5 ~ 1.0.

📸 **__L4___TMP.png** — Doppler Level 설정 화면

### 6-2. Reverb Zone Mix

Reverb Zone 사용 안 하면 `0`. 동굴·홀 등 공간감 효과 쓰면 `1`.

---

## 정상 동작 체크리스트

- [ ] AudioSource Spatial Blend = 1.0 (3D)
- [ ] 캐릭터 사이 2m 이내: 최대 음량
- [ ] 캐릭터 사이 20m+: 거의 안 들림
- [ ] 좌우 위치에 따라 Stereo Pan 변화 (헤드폰 필수)
- [ ] AudioListener 가 씬에 딱 1개

다음 실습 **L5 — RPC 기반 텍스트 채팅** 에서는 음성 외에 텍스트로도 소통하는 채널을 만든다.

---

## 🚀 응용 질문

### Q1. 벽 뒤에 있는 사람의 음성이 막히거나 작게 들리도록 (Occlusion) 구현하려면?

지금은 벽이 있어도 거리만 같으면 똑같이 들림. 실제 메타버스라면 벽 뒤 음성은 막혀야 자연스러움.

힌트:
- `Physics.Raycast` 로 청취자 ↔ 화자 사이에 장애물 검출
- 장애물 있으면 AudioSource.volume 또는 LowPassFilter cutoff 조정
- Steam Audio·Resonance Audio 같은 외부 솔루션도 검토 (HRTF + Occlusion)

### Q2. 가청 거리 밖 플레이어의 음성 패킷 처리를 줄이려면?

20m 밖이면 어차피 안 들리는데 패킷은 계속 전송·디코딩됨 → 네트워크·CPU 낭비.

힌트:
- `PhotonVoiceNetwork.AddSpeakerFactoryOverride` 로 Speaker 생성 시점 제어
- 거리 기반으로 Speaker.gameObject.SetActive(false) (Pause Decoding)
- Quantum 의 Interest Management (AOI) 와 비슷한 개념

### Q3. 화자별로 다른 음향 효과 (예: 보스 캐릭터는 에코) 적용하려면?

특정 플레이어/NPC의 음성에만 리버브·에코 효과를 입히려면?

힌트:
- AudioSource 에 `AudioReverbFilter`, `AudioEchoFilter` 추가
- 캐릭터 데이터에 voiceProfile (`Normal`, `Boss`, `Robot`) 추가
- 프로필별 필터 파라미터 적용
