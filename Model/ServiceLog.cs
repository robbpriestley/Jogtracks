using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalWizardry.Jogtracks
{	
	public enum ServiceLogType
	{
		ACCESS,
		ERROR,
		SIGNUP,
		SIGNIN,
		TEST
	}
	
	public class ServiceLog
	{
		public ServiceLog(){}
 
		public ServiceLog(ServiceLogType type, string path, string message, string userId)
		{
			Id = 0;
			UtcTime = DateTime.UtcNow;
			Type = type.ToString();
			Path = path;
			Message = message;
			UserId = userId;
		}
		
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public DateTime UtcTime { get; set; }
		public string Type { get; set; }
		public string Path { get; set; }
		public string Message { get; set; }
		public string UserId { get; set; }
	}
}