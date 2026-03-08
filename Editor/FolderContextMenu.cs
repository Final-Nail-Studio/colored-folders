using UnityEngine;
using UnityEditor;

namespace FinalNailStudio.ColoredFolders
{
    [InitializeOnLoad]
    static class FolderContextMenu
    {
        // Set on MouseDown, stays true until ContextClick is consumed or expires.
        static bool s_BlockNextContextClick;

        static FolderContextMenu()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            var e = Event.current;

            // Swallow the ContextClick that Unity fires after our captured MouseDown,
            // so Unity's default right-click menu doesn't appear alongside our popup.
            if (e.type == EventType.ContextClick && s_BlockNextContextClick)
            {
                s_BlockNextContextClick = false;
                e.Use();
                return;
            }

            if (e.type != EventType.MouseDown || e.button != 1 || !e.alt)
                return;
            if (!selectionRect.Contains(e.mousePosition))
                return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path) || !PathUtility.IsAssetsPath(path))
                return;
            if (!AssetDatabase.IsValidFolder(path))
                return;

            s_BlockNextContextClick = true;
            string folderName = PathUtility.GetFolderName(path);
            Vector2 screenPos = GUIUtility.GUIToScreenPoint(e.mousePosition);
            e.Use();

            // Delay one frame so the current OnGUI pass finishes cleanly
            // before we open a new window.
            EditorApplication.delayCall += () =>
            {
                FolderColorPopup.Show(folderName, screenPos);
            };
        }
    }
}
