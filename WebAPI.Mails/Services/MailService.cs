using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WebAPI.Mails.Models;
using WebAPI.Mails.Settings;

namespace WebAPI.Mails.Services
{
	//in real life project this should be in separate file.
	public interface IMailService
	{
		Task SendAsync(MailRequest mailRequest);
	}

	public class MailService : IMailService
	{
		private readonly MailSettings _settings;

		public MailService(IOptions<MailSettings> settings)
		{
			_settings = settings.Value; // auto dependency injection
		}

		public async Task SendAsync(MailRequest mailRequest)
		{
			MailMessage message = new MailMessage();
			SmtpClient client = new SmtpClient();
			message.From = new MailAddress(_settings.Mail, _settings.DisplayName);
			message.To.Add(new MailAddress(mailRequest.Receiver));
			message.Subject = mailRequest.Subject;
			if(mailRequest != null)
			{
        		foreach (var file in mailRequest.Attachments)
        		{
            		if (file.Length > 0)
            		{
                		using (var ms = new MemoryStream())
                		{
                    		file.CopyTo(ms);
                    		var fileBytes = ms.ToArray();
                    		Attachment att = new Attachment(new MemoryStream(fileBytes), file.FileName);
                    		message.Attachments.Add(att);
                		}
            		}
        		}				
			}
			message.IsBodyHtml = false;
    		message.Body = mailRequest.Body;
    		client.Port = _settings.Port;
    		client.Host = _settings.Host;
    		client.EnableSsl = true;
    		client.UseDefaultCredentials = false;
    		client.Credentials = new NetworkCredential(_settings.Mail, _settings.Password);
    		client.DeliveryMethod = SmtpDeliveryMethod.Network;
    		await client.SendMailAsync(message);
		}
	}
}