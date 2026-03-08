using System.Collections.Generic;
using UnityEngine;

namespace FinalNailStudio.ColoredFolders
{
    public static class FolderColorResolver
    {
        static readonly Dictionary<string, Color?> s_Cache = new Dictionary<string, Color?>();

        public static void ClearCache()
        {
            s_Cache.Clear();
        }

        public static bool TryResolve(string assetPath, out Color color)
        {
            color = default;
            if (string.IsNullOrEmpty(assetPath) || !PathUtility.IsAssetsPath(assetPath))
                return false;

            if (s_Cache.TryGetValue(assetPath, out var cached))
            {
                if (cached.HasValue)
                {
                    color = cached.Value;
                    return true;
                }
                return false;
            }

            bool found = ResolveInternal(assetPath, out color);
            s_Cache[assetPath] = found ? color : (Color?)null;
            return found;
        }

        static bool ResolveInternal(string assetPath, out Color color)
        {
            color = default;
            var settings = ColoredFoldersSettings.Instance;

            string folderName = PathUtility.GetFolderName(assetPath);
            if (string.IsNullOrEmpty(folderName))
                return false;

            // Direct name match takes priority
            var directRule = settings.FindRule(folderName);
            if (directRule != null)
            {
                color = directRule.GetColor();
                return true;
            }

            // Walk up parents, nearest recursive ancestor wins
            string parentPath = PathUtility.GetParentPath(assetPath);
            while (!string.IsNullOrEmpty(parentPath))
            {
                string parentName = PathUtility.GetFolderName(parentPath);
                var parentRule = settings.FindRule(parentName);
                if (parentRule != null && parentRule.recursive)
                {
                    color = parentRule.GetColor();
                    return true;
                }
                parentPath = PathUtility.GetParentPath(parentPath);
            }

            return false;
        }
    }
}
