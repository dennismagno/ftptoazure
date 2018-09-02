using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace AzureAPI.Providers
{
	public class AzureStorageMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
	{
		private readonly CloudBlobContainer _blobContainer;
		private readonly string[] _supportedMimeTypes = { "image/png", "image/jpeg", "image/jpg" };

		public AzureStorageMultipartFormDataStreamProvider(CloudBlobContainer blobContainer) : base("azure")
		{
			_blobContainer = blobContainer;
		}

		public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
		{
			if (parent == null) throw new ArgumentNullException(nameof(parent));
			if (headers == null) throw new ArgumentNullException(nameof(headers));

			if (!_supportedMimeTypes.Contains(headers.ContentType.ToString().ToLower()))
			{
				throw new NotSupportedException("Only jpeg and png are supported");
			}

			ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
			var fileName = "";
			if (contentDisposition != null)
			{
				fileName = contentDisposition.FileName.Replace("\"", "");
			} else
			{
				fileName = Guid.NewGuid().ToString();
			}			
			

			CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(fileName);

			if (headers.ContentType != null)
			{
				// Set appropriate content type for your uploaded file
				blob.Properties.ContentType = headers.ContentType.MediaType;
			}

			this.FileData.Add(new MultipartFileData(headers, blob.Name));

			return blob.OpenWrite();
		}
	}
}
