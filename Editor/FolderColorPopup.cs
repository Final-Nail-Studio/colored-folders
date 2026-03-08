using UnityEngine;
using UnityEditor;

namespace FinalNailStudio.ColoredFolders
{
    /// <summary>
    /// Dropdown popup shown on Alt+Right Click. Uses EditorWindow.ShowAsDropDown
    /// instead of GenericMenu so Unity's own context menu can't dismiss it.
    /// </summary>
    public class FolderColorPopup : EditorWindow
    {
        const float k_SwatchSize = 14f;
        const float k_RowHeight = 20f;
        const float k_Width = 180f;

        string _folderName;
        GUIStyle _itemStyle;
        GUIStyle _activeItemStyle;

        public static void Show(string folderName, Vector2 screenPos)
        {
            // Close any existing popup first
            var existing = Resources.FindObjectsOfTypeAll<FolderColorPopup>();
            foreach (var w in existing) w.Close();

            var popup = CreateInstance<FolderColorPopup>();
            popup._folderName = folderName;
            popup.wantsMouseMove = true;

            // 16 color rows + header + recursive + remove + settings + separators + padding
            float height = k_RowHeight                   // header
                + 2                                       // separator
                + ColorPalette.Colors.Length * k_RowHeight // colors
                + 2                                       // separator
                + k_RowHeight                             // recursive
                + k_RowHeight + 2                         // remove
                + 2                                       // separator
                + k_RowHeight                             // settings
                + 12;                                     // padding

            popup.ShowAsDropDown(new Rect(screenPos, Vector2.zero), new Vector2(k_Width, height));
        }

        void OnEnable()
        {
            _itemStyle = null;
            _activeItemStyle = null;
        }

        void EnsureStyles()
        {
            if (_itemStyle != null) return;

            _itemStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(4, 4, 2, 2),
                margin = new RectOffset(0, 0, 0, 0),
                fixedHeight = k_RowHeight
            };

            _activeItemStyle = new GUIStyle(_itemStyle)
            {
                fontStyle = FontStyle.Bold
            };
        }

        void OnGUI()
        {
            if (string.IsNullOrEmpty(_folderName)) { Close(); return; }
            EnsureStyles();

            var settings = ColoredFoldersSettings.Instance;
            var rule = settings.FindRule(_folderName);

            // Header
            EditorGUILayout.LabelField($"\"{_folderName}\"", EditorStyles.boldLabel,
                GUILayout.Height(k_RowHeight));
            DrawSeparator();

            // Color palette
            foreach (var (name, color) in ColorPalette.Colors)
            {
                bool isActive = rule != null &&
                    ColorUtility.ToHtmlStringRGB(rule.GetColor()) == ColorUtility.ToHtmlStringRGB(color);

                Rect row = EditorGUILayout.GetControlRect(false, k_RowHeight);

                // Hover highlight
                bool hovered = row.Contains(Event.current.mousePosition);
                if (hovered)
                {
                    float h = EditorGUIUtility.isProSkin ? 0.35f : 0.55f;
                    EditorGUI.DrawRect(row, new Color(0.2f, 0.5f, 0.9f, h));
                    Repaint();
                }

                // Color swatch
                var swatch = new Rect(row.x + 4, row.y + (row.height - k_SwatchSize) * 0.5f,
                    k_SwatchSize, k_SwatchSize);
                EditorGUI.DrawRect(swatch, color);

                // Label
                var label = new Rect(row.x + k_SwatchSize + 10, row.y, row.width - k_SwatchSize - 14, row.height);
                string text = isActive ? "\u2713 " + name : "  " + name;
                GUI.Label(label, text, isActive ? _activeItemStyle : _itemStyle);

                // Click
                if (hovered && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    bool recursive = rule?.recursive ?? true;
                    settings.AddRule(new FolderColorRule(_folderName, color, recursive));
                    Event.current.Use();
                    Close();
                    GUIUtility.ExitGUI();
                }
            }

            DrawSeparator();

            // Recursive toggle
            if (rule != null)
            {
                EditorGUI.BeginChangeCheck();
                bool rec = EditorGUILayout.ToggleLeft("Recursive", rule.recursive, GUILayout.Height(k_RowHeight));
                if (EditorGUI.EndChangeCheck())
                {
                    rule.recursive = rec;
                    settings.SaveAndNotify();
                }

                if (GUILayout.Button("Remove Rule", GUILayout.Height(k_RowHeight)))
                {
                    settings.RemoveRule(_folderName);
                    Close();
                    GUIUtility.ExitGUI();
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ToggleLeft("Recursive", true, GUILayout.Height(k_RowHeight));
                GUILayout.Button("Remove Rule", GUILayout.Height(k_RowHeight));
                EditorGUI.EndDisabledGroup();
            }

            DrawSeparator();

            if (GUILayout.Button("Settings...", GUILayout.Height(k_RowHeight)))
            {
                ColoredFoldersSettingsWindow.ShowWindow();
                Close();
                GUIUtility.ExitGUI();
            }
        }

        static void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        }
    }
}
