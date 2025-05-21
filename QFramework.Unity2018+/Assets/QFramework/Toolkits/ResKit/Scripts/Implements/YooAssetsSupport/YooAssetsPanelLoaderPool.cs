using System;
using UnityEngine;

namespace QFramework
{
    public class YooAssetsPanelLoaderPool : AbstractPanelLoaderPool
    {
        public class QFYooAssetsPanelLoader : IPanelLoader
        {
            private ResLoader mResLoader;

            public GameObject LoadPanelPrefab(PanelSearchKeys panelSearchKeys)
            {
                if (mResLoader == null)
                {
                    mResLoader = ResLoader.Allocate();
                }

                if (panelSearchKeys.PanelType.IsNotNull() && panelSearchKeys.GameObjName.IsNullOrEmpty())
                {
                    //return mResLoader.LoadSync<GameObject>($"yoo:Assets/{panelSearchKeys.PanelType.Name}.prefab");
                    return mResLoader.LoadSync<GameObject>($"{YooAssetsConst.YooAssetPrefix}{panelSearchKeys.PanelType.Name}");
                }

                // return mResLoader.LoadSync<GameObject>($"yoo:Assets/{panelSearchKeys.GameObjName}.prefab");
                return mResLoader.LoadSync<GameObject>($"{YooAssetsConst.YooAssetPrefix}{panelSearchKeys.GameObjName}");
            }

            public void LoadPanelPrefabAsync(PanelSearchKeys panelSearchKeys, Action<GameObject> onLoad)
            {
                if (mResLoader == null)
                {
                    mResLoader = ResLoader.Allocate();
                }

                if (panelSearchKeys.PanelType.IsNotNull() && panelSearchKeys.GameObjName.IsNullOrEmpty())
                {
                    mResLoader.Add2Load<GameObject>($"{YooAssetsConst.YooAssetPrefix}{panelSearchKeys.PanelType.Name}", (success, res) =>
                    {
                        if (success)
                        {
                            onLoad(res.Asset as GameObject);
                        }
                    });
                    mResLoader.LoadAsync();
                    return;
                }

                mResLoader.Add2Load<GameObject>($"{YooAssetsConst.YooAssetPrefix}{panelSearchKeys.GameObjName}", (success, res) =>
                {
                    if (success)
                    {
                        onLoad(res.Asset as GameObject);
                    }
                });
                mResLoader.LoadAsync();
            }

            public void Unload()
            {
                mResLoader?.Recycle2Cache();
                mResLoader = null;
            }
        }

        protected override IPanelLoader CreatePanelLoader()
        {
            return new QFYooAssetsPanelLoader();
        }
    }
}