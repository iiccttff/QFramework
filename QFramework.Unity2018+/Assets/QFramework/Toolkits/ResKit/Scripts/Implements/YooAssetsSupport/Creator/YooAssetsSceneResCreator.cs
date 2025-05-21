using UnityEngine.SceneManagement;

namespace QFramework
{
    public class YooAssetsSceneResCreator : IResCreator
    {
        public bool Match(ResSearchKeys resSearchKeys)
        {
            return resSearchKeys.AssetName.StartsWith(YooAssetsConst.YooScenePrefix);
        }

        public IRes Create(ResSearchKeys resSearchKeys)
        {
            string originalAssetName = resSearchKeys.OriginalAssetName;
            LoadSceneMode sceneMode = LoadSceneMode.Single;
            
            if (originalAssetName.StartsWith(YooAssetsConst.YooSceneSinglePrefix))
            {
                originalAssetName = originalAssetName.Substring(YooAssetsConst.YooSceneSinglePrefix.Length);
                sceneMode = LoadSceneMode.Single;
            }
            
            if (originalAssetName.StartsWith(YooAssetsConst.YooSceneAdditivePrefix))
            {
                originalAssetName = originalAssetName.Substring(YooAssetsConst.YooSceneAdditivePrefix.Length);
                sceneMode = LoadSceneMode.Additive;
            }
            
            return YooAssetsSceneRes.Allocate(resSearchKeys.AssetName, originalAssetName, sceneMode);
        }
    }
}