using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DigitalWizardry.Jogtracks
{	
	public class Jog
	{
		public Jog(){}
		
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public DateTime Date { get; set; }  // No time component (00:00:00)
		public int Distance { get; set; }     // Meters
		public int Time { get; set; }         // Seconds
	}
}