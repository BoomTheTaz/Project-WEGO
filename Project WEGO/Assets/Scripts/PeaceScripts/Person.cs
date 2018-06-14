using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person  {
    
	public Professions Profession { get; protected set; }
	int baseProduction = 100;
	float productionModifier = 1;
    
    public Person(Professions p)
	{
		Profession = p;
	}

    public float GetProduction()
	{
		return baseProduction * productionModifier;
	}

}
