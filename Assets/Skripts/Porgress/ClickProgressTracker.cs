// ����: Scripts/Progress/ClickProgressTracker.cs
using System;

namespace PokeClicker
{
    /// <summary>
    /// Ʈ���̳ʺ� Ŭ�� ����/������ ���൵(ģ�е� ���� �ֱ� ����).
    /// ���� ������ Ŭ����: ����/�ε�� ITrainerRepository���� ����.
    /// </summary>
    [Serializable]
    public class ClickProgressTracker
    {
        /// <summary>�� �Է� Ƚ��(���/�α׿�, ���� ��꿡�� �ʼ� �ƴ�)</summary>
        public long totalInputs;

        /// <summary>ģ�е� ���� �ֱ⸦ ���� ������ ���� (0..clicksPerFriendship-1)</summary>
        public int remainderFriendship;

        /// <summary>�Է� 1ȸ�� �����ϰ�, �̹��� �����ؾ� �� "�ֱ� ���� Ƚ��"�� ��ȯ</summary>
        public int OnInput(int clicksPerFriendship)
        {
            totalInputs++;
            remainderFriendship++;

            if (remainderFriendship >= clicksPerFriendship)
            {
                int delta = remainderFriendship / clicksPerFriendship; // ���� �ֱ� �� ���� ��ȭ
                remainderFriendship = remainderFriendship % clicksPerFriendship;
                return delta;
            }
            return 0;
        }
    }
}
