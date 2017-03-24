using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace AsyncStreaming.NetCore.Api
{
	[Route("api/[controller]")]
	public class AsyncStreamingController : Controller
	{
		private static HttpClient Client { get; } = new HttpClient();
		private readonly IHostingEnvironment _hostingEnvironment;

		public AsyncStreamingController(IHostingEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}

		// http://localhost:24425/api/file
		[HttpGet]
		public IActionResult Get()
		{
			var filename = "frag_bunny.mp4";
			return new FileCallbackResult(new MediaTypeHeaderValue("application/octet-stream"), async (outputStream, _) =>
			{
				var filePath = _hostingEnvironment.WebRootPath + $"/{filename}";

				//here set the size of buffer, you can set any size  
				int bufferSize = 1000;
				byte[] buffer = new byte[bufferSize];
				//here we re using FileStream to read file from server//  
				using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					int totalSize = (int)fileStream.Length;
					/*here we are saying read bytes from file as long as total size of file 

					is greater then 0*/
					while (totalSize > 0)
					{
						int count = totalSize > bufferSize ? bufferSize : totalSize;
						//here we are reading the buffer from orginal file  
						int sizeOfReadedBuffer = fileStream.Read(buffer, 0, count);
						//here we are writing the readed buffer to output//  
						await outputStream.WriteAsync(buffer, 0, sizeOfReadedBuffer);
						//and finally after writing to output stream decrementing it to total size of file.  
						totalSize -= sizeOfReadedBuffer;
					}
				}
			})
			{
				FileDownloadName = filename
			};
		}
	}
}
