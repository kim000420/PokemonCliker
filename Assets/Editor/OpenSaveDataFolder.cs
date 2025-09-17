// 파일: Assets/Editor/OpenSaveDataFolder.cs
using UnityEditor;
using UnityEngine;
using System.Diagnostics; // Process.Start를 사용하기 위함

namespace PokeClicker.EditorTools
{
    /// <summary>
    /// 유니티 에디터 메뉴에 '저장 데이터 폴더 열기' 기능을 추가합니다.
    /// </summary>
    public static class OpenSaveDataFolder
    {
        [MenuItem("Tools/Open Persistent Data Path")]
        public static void OpenPath()
        {
            // Application.persistentDataPath는 플랫폼에 따라 경로가 달라짐
            string path = Application.persistentDataPath;
            UnityEngine.Debug.Log($"Persistent Data Path: {path}");

            // 해당 경로를 운영체제 파일 탐색기로 엽니다.
            Process.Start(path);
        }
    }
}