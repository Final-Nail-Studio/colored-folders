using System;
using UnityEngine;

namespace FinalNailStudio.ColoredFolders
{
    [Serializable]
    public class FolderColorRule
    {
        public string folderName;
        public string colorHex;
        public bool recursive;

        public FolderColorRule() { }

        public FolderColorRule(string folderName, Color color, bool recursive = true)
        {
            this.folderName = folderName;
            this.colorHex = ColorUtility.ToHtmlStringRGBA(color);
            this.recursive = recursive;
        }

        public Color GetColor()
        {
            if (ColorUtility.TryParseHtmlString("#" + colorHex, out var color))
                return color;
            return Color.white;
        }

        public void SetColor(Color color)
        {
            colorHex = ColorUtility.ToHtmlStringRGBA(color);
        }

        public string GetNormalizedName()
        {
            return folderName?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(folderName);
        }
    }
}
