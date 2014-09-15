using UnityEngine;
using System.Collections;

public class Strings {

	public const string gameVersion = "BRAINSTORM v0.5-a";
	
	public const string developers = 
		"SUPERCORE.CO.UK \n" +
		"Matt Blickem \n" +
		"Dan Cohen \n" + 
		"Luke Walker \n";
	
	public const string assets = 
		"Unity3D \n" +
		"AstarPathfindingProject \n" +
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
