using System;
using System.IO;
using System.Collections.Specialized;
using System.Net;

namespace qiniu
{
	public class QiniuWebClient:WebClient
	{
		private string uptoken;

		/// <summary>
		/// Gets or sets upload token.
		/// </summary>
		/// <value>Up token.</value>
		public string UpToken {
			get {
				return uptoken;
			}
			set {
				uptoken = value;
				this.Headers.Add ("Authorization", "UpToken " + this.uptoken);
			}	
		}

		/// <summary>
		/// Posts the form.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="values">Values.</param>
		/// <param name="filename">Filename.</param>
		public void AsyncPostForm(string url,NameValueCollection formData,string filename){
			FileInfo fileInfo = new FileInfo (filename);
			string boundary = RandomBoundary ();
			using (FileStream fileStream = fileInfo.OpenRead())
			using (Stream postDataStream = GetPostStream (fileStream, formData ["key"], formData, boundary)) {
				this.Headers.Add ("Content-Type", "multipart/form-data; boundary=" + boundary);
				byte[] hugeBuffer = new byte[postDataStream.Length];
				postDataStream.Seek (0, SeekOrigin.Begin);
				postDataStream.Read (hugeBuffer, 0, (int)postDataStream.Length);
				UploadDataAsync (new Uri(url), "POST", hugeBuffer);
			}	
		}

		/// <summary>
		/// Posts the form.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="values">Values.</param>
		/// <param name="filename">Filename.</param>
		public byte[] PostForm(string url,NameValueCollection formData,string filename){
			FileInfo fileInfo = new FileInfo (filename);
			string boundary = RandomBoundary ();
			using (FileStream fileStream = fileInfo.OpenRead())
			using (Stream postDataStream = GetPostStream (fileStream, formData ["key"], formData, boundary)) {
				this.Headers.Add ("Content-Type", "multipart/form-data; boundary=" + boundary);
				byte[] hugeBuffer = new byte[postDataStream.Length];
				postDataStream.Seek (0, SeekOrigin.Begin);
				postDataStream.Read (hugeBuffer, 0, (int)postDataStream.Length);
				return UploadData (new Uri(url), "POST", hugeBuffer);
			}	
		}

		/// <summary>
		/// Call the specified url and mac.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="mac">Mac.</param>
		public string Call(string url,MAC mac){
			try{
			HttpWebRequest req = this.GetWebRequest (new Uri (url)) as HttpWebRequest;
			mac.SignRequest (req, null);
			using (HttpWebResponse response = req.GetResponse() as HttpWebResponse) {
				using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
					return reader.ReadToEnd ();
				}
			}
			}catch(Exception e){
				throw e;
			}
		}

		private Stream GetPostStream (Stream putStream, string fileName, NameValueCollection formData, string boundary)
		{
			Stream postDataStream = new System.IO.MemoryStream ();

			//adding form data

			string formDataHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
				"Content-Disposition: form-data; name=\"{0}\";" + Environment.NewLine + Environment.NewLine + "{1}";

			foreach (string key in formData.Keys) {
				byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes (string.Format (formDataHeaderTemplate,
					key, formData [key]));
				postDataStream.Write (formItemBytes, 0, formItemBytes.Length);
			}

			//adding file,Stream data
			#region adding file data

			string fileHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
				"Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
				Environment.NewLine + "Content-Type: application/octet-stream" + Environment.NewLine + Environment.NewLine;
			byte[] fileHeaderBytes = System.Text.Encoding.UTF8.GetBytes (string.Format (fileHeaderTemplate,
				"file", fileName));
			postDataStream.Write (fileHeaderBytes, 0, fileHeaderBytes.Length);

			byte[] buffer = new byte[1024*1024];
			int bytesRead = 0;
			while ((bytesRead = putStream.Read(buffer, 0, buffer.Length)) != 0) {
				postDataStream.Write (buffer, 0, bytesRead);
			}
			putStream.Close ();
			#endregion

			#region adding end
			byte[] endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes (Environment.NewLine + "--" + boundary + "--" + Environment.NewLine);
			postDataStream.Write (endBoundaryBytes, 0, endBoundaryBytes.Length);
			#endregion

			return postDataStream;

		}

		private string RandomBoundary (){
			return String.Format ("----------{0:N}", Guid.NewGuid ());
		}
	}
}

