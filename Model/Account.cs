using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DigitalWizardry.Jogtracks
{	
	public class Account
	{
		public Account(){}
		
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string UserName { get; set; }
		public string UserColor { get; set; }
		public string AccountType { get; set; }
		public byte[] Salt { get; set; }
		public string Hash { get; set; }
		public Guid Token { get; set; }
	}
}