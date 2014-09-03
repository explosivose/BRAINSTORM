using UnityEngine;
using System.Collections;

// get brighter as player gets closer
// when player is close enough, whiteout and load lobby 

public class Artefact : MonoBehaviour {

	public Scene.Tag 		artefactKind;
	public float closestDistance = 5f;
	public float farthestDistance = 10f;

	public bool				_revealed;
	private bool 			_playerNear;
	private Transform 		_player;
	private ParticleSystem 	_particles;
	private Collider		_collider;
	private Renderer		_renderer;
	
	void Start() {
		_player = Player.Instance.transform;
		_particles = GetComponentInChildren<ParticleSystem>();
		_collider = GetComponentInChildren<Collider>();
		_renderer = GetComponentInChildren<Renderer>();
	}
	
	public void Reveal() {
		_revealed = true;
		rigidbody.isKinematic = false;
		_collider.enabled = true;
		_renderer.enabled = true;
		_particles.Play();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (!_revealed) return;
		
		float playerDistance = Vector3.Distance(_player.position, transform.position);
		
		float farDist = farthestDistance - closestDistance;
		playerDistance -= closestDistance;
		
		float t = (farDist - playerDistance)/farDist;
		
		if (playerDistance < 0f) {
			GameManager.Instance.ChangeScene(Scene.Tag.Lobby);
		}
		
		if (t < 1f && !_playerNear) {
			_playerNear = true;
			Player.Instance.screenEffects = false;
		}
		else if (t > 1f && _playerNear) {
			_playerNear = false;
			Player.Instance.screenEffects = true;
		}
		
		if(_playerNear) {
			Color screenOverlay = Color.Lerp (Color.clear, Color.cyan, t);
			ScreenFade.Instance.SetScreenOverlayColor(screenOverlay);
		}
	}
}
