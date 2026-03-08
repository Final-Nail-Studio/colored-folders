using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FinalNailStudio.ColoredFolders
{
    [Serializable]
    public class ColoredFoldersSettings
    {
        const string k_SettingsPath = "ProjectSettings/ColoredFoldersSettings.json";

        [SerializeField] List<FolderColorRule> rules = new List<FolderColorRule>();

        [SerializeField] float gradientLeftAlpha = 0.01f;
        [SerializeField] float gradientRightAlpha = 1.0f;
        [SerializeField] float selectedAlphaMultiplier = 0.55f;
        [SerializeField] bool colorFolderLabels;

        public float GradientLeftAlpha
        {
            get => gradientLeftAlpha;
            set => gradientLeftAlpha = Mathf.Clamp01(value);
        }

        public float GradientRightAlpha
        {
            get => gradientRightAlpha;
            set => gradientRightAlpha = Mathf.Clamp01(value);
        }

        public float SelectedAlphaMultiplier
        {
            get => selectedAlphaMultiplier;
            set => selectedAlphaMultiplier = Mathf.Clamp01(value);
        }

        public bool ColorFolderLabels
        {
            get => colorFolderLabels;
            set => colorFolderLabels = value;
        }

        static ColoredFoldersSettings s_Instance;

        public static ColoredFoldersSettings Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = Load();
                return s_Instance;
            }
        }

        public IReadOnlyList<FolderColorRule> Rules => rules;

        public static event Action SettingsChanged;

        public void AddRule(FolderColorRule rule)
        {
            if (!rule.IsValid()) return;

            string normalized = rule.GetNormalizedName();
            rules.RemoveAll(r => r.GetNormalizedName() == normalized);
            rules.Add(rule);
            SaveAndNotify();
        }

        public void RemoveRule(string folderName)
        {
            string normalized = folderName?.Trim().ToLowerInvariant() ?? "";
            if (rules.RemoveAll(r => r.GetNormalizedName() == normalized) > 0)
                SaveAndNotify();
        }

        public FolderColorRule FindRule(string folderName)
        {
            string normalized = folderName?.Trim().ToLowerInvariant() ?? "";
            return rules.Find(r => r.GetNormalizedName() == normalized);
        }

        public void SetRules(List<FolderColorRule> newRules)
        {
            rules = newRules ?? new List<FolderColorRule>();
            rules.RemoveAll(r => !r.IsValid());
            SaveAndNotify();
        }

        public void ClearAllRules()
        {
            rules.Clear();
            SaveAndNotify();
        }

        public void SaveAndNotify()
        {
            Save();
            FolderColorResolver.ClearCache();
            SettingsChanged?.Invoke();
            EditorApplication.RepaintProjectWindow();
        }

        void Save()
        {
            var json = JsonUtility.ToJson(this, true);
            File.WriteAllText(k_SettingsPath, json);
        }

        static ColoredFoldersSettings Load()
        {
            if (File.Exists(k_SettingsPath))
            {
                try
                {
                    var json = File.ReadAllText(k_SettingsPath);
                    var settings = JsonUtility.FromJson<ColoredFoldersSettings>(json);
                    if (settings != null) return settings;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ColoredFolders] Failed to load settings: {e.Message}");
                }
            }
            return new ColoredFoldersSettings();
        }
    }
}
