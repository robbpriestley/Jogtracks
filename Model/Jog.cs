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
		public string UserName { get; set; }      // Username of the jogger.
		public string UpdatedBy { get; set; }     // Username of the jogger, coach, or admin.
		public DateTime Date { get; set; }         // Just date: no time component (00:00:00)
		public decimal Distance { get; set; }      // Kilometres
		public int Time { get; set; }              // Seconds
		public decimal AverageSpeed { get; set; }  // Kilometres per hour
	}
}