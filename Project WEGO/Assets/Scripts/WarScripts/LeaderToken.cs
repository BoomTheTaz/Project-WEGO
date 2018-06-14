using UnityEngine;

public class LeaderToken : Token {

    // Do nothing
	public override void RefactorTarget()
	{
        return;
	}

	public override void SetColors(Color main, Color accent)
	{
		LeftSprite.color = accent;
		RightSprite.color = accent;
	}

}
