// See https://aka.ms/new-console-template for more information
using ExternalSortLib.Entity;


class Program
{
	static void Main(string[] args)
	{
		var fileName = "input.txt";
		int size = 10000;
		if (args.Length>0) 
			size =int.Parse(args[0]);
		if (args.Length>1) 
			fileName = args[1];
		using var writer = new StreamWriter(fileName);

		for (int i = 0; i < size; i++)
		{
			var item = new TestLineItem() { Name = RandomHelper.RandomString(RandomHelper.RandomNumber(100)+1), Number = RandomHelper.RandomNumber(int.MaxValue) };
			item.WriteTo(writer);
		}
	}
}


public class RandomHelper
{
	static Random random = new Random();
	public static string RandomString(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}

	public static int RandomNumber(int max)
	{ return random.Next(max); }
}

