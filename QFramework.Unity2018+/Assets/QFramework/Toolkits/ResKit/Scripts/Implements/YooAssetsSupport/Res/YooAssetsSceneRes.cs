using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace QFramework
{
    public class YooAssetsSceneRes : Res
    {
        private SceneHandle _sceneHandle;
        private const string PackageName = "DefaultPackage";
        private string _location;
        private LoadSceneMode _sceneMode;
        
        public static YooAssetsSceneRes Allocate(string name, string originalAssetName, LoadSceneMode sceneMode = LoadSceneMode.Single)
        {
            var res = SafeObjectPool<YooAssetsSceneRes>.Instance.Allocate();
            if (res == null) return null;
            

            
            res.AssetName = name;
            res.AssetType = typeof(SceneHandle);
            res._location = originalAssetName;
            res._sceneMode = sceneMode;

            return res;
        }
        
        public override bool LoadSync()
        {
            var pg = YooAssets.GetPackage(PackageName);
            _sceneHandle = pg.LoadSceneSync(_location, _sceneMode);
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
            _sceneHandle = pg.LoadSceneAsync(_location, _sceneMode);
            await _sceneHandle;
            
            State = ResState.Ready;
        }
        
        protected override void OnReleaseRes()
        {
            ReleaseResAsync().Forget();
        }

        private async UniTask ReleaseResAsync()
        {
            if (_sceneHandle != null)
            {
                await _sceneHandle.UnloadAsync();
            }
            _sceneHandle = null;
            mAsset = null;
        }
        
        public override void Recycle2Cache()
        {
            SafeObjectPool<YooAssetsSceneRes>.Instance.Recycle(this);
        }
        
        public override string ToString()
        {
            return $"Type:YooAssets\t {base.ToString()}";
        }
    }
}