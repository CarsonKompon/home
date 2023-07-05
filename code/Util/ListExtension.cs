namespace Home.Util;

public static class ListExtension
{
	//TODO: restore the original type before returning
	
	/// <summary>
	/// Shuffles the given list
	/// </summary>
	public static IList Shuffle<T>( this IList<T> list )
	{
		return list.OrderBy( o => Game.Random.Int( 100 ) ).ToList();
	}
}
