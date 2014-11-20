using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

static class BountyExtensions {

	public const string bountyKey = "bty";
	public const string earningsKey = "ear";
	public const string cashPoolKey = "csh";
	public const int minimumBounty = 200;
	public const int bountyIncrease = 100;
	
	public static bool emptyCashPool {
		get; private set;
	}
	
	public static void SetCashPool(int newPool) {
		Hashtable cashHash = new Hashtable();
		cashHash[cashPoolKey] = newPool;
		PhotonNetwork.room.SetCustomProperties(cashHash);
		emptyCashPool = (newPool <= 0);
	}
	
	public static void SubCashPool(int amount) {
		int current = GetCashPool();
		current -= amount;
		SetCashPool(current);
	}
	
	public static int GetCashPool() {
		object teamId;
		if (PhotonNetwork.room.customProperties.TryGetValue(cashPoolKey, out teamId)) {
			return (int)teamId;
		}
		return 0;
	}
	
	public static void SetBounty(this PhotonPlayer player, int bounty) {
		Hashtable bountyHash = new Hashtable();
		bountyHash[bountyKey] = bounty;
		player.SetCustomProperties(bountyHash);
	}
	
	public static void AddBounty(this PhotonPlayer player, int bounty) {
		int current = player.GetBounty();
		current += bounty;
		Hashtable bountyHash = new Hashtable();
		bountyHash[bountyKey] = current;
		player.SetCustomProperties(bountyHash);
	}
	
	public static int GetBounty(this PhotonPlayer player) {
		object teamId;
		if (player.customProperties.TryGetValue(bountyKey, out teamId)) {
			return (int)teamId;
		}
		return 0;
	}
	
	public static void SetEarnings(this PhotonPlayer player, int earnings) {
		Hashtable earningsHash = new Hashtable();
		earningsHash[earningsKey] = earnings;
		player.SetCustomProperties(earningsHash);
	}
	
	public static void AddEarnings(this PhotonPlayer player, int earnings) {
		int current = player.GetEarnings();
		current += earnings;
		Hashtable earningsHash = new Hashtable();
		earningsHash[earningsKey] = current;
		player.SetCustomProperties(earningsHash);
		// deduct from cashpool
		SubCashPool(earnings);
	}
	
	public static int GetEarnings(this PhotonPlayer player) {
		object teamId;
		if (player.customProperties.TryGetValue(earningsKey, out teamId)) {
			return (int)teamId;
		}
		return 0;
	}
}