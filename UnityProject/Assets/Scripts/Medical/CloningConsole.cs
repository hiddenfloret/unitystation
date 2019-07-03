﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main component for cloning console
/// </summary>
public class CloningConsole : MonoBehaviour
{
	public delegate void ChangeEvent();
	public static event ChangeEvent changeEvent;

	public List<CloningRecord> CloningRecords = new List<CloningRecord>();
	public DNAscanner scanner;
	public CloningPod cloningPod;

	public void ToggleLock()
	{
		if(scanner && scanner.IsClosed)
		{
			scanner.IsLocked = !scanner.IsLocked;
		}
	}

	public void Scan()
	{
		if (scanner && scanner.occupant)
		{
			var mob = scanner.occupant;
			var mobID = scanner.occupant.mobID;
			var playerScript = mob.GetComponent<PlayerScript>();
			if(playerScript.mind.bodyMobID != mobID)
			{
				return;
			}
			for (int i = 0; i < CloningRecords.Count; i++)
			{
				var record = CloningRecords[i];
				if (mobID == record.mobID)
				{
					record.UpdateRecord(mob, playerScript);
					return;
				}
			}
			CreateRecord(mob, playerScript);
		}
	}

	public void TryClone(CloningRecord record)
	{
		if (cloningPod && cloningPod.CanClone())
		{
			if(record.mind.ConfirmClone(record.mobID))
			{
				cloningPod.StartCloning(record);
				CloningRecords.Remove(record);
			}
		}
	}

	private void CreateRecord(LivingHealthBehaviour mob, PlayerScript playerScript)
	{
		var record = new CloningRecord();
		record.UpdateRecord(mob, playerScript);
		CloningRecords.Add(record);
	}

}

public class CloningRecord
{
	public string name;
	public string scanID;
	public float oxyDmg;
	public float burnDmg;
	public float toxinDmg;
	public float bruteDmg;
	public string uniqueIdentifier;
	public CharacterSettings characterSettings;
	public int mobID;
	public Mind mind;

	public CloningRecord()
	{
		scanID = Random.Range(0, 9999).ToString();
	}

	public void UpdateRecord(LivingHealthBehaviour mob, PlayerScript playerScript)
	{
		mobID = mob.mobID;
		mind = playerScript.mind;
		name = playerScript.playerName;
		characterSettings = playerScript.characterSettings;
		oxyDmg = mob.bloodSystem.oxygenDamage;
		burnDmg = mob.GetTotalBurnDamage();
		toxinDmg = 0;
		bruteDmg = mob.GetTotalBruteDamage();
		uniqueIdentifier = "35562Eb18150514630991";
	}
}