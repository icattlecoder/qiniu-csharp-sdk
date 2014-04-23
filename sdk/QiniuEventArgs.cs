using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Encoding = System.Text.UTF8Encoding;
namespace qiniu
{
	/// <summary>
	/// Qiniu upload failed event arguments.
	/// </summary>
	public class QiniuUploadFailedEventArgs: EventArgs
	{
		private Exception error;

		/// <summary>
		/// Gets the error.
		/// </summary>
		/// <value>The error.</value>
		public Exception Error {
			get { return error; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuUploadFailedEventArgs"/> class.
		/// </summary>
		/// <param name="e">E.</param>
		public  QiniuUploadFailedEventArgs(Exception e){
			this.error = e;
		}
	}

	/// <summary>
	/// Qiniu upload completed event arguments.
	/// </summary>
	public class QiniuUploadCompletedEventArgs:EventArgs{

		/// <summary>
		/// 上传结果的原始JOSN字符串。
		/// </summary>
		/// <value>The raw string.</value>
		public string RawString { get; private set;}

		/// <summary>
		/// 如果 uptoken 没有指定 ReturnBody，那么返回值是标准的 PutRet 结构
		/// </summary>
		public string Hash { get; private set; }

		/// <summary>
		/// 如果传入的 key == UNDEFINED_KEY，则服务端返回 key
		/// </summary>
		public string key { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuUploadCompletedEventArgs"/> class.
		/// </summary>
		/// <param name="result">Result.</param>
		public QiniuUploadCompletedEventArgs(byte[] result){
			if(result!=null){
				try
				{
					string json  = Encoding.UTF8.GetString(result);
					this.RawString = json;
					Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
					object tmp;
					if (dict.TryGetValue("hash", out tmp))
						Hash = (string)tmp;
					if (dict.TryGetValue("key", out tmp))
						key = (string)tmp;
				}
				catch (Exception e)
				{
					//throw e;
				}
			}
		}

		public QiniuUploadCompletedEventArgs(string result){
			this.RawString = result;
			Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>> (result);
			object tmp;
			if (dict.TryGetValue ("hash", out tmp))
				Hash = (string)tmp;
			if (dict.TryGetValue ("key", out tmp))
				key = (string)tmp;
					
		}	
	}

	/// <summary>
	/// Qiniu upload progress changed event arguments.
	/// </summary>
	public class QiniuUploadProgressChangedEventArgs:EventArgs{
		#region
		private long bytesSent;
		private long totalBytes;
		#endregion

		/// <summary>
		/// Gets the bytes sent.
		/// </summary>
		/// <value>The bytes sent.</value>
		public long BytesSent {
			get { return bytesSent; }
		}

		/// <summary>
		/// Gets the total sent.
		/// </summary>
		/// <value>The total sent.</value>
		public long TotalBytes {
			get { return totalBytes; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuUploadProgressChangedEventArgs"/> class.
		/// </summary>
		/// <param name="sent">Sent.</param>
		/// <param name="total">Total.</param>
		public QiniuUploadProgressChangedEventArgs(long sent,long total){
			this.bytesSent = sent;
			this.totalBytes = total;
		}
	}

	/// <summary>
	/// Qiniu upload block completed event arguments.
	/// </summary>
	public class QiniuUploadBlockCompletedEventArgs:EventArgs{
		#region vars
		private int index;
		private BlkputRet ctx;
		#endregion

		/// <summary>
		/// Gets the index.
		/// </summary>
		/// <value>The index.</value>
		public int Index {
			get { return index; }
		}

		/// <summary>
		/// Gets the context.
		/// </summary>
		/// <value>The context.</value>
		public BlkputRet Ctx {
			get { return ctx; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuUploadBlockCompletedEventArgs"/> class.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="ctx">Context.</param>
		public QiniuUploadBlockCompletedEventArgs(int index ,BlkputRet ctx){
			this.index = index;
			this.ctx = ctx;
		}
	}

	/// <summary>
	/// Qiniu upload block failed event arguments.
	/// </summary>
	public class QiniuUploadBlockFailedEventArgs:EventArgs{

		#region
		private int index;
		private Exception error;
		#endregion

		/// <summary>
		/// Gets the index.
		/// </summary>
		/// <value>The index.</value>
		public int Index {
			get { return index; }
		}

		/// <summary>
		/// Gets the error.
		/// </summary>
		/// <value>The error.</value>
		public Exception Error {
			get { return error; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuUploadBlockFailedEventArgs"/> class.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="e">E.</param>
		public QiniuUploadBlockFailedEventArgs(int index,Exception e){
			this.index = index;
			this.error = e;
		}
	}
}

