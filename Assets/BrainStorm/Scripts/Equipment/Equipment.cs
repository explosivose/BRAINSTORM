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
	private GameObject 		_tooltip;
	
	public bool equipped {
		get { return _equipped; }
	}
	
	public float energy {
		get; set;
	}
	
	public Player owner {
		get; private set;
	}
	
	void Start() {
		_tooltip = transform.Find("tooltip").gameObject;
		if (!_tooltip) Debug.LogWarning("Equipment is missing a tooltip.");
		
		// set kinematic if we're not master
		if (PhotonNetwork.inRoom)
			rigidbody.isKinematic = !PhotonNetwork.isMasterClient;
	}
	
	[RPC]
	public void Equip(int playerPhotonViewID) {
		PhotonView playerPhotonView = PhotonView.Find(playerPhotonViewID);
		
		owner = playerPhotonView.GetComponent<Player>();
		
		// equip for just us
		if (playerPhotonView.isMine)
			_equipped = true;
		
		
		// position for all clients
		if (parent == EquipParent.camera) {
			transform.parent = owner.head;
		}
		else {
			transform.parent = owner.transform;
		}
		
		transform.localPosition = equippedPosition;
		transform.localRotation = Quaternion.Euler(defaultRotation);
		rigidbody.isKinematic = true;
		collider.enabled = false;
		if (_tooltip) _tooltip.SetActive(false);
		SendMessage("OnEquip", SendMessageOptions.DontRequireReceiver);
	}
	
	[RPC]
	public void Drop() {
		_equipped = false;
		SendMessage("OnDrop", SendMessageOptions.DontRequireReceiver);
		transform.parent = GameManager.Instance.activeScene.instance;
		transform.position = owner.head.position;
		transform.position += owner.head.forward;
		rigidbody.isKinematic = !PhotonNetwork.isMasterClient;
		collider.enabled = true;
		if (_tooltip) _tooltip.SetActive(true);
		owner = null;
	}
	
	[RPC]
	public void Holster() {
		_equipped = false;
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
