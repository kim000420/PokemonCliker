// ����: Scripts/Input/InputCapture.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ���콺/Ű���� �Է� 1ȸ�� "���� ��ǲ"���� ����ȭ�� �̺�Ʈ�� ����.
    /// - UI ���� ����, ��Ÿ ����/��ٿ ����
    /// - Update ���� Input.anyKeyDown �� ���(���콺 ��ư�� ���Ե�)
    /// </summary>
    public class InputCapture : MonoBehaviour
    {
        /// <summary>���� �Է��� 1ȸ �߻����� �� ȣ��</summary>
        public event Action OnGameInput;

        void Update()
        {
            // anyKeyDown: Ű����/���콺 ��ư �� �ϳ��� ������ true (��/���� ����)
            if (Input.anyKeyDown)
            {
                OnGameInput?.Invoke();
            }
        }
    }
}
