﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace CaveBiome
{
    /// <summary>
    /// MapComponent_CaveWellLight class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class MapComponent_CaveWellLight : MapComponent
    {
        public const int sunriseBeginHour = 6;
        public const int sunriseBeginHourInTicks = sunriseBeginHour * GenDate.TicksPerHour;
        public const int sunriseEndHour = 10;
        public const int sunriseEndHourInTicks = sunriseEndHour * GenDate.TicksPerHour;
        public const int sunriseDurationInTicks = sunriseEndHourInTicks - sunriseBeginHourInTicks;
        public const int sunsetBeginHour = 16;
        public const int sunsetBeginHourInTicks = sunsetBeginHour * GenDate.TicksPerHour;
        public const int sunsetEndHour = 20;
        public const int sunsetEndHourInTicks = sunsetEndHour * GenDate.TicksPerHour;
        public const int sunsetDurationInTicks = sunsetEndHourInTicks - sunsetBeginHourInTicks;
        public const int lightCheckPeriodInTicks = GenTicks.TicksPerRealSecond;
        public int nextLigthCheckTick = 1;

        public const float lightRadiusCaveWellMin = 0f;
        public const float lightRadiusCaveWellMax = 10f;

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                GenStep_CaveSetRules.SetCaveRules();
                
                // Set default cave well glowing radius.
                List<Thing> caveWellsList = Find.ListerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
                foreach (Thing caveWell in caveWellsList)
                {
                    SetGlowRadius(caveWell, 0f);
                }
            }
        }

        public override void MapComponentTick()
        {
            if (Find.Map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                return;
            }

            if (Find.TickManager.TicksGame >= nextLigthCheckTick)
            {
                nextLigthCheckTick = Find.TickManager.TicksGame + lightCheckPeriodInTicks;
                int hour = GenDate.HourOfDay;
                if ((hour >= sunriseBeginHour)
                    && (hour < sunriseEndHour))
                {
                    // Sunrise.
                    int currentDayTick = Find.TickManager.TicksAbs % GenDate.TicksPerDay;
                    int ticksSinceSunriseBegin = currentDayTick - sunriseBeginHourInTicks;
                    float sunriseProgress = (float)ticksSinceSunriseBegin / (float)sunriseDurationInTicks;
                    float caveWellLigthRadius = Mathf.Lerp(lightRadiusCaveWellMin, lightRadiusCaveWellMax, sunriseProgress);
                    List<Thing> caveWellsList = Find.ListerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
                    foreach (Thing caveWell in caveWellsList)
                    {
                        SetGlowRadius(caveWell, caveWellLigthRadius);
                    }
                }
                else if ((hour >= sunsetBeginHour)
                    && (hour < sunsetEndHour))
                {
                    // Sunset.
                    int currentDayTick = Find.TickManager.TicksAbs % GenDate.TicksPerDay;
                    int ticksSinceSunsetBegin = currentDayTick - sunsetBeginHourInTicks;
                    float sunsetProgress = 1f - ((float)ticksSinceSunsetBegin / (float)sunriseDurationInTicks);
                    float caveWellLigthRadius = Mathf.Lerp(lightRadiusCaveWellMin, lightRadiusCaveWellMax, sunsetProgress);
                    List<Thing> caveWellsList = Find.ListerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
                    foreach (Thing caveWell in caveWellsList)
                    {
                        SetGlowRadius(caveWell, caveWellLigthRadius);
                    }
                }
            }
        }
        
        public void SetGlowRadius(Thing caveWell, float glowradius)
        {
            CompGlower glowerComp = caveWell.TryGetComp<CompGlower>();
            if (glowerComp != null)
            {
                glowerComp.Props.glowRadius = glowradius;
                glowerComp.Props.overlightRadius = glowradius;
                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
            }
        }
    }
}
