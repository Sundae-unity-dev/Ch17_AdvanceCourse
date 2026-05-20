// =============================================================================
// MicVolumeMeter.cs — 자기 마이크 음량을 막대 그래프(fillAmount)로 시각화
// -----------------------------------------------------------------------------
// 학습 포인트:
// 1) Vivox 의 VivoxParticipant.AudioEnergy (0.0 ~ 1.0) 가 음성 세기.
// 2) 자기 자신은 IsSelf == true 인 참가자로 표현된다.
// 3) ParticipantAddedToChannel / Removed 이벤트로 참가자 추가·제거 추적.
// 4) Update() 에서 selfParticipant.AudioEnergy 를 읽어 Image.fillAmount 에 반영.
//
// 타이밍 주의:
//   OnEnable() 은 GameObject 활성화 직후 즉시 호출되는 반면, VivoxService.Instance
//   는 VivoxManager.InitializeAsync() 가 비동기로 완료된 다음에야 사용 가능하다.
//   따라서 OnEnable 안에서 곧장 구독하면 NullReferenceException 위험.
//   → VivoxService.Instance 가 준비될 때까지 한 프레임씩 대기 후 구독한다.
// =============================================================================

using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class MicVolumeMeter : MonoBehaviour
{
    [SerializeField] Image fillBar;   // 채워질 막대 (Fill Image)
    [SerializeField] float boost = 3f; // 시각적으로 변화를 키워주는 배율

    // 자기 자신(IsSelf=true) 을 한 번 잡으면 캐시해두고 매 프레임 AudioEnergy 를 읽는다.
    VivoxParticipant selfParticipant;

    // 이벤트 중복 구독·해제 방지용 플래그
    bool subscribed = false;

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
        if (selfParticipant == null)
        {
            fillBar.fillAmount = 0f;
            return;
        }

        // AudioEnergy(0~1) 에 boost 를 곱해 살짝 키운 뒤 fillAmount 에 반영
        float energy = (float)selfParticipant.AudioEnergy;
        fillBar.fillAmount = Mathf.Clamp01(energy * boost);
    }
}
