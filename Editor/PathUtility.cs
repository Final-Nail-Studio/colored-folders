namespace FinalNailStudio.ColoredFolders
{
    public static class PathUtility
    {
        public static string GetFolderName(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return string.Empty;
            int lastSlash = assetPath.LastIndexOf('/');
            return lastSlash >= 0 ? assetPath.Substring(lastSlash + 1) : assetPath;
        }

        public static string GetParentPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return string.Empty;
            int lastSlash = assetPath.LastIndexOf('/');
            return lastSlash > 0 ? assetPath.Substring(0, lastSlash) : string.Empty;
        }

        public static bool IsAssetsPath(string assetPath)
        {
            return assetPath == "Assets" || assetPath.StartsWith("Assets/");
        }
    }
}
