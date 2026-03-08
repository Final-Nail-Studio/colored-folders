using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FinalNailStudio.ColoredFolders
{
    [Serializable]
    public class FolderColorPreset
    {
        public string presetName;
        public List<FolderColorRule> rules = new List<FolderColorRule>();
    }

    public static class FolderColorPresetIO
    {
        public static bool Export(string filePath, string presetName, IReadOnlyList<FolderColorRule> rules)
        {
            try
            {
                var preset = new FolderColorPreset
                {
                    presetName = presetName,
                    rules = new List<FolderColorRule>(rules)
                };
                var json = JsonUtility.ToJson(preset, true);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ColoredFolders] Export failed: {e.Message}");
                return false;
            }
        }

        public static List<FolderColorRule> Import(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var preset = JsonUtility.FromJson<FolderColorPreset>(json);
                if (preset?.rules == null)
                {
                    Debug.LogWarning("[ColoredFolders] Import failed: invalid preset format.");
                    return null;
                }
                preset.rules.RemoveAll(r => !r.IsValid());
                return preset.rules;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ColoredFolders] Import failed: {e.Message}");
                return null;
            }
        }
    }
}
