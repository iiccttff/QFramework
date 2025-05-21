using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset.Editor;

public class CollectCharacter : IFilterRule
{
    private readonly HashSet<string> _validExtensions = new HashSet<string> { ".controller", ".anim", ".prefab", ".fbx", ".mat" };

    public bool IsCollectAsset(FilterRuleData data)
    {
        string extension = Path.GetExtension(data.AssetPath).ToLower();
        return _validExtensions.Contains(extension);
    }
}
