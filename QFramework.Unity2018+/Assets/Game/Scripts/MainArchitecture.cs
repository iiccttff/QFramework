using QFramework;
using UnityEngine;

namespace Game
{
    public class MainArchitecture : Architecture<MainArchitecture>
    {
        protected override void Init()
        {
            this.RegisterSystem(new YooassetSystem());
        }
    }
}
