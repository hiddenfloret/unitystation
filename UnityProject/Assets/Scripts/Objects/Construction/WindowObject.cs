﻿using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// The main girder component
/// </summary>
public class WindowObject : NetworkBehaviour, ICheckedInteractable<HandApply>, IServerSpawn
{
	private TileChangeManager tileChangeManager;

	private RegisterObject registerObject;
	private ObjectBehaviour objectBehaviour;

	[Tooltip("Layer tile which this will create when placed.")]
	public LayerTile layerTile;

	private void Start()
	{
		tileChangeManager = GetComponentInParent<TileChangeManager>();
		registerObject = GetComponent<RegisterObject>();
		GetComponent<Integrity>().OnWillDestroyServer.AddListener(OnWillDestroyServer);
		objectBehaviour = GetComponent<ObjectBehaviour>();
	}

	public void OnSpawnServer(SpawnInfo info)
	{

	}

	private void OnWillDestroyServer(DestructionInfo arg0)
	{
		Spawn.ServerPrefab(CommonPrefabs.Instance.GlassSheet, gameObject.TileWorldPosition().To3Int(), transform.parent, count: 1,
			scatterRadius: Spawn.DefaultScatterRadius, cancelIfImpassable: true);
	}

	public bool WillInteract(HandApply interaction, NetworkSide side)
	{
		//start with the default HandApply WillInteract logic.
		if (!DefaultWillInteract.Default(interaction, side)) return false;

		//only care about interactions targeting us
		if (interaction.TargetObject != gameObject) return false;
		//only try to interact if the user has a wrench, screwdriver in their hand
		if (!Validations.HasItemTrait(interaction.HandObject, CommonTraits.Instance.Wrench) &&
			!Validations.HasItemTrait(interaction.HandObject, CommonTraits.Instance.Screwdriver)) return false;
		return true;
	}

	public void ServerPerformInteraction(HandApply interaction)
	{
		if (interaction.TargetObject != gameObject) return;

		
		if (Validations.HasItemTrait(interaction.HandObject, CommonTraits.Instance.Screwdriver))
		{
			if (objectBehaviour.IsPushable)
			{
				//secure it if there's floor
				if (MatrixManager.IsSpaceAt(registerObject.WorldPositionServer, true))
				{
					Chat.AddExamineMsg(interaction.Performer, "A floor must be present to secure the window!");
					return;
				}

				if (!ServerValidations.IsAnchorBlocked(interaction))
				{

					ToolUtils.ServerUseToolWithActionMessages(interaction, 4f,
						"You start securing the window...",
						$"{interaction.Performer.ExpensiveName()} starts securing the window...",
						"You secure the window.",
						$"{interaction.Performer.ExpensiveName()} secures the window.",
						() => ScrewToFloor(interaction));
				}
			}
			else
			{
				//unsecure it
				ToolUtils.ServerUseToolWithActionMessages(interaction, 4f,
					"You start unsecuring the girder...",
					$"{interaction.Performer.ExpensiveName()} starts unsecuring the window...",
					"You unsecure the window.",
					$"{interaction.Performer.ExpensiveName()} unsecures the window.",
					() => objectBehaviour.ServerSetAnchored(false, interaction.Performer));
			}

		}
		else if (Validations.HasItemTrait(interaction.HandObject, CommonTraits.Instance.Wrench))
		{
			//disassemble if it's unanchored
			if (objectBehaviour.IsPushable)
			{
				ToolUtils.ServerUseToolWithActionMessages(interaction, 4f,
					"You start to disassemble the window...",
					$"{interaction.Performer.ExpensiveName()} starts to disassemble the window...",
					"You disassemble the window.",
					$"{interaction.Performer.ExpensiveName()} disassembles the window.",
					() => Disassemble(interaction));
			}
			else
			{
				Chat.AddExamineMsg(interaction.Performer, "You must unsecure it first.");
			}
		}
	}

	[Server]
	private void ScrewToFloor(HandApply interaction)
	{
		var interactableTiles = InteractableTiles.GetAt(interaction.TargetObject.TileWorldPosition(), true);
		Vector3Int cellPos = interactableTiles.WorldToCell(interaction.TargetObject.TileWorldPosition());
		interactableTiles.TileChangeManager.UpdateTile(cellPos, layerTile);
		interactableTiles.TileChangeManager.SubsystemManager.UpdateAt(cellPos);
		GetComponent<CustomNetTransform>().DisappearFromWorldServer();
	}
	[Server]
	private void Disassemble(HandApply interaction)
	{
		Spawn.ServerPrefab(CommonPrefabs.Instance.GlassSheet, registerObject.WorldPositionServer, count: 4);
		GetComponent<CustomNetTransform>().DisappearFromWorldServer();
	}

}
