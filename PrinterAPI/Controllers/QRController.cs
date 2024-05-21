using PrinterAPI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PrinterAPI.Controllers
{
	public class QRController : ApiController
	{
		public QRHelper qrh = new QRHelper();

		// GET api/<controller>
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/<controller>/5
		public string Get(int id)
		{
			return "value";
		}

		[Route("~/api/QR")]
		[HttpGet]
		// POST api/<controller>
		public HttpResponseMessage Get(string textname, int otype, int dimension)
		{
			byte[] buffer = qrh.bargenerator(textname, otype, dimension);
			MemoryStream ms = new MemoryStream(buffer);
			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
			response.Content = new StreamContent(ms);
			response.Content.Headers.ContentType = new
			System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

			return response;
		}

		// PUT api/<controller>/5
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/<controller>/5
		public void Delete(int id)
		{
		}
	}
}