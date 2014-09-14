using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Spiderette")]
[RequireComponent(typeof(NPCPathFinder))]
public class Spiderette : NPC {

	public float stickyness = 10f;
	public float maxGndDist = 0.75f;
	public float jumpDuration = 1f;
	private bool stalking;

	private NPCPathFinder _pathfinder;
	private MeshRenderer _ren;
	private float _lastAttackTime = -999f;
	private float _lastJumpTime = -999f;

	private bool grounded {
		get {
			Ray ray = new Ray(transform.position, -transform.up);
			return Physics.Raycast(ray, maxGndDist);
		}
	}
	
	private bool canAttack {
		get {
			return _lastAttackTime + 1f/attackRate < Time.time;
		}
	}

	public bool isJumping {
		get {
			return _lastJumpTime + jumpDuration > Time.time;
		}
	}

	protected override void Awake ()
	{
		base.Awake ();
		_pathfinder = GetComponent<NPCPathFinder>();
		_ren = GetComponentInChildren<MeshRenderer>();
	}

	// Use this for initialization
	void Start () {
		target = Player.Instance.transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (targetLOS && targetIsNear && grounded) {
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			AttackJump();
			return;
		}
		if (targetIsInAttackRange && canAttack) {
			_lastAttackTime = Time.time;
			Player.Instance.SendMessage("Damage", _damage); 
		}
		
		// if we can't be seen and we have LOS then move toward player
		if (!_ren.isVisible && targetLOS) {
			stalking = true;
			if (!audio.isPlaying) audio.Play();
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			// update path if target has moved
			if (Vector3.Distance(_pathfinder.destination, target.position) > 1f) {
				_pathfinder.destination = target.position;
			}
		}
		// otherwise, be still
		else if (!isJumping) {	
			stalking = false;
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation  |
									RigidbodyConstraints.FreezePositionX | 
									RigidbodyConstraints.FreezePositionZ;
			if (audio.isPlaying) audio.Stop();
		}
		
		if (isDead) transform.Recycle();
	}
	
	void FixedUpdate() {
		if (isJumping) return;
		Vector3 stickToWall;
		if (grounded) {
			stickToWall = -transform.up;
		}
		else {
			stickToWall = Vector3.down;
		}
		
		stickToWall *= stickyness * rigidbody.mass * rigidbody.drag;
		rigidbody.AddForce(stickToWall);
	}
	
	void AttackJump() {
		_lastJumpTime = Time.time;
		// ballistic calculation for jumping at the player
		Vector3 dir = target.position - transform.position;
		float h = dir.y; // height difference
		dir.y = 0f;
		float dist = dir.magnitude; // horz distance
		float angle = 45f * Mathf.Deg2Rad; // convert to rads
		dir.y = dist * Mathf.Tan(angle); // dir to elevation angle
		dist += h / Mathf.Tan(angle); // correct for small h differences
		float m = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * angle));
		if (float.IsNaN(m)) return;
		Vector3 v = dir.normalized * m;
		rigidbody.velocity = v;
	}
	
	
}
