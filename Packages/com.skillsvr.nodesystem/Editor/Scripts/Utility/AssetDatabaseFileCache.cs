using SkillsVRNodes.Managers.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Utility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class AssetDatabaseAdder : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        List<Type> typesToRefresh = new();
        
        foreach (string assetPath in importedAssets)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object asset in assets)
            {
                if (asset == null || asset is SceneAsset)
                {
                    continue;
                }
                if (AssetDatabaseFileCache.GetAllTypes().Any(x => x == asset.GetType()))
                {
                    typesToRefresh.Add(asset.GetType());
                }
                
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                AssetDatabaseFileCache.AddRefrence(asset, assetPath, guid);
            }
        }

        foreach (string asset in deletedAssets)
        {
            AssetDatabaseFileCache.RemoveRefrence(asset);
        }

        for (int index = 0; index < movedFromAssetPaths.Length; index++)
        {
            AssetDatabaseFileCache.RemoveRefrence(movedFromAssetPaths[index]);
            string assetPath = movedAssets[index];
            
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var asset in assets)
            {
                if (asset == null)
                {
                    continue;
                }
                
                if (AssetDatabaseFileCache.GetAllTypes().Any(x => x == asset.GetType()))
                {
                    typesToRefresh.Add(asset.GetType());
                }
                
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                AssetDatabaseFileCache.AddRefrence(asset, assetPath, guid);
            }
        }

        foreach (Type type in typesToRefresh)
        {
            AssetDatabaseFileCache.RegenCacheType(type);
        }
        
        AssetDatabaseFileCache.OnDatabaseChanged?.Invoke();
    }
}

public static class AssetDatabaseFileCache
{
    public static Action OnDatabaseChanged;
    
    private static readonly Dictionary<Type, List<ObjectReference>> AllAssetReferences = new();
    private static readonly Dictionary<Type, List<ObjectReference>> ProjectAssetReferences = new();
    public static Dictionary<Type, List<ObjectReference>> GetAllAssetReferences => AllAssetReferences;
    public static Dictionary<Type, List<ObjectReference>> GetProjectAssetReferences => ProjectAssetReferences;

    public static List<ObjectReference> AllAssetReferencesList() => AllAssetReferences.Values.SelectMany(x => x).ToList();
    
    // Assets types that should not be cached
    private static readonly List<Type> blacklistTypes = new()
    {
        typeof(DefaultAsset),
        typeof(MonoScript)
    };
    
    // Assets types that if they are in a package, they should not be cached (there are often many of these)
    private static readonly List<Type> PackageBlacklistTypes = new()
    {
        typeof(Texture2D),
        typeof(Material),
        typeof(Shader),
        typeof(AudioClip),
        typeof(TextAsset),
        typeof(Font),
        typeof(Sprite),
    };
    
    /// <summary>
    /// Resets and precaches the All Cache
    /// </summary>
    [InitializeOnLoadMethod]
    [MenuItem("Tools/Asset Database File Cache/Reset All Cache")]
    public static void PreCache()
    {
        AllAssetReferences.Clear();
        ProjectAssetReferences.Clear();
        
        ValidateCache<Texture2D>();
        ValidateCache<AudioClip>();
        ValidateCache<Sprite>();
        ValidateCache<Animation>();

        OnDatabaseChanged?.Invoke();
    }

    /// <summary>
    /// Done When Switching Projects
    /// </summary>
    [MenuItem("Tools/Asset Database File Cache/Reset Project Cache")]
    public static void ClearAllProjectData()
    {
        ProjectAssetReferences.Clear();
    }
    
    /// <summary>
    /// Adds a reference to the cache to save on asset database calls
    /// </summary>
    /// <param name="asset">The added asset</param>
    /// <typeparam name="TObject">The type of added asset</typeparam>
    public static void AddRefrence(Object asset)
    {
        string path = AssetDatabase.GetAssetPath(asset);
        string guid = AssetDatabase.AssetPathToGUID(path);
        
        AddRefrence(asset, path, guid);
    }

    public static void RemoveRefrence(string assetPath)
    {
        var objectReferences = GetObjectRefrenceFromPath(assetPath);
        foreach (var objectReference in objectReferences)
        {
            if (objectReference != null)
            {
                RemoveRefrence(objectReference);
            }
        }
    }

    /// <summary>
    /// Returns references to asset at path
    /// </summary>
    /// <remarks>May return many assets as an FBX can include animation clips, models and materials</remarks>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    public static IEnumerable<ObjectReference> GetObjectRefrenceFromPath(string assetPath)
    {
        return AllAssetReferencesList().Where(t => t.Path == assetPath);
    }

    public static void RemoveRefrence(ObjectReference reference)
    {
        foreach (List<ObjectReference> objects in AllAssetReferences.Values)
        {
            objects.Remove(reference);
        }
        foreach (List<ObjectReference> objects in ProjectAssetReferences.Values)
        {
            objects.Remove(reference);
        }
    }

    public static void AddRefrence(Object asset, string path, string guid)
    {
        string attachedProject = "";

        if (path.IsNullOrWhitespace() || guid.IsNullOrWhitespace())
        {
            return;
        }

        var pathList = path.Split('/');
        if (pathList.IsNullOrEmpty())
        {
            return;
        }

        // If the asset is in a project
        if (pathList.First() != "Packages")
        {
            // If the Asset is not in a project
            if (pathList.Length <= 3 || pathList[1] != "Contexts")
            {
                return;
            }
            
            attachedProject = pathList[2];
        }

        
            
        Type type = asset.GetType();
        
        if (AllAssetReferences.ContainsKey(type) == false)
        {
            AllAssetReferences.Add(type, new List<ObjectReference>());
        }

        if (AllAssetReferences[type].Any(x => x.GUID == guid))
        {
            return;
        }

        ObjectReference reference = new()
        {
            GUID = guid,
            Path = path,
            Name = asset.name,
            attachedProject = attachedProject,
            labels = AssetDatabase.GetLabels(asset).ToList()
        };
        AllAssetReferences[type].Add(reference);
        
        if (ShouldBeCached(type, reference))
        {
            if (ProjectAssetReferences.ContainsKey(type) == false)
            {
                ProjectAssetReferences.Add(type, new List<ObjectReference>());
            }
            ProjectAssetReferences[type].Add(reference);
        }
    }

