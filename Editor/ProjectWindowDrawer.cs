using UnityEngine;
using UnityEditor;

namespace FinalNailStudio.ColoredFolders
{
    [InitializeOnLoad]
    static class ProjectWindowDrawer
    {
        const int k_GradientWidth = 256;
        const float k_ListRowMaxHeight = 20f;

        static Texture2D s_GradientTex;
        static Texture2D s_SelectedGradientTex;
        static GUIStyle s_LabelStyle;

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
            s_LabelStyle = null;
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

        static void EnsureLabelStyle()
        {
            if (s_LabelStyle != null) return;
            s_LabelStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
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

            // Icon tint
            var icon = AssetDatabase.GetCachedIcon(path);
            if (icon != null)
            {
                bool listView = selectionRect.height <= k_ListRowMaxHeight;
                Rect iconRect;

                if (listView)
                {
                    float h = selectionRect.height;
                    iconRect = new Rect(selectionRect.x, selectionRect.y, h, h);
                }
                else
                {
                    // Grid/icon view: icon centered in upper portion, label at bottom
                    float labelHeight = 16f;
                    float iconArea = selectionRect.height - labelHeight;
                    float iconSize = Mathf.Min(selectionRect.width, iconArea);
                    float iconX = selectionRect.x + (selectionRect.width - iconSize) * 0.5f;
                    iconRect = new Rect(iconX, selectionRect.y, iconSize, iconSize);
                }

                GUI.color = new Color(color.r, color.g, color.b, rightAlpha);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            }

            // Label text coloring
            if (settings.ColorFolderLabels && selectionRect.height <= k_ListRowMaxHeight)
            {
                EnsureLabelStyle();
                float h = selectionRect.height;
                var labelRect = new Rect(selectionRect.x + h + 2f, selectionRect.y,
                    selectionRect.width - h - 2f, h);

                s_LabelStyle.normal.textColor = new Color(color.r, color.g, color.b, 1f);
                GUI.Label(labelRect, PathUtility.GetFolderName(path), s_LabelStyle);
            }

            GUI.color = prev;
        }
    }
}
