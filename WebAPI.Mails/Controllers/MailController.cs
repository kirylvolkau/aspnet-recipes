using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Mails.Services;
using WebAPI.Mails.Models;

namespace WebAPI.Mails.Controllers
{
    [ApiController]
    [Route("/mail")]
    public class MailController : ControllerBase
    {
        private readonly IMailService _service;

		public MailController(IMailService service)
		{
			_service = service;
		}

		[HttpPost("send")]
		public async Task<IActionResult> SendMail([FromForm]MailRequest request)
		{
			try
			{
				await _service.SendAsync(request);
				return Ok();
			}
			catch
			{
				throw;
			}
		}
    }
}
