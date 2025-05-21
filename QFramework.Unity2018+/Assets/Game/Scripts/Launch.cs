using System;
using Cysharp.Threading.Tasks;
using Game;
using Game.UI;
using QFramework;
using UnityEngine;

public class Launch : MonoBehaviour, IController
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 启动状态机
        
        InitAsync().Forget();
    }

    private async UniTask InitAsync()
    {
        await this.GetSystem<YooassetSystem>().InitAsync();
        await UIKit.OpenPanel<UITestPanel>();
    }
    
    public IArchitecture GetArchitecture()
    {
        return Game.MainArchitecture.Interface;
    }
}
