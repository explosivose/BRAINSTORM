using UnityEngine;
using System.Collections;

public static class Pitch {

	
	public static float[] Freq = new float[] {
		261.63f, 	// [0] C4
		293.66f,	// [1] D4
		329.63f,	// [2] E4
		349.32f,	// [3] F4
		392.00f,	// [4] G4
		440.00f,	// [5] A4
		493.88f		// [6] B4
	};
	
	public enum Enum {
		C4 = 0,
		D4 = 1,
		E4 = 2,
		F4 = 3,
		G4 = 4,
		A4 = 5,
		B4 = 6
	}
	
	public static float Shift(Enum from, Enum to) {
		return Freq[(int)to]/Freq[(int)from];
	}
	
	public static float RandomNote {
		get {
			int index = Random.Range(0, Freq.Length);
			return Freq[index];
		}
	}
}

public class PitchShiftByBearing : MonoBehaviour {

	// this class assumes that the audio source is A4 pitch
	
	public Pitch.Enum[] notes;
	
	private float[] sequence;
	private int 	sequenceIndex = 0;
	private Vector3 lastChange = Vector3.zero;
	
	// Use this for initialization
	void Start () {
		
		sequence = new float[notes.Length];
		for (int i = 0; i < sequence.Length; i++) {
			//float freq = Pitch.Freq[(int)notes[i]];
			sequence[i] = Pitch.Shift(Pitch.Enum.A4, notes[i]);
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		AngleSelectPitch();
		
	}
	
	void SequenceAndAngleThreshold() {
		float theta = 360f/sequence.Length;
		float angle = Vector3.Angle (transform.forward, lastChange);
		if (angle > theta) {
			sequenceIndex++;
			if (sequenceIndex >= sequence.Length) 
				sequenceIndex = 0;
			audio.pitch = sequence[sequenceIndex];
			lastChange = transform.forward;
			if (!audio.loop)
				audio.Play();
		}
	}
	
	void SequenceByDistance() {
		float dist = Vector3.Distance(transform.position, lastChange);
		if (dist > 5f) {
			sequenceIndex++;
			if (sequenceIndex > sequence.Length) 
				sequenceIndex = 0;
			audio.pitch = sequence[sequenceIndex];
			lastChange = transform.position;
			if (!audio.loop)
				audio.Play();
		}
	}
	
	void AngleSelectPitch() {
		float angle = Vector3.Angle (transform.forward, Vector3.forward);
		int index = Mathf.FloorToInt(angle*sequence.Length/180f);
		if (index != sequenceIndex) {
			sequenceIndex = index;
			audio.pitch = sequence[sequenceIndex];
			if (!audio.loop)
				audio.Play();
		}

	}
}
