// 파일: Scripts/Input/InputCapture.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 마우스/키보드 입력 1회를 "게임 인풋"으로 정규화해 이벤트로 방출.
    /// - UI 없음 전제, 연타 필터/디바운스 없음
    /// - Update 에서 Input.anyKeyDown 만 사용(마우스 버튼도 포함됨)
    /// </summary>
    public class InputCapture : MonoBehaviour
    {
        /// <summary>게임 입력이 1회 발생했을 때 호출</summary>
        public event Action OnGameInput;

        void Update()
        {
            // anyKeyDown: 키보드/마우스 버튼 중 하나라도 눌리면 true (휠/축은 제외)
            if (Input.anyKeyDown)
            {
                OnGameInput?.Invoke();
            }
        }
    }
}
