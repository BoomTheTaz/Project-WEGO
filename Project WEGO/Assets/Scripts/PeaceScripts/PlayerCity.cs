using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Professions { Scientist, Builder, Weaponsmith, Student, Banker, Farmer, Miner, Soldier, Unemployed, NUM };
public enum Productions { Food, Science, Production, Money, Ores, NUM };

public class PlayerCity  {

	int PlayerID;
    
	List<Person> Population;

	int[] Workers;
	float[] ProductionPerTurn;
	float[] ResourceModifiers;
	int TaxesPerYear = 100;

	//  TODO: add armory


	PeaceUI UI;

    public PlayerCity( int player )
	{
		PlayerID = player;

		Population = new List<Person>();
		Workers = new int[(int)Professions.NUM];
		ProductionPerTurn = new float[(int)Productions.NUM];
		ResourceModifiers = new float[(int)Productions.NUM];

        
		for (int i = 0; i < ResourceModifiers.Length; i++)
        {
			ResourceModifiers[i] = Random.Range(.8f,1.2f);
        }

		UI = PeaceUI.instance;
      
		AddStartingPopulation();


	}

    
    void AddStartingPopulation()
	{
		/*
         * ======== STARTING POPULATION IDEAS ==========
         * - Farmer
         * - Builder
         * - Miner
         * - Scientist
         * - Soldier
        */

		Population.Add(new Person(Professions.Farmer));
		Workers[(int)Professions.Farmer] += 1;

		Population.Add(new Person(Professions.Builder));
		Workers[(int)Professions.Builder] += 1;

		Population.Add(new Person(Professions.Miner));
		Workers[(int)Professions.Miner] += 1;

		Population.Add(new Person(Professions.Scientist));
		Workers[(int)Professions.Scientist] += 1;

		Population.Add(new Person(Professions.Soldier));
		Workers[(int)Professions.Soldier] += 1;


		CalculateProduction();
	}
    
    public void AdvanceToNextTurn()
	{
		Debug.Log("Player " + PlayerID.ToString() + " Advancing!");

        

		//Debug.Log("Food: " + Stockpile[0]);
		//Debug.Log("Science: " + Stockpile[1]);
		//Debug.Log("Production: " + Stockpile[2]);
		//Debug.Log("Money: " + Stockpile[3]);
		//Debug.Log("Ores: " + Stockpile[4]);

	}
    
	void CalculateProduction()
	{
		for (int i = 0; i < ProductionPerTurn.Length; i++)
		{
			ProductionPerTurn[i] = 0;
		}

		foreach (var p in Population)
		{
			switch (p.Profession)
            {
                case Professions.Scientist:
                    ProductionPerTurn[(int)Productions.Science] += p.GetProduction() * ResourceModifiers[(int)Productions.Science];
                    break;

                case Professions.Builder:
                    ProductionPerTurn[(int)Productions.Production] += p.GetProduction() * ResourceModifiers[(int)Productions.Production];
                    break;

                case Professions.Weaponsmith:

                    break;

                case Professions.Banker:
                    ProductionPerTurn[(int)Productions.Money] += p.GetProduction() * ResourceModifiers[(int)Productions.Money];
                    break;

                case Professions.Farmer:
                    ProductionPerTurn[(int)Productions.Food] += p.GetProduction() * ResourceModifiers[(int)Productions.Food];
                    break;

                case Professions.Miner:
                    ProductionPerTurn[(int)Productions.Ores] += p.GetProduction() * ResourceModifiers[(int)Productions.Ores];
                    break;



                default:
                    break;
            }
		}

		ProductionPerTurn[(int)Productions.Money] += TaxesPerYear;
		if (UI == null)
			Debug.Log("NO UI");
		UI.UpdateProduction(ProductionPerTurn);
	}

}
