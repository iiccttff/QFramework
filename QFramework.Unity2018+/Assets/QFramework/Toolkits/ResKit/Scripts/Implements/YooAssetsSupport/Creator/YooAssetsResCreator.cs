namespace QFramework
{
    public class YooAssetsResCreator : IResCreator
    {
        public bool Match(ResSearchKeys resSearchKeys)
        {
            return resSearchKeys.AssetName.StartsWith(YooAssetsConst.YooAssetPrefix);
        }

        public IRes Create(ResSearchKeys resSearchKeys)
        {
            return YooAssetsRes.Allocate(resSearchKeys.AssetName, resSearchKeys.OriginalAssetName);
        }
    }
}