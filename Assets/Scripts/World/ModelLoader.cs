using UnityEngine;

public class ModelLoader
{
    public static GameObject Load(string resourceName, string assetPath)
    {
        var fromResources = Resources.Load<GameObject>(resourceName);
        if (fromResources != null)
            return fromResources;

#if UNITY_EDITOR
        var fromAssets = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (fromAssets != null)
            return fromAssets;
#endif
        return null;
    }
}
