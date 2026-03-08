using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FinalNailStudio.ColoredFolders
{
    public class ColoredFoldersSettingsWindow : EditorWindow
    {
        Vector2 _scrollPos;
        string _newName = "";
        Color _newColor = new Color(0.3f, 0.6f, 1f);
        bool _newRecursive = true;

        [MenuItem("Tools/Colored Folders/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<ColoredFoldersSettingsWindow>("Colored Folders");
            window.minSize = new Vector2(420, 300);
        }

        void OnEnable()
        {
            ColoredFoldersSettings.SettingsChanged += Repaint;
        }

        void OnDisable()
        {
            ColoredFoldersSettings.SettingsChanged -= Repaint;
        }

        void OnGUI()
        {
            var settings = ColoredFoldersSettings.Instance;
            var rules = settings.Rules;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField($"Rules ({rules.Count})", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            bool changed = false;
            int removeIndex = -1;

            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                EditorGUILayout.BeginHorizontal();

                // Name (read-only label)
                EditorGUILayout.LabelField(rule.folderName, GUILayout.MinWidth(80));

                // Color swatch
                EditorGUI.BeginChangeCheck();
                Color c = EditorGUILayout.ColorField(GUIContent.none, rule.GetColor(),
                    false, false, false, GUILayout.Width(50));
                if (EditorGUI.EndChangeCheck())
                {
                    rule.SetColor(c);
                    changed = true;
                }

                // Recursive toggle
                EditorGUI.BeginChangeCheck();
                bool rec = EditorGUILayout.ToggleLeft("Recursive", rule.recursive, GUILayout.Width(82));
                if (EditorGUI.EndChangeCheck())
                {
                    rule.recursive = rec;
                    changed = true;
                }

                // Remove button
                if (GUILayout.Button("\u00d7", GUILayout.Width(24)))
                    removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            if (rules.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No rules defined. Alt + Right Click a folder in the Project window to add one, or add one below.",
                    MessageType.Info);
            }

            EditorGUILayout.EndScrollView();

            if (removeIndex >= 0)
            {
                settings.RemoveRule(rules[removeIndex].folderName);
            }
            else if (changed)
            {
                settings.SaveAndNotify();
            }

            // Add new rule
            EditorGUILayout.Space(8);
            DrawSeparator();
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Add Rule", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _newName = EditorGUILayout.TextField(_newName, GUILayout.MinWidth(80));
            _newColor = EditorGUILayout.ColorField(GUIContent.none, _newColor,
                false, false, false, GUILayout.Width(50));
            _newRecursive = EditorGUILayout.ToggleLeft("Recursive", _newRecursive, GUILayout.Width(82));

            EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_newName));
            if (GUILayout.Button("Add", GUILayout.Width(40)))
            {
                settings.AddRule(new FolderColorRule(_newName.Trim(), _newColor, _newRecursive));
                _newName = "";
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Gradient
            EditorGUILayout.Space(12);
            DrawSeparator();
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Gradient", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            float left = EditorGUILayout.Slider("Left Alpha", settings.GradientLeftAlpha, 0f, 1f);
            float right = EditorGUILayout.Slider("Right Alpha", settings.GradientRightAlpha, 0f, 1f);
            float selMul = EditorGUILayout.Slider("Selected Multiplier", settings.SelectedAlphaMultiplier, 0f, 1f);
            bool colorLabels = EditorGUILayout.ToggleLeft("Color Folder Labels", settings.ColorFolderLabels);
            if (EditorGUI.EndChangeCheck())
            {
                settings.GradientLeftAlpha = left;
                settings.GradientRightAlpha = right;
                settings.SelectedAlphaMultiplier = selMul;
                settings.ColorFolderLabels = colorLabels;
                settings.SaveAndNotify();
            }

            // Presets
            EditorGUILayout.Space(12);
            DrawSeparator();
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Preset"))
            {
                string path = EditorUtility.OpenFilePanel("Load Preset", "", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    var imported = FolderColorPresetIO.Import(path);
                    if (imported != null)
                        settings.SetRules(imported);
                    else
                        EditorUtility.DisplayDialog("Import Failed",
                            "Could not read the preset file. Check the console for details.", "OK");
                }
            }

            if (GUILayout.Button("Save Preset"))
            {
                string path = EditorUtility.SaveFilePanel("Save Preset", "", "ColoredFoldersPreset", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    if (!FolderColorPresetIO.Export(path, "Custom", settings.Rules))
                        EditorUtility.DisplayDialog("Export Failed",
                            "Could not write the preset file. Check the console for details.", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.55f, 0.55f);
            if (GUILayout.Button("Clear All Rules"))
            {
                if (EditorUtility.DisplayDialog("Clear All Rules",
                    "Remove all folder color rules?", "Clear", "Cancel"))
                {
                    settings.ClearAllRules();
                }
            }
            GUI.backgroundColor = prevColor;

            EditorGUILayout.Space(4);
        }

        static void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }
    }
}
