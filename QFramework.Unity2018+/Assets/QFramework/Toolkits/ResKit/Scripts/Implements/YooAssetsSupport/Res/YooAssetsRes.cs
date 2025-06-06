using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace QFramework
{
    public class YooAssetsRes : Res
    {
        private string _location;
        private AssetHandle _assetHandle;
        private const string PackageName = "DefaultPackage";
        
        public static YooAssetsRes Allocate(string name, string originalAssetName)
        {
            var res = SafeObjectPool<YooAssetsRes>.Instance.Allocate();
            if (res == null) return null;
            
            if (originalAssetName.StartsWith(YooAssetsConst.YooAssetPrefix))
            {
                originalAssetName = originalAssetName.Substring(YooAssetsConst.YooAssetPrefix.Length);
            }
            
            res.AssetName = name;
            res.AssetType = typeof(AssetHandle);
            res._location = originalAssetName;

            return res;
        }

        public override bool LoadSync()
        {
            var pg = YooAssets.GetPackage(PackageName);
            var syncOperationHandle = pg.LoadAssetSync<Object>(_location);
            mAsset = syncOperationHandle.AssetObject;
            State = ResState.Ready;
            return true;
        }

        public override void LoadAsync()
        {
            State = ResState.Loading;
            LoadAsyncByUniTask().Forget();
        }

        private async UniTask LoadAsyncByUniTask()
        {
            var pg = YooAssets.GetPackage(PackageName);
            _assetHandle = pg.LoadAssetAsync<Object>(_location);
            await _assetHandle;
            mAsset = _assetHandle.AssetObject;

            State = ResState.Ready;
        }

        protected override void OnReleaseRes()
        {
            mAsset = null;
            _assetHandle?.Release();
            _assetHandle = null;
        }
        
        public override void Recycle2Cache()
        {
            SafeObjectPool<YooAssetsRes>.Instance.Recycle(this);
        }
        
        public override string ToString()
        {
            return $"Type:YooAssets\t {base.ToString()}";
        }
    }
}