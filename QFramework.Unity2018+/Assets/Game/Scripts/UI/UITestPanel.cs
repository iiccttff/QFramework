using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Game.UI
{
	public class UITestPanelData : UIPanelData
	{
	}
	public partial class UITestPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UITestPanelData ?? new UITestPanelData();
			// please add init code here
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}
	}
}
