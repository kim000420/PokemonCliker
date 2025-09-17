// ����: Assets/Editor/OpenSaveDataFolder.cs
using UnityEditor;
using UnityEngine;
using System.Diagnostics; // Process.Start�� ����ϱ� ����

namespace PokeClicker.EditorTools
{
    /// <summary>
    /// ����Ƽ ������ �޴��� '���� ������ ���� ����' ����� �߰��մϴ�.
    /// </summary>
    public static class OpenSaveDataFolder
    {
        [MenuItem("Tools/Open Persistent Data Path")]
        public static void OpenPath()
        {
            // Application.persistentDataPath�� �÷����� ���� ��ΰ� �޶���
            string path = Application.persistentDataPath;
            UnityEngine.Debug.Log($"Persistent Data Path: {path}");

            // �ش� ��θ� �ü�� ���� Ž����� ���ϴ�.
            Process.Start(path);
        }
    }
}