    public static string GetCurrentMainGraphName()
	{
        return GraphFinder.CurrentActiveProject == null ? "MissingGraph" : GraphFinder.CurrentActiveProject.GetProjectName;
    }

    public static List<Type> GetAllTypes()
    {
        return AllAssetReferences.Keys.ToList();
    }
    
    public static void SwitchProject(string projectGraphsName)
    {
        ProjectAssetReferences.Clear();
    }
    
    /// <summary>
    /// Call this for both first Gen or Regen of the cache of this type
    /// </summary>
    /// <typeparam name="TObject">The asset type to cache</typeparam>
    public static void RegenCacheType<TObject>() where TObject : Object
    {
        Type type = typeof(TObject);
        RegenCacheType(type);
    }
    
    public static void RegenCacheType(Type type)
    {
        if (AllAssetReferences.ContainsKey(type))
        {
            AllAssetReferences.Remove(type);
        }
        if (ProjectAssetReferences.ContainsKey(type))
        {
            ProjectAssetReferences.Remove(type);
        }
        
        AllAssetReferences.Add(type, new List<ObjectReference>());
        ProjectAssetReferences.Add(type, new List<ObjectReference>());

        string[] guidList = AssetDatabase.FindAssets($"t:{type.Name}");

        foreach (string guid in guidList)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            if (PackageBlacklistTypes.Contains(type) && path.StartsWith("Packages", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            AddRefrence(AssetDatabase.LoadAssetAtPath(path, type));
        }
    }

    public static List<string> GetAssetGUIDsOfType<TObject>(string assetTags = null) where TObject : Object
    {
        Type type = typeof(TObject);
        ValidateCache<TObject>();

        if (!assetTags.IsNullOrWhitespace())
        {
            return ProjectAssetReferences[type].Where(x => x.labels.Contains(assetTags)).Select(x => x.GUID).ToList();
        }
        
        return ProjectAssetReferences[type].Select(x => x.GUID).ToList();
    }

    public static List<string> GetAssetPathsOfType<TObject>(string assetTags = null) where TObject : Object
    {
        Type type = typeof(TObject);
        ValidateCache<TObject>();

        if (!assetTags.IsNullOrWhitespace())
        {
            return ProjectAssetReferences[type].Where(x => x.labels.Contains(assetTags)).Select(x => x.Path).ToList();
        }
        
        return ProjectAssetReferences[type].Select(x => x.Path).ToList();
    }
    
    private static void ValidateCache(Type type)
    {
        if (!AllAssetReferences.ContainsKey(type))
        {
            RegenCacheType(type);
        }
        else if (!ProjectAssetReferences.ContainsKey(type))
        {
            CopyCache(type);
        }
    }

    private static void ValidateCache<TObject>() where TObject : Object
    {
        Type type = typeof(TObject);
        if (!AllAssetReferences.ContainsKey(type))
        {
            RegenCacheType<TObject>();
        }
        else if (!ProjectAssetReferences.ContainsKey(type))
        {
            CopyCache(type);
        }
    }

    private static void CopyCache(Type type)
    {
        if (ProjectAssetReferences.ContainsKey(type))
        {
            ProjectAssetReferences.Remove(type);
        }
        ProjectAssetReferences.Add(type, new List<ObjectReference>());
        foreach (ObjectReference reference in AllAssetReferences[type])
        {
            if (ShouldBeCached(type, reference))
            {
                ProjectAssetReferences[type].Add(reference);
            }
        }
    }

    private static bool ShouldBeCached(Type type, ObjectReference reference)
    {
        return reference.IsProject || (reference.IsPackage && !PackageBlacklistTypes.Contains(type));
    }

    public static IEnumerable<TObject> GetAllObjects<TObject>() where TObject : Object
    {
        ValidateCache<TObject>();
        foreach (var asset in ProjectAssetReferences[typeof(TObject)])
        {
            yield return AssetDatabase.LoadAssetAtPath<TObject>(asset.Path);
        }
    }

    public static List<ObjectReference> GetAssetReferencesFromProject(List<Type> types, GraphProjectData project = null)
    {
        List<ObjectReference> objectReferences = new ();
        
        foreach (var type in types)
        {
            if (type == null)
            {
                continue;
            }
            objectReferences.AddRange(GetAssetReferencesFromProject(type, project));
        }
        return objectReferences;
    }
    
    public static List<ObjectReference> GetAssetReferencesFromProject(Type type, GraphProjectData project = null)
    {
        ValidateCache(type);
        if (type == null)
        {
            List<ObjectReference> objectReferences = AllAssetReferences.Values.SelectMany(x => x).ToList();
            if (project == null)
            {
                return objectReferences;
            }
            else
            {
                return objectReferences.Where(x => x.attachedProject == project.GetProjectName).ToList();
            }
        }
        
        if (!AllAssetReferences.TryGetValue(type, out List<ObjectReference> fromProject))
        {
            return new List<ObjectReference>();
        }
        if (project == null)
        {
            return fromProject;
        }
        return AllAssetReferences[type].Where(x => x.attachedProject == project.GetProjectName).ToList();
    }
}

