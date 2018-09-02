using AzureAPI.Providers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureAPI.Controllers
{
	[RoutePrefix("api")]
	public class APIController : ApiController
	{
		private class FileInfo
		{
			public string Name { get; set; }

			public string Description { get; set; }
			public string Size { get; set; }

			public string Extension { get; set; }
		}

		private readonly NameValueCollection _settings = ConfigurationManager.AppSettings;

		public APIController()
		{
			
		}

		public APIController(NameValueCollection settings)
		{
			_settings = settings;
		}

		private FtpWebRequest FTPRequest(string addUrl = "")
		{
			var accountName = _settings["ftp:acccount:name"];
			var accountPassword = _settings["ftp:acccount:password"];
			var ftpUrl = _settings["ftp:web:url"];

			FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpUrl + addUrl);
			ftpRequest.Credentials = new NetworkCredential(accountName, accountPassword);

			return ftpRequest;
		}

		[HttpGet, Route("ftpfiles")]
		public IHttpActionResult GetFTPFiles()
		{
			FtpWebRequest ftpRequest = FTPRequest();
			ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
			FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
			StreamReader streamReader = new StreamReader(response.GetResponseStream());

			List<FileInfo> fileList = new List<FileInfo>();

			string line = streamReader.ReadLine();
			while (!string.IsNullOrEmpty(line))
			{
				if (line == "." || line == "..")
				{
					line = streamReader.ReadLine();
					continue;
				}

				fileList.Add(new FileInfo() { Name = line });
				line = streamReader.ReadLine();
			}

			streamReader.Close();

			return Ok(fileList);
		}

		[HttpPost, Route("ftpupload")]
		public async Task<System.Web.Http.IHttpActionResult> FTPUpload()
		{
			if (!Request.Content.IsMimeMultipartContent("form-data"))
			{
				throw new System.Web.Http.HttpResponseException(HttpStatusCode.UnsupportedMediaType);
			}

			string fileName = "";
			var provider = new FTPMultipartMemoryStreamProvider();
			await Request.Content.ReadAsMultipartAsync(provider);

			try
			{
				if (provider.Contents.Count > 0)
				{
					HttpContent ctnt = provider.Contents[0];
					fileName = ctnt.Headers.ContentDisposition.FileName.Replace("\"", "");

					//now read individual part into STREAM
					var fileBytes = await ctnt.ReadAsByteArrayAsync();
					if (fileBytes.Length != 0)
					{
						//Create FTP Request.
						var ftpUrl = _settings["ftp:web:url"];
						FtpWebRequest request = FTPRequest("/" + fileName);
						request.Method = WebRequestMethods.Ftp.UploadFile;

						//Enter FTP Server credentials.
						request.ContentLength = fileBytes.Length;
						request.UsePassive = true;
						request.UseBinary = true;
						request.ServicePoint.ConnectionLimit = fileBytes.Length;
						request.EnableSsl = false;

						using (Stream requestStream = request.GetRequestStream())
						{
							requestStream.Write(fileBytes, 0, fileBytes.Length);
							requestStream.Close();
						}

						FtpWebResponse response = (FtpWebResponse)request.GetResponse();

						response.Close();
					}
				}
			}
			catch (WebException ex)
			{
				throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
			}

			if (string.IsNullOrEmpty(fileName))
			{
				return BadRequest("An error has occured while uploading your file. Please try again.");
			}

			return Ok($"File: {fileName} has successfully uploaded");
		}

		[HttpPost, Route("ftpdelete")]
		public IHttpActionResult FTPDelete()
		{
			string filename = Request.Headers.FirstOrDefault(x => x.Key == "filename").Value?.FirstOrDefault();
			try
			{
				FtpWebRequest request = FTPRequest("/" + filename);
				request.Method = WebRequestMethods.Ftp.DeleteFile;
				FtpWebResponse response = (FtpWebResponse)request.GetResponse();
				response.Close();

				return Ok($"File: {filename} has successfully deleted");
			}
			catch (Exception ex)
			{

				return BadRequest($"An error has occured. Details: {ex.Message}");
			}
		}

		private CloudBlobContainer AzureContainer()
		{
			var accountName = _settings["storage:account:name"];
			var accountKey = _settings["storage:account:key"];
			var container = _settings["storage:container:name"];
			var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
			CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

			return blobClient.GetContainerReference(container);
		}

		[HttpGet, Route("azurefiles")]
		public IHttpActionResult GetAzureFiles()
		{
			CloudBlobContainer blobContainer = AzureContainer();
			var list = blobContainer.ListBlobs();

			List<FileInfo> blobNames = list.OfType<CloudBlockBlob>().Select(b => new FileInfo() { Name = b.Name }).ToList();

			return Ok(blobNames);
		}

		[HttpPost, Route("azureupload")]
		public async Task<System.Web.Http.IHttpActionResult> AzureUpload()
		{
			if (!Request.Content.IsMimeMultipartContent("form-data"))
			{
				throw new System.Web.Http.HttpResponseException(HttpStatusCode.UnsupportedMediaType);
			}

			CloudBlobContainer blobContainer = AzureContainer();
			var provider = new AzureStorageMultipartFormDataStreamProvider(blobContainer);
			try
			{
				await Request.Content.ReadAsMultipartAsync(provider);
			}
			catch (Exception ex)
			{
				return BadRequest($"An error has occured. Details: {ex.Message}");
			}

			// Retrieve the filename of the file you have uploaded
			var filename = provider.FileData.FirstOrDefault()?.LocalFileName;
			if (string.IsNullOrEmpty(filename))
			{
				return BadRequest("An error has occured while uploading your file. Please try again.");
			}

			return Ok($"File: {filename} has successfully uploaded");
		}

		[HttpPost, Route("azuredelete")]
		public async Task<System.Web.Http.IHttpActionResult> AzureDelete()
		{
			string filename = Request.Headers.FirstOrDefault(x => x.Key == "filename").Value?.FirstOrDefault();

			CloudBlobContainer blobContainer = AzureContainer();

			// Retrieve reference to a blob named "myblob.txt".
			CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);

			try
			{
				await blockBlob.DeleteIfExistsAsync();
			}
			catch (Exception ex)
			{
				return BadRequest($"An error has occured. Details: {ex.Message}");
			}

			return Ok($"File: {filename} has successfully deleted");
		}

		[HttpPost, Route("ftptoazure")]
		public async Task<System.Web.Http.IHttpActionResult> FTPtoAzure()
		{
			string filename = Request.Headers.FirstOrDefault(x => x.Key == "filename").Value?.FirstOrDefault();
			Stream ftpFile = DownloadFTPFile(filename);
			if (ftpFile == null)
			{
				var response = new HttpResponseMessage(HttpStatusCode.NotFound)
				{
					ReasonPhrase = $"File: {filename} not found on FTP"
				};

				throw new HttpResponseException(response);
			}

			var accountName = ConfigurationManager.AppSettings["storage:account:name"];
			var accountKey = ConfigurationManager.AppSettings["storage:account:key"];
			var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
			CloudBlobContainer blobContainer = AzureContainer();
			CloudBlockBlob cloudBlockBlob = blobContainer.GetBlockBlobReference(filename);

			try
			{
				await cloudBlockBlob.UploadFromStreamAsync(ftpFile);
			}
			catch (Exception ex)
			{
				return BadRequest($"An error has occured. Details: {ex.Message}");
			}

			return Ok($"File: {filename} has successfully copied");
		}

		private Stream DownloadFTPFile(string filename)
		{
			try
			{
				// Get the object used to communicate with the server.
				FtpWebRequest request = FTPRequest("/" + filename);
				request.Method = WebRequestMethods.Ftp.DownloadFile;

				FtpWebResponse response = (FtpWebResponse)request.GetResponse();

				Stream responseStream = response.GetResponseStream();

				return responseStream;
			}
			catch (Exception)
			{

				return null;
			}
		}
	}
}