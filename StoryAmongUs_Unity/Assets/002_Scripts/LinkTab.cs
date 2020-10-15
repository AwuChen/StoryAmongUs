using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class LinkTab : MonoBehaviour 
{
    public string url;

	public void OpenLink()
	{
		Application.OpenURL(url);
	}

	public void OpenLinkJS()
	{
		Application.ExternalEval("window.open('"+url+"');");
    }

	public void OpenLinkJSPlugin()
	{
		#if !UNITY_EDITOR
		openWindow(url);
		#endif
	}

	[DllImport("__Internal")]
	private static extern void openWindow(string url);

}