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
	public Material			defaultMaterial;
	
	
	private bool 			_equipped;
	private GameObject 		_tooltip;
	private GameObject		_graphic;
	
	public bool equipped {
		get { return _equipped; }
	}
	
	public float energy {
		get; set;
	}
	
	public Player owner {
		get; private set;
	}
	
	public Material	overrideMaterial {get;set;}
	
	public bool materialOverride {
		set {
			Material m = value ? overrideMaterial : defaultMaterial;
			foreach(Transform child in _graphic.transform) {
				child.renderer.material = m;
			}
		}
	}
	
	void Start() {
		_tooltip = transform.Find("tooltip").gameObject;
		_graphic = transform.Find("Graphic").gameObject;
		
		// set kinematic if we're not master
		if (PhotonNetwork.inRoom)
			rigidbody.isKinematic = !PhotonNetwork.isMasterClient;
	}

	public void Equip(int ownerViewID) {
		
		PhotonView ownerView = PhotonView.Find(ownerViewID);
		owner = ownerView.GetComponent<Player>();
		
		// equip for just us
		if (ownerView.isMine)
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
	
	public void Drop() {
		_equipped = false;
		SendMessage("OnDrop", SendMessageOptions.DontRequireReceiver);
		transform.parent = null;
		if (PhotonNetwork.isMasterClient) {
			// position is sync'd to other clients via photonview
			transform.position = owner.head.position;
			transform.position += owner.head.forward;
			rigidbody.isKinematic = false;
		}
		collider.enabled = true;
		materialOverride = false;
		if (_tooltip) _tooltip.SetActive(true);
		owner = null;
	}
	

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
