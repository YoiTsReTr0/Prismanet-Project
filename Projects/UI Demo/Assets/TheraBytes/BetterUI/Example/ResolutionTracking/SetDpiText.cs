using System.Collections;
using System.Collections.Generic;
using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Examples.ResolutionTracking
{
	public class SetDpiText : MonoBehaviour, IResolutionDependency
	{
		[SerializeField] Text label;


		void Start()
		{
			OnResolutionChanged();
		}

		public void OnResolutionChanged()
		{
			label.text = $"UnityEngine.Screen.dpi:\t\t\t\t\t{Screen.dpi}\n\n" +
				$"ResolutionMonitor.CurrentDpi:  \t\t{ResolutionMonitor.CurrentDpi}\n\n\n" +
				$"Resolution:\t\t\t\t\t\t\t\t\t\t{Screen.width} x {Screen.height}";
		}

	}
}
