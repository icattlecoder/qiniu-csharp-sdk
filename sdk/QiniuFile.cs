using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Encoding = System.Text.UTF8Encoding;

namespace qiniu
{
	public class QiniuFile
	{

		#region events
		/// <summary>
		/// Occurs when upload progress changed.
		/// </summary>
		public event EventHandler<QiniuUploadProgressChangedEventArgs> UploadProgressChanged;
		protected void onQiniuUploadProgressChanged(QiniuUploadProgressChangedEventArgs e){
			if (this.UploadProgressChanged != null) {
				UploadProgressChanged (this,e);
			}
		}

		/// <summary>
		/// Occurs when upload failed.
		/// </summary>
		public event EventHandler<QiniuUploadFailedEventArgs> UploadFailed;
		protected void onUploadFailed(QiniuUploadFailedEventArgs e){
			if (this.UploadFailed != null) {
				UploadFailed (this,e);
			}
		}

		/// <summary>
		/// Occurs when upload completed.
		/// </summary>
		public event EventHandler<QiniuUploadCompletedEventArgs> UploadCompleted;
		protected void onQiniuUploadCompleted(QiniuUploadCompletedEventArgs e){
			if (this.UploadCompleted != null) {
				UploadCompleted (this,e);
			}
		}

		/// <summary>
		/// Occurs when upload block completed.
		/// </summary>
		public event EventHandler<QiniuUploadBlockCompletedEventArgs> UploadBlockCompleted;
		protected void onQiniuUploadBlockCompleted(QiniuUploadBlockCompletedEventArgs e){
			if (this.UploadBlockCompleted != null) {
				UploadBlockCompleted (this,e);
			}
		}

		/// <summary>
		/// Occurs when upload block failed.
		/// </summary>
		public event EventHandler<QiniuUploadBlockFailedEventArgs> UploadBlockFailed;
		protected void onQiniuUploadBlockFailed(QiniuUploadBlockFailedEventArgs e){
			if (this.UploadBlockFailed != null) {
				UploadBlockFailed (this,e);
			}
		}

		/// <summary>
		/// Occurs when upload canceled.
		/// </summary>
		public event EventHandler UploadCanceled;
		protected void onQiniuUploadCanceld(){
			UploadCanceled (this, null);
		}
		#endregion

		#region localVars
		private string bucketName;
		private string key;
		private string localfile;
		private bool uploading = false;
		#endregion

