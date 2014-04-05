using System;
using System.IO;
using System.Security.Cryptography;
using Encoding = System.Text.UTF8Encoding;

namespace qiniu
{
	public class MAC
	{

		private string accessKey;
		private byte[] secretKey;

		/// <summary>
		/// Gets or sets the access key.
		/// </summary>
		/// <value>The access key.</value>
		public string AccessKey {
			get { return accessKey; }
			set { accessKey = value; }
		}	

		/// <summary>
		/// Gets or sets the secret key.
		/// </summary>
		/// <value>The secret key.</value>
		public string SecretKey {
			set { secretKey = Encoding.UTF8.GetBytes(value); }
		}	

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.MAC"/> class.
		/// </summary>
		/// <param name="accessKey">Access key.</param>
		/// <param name="secretKey">Secret key.</param>
		public MAC (string accessKey,string secretKey )
		{
			this.accessKey = accessKey;
			SecretKey = secretKey;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.MAC"/> class.
		/// Get accesskey and secretKey from the Config
		/// </summary>
		public MAC()
		{
			this.accessKey = Config.ACCESS_KEY;
			SecretKey = Config.SECRET_KEY;
		}

		private string _sign (byte[] data)
		{
			HMACSHA1 hmac = new HMACSHA1 (this.secretKey);
			byte[] digest = hmac.ComputeHash (data);
			return Base64URLSafe.Encode (digest);
		}

		/// <summary>
		/// Sign
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public string Sign (byte[] b)
		{
			return string.Format ("{0}:{1}", this.accessKey, _sign (b));
		}


		/// <summary>
		/// SignWithData
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public string SignWithData (byte[] b)
		{
			string data = Base64URLSafe.Encode (b);
			return string.Format ("{0}:{1}:{2}", this.accessKey, _sign (Encoding.UTF8.GetBytes (data)), data);
		}

		/// <summary>
		/// SignRequest
		/// </summary>
		/// <param name="request"></param>
		/// <param name="body"></param>
		/// <returns></returns>
		public void SignRequest (System.Net.HttpWebRequest request, byte[] body)
		{
			Uri u = request.Address;
			using (HMACSHA1 hmac = new HMACSHA1(secretKey)) {
				string pathAndQuery = request.Address.PathAndQuery;
				byte[] pathAndQueryBytes = Encoding.UTF8.GetBytes (pathAndQuery);
				using (MemoryStream buffer = new MemoryStream()) {
					buffer.Write (pathAndQueryBytes, 0, pathAndQueryBytes.Length);
					buffer.WriteByte ((byte)'\n');
					if (body!=null&&body.Length > 0) {
						buffer.Write (body, 0, body.Length);
					}
					byte[] digest = hmac.ComputeHash (buffer.ToArray ());
					string digestBase64 = Base64URLSafe.Encode (digest);
					request.Headers.Add ("Authorization", "QBox " + this.accessKey+":"+digestBase64);
				}
			}
		}

	}
}

