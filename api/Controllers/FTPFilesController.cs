using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Http;

namespace AzureAPI.Controllers
{
	[RoutePrefix("api/ftpfiles")]
	public class FTPFilesController : ApiController
    {
		private class FileInfo
		{
			public string Name { get; set; }

			public string Description { get; set; }
			public string Size { get; set; }
		}
		public IHttpActionResult GetFTPFiles()
		{
			FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://denricdenise.info");
			ftpRequest.Credentials = new NetworkCredential("u732671654.deniseazure", "denric1825");
			ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
			FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
			StreamReader streamReader = new StreamReader(response.GetResponseStream());

			List<FileInfo> fileList = new List<FileInfo>();

			string line = streamReader.ReadLine();
			while (!string.IsNullOrEmpty(line))
			{
				FileInfo file = new FileInfo();
				file.Name = line;

				fileList.Add(file);
				line = streamReader.ReadLine();
			}

			streamReader.Close();

			return Ok(fileList);
		}
	}
}