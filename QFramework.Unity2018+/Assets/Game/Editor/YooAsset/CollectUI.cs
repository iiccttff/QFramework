
using System.Collections.Generic;
using System.IO;
using YooAsset.Editor;

public class CollectUI : IFilterRule
{
    private readonly HashSet<string> _validExtensions = new HashSet<string> { ".anim", ".spriteatlas", ".prefab", ".png", ".asset"};

    public bool IsCollectAsset(FilterRuleData data)
    {
        string extension = Path.GetExtension(data.AssetPath).ToLower();
        return _validExtensions.Contains(extension);
    }
}