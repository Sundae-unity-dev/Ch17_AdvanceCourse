// =============================================================================
// VivoxPositionalSync.cs — Spatial 채널 입장 후, 매 프레임 자기 위치·방향을
//                          Vivox 에 알려서 거리·방향감 있는 음성이 들리게 한다.
// -----------------------------------------------------------------------------
// 학습 포인트:
// 1) Vivox 의 Positional 채널은 입장만으로 끝이 아니다. 참가자의 좌표가
//    매 프레임 갱신돼야 다른 사람 귀에 "내가 다가오는 소리·내가 멀어지는 소리"
//    가 자연스럽게 변한다.
// 2) Set3DPosition(GameObject, string channelName) 한 번 호출하면 Vivox 가
//    그 GameObject 의 Transform 에서 위치·forward·up 을 읽어 처리한다.
// 3) PlayerCharacter 에 이 컴포넌트를 붙여 두면 캐릭터가 어디로 움직이든
//    Vivox 에 자동으로 반영된다.
// 4) Inspector 의 listenerOrigin 이 비어 있으면 자기 자신(transform) 을 쓴다.
//    1인칭 게임이면 카메라 위치를, 3인칭이면 캐릭터 머리/몸통을 지정한다.
// =============================================================================

using Unity.Services.Vivox;
using UnityEngine;

[DisallowMultipleComponent]
public class VivoxPositionalSync : MonoBehaviour
{
    // ---------------------------------------------------------------------
    // 위치 기준 — 보통 캐릭터의 머리 (귀) 위치가 자연스럽다.
    // 비워두면 자기 자신의 transform 을 쓴다.
    // ---------------------------------------------------------------------
    [SerializeField] Transform listenerOrigin;

    // ---------------------------------------------------------------------
    // 채널 이름 — 비우면 VivoxManager 의 SpatialChannelName 을 자동 사용.
    // 한 씬에 여러 Spatial 채널을 두는 경우만 직접 지정한다.
    // ---------------------------------------------------------------------
    [SerializeField] string channelName = "";

    // ---------------------------------------------------------------------
    // 매번 채널 입장 여부 확인이 부담스러우니, 첫 갱신 성공 후엔 true 로
    // 마크해 두고 가볍게 Set3DPosition 만 돌린다.
    // ---------------------------------------------------------------------
    bool channelJoined = false;

    void Reset()
    {
        // 컴포넌트 추가 시 한 번만 호출 — listenerOrigin 기본값 채워준다
        if (listenerOrigin == null) listenerOrigin = transform;
    }

    string EffectiveChannel
    {
        get
        {
            if (!string.IsNullOrEmpty(channelName)) return channelName;
            if (VivoxManager.Instance != null) return VivoxManager.Instance.SpatialChannelName;
            return null;
        }
    }

    void Update()
    {
        // VivoxService 가 아직 준비 안 됐으면 그냥 대기 — InitializeAsync 중
        if (VivoxService.Instance == null) return;
        if (!VivoxService.Instance.IsLoggedIn) return;

        var ch = EffectiveChannel;
        if (string.IsNullOrEmpty(ch)) return;
        if (listenerOrigin == null) return;

        // 입장 여부 확인 — ActiveChannels 에 ch 가 있어야 Set3DPosition 이 의미 있음
        if (!IsChannelActive(ch))
        {
            channelJoined = false;
            return;
        }
        channelJoined = true;

        // 핵심 한 줄 — Vivox 가 listenerOrigin 의 Transform 에서
        // position · forward · up 을 읽어 거리·방향감 처리를 해준다.
        VivoxService.Instance.Set3DPosition(listenerOrigin.gameObject, ch);
    }

    // ---------------------------------------------------------------------
    // IsChannelActive — 지정 채널에 현재 입장해 있는지 확인.
    // ActiveChannels 는 채널명을 key 로 한 dictionary 라서 O(1) 조회.
    // ---------------------------------------------------------------------
    bool IsChannelActive(string ch)
    {
        var active = VivoxService.Instance.ActiveChannels;
        if (active == null) return false;
        return active.ContainsKey(ch);
    }
}
