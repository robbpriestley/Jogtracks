using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DigitalWizardry.SPA_Template
{	
	public class Account
	{
		public Account(){}
		
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string UserName { get; set; }
		public byte[] Salt { get; set; }
		public string Hash { get; set; }
		public Guid Token { get; set; }
	}
}