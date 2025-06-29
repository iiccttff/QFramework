using System;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
using System.Threading;
using UnityEngine.SceneManagement;

public static class YooAssetsExtensions
{
    /*
     * 使用示例：
        ResLoader resLoader = ResLoader.Allocate();
        var asset = await resLoader.LoadAssetUniTask<T>("test");
        resLoader.Recycle2Cache();
     */
    public static async UniTask<T> LoadAssetUniTask<T>(this ResLoader resLoader, string assetPath, CancellationToken cancellationToken = default) where T : class
    {
        if (!assetPath.StartsWith(YooAssetsConst.YooAssetPrefix))
        {
            assetPath = $"{YooAssetsConst.YooAssetPrefix}{assetPath}";
        }

        var tcs = new UniTaskCompletionSource<T>();

        T obj = null;
        
        resLoader.Add2Load<T>(assetPath, (success, res) =>
        {
            obj = res.Asset as T;
        });

        resLoader.LoadAsync(() =>
        {
            tcs.TrySetResult(obj);
        }); // 确保回调执行

        // 处理取消情况
        try
        {
            return await tcs.Task.AttachExternalCancellation(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"[LoadAssetUniTask] Asset load canceled: {assetPath}");
            return null;
        }
    }
    
    /*
     * 使用示例：
        ResLoader resLoader = ResLoader.Allocate();
        await resLoader.LoadSceneUniTask("test");
        resLoader.Recycle2Cache();
     */
    public static async UniTask LoadSceneUniTask(this ResLoader resLoader, string sceneName, LoadSceneMode sceneMode = LoadSceneMode.Single, CancellationToken cancellationToken = default)
    {
        switch (sceneMode)
        {
            case LoadSceneMode.Single:
                if (!sceneName.StartsWith(YooAssetsConst.YooSceneSinglePrefix))
                {
                    sceneName = $"{YooAssetsConst.YooSceneSinglePrefix}{sceneName}";
                }
                break;
            case LoadSceneMode.Additive:
                if (!sceneName.StartsWith(YooAssetsConst.YooSceneAdditivePrefix))
                {
                    sceneName = $"{YooAssetsConst.YooSceneAdditivePrefix}{sceneName}";
                }
                break;
        }
        
        var tcs = new UniTaskCompletionSource<bool>();

        resLoader.Add2Load(sceneName, (success, res) => { tcs.TrySetResult(success); });

        resLoader.LoadAsync();

        // 处理取消情况
        try
        {
            await tcs.Task.AttachExternalCancellation(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"[LoadSceneUniTask] Scene load canceled: {sceneName}");
            return;
        }
    }

}