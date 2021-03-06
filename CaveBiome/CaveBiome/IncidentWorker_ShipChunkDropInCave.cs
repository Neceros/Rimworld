﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace CaveBiome
{
    public class IncidentWorker_ShipChunkDropInCave : IncidentWorker_ShipChunkDrop
    {
		private static readonly Pair<int, float>[] CountChance = new Pair<int, float>[]
		{
			new Pair<int, float>(1, 1f),
			new Pair<int, float>(2, 0.95f),
			new Pair<int, float>(3, 0.7f),
			new Pair<int, float>(4, 0.4f)
		};
		private int RandomCountToDrop
		{
			get
			{
				float x2 = (float)Find.TickManager.TicksGame / 3600000f;
				float timePassedFactor = Mathf.Clamp(GenMath.LerpDouble(0f, 1.2f, 1f, 0.1f, x2), 0.1f, 1f);
				return IncidentWorker_ShipChunkDropInCave.CountChance.RandomElementByWeight(delegate(Pair<int, float> x)
				{
					if (x.First == 1)
					{
						return x.Second;
					}
					return x.Second * timePassedFactor;
				}).First;
			}
		}

		public override bool TryExecute(IncidentParms parms)
		{
            if (Find.Map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                return base.TryExecute(parms);
            }
            
            IntVec3 firstChunkPosition = IntVec3.Invalid;
            TryFindShipChunkDropSpot(out firstChunkPosition);
            if (firstChunkPosition.IsValid)
            {
                // Spawn ship chunks.
                int partsCount = this.RandomCountToDrop;
                GenSpawn.Spawn(ThingDefOf.ShipChunk, firstChunkPosition);
                for (int shipShunkIndex = 0; shipShunkIndex < partsCount - 1; shipShunkIndex++)
                {
                    IntVec3 nexChunkPosition = IntVec3.Invalid;
                    TryFindShipChunkDropSpotNear(firstChunkPosition, out nexChunkPosition);
                    if (nexChunkPosition.IsValid)
                    {
                        GenSpawn.Spawn(ThingDefOf.ShipChunk, nexChunkPosition);
                    }
                }
                return true;
            }
            return false;
        }

        public void TryFindShipChunkDropSpot(out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;
            List<Thing> caveWellsList = Find.ListerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
            foreach (Thing caveWell in caveWellsList.InRandomOrder())
            {
	            if (IsValidPositionToSpawnShipChunk(caveWell.Position))
                {
                    spawnCell = caveWell.Position;
                    return;
                }
            }
        }

        public void TryFindShipChunkDropSpotNear(IntVec3 root, out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;
            foreach (IntVec3 checkedPosition in GenRadial.RadialCellsAround(root, 5f, false))
            {
	            if (IsValidPositionToSpawnShipChunk(checkedPosition))
                {
                    spawnCell = checkedPosition;
                    return;
                }
            }
        }

        public bool IsValidPositionToSpawnShipChunk(IntVec3 position)
        {
	        ThingDef chunkDef = ThingDefOf.ShipChunk;
            if ((position.InBounds() == false)
                || position.Fogged()
                || (position.Standable() == false)
                || (position.Roofed()
                    && position.GetRoof().isThickRoof))
            {
                return false;
            }
            if (position.SupportsStructureType(chunkDef.terrainAffordanceNeeded) == false)
            {
                return false;
            }
            List<Thing> thingList = position.GetThingList();
            for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
            {
                Thing thing = thingList[thingIndex];
                if ((thing.def.category != ThingCategory.Plant)
                    && GenSpawn.SpawningWipes(chunkDef, thing.def))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
