using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Game.UI
{
	// Generate Id:edb88928-b81e-489b-987a-b620a9f2da35
	public partial class UITestPanel
	{
		public const string Name = "UITestPanel";
		
		[SerializeField]
		public UnityEngine.UI.Button TestButton;
		
		private UITestPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			TestButton = null;
			
			mData = null;
		}
		
		public UITestPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UITestPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UITestPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
