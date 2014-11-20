using UnityEngine;
using System.Collections;

public class CTRLscoreboard : CTRLelement {

	public enum Column {
		PlayerName,
		Bounty,
		Earnings,
		CashPool
	}

	public Column column;

	protected override void OnEnable ()
	{
		finalText = "";
		switch (column) {
		case Column.PlayerName:
			foreach(PhotonPlayer player in PhotonNetwork.playerList) 
				finalText += player.ToString() + "\n";
			break;
		case Column.Bounty:
			foreach(PhotonPlayer player in PhotonNetwork.playerList)
				finalText += player.GetBounty().ToString() + "\n";
			break;
		case Column.Earnings:
			foreach(PhotonPlayer player in PhotonNetwork.playerList)
				finalText += player.GetEarnings().ToString() + "\n";
			break;
		case Column.CashPool:
			finalText = BountyExtensions.GetCashPool().ToString();
			break;
		}
		base.OnEnable ();
	}
}
