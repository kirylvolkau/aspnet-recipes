using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Mails.Models
{
	public class MailRequest 
	{
		public string Receiver { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public List<IFormFile> Attachments { get; set; }
	}
}