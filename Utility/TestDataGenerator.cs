using System;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	class TestJog
	{
		public DateTime Date;
		public decimal Distance;
		public int Time;
		public decimal Kph;
	}
	
	public static class TestDataGenerator
	{
		public static void Generate(string userName)
		{	
			List<TestJog> jogs = new List<TestJog>();

			Random r = new Random();

			int baseRate = r.Next(2, 4);

			double max = baseRate / 1000.0d + 0.00025d;
			double min = baseRate / 1000.0d - 0.00025d;
			
			DateTime date = DateTime.Now;

			for (int i = 0; i < 50; i++)
			{
				date = date.AddDays(-1);

				TestJog jog = new TestJog();
				jog.Date = date;
				jog.Distance = ((decimal)r.Next(2000) + 1.0m) / 100.00m;

				double kilometresPerSecond = r.NextDouble() * (max - min) + min;
				int totalTime = (int)((double)jog.Distance / kilometresPerSecond);

				jog.Time = totalTime;
				jog.Kph = (decimal)((double)jog.Distance / ((double)totalTime / 3600.0d));

				jogs.Add(jog);
			}

			foreach (TestJog jog in jogs)
			{
				//Console.WriteLine(jog.Date + " " + jog.Distance + "km " + jog.Time + "s " + jog.Kph + "kph");
				string script = "curl -s -u ${BASICAUTH} --request POST http://0.0.0.0:5000/api/jogs/add -H \"Content-Type: application/json\" -d '{ \"Token\": \"'\"${" + userName.ToUpper() + "TOKEN}\"'\", \"UserName\": \"" + userName + "\", \"Date\": \"" + jog.Date + "\", \"Distance\": " + jog.Distance + ", \"Time\": " + jog.Time + " }' > /dev/null";
				Console.WriteLine(script);
			}
		}
	}
}