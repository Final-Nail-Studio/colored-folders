using UnityEngine;
using UnityEditor;

namespace FinalNailStudio.ColoredFolders
{
    [InitializeOnLoad]
    static class ProjectWindowDrawer
    {
        const int k_GradientWidth = 256;
        const float k_ListRowMaxHeight = 20f;
        const float k_ListModeIconPadding = 3f; // k_ListModeExternalIconPadding / 2

        static Texture2D s_GradientTex;
        static Texture2D s_SelectedGradientTex;
        static GUIStyle s_TextLabelStyle;

        static ProjectWindowDrawer()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

            EditorApplication.projectChanged -= FolderColorResolver.ClearCache;
            EditorApplication.projectChanged += FolderColorResolver.ClearCache;

            ColoredFoldersSettings.SettingsChanged -= OnSettingsChanged;
            ColoredFoldersSettings.SettingsChanged += OnSettingsChanged;

            RebuildTextures();
        }

        static void OnSettingsChanged()
        {
            s_TextLabelStyle = null;
            RebuildTextures();
        }

        static void RebuildTextures()
        {
            var settings = ColoredFoldersSettings.Instance;
            float left = settings.GradientLeftAlpha;
            float right = settings.GradientRightAlpha;
            float selMul = settings.SelectedAlphaMultiplier;

            s_GradientTex = CreateGradientTexture(left, right);
            s_SelectedGradientTex = CreateGradientTexture(left * selMul, right * selMul);
        }

        static Texture2D CreateGradientTexture(float leftAlpha, float rightAlpha)
        {
            var tex = new Texture2D(k_GradientWidth, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };

            var pixels = new Color[k_GradientWidth];
            for (int x = 0; x < k_GradientWidth; x++)
            {
                float t = (float)x / (k_GradientWidth - 1);
                pixels[x] = new Color(1f, 1f, 1f, Mathf.Lerp(leftAlpha, rightAlpha, t));
            }
            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return tex;
        }

        // Tree view items land at x = 16 + n*14 (baseIndent 2 + foldout 14 + depth*14).
        // Flat list items do not follow this pattern.
        static bool IsTreeView(Rect rect)
        {
            return ((int)(rect.x - 16)) % 14 == 0;
        }

        static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            if (Event.current.type != EventType.Repaint) return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return;
            if (!PathUtility.IsAssetsPath(path)) return;
            if (!AssetDatabase.IsValidFolder(path)) return;

            if (!FolderColorResolver.TryResolve(path, out var color))
                return;

            if (s_GradientTex == null || s_SelectedGradientTex == null)
                RebuildTextures();

            var settings = ColoredFoldersSettings.Instance;
            bool isSelected = System.Array.IndexOf(Selection.assetGUIDs, guid) >= 0;
            float rightAlpha = isSelected
                ? settings.GradientRightAlpha * settings.SelectedAlphaMultiplier
                : settings.GradientRightAlpha;

            // Background gradient
            var gradTex = isSelected ? s_SelectedGradientTex : s_GradientTex;
            var prev = GUI.color;
            GUI.color = new Color(color.r, color.g, color.b, 1f);
            GUI.DrawTexture(selectionRect, gradTex);

            // Icon and label tinting (list view only)
            bool isListMode = selectionRect.height <= k_ListRowMaxHeight;
            var icon = AssetDatabase.GetCachedIcon(path);
            if (icon != null && isListMode)
            {
                bool isTree = IsTreeView(selectionRect);
                float iconPad = isTree ? 0f : k_ListModeIconPadding;

                // Colored label text — covers Unity's original text with an
                // opaque background, redraws the gradient with correct UVs,
                // then draws colored text without an icon (no duplicate).
                if (settings.ColorFolderLabels)
                {
                    Rect textRect = selectionRect;
                    textRect.xMin = selectionRect.x + selectionRect.height + iconPad;

                    // Cover Unity's original text with the appropriate bg color
                    Color bg;
                    if (isSelected)
                        bg = EditorGUIUtility.isProSkin
                            ? new Color(0.19f, 0.39f, 0.57f, 1f)
                            : new Color(0.24f, 0.48f, 0.90f, 1f);
                    else
                        bg = EditorGUIUtility.isProSkin
                            ? new Color(0.22f, 0.22f, 0.22f, 1f)
                            : new Color(0.76f, 0.76f, 0.76f, 1f);

                    EditorGUI.DrawRect(textRect, bg);

                    // Re-draw gradient with UV coords that match the row-wide gradient
                    float uvStart = (textRect.xMin - selectionRect.xMin) / selectionRect.width;
                    GUI.color = new Color(color.r, color.g, color.b, 1f);
                    GUI.DrawTextureWithTexCoords(textRect, gradTex,
                        new Rect(uvStart, 0f, 1f - uvStart, 1f));

                    // Draw colored text (no icon in content)
                    if (s_TextLabelStyle == null)
                        s_TextLabelStyle = new GUIStyle(EditorStyles.label);
                    s_TextLabelStyle.normal.textColor = new Color(color.r, color.g, color.b, 1f);
                    GUI.color = Color.white;
                    GUI.Label(textRect, PathUtility.GetFolderName(path), s_TextLabelStyle);
                }

                // Tinted icon overlay — drawn last on top of everything.
                // Make the rect square (width = height) to match Unity's 16x16 icon area.
                Rect iconRect = selectionRect;
                iconRect.width = iconRect.height;
                iconRect.x += iconPad;

                GUI.color = new Color(color.r, color.g, color.b, rightAlpha);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            }

            GUI.color = prev;
        }
    }
}
