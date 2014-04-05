using System;
using System.Net;

namespace qiniu
{
	/// <summary>
	/// Qiniu web exception.
	/// </summary>
	public class QiniuWebException:Exception
	{
	
		#region
		private QiniuErrors error;
		private string x_Reqid;
		private string x_Log;
		#endregion

		/// <summary>
		/// Gets the x_reqid.
		/// </summary>
		/// <value>The x_reqid.</value>
		public string X_Reqid {
			get { return x_Reqid; }
		}

		/// <summary>
		/// Gets the x_log.
		/// </summary>
		/// <value>The x_log.</value>
		public string X_Log {
			get { return x_Log; }
		}

		/// <summary>
		/// Gets the error.
		/// </summary>
		/// <value>The error.</value>
		public QiniuErrors Error {
			get { return error; }
		}
			
		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuWebException"/> class.
		/// </summary>
		/// <param name="we">We.</param>
		public QiniuWebException (WebException we) {
			if (we.Response is HttpWebResponse) {
				HttpWebResponse hwr = we.Response as HttpWebResponse;
				this.error = new QiniuErrors ((int)hwr.StatusCode);
				this.x_Reqid = hwr.Headers ["X-Reqid"];
				this.x_Log = hwr.Headers ["X-Log"];
			}
		}
	}
}

