using Cysharp.Threading.Tasks;
using Game;
using Game.UI;
using QFramework;
using UnityEngine;

namespace Game
{
    public class InitState : AbstractState<LaunchState, Launch>, IController
    {
        public InitState(FSM<LaunchState> fsm, Launch target) : base(fsm, target)
        {
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            InitAsync().Forget();
        }

        protected override void OnExit()
        {
            base.OnExit();
        }

        private async UniTask InitAsync()
        {
            await this.GetSystem<YooassetSystem>().InitAsync();

            var resLoader = ResLoader.Allocate();
            await resLoader.LoadSceneUniTask("Main");

            await UIKit.OpenPanel<UITestPanel>();
        }

        public IArchitecture GetArchitecture()
        {
            return MainArchitecture.Interface;
        }
    }
}