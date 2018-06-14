using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {
    

    
    // ========= TERRAIN BONUSES ==========

    
    // ON TERRAIN
	public const float OnHillATKBonus = 0.25f;
	//public const float OnForestATKBonus = 0.1f;
	public const float OnForestDEFBonus = 0.25f;
	public const float OnWetlandATKPenalty = 0.2f;
	public const float OnWetlandDEFPenalty = 0.2f;


	// TODO: Decide if want to include this,
	//        May just want to look at hex on, for most things
	// ATTACKING TERRAIN
	//public const float AttackingHillPenalty = 0.2f;


	// ========== FORTIFICATION ==========
	public const float FortificationModifier = 1f;


    // ========== GOODNESS SCALERS ==========

    // BASE TERRAIN GOODNESS



    // UNIT ADDED GOODNESS
	public const float RangedBehindBias = 0.1f;
	public const float MeleeBesideBias = 0.1f;
	public const float SameTypeBonus = 0.1f;


	// ========== UI SCALERS ==========

	public const float SidePanelSpeed = .1f;



	// ========== CAMERA SCALERS ==========
	public const float RightOffset = 1f;
	public const float LeftOffset = 1f;
    

}
