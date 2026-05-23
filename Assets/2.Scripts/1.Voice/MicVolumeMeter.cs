// =============================================================================
// MicVolumeMeter.cs — 자기 마이크 음량을 막대 그래프(fillAmount)로 시각화
// -----------------------------------------------------------------------------
// 학습 포인트:
// 1) Vivox 의 VivoxParticipant.AudioEnergy (0.0 ~ 1.0) 가 음성 세기.
// 2) 자기 자신은 IsSelf == true 인 참가자로 표현된다.
// 3) ParticipantAddedToChannel / Removed 이벤트로 참가자 추가·제거 추적.
// 4) Update() 에서 selfParticipant.AudioEnergy 를 읽어 Image.fillAmount 에 반영.
// 5) Attack/Release 평활화로 VU 미터처럼 자연스럽게 차고 빠진다.
//
// 타이밍 주의:
//   OnEnable() 은 GameObject 활성화 직후 즉시 호출되는 반면, VivoxService.Instance
//   는 VivoxManager.InitializeAsync() 가 비동기로 완료된 다음에야 사용 가능하다.
//   따라서 OnEnable 안에서 곧장 구독하면 NullReferenceException 위험.
//   → VivoxService.Instance 가 준비될 때까지 한 프레임씩 대기 후 구독한다.
//
// 평활화 주의:
//   AudioEnergy 는 짧은 윈도우 RMS 라 발성 도중에도 순간적으로 0 가까이 떨어지는
//   구간이 있다. 그대로 fillAmount 에 꽂으면 막대가 뚝뚝 끊겨 보인다.
//   → 올라갈 때는 빠르게(attack), 내려갈 때는 천천히(release) 보간한다.
// =============================================================================

using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class MicVolumeMeter : MonoBehaviour
{
    [SerializeField] Image fillBar;   // 채워질 막대 (Fill Image)
    [SerializeField] float boost = 3f; // 시각적으로 변화를 키워주는 배율

    // Attack: target 이 displayLevel 보다 클 때 (값이 올라갈 때) 따라가는 속도.
    // Release: target 이 작을 때 (값이 내려갈 때) 따라가는 속도.
    // 단위는 "초당 fillAmount 변화량" — 1f 면 1초에 0→1 까지 차오름.
    [SerializeField] float attackSpeed = 25f;
    [SerializeField] float releaseSpeed = 6f;

    // 자기 자신(IsSelf=true) 을 한 번 잡으면 캐시해두고 매 프레임 AudioEnergy 를 읽는다.
    VivoxParticipant selfParticipant;

    // 이벤트 중복 구독·해제 방지용 플래그
    bool subscribed = false;

    // 평활화된 현재 표시값 (이전 프레임의 fillAmount).
    float displayLevel = 0f;

    async void OnEnable()
    {
        // VivoxService 가 초기화될 때까지 한 프레임씩 양보(Yield).
        // VivoxManager.InitializeAsync() 가 끝나는 순간 VivoxService.Instance 가 채워진다.
        while (VivoxService.Instance == null)
        {
            await Task.Yield();
            if (this == null) return;  // 도중에 GameObject 가 파괴되면 중단
        }

        VivoxService.Instance.ParticipantAddedToChannel   += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;
        subscribed = true;
    }

    void OnDisable()
    {
        // 구독한 적이 없거나 서비스가 이미 종료됐으면 해제 시도 안 함.
        if (!subscribed || VivoxService.Instance == null) return;

        VivoxService.Instance.ParticipantAddedToChannel   -= OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
        subscribed = false;
    }

    void OnParticipantAdded(VivoxParticipant p)
    {
        if (p.IsSelf) selfParticipant = p;   // 자기 자신만 시각화 대상
    }

    void OnParticipantRemoved(VivoxParticipant p)
    {
        if (p.IsSelf) selfParticipant = null;
    }

    void Update()
    {
        // 이번 프레임의 "목표값" 계산. 참가자가 없으면 0 으로 수렴.
        float target = 0f;
        if (selfParticipant != null)
        {
            float energy = (float)selfParticipant.AudioEnergy;
            target = Mathf.Clamp01(energy * boost);
        }

        // 올라갈 때는 attackSpeed, 내려갈 때는 releaseSpeed 로 보간.
        // MoveTowards 는 deltaTime 곱해 사용하면 프레임레이트와 무관하게 일정 속도.
        float speed = target > displayLevel ? attackSpeed : releaseSpeed;
        displayLevel = Mathf.MoveTowards(displayLevel, target, speed * Time.deltaTime);
        fillBar.fillAmount = displayLevel;
    }
}
