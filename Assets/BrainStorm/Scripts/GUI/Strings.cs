using UnityEngine;
using System.Collections;

public class Strings {

	public const string gameTitle = "chasers";

	public const string gameVersion = "v0.8dev";
	
	public const string developers = 
		"SUPERCORE.CO.UK \n" +
		"Matt Blickem \n" +
		"Dan Cohen \n" + 
		"Luke Walker \n";
	
	public const string assets = 
		"Unity3D \n" +
		"Photon Unity Networking \n" +
		"AstarPathfindingProject \n" +
		"ProCore \n" +
		"GradientGUIBars \n" +
		"ObjectPool \n" +
		"sky5X \n";
		
	public static string OmitCloneSuffix(string input) {
		if (input.Contains("(Clone)")) {
			return input.Remove(input.Length - "(Clone)".Length);
		}
		else {
			return input;
		}
	}
}