		#region construct
		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuFile"/> class.
		/// </summary>
		public QiniuFile (string bucketName)
		{
			this.bucketName = bucketName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuFile"/> class.
		/// </summary>
		/// <param name="bucket">Bucket.</param>
		/// <param name="key">Key.</param>
		public QiniuFile(string bucketName,string key){
			this.bucketName = bucketName;
			this.key = key;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuFile"/> class.
		/// </summary>
		/// <param name="bucket">Bucket.</param>
		/// <param name="key">Key.</param>
		/// <param name="localfile">Localfile.</param>
		public QiniuFile(string bucketName,string key,string localfile){
			this.bucketName = bucketName;
			this.key = key;
			if (!File.Exists(localfile))
			{
				throw new Exception(string.Format("{0} does not exist", localfile));
			}
			this.localfile = localfile;
		}
		#endregion

		/// <summary>
		/// Stat the QiniuFile with mac.
		/// </summary>
		/// <param name="mac">Mac.</param>
		public QiniuFileInfo Stat (MAC mac=null){
			if (mac == null)
				mac = new MAC ();
			string url = string.Format ("{0}/{1}/{2}", Config.RS_HOST, "stat", Base64URLSafe.Encode (this.bucketName + ":" + this.key));
			try {
				using (QiniuWebClient client = new QiniuWebClient ()) {
					string result = client.Call (url, mac);
					return GetQiniuEntry (result);
				}
			} catch (WebException e) {
				throw new QiniuWebException(e);
			}catch(Exception e){
				throw e;
			}
		}	

		/// <summary>
		/// Delete the QiniuFile with mac.
		/// </summary>
		/// <param name="mac">Mac.</param>
		public bool Delete(MAC mac=null){

			if (mac == null)
				mac = new MAC ();
			string url = string.Format ("{0}/{1}/{2}", Config.RS_HOST, "delete", Base64URLSafe.Encode (this.bucketName + ":" + this.key));
			try {
				using (QiniuWebClient client = new QiniuWebClient ()) {
					client.Call (url, mac);
					return true;
				}
			} catch (WebException e) {
				throw new QiniuWebException(e);
			} catch (Exception e) {
				throw e;
			}
		}

		/// <summary>
		/// Move the specified destBucket, destKey and mac.
		/// </summary>
		/// <param name="destBucket">Destination bucket.</param>
		/// <param name="destKey">Destination key.</param>
		/// <param name="mac">Mac.</param>
		public bool Move(string destBucket,string destKey,MAC mac=null){
			if (mac == null) {
				mac = new MAC ();
			}
			string url = string.Format ("{0}/{1}/{2}/{3}",
				Config.RS_HOST,
				"move",
				Base64URLSafe.Encode (this.bucketName+":"+this.key),
				Base64URLSafe.Encode (destBucket+":"+destKey));
			try {
				using (QiniuWebClient client = new QiniuWebClient ()) {
					client.Call (url, mac);
					return true;
				}
			} catch (WebException e) {
				throw new QiniuWebException(e);
			} catch (Exception e) {
				throw e;
			}
		}

		/// <summary>
		/// Uploads the string.
		/// </summary>
		/// <param name="base64Content">Base64 content.</param>
		public void UploadString(int filesize,string mimeType,string base64Content){
			string token = getDefalutToken (this.bucketName, this.key);
			UploadString (token, filesize, mimeType, base64Content);
		}

		/// <summary>
		/// Uploads the string.
		/// </summary>
		/// <param name="token">Token.</param>
		/// <param name="base64Content">Base64 content.</param>
		public void UploadString(string token,int fileSize,string mimeType,string base64Content){
			using (QiniuWebClient qwc = new QiniuWebClient ()) {
				qwc.UpToken = token;
				string url = Config.UP_HOST +
					string.Format("/putb64/{0}/key/{1}/mimeType/{2}",
						fileSize,
						Base64URLSafe.Encode(this.key),
						Base64URLSafe.Encode(mimeType));

				qwc.UploadStringCompleted += (sender, e) => {
					if (e.Error != null && e.Error is WebException) {
						if (e.Error is WebException) {
							QiniuWebException qwe = new QiniuWebException (e.Error as WebException);
							onUploadFailed (new QiniuUploadFailedEventArgs (qwe));
						} else {	
							onUploadFailed (new QiniuUploadFailedEventArgs (e.Error));
						}
					} else {
						onQiniuUploadCompleted(new QiniuUploadCompletedEventArgs(e.Result));

						onQiniuUploadCompleted (new QiniuUploadCompletedEventArgs (e.Result));
					}
				};	
			
				qwc.UploadProgressChanged += (sender, e) => {
					onQiniuUploadProgressChanged (new QiniuUploadProgressChangedEventArgs (e.BytesSent, e.TotalBytesToSend));
				};

				qwc.Headers.Add("Content-Type", "application/octet-stream");
				qwc.UploadStringAsync (new Uri (url), "POST", base64Content);
			}
		}	

		/// <summary>
		/// Asyncs the upload.
		/// </summary>
		public void Upload(BlkputRet[] blkRets=null){
			if (!uploading) {
				string token = getDefalutToken (this.bucketName,this.key);
				Upload (token,blkRets);
			}
		}

		/// <summary>
		/// Upload the specified token.
		/// </summary>
		/// <param name="token">Token.</param>
		public void Upload(string token,BlkputRet[] blkRets=null){
			if (uploading) {
				return;
			}

			FileInfo finfo = new FileInfo (this.localfile);
			if (finfo.Length < (long)(1024 * 1024 * 8)) {
				uploadSmallFile (token);
			} else {
				uploadBigFile (token,blkRets);
			}
		}

		/// <summary>
		/// Uploads the small file ( < 8MB).
		/// </summary>
		/// <param name="token">Token.</param>
		private void uploadSmallFile(string token){
			if (uploading) {
				return;
			}
			uploading = true;
			NameValueCollection formData = new NameValueCollection ();
			formData ["key"] = this.key;
			formData ["token"] = token;
			try {
				using (QiniuWebClient qwc = new QiniuWebClient ()) {
					qwc.UploadDataCompleted += (sender, e) => {
						if (e.Error != null && e.Error is WebException) {
							if (e.Error is WebException) {
								QiniuWebException qwe = new QiniuWebException (e.Error as WebException);
								onUploadFailed (new QiniuUploadFailedEventArgs (qwe));
							} else {	
								onUploadFailed (new QiniuUploadFailedEventArgs (e.Error));
							}
						} else {
							onQiniuUploadCompleted (new QiniuUploadCompletedEventArgs (e.Result));
						}
					};
					qwc.UploadProgressChanged += (sender, e) => {
						onQiniuUploadProgressChanged (new QiniuUploadProgressChangedEventArgs (e.BytesSent, e.TotalBytesToSend));
					};
					qwc.AsyncPostForm (Config.UP_HOST, formData, this.localfile);
				}
			} catch (WebException we) {
				onUploadFailed (new QiniuUploadFailedEventArgs (new QiniuWebException (we)));
			} catch (Exception e) {
				onUploadFailed (new QiniuUploadFailedEventArgs (e));
			} finally {
				uploading = false;
			}
		}	

		/// <summary>
		/// Uploads the big file ( > 8MB ).
		/// </summary>
		/// <param name="token">Token.</param>
		private void uploadBigFile(string token,BlkputRet[] puttedBlk=null){
			uploading = true;
			Action a = () => {
				FileInfo finfo = new FileInfo (this.localfile);
				int blockcount = block_count (finfo.Length);

				BlkputRet []blkRets = new BlkputRet[blockcount];
				using (FileStream fs = File.OpenRead (this.localfile)) {
					long totalSent = 0;
					int readLen = BLOCKSIZE;
					byte[] buf = new byte[readLen];
					for (int i = 0; i < blockcount; i++) {
						if (puttedBlk!=null&&i< puttedBlk.Length) {
							blkRets[i] = puttedBlk[i];
							totalSent +=(long)blkRets[i].offset;
							continue;
						}
						if (i == blockcount - 1) {
							readLen = (int)(finfo.Length - i * BLOCKSIZE);
							buf = new byte[readLen];
						}
						fs.Seek ((long)i * BLOCKSIZE, SeekOrigin.Begin);
						fs.Read (buf, 0, readLen);
						using (QiniuWebClient client = new QiniuWebClient ()) {
							bool failed = false;
							client.UploadDataCompleted += (sender, e) => {
								if (e.Error != null) {
									onQiniuUploadBlockFailed (new QiniuUploadBlockFailedEventArgs (i, e.Error));
									failed =true;
									return;
								} else {
									blkRets [i] = GetBlkPutRet (e.Result);
									onQiniuUploadBlockCompleted (new QiniuUploadBlockCompletedEventArgs (i, blkRets [i]));
								}
							};
							client.UploadProgressChanged += (sender, e) => {
								onQiniuUploadProgressChanged (new QiniuUploadProgressChangedEventArgs (totalSent + e.BytesSent, finfo.Length));
							};
							client.Timeout+= (sender, e) => {
								onQiniuUploadBlockFailed(new QiniuUploadBlockFailedEventArgs(i,new Exception("QiniuWebClient Timeout.")));
								failed = true;
							};
							client.UpToken = token;
							client.Headers.Add("Content-Type", "application/octet-stream");
							string url = string.Format("{0}/mkblk/{1}", Config.UP_HOST, readLen);
							client.iUploadDataAsync ( url, "POST", buf);
							if(failed){
								return;
							}
							totalSent += readLen;
						}
					}
				}
				try {
					byte[] result = Mkfile (blkRets, token, this.key, finfo.Length);
					if (result != null && result.Length > 0) {
						onQiniuUploadCompleted (new QiniuUploadCompletedEventArgs (result));
					}
				} catch (Exception e) {
					onUploadFailed (new QiniuUploadFailedEventArgs (e));
				}
			};
			a.BeginInvoke (null, null);
		}	

		/// <summary>
		/// Mkfile the specified blkRets, key and fsize.
		/// </summary>
		/// <param name="blkRets">Blk rets.</param>
		/// <param name="key">Key.</param>
		/// <param name="fsize">Fsize.</param>
		private static byte[] Mkfile(BlkputRet[] blkRets,string token, string key, long fsize)
		{
			StringBuilder urlBuilder = new StringBuilder();
			urlBuilder.AppendFormat("{0}/mkfile/{1}", Config.UP_HOST, fsize);
			if (!string.IsNullOrEmpty(key))
			{
				urlBuilder.AppendFormat("/key/{0}", Base64URLSafe.ToBase64URLSafe(key));
			}
			int proCount = blkRets.Length;
			using (Stream body = new MemoryStream())
			{
				for (int i = 0; i < proCount; i++)
				{
					byte[] bctx = Encoding.ASCII.GetBytes(blkRets[i].ctx);
					body.Write(bctx, 0, bctx.Length);
					if (i != proCount - 1)
					{
						body.WriteByte((byte)',');
					}
				}
				body.Seek(0, SeekOrigin.Begin);
				byte[] data = new byte[body.Length];
				body.Read (data, 0, (int)body.Length);
				using(QiniuWebClient client = new QiniuWebClient ()){
					client.UpToken = token;
					client.Headers.Add("Content-Type", "application/octet-stream");
					return	client.UploadData (urlBuilder.ToString(), "POST", data);
				}
			}
		}

		#region Config
		private const int blockBits = 22;
		private readonly static int BLOCKSIZE = 1 << blockBits;
		private readonly static int blockMask = BLOCKSIZE - 1;
		private static int block_count(long fsize)
		{
			return (int)((fsize + blockMask) >> blockBits);
		}
		private static BlkputRet GetBlkPutRet(byte[] result){
			return JsonConvert.DeserializeObject<BlkputRet> (Encoding.UTF8.GetString (result));
		}
		private static QiniuFileInfo GetQiniuEntry(string result){
			return JsonConvert.DeserializeObject<QiniuFileInfo> (result);
		}
		private static string getDefalutToken(string bucketName,string key){
			string scope = bucketName;
			if(!string.IsNullOrEmpty(key)){
				scope += ":" + key;
			}
			PutPolicy policy = new PutPolicy (scope, 3600 * 24);
			return policy.Token ();
		}
		#endregion
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class BlkputRet
	{
		[JsonProperty("ctx")]
		public string ctx;
		[JsonProperty("checksum")]
		public string checkSum;
		[JsonProperty("crc32")]
		public UInt32 crc32;
		[JsonProperty("offset")]
		public ulong offset;
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class QiniuFileInfo 
	{
		/// <summary>
		/// 文件的Hash值
		/// </summary>
		/// <value><c>true</c> if this instance hash; otherwise, <c>false</c>.</value>
		[JsonProperty("hash")]
		public string Hash;

		/// <summary>
		/// 文件的大小(单位: 字节)
		/// </summary>
		/// <value>The fsize.</value>
		[JsonProperty("fsize")]
		public long Fsize;

		/// <summary>
		/// 文件上传到七牛云的时间(Unix时间戳)
		/// </summary>
		/// <value>The put time.</value>
		[JsonProperty("putTime")]
		public long PutTime;

		/// <summary>
		/// 文件的媒体类型，比如"image/gif"
		/// </summary>
		/// <value>The type of the MIME.</value>
		[JsonProperty("mimeType")]
		public string MimeType;

		/// <summary>
		/// Gets the customer.
		/// </summary>
		/// <value>The customer.</value>
		[JsonProperty("customer")]
		public string Customer;

	}
}

