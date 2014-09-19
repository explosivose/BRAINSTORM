using UnityEngine;
using System.Collections;

// in future, perhaps, this could inherit from a PhysicsObject MonoBehaviour
// which plays audio for collisions 

[RequireComponent(typeof(Rigidbody))]
public class Equipment : Photon.MonoBehaviour {
	
	[System.Serializable]
	public class AudioLibrary {
		public float volume;
		public AudioClip start;
		public AudioClip loop;
		public AudioClip stop;
	}

	public enum Type {
		weapon, 
		utility1, 	// utility1 is operated with the Jump button
		utility2	// utility2 is operated with the Sprint button
	}
	
	
	public enum EquipParent {
		player, camera
	}
	

	public Type 			type;
	public EquipParent 		parent;
	public Vector3 			equippedPosition;
	public Vector3 			defaultRotation;
	public Vector3 			holsteredPosition;
	public Vector3 			holsteredRotation;
	public AudioLibrary 	sounds = new AudioLibrary();
	
	private bool 			_equipped;
	private Transform 		_parent;
	private GameObject 		_tooltip;
	
	public bool equipped {
		get { return _equipped; }
	}
	
	public float energy {
		get; set;
	}
	
	void Start() {
		if (parent == EquipParent.camera) _parent = Camera.main.transform;
		if (parent == EquipParent.player) _parent = Player.localPlayer.transform;
		_tooltip = transform.Find("tooltip").gameObject;
		if (!_tooltip) Debug.LogWarning("Equipment is missing a tooltip.");
	}
	
	[RPC]
	public void Equip(int playerPhotonViewID) {
		_equipped = true;
		PhotonView playerPhotonView = PhotonView.Find(playerPhotonViewID);
		// main camera will be disabled in hierarchy on remote players
		//if (parent == EquipParent.camera) {
		//	transform.parent = playerPhotonView.GetComponent<Player>().mainCamera.transform;
		//}
		//else {
			transform.parent = playerPhotonView.transform;
		//}
		
		transform.localPosition = equippedPosition;
		transform.localRotation = Quaternion.Euler(defaultRotation);
		rigidbody.isKinematic = true;
		collider.enabled = false;
		if (_tooltip) _tooltip.SetActive(false);
		SendMessage("OnEquip", SendMessageOptions.DontRequireReceiver);
	}
	
	public void Drop() {
		_equipped = false;
		transform.parent = GameManager.Instance.activeScene.instance;
		transform.position = Camera.main.transform.position;
		transform.position += Camera.main.transform.forward;
		rigidbody.isKinematic = false;
		collider.enabled = true;
		if (_tooltip) _tooltip.SetActive(true);
		SendMessage("OnDrop", SendMessageOptions.DontRequireReceiver);
	}
	
	public void Holster() {
		_equipped = false;
		transform.parent = _parent;
		transform.localPosition = holsteredPosition;
		transform.localRotation = Quaternion.Euler(holsteredRotation);
		rigidbody.isKinematic = true;
		collider.enabled = false;
		SendMessage("OnHolster", SendMessageOptions.DontRequireReceiver);
	}
	
	public void AudioStart() {
		PlaySound(sounds.start, false);
	}
	
	public void AudioLoop() {
		PlaySound(sounds.loop, true);
	}
	
	public void AudioStop() {
		PlaySound(sounds.stop, false);
	}
	
	protected void PlaySound(AudioClip clip, bool loop) {
		audio.clip = clip;
		audio.loop = loop;
		audio.volume = sounds.volume;
		audio.Play();
	}
}
