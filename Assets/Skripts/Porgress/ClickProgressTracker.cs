// 파일: Scripts/Progress/ClickProgressTracker.cs
using System;

namespace PokeClicker
{
    /// <summary>
    /// 트레이너별 클릭 누적/나머지 진행도(친밀도 지급 주기 계산용).
    /// 순수 데이터 클래스: 저장/로드는 ITrainerRepository에서 수행.
    /// </summary>
    [Serializable]
    public class ClickProgressTracker
    {
        /// <summary>총 입력 횟수(통계/로그용, 보상 계산에는 필수 아님)</summary>
        public long totalInputs;

        /// <summary>친밀도 보상 주기를 위한 나머지 누적 (0..clicksPerFriendship-1)</summary>
        public int remainderFriendship;

        /// <summary>입력 1회를 누적하고, 이번에 지급해야 할 "주기 도달 횟수"를 반환</summary>
        public int OnInput(int clicksPerFriendship)
        {
            totalInputs++;
            remainderFriendship++;

            if (remainderFriendship >= clicksPerFriendship)
            {
                int delta = remainderFriendship / clicksPerFriendship; // 여러 주기 한 번에 소화
                remainderFriendship = remainderFriendship % clicksPerFriendship;
                return delta;
            }
            return 0;
        }
    }
}
