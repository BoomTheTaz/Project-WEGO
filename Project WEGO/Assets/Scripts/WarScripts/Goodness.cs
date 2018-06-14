using UnityEngine;


// Basic class to store various metrics for goodness
public class Goodness
{

	public float Ranged { get; private set; }
	public float Melee { get; private set; }
	public float Cavalry { get; private set; }

	public Goodness(float r, float m, float c)
	{
		Ranged = r;
		Melee = m;
		Cavalry = c;
	}

    // Method to add one Goodness to another
	public static Goodness AddGoodness(Goodness a, Goodness b, Goodness c = null)
	{
		if (a == null && b == null)
		{
			Debug.LogError("Trying to add to null Goodness objects. What is going on?!");
			return null;
		}
        
		if (b == null)
			return a;
		if (a == null)
			return b;


		if (c == null)
			return new Goodness(a.Ranged + b.Ranged, a.Melee + b.Melee, a.Cavalry + b.Cavalry);
		else
			return new Goodness(a.Ranged + b.Ranged + c.Ranged, a.Melee + b.Melee + c.Melee, a.Cavalry + b.Cavalry + c.Cavalry);


	}


    public Goodness DivideBy(float f)
	{
		Ranged /= f;
		Melee /= f;
		Cavalry /= f;

		return this;
	}


	public void AddToAll(float f)
	{
		Ranged += f;
        Melee += f;
        Cavalry += f;
	}

    public void AddTo(int i, float f)
	{
		switch (i)
		{
			case (int) UnitTypes.Ranged:
				Ranged += f;
				break;

			case (int)UnitTypes.Melee:
				Melee += f;
				break;

			case (int)UnitTypes.Cavalry:
				Cavalry += f;
				break;

			default:
				Debug.LogError("No types corresponding to: " + i.ToString() + ". Failed to add Goodness.");
				break;
		}


	}
    
}
