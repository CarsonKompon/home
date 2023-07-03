namespace Home.Games.Trivia;

public class TriviaGame : Entity
{
	[Property, Title("First Contestant Panel")]
	public TargetEntity ContestPanel1 { get; set; }

	[Property, Title("Second Contestant Panel")]
	public TargetEntity ContestPanel2 { get; set; }
}
