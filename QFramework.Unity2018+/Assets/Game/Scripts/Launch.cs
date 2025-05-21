using System;
using Cysharp.Threading.Tasks;
using Game;
using Game.UI;
using QFramework;
using UnityEngine;

namespace Game
{
    public enum LaunchState
    {
        Init,
        Main,
    }

    public class Launch : MonoBehaviour, IController
    {
        public FSM<LaunchState> Fsm = new FSM<LaunchState>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // 启动状态机
            Fsm.AddState(LaunchState.Init, new InitState(Fsm, this));

            Fsm.StartState(LaunchState.Init);
        }

        public IArchitecture GetArchitecture()
        {
            return Game.MainArchitecture.Interface;
        }
    }
}
