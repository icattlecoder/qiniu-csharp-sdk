using System;
using qiniu;
using System.Threading;

namespace demo
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// 初始化qiniu配置，主要是API Keys
			qiniu.Config.ACCESS_KEY = "IT9iP3J9wdXXYsT1p8ns0gWD-CQOdLvIQuyE0FOi";
			qiniu.Config.SECRET_KEY = "zUCzekBtEqTZ4-WJPCGlBrr2PeyYxsYn98LxaivM";

			/**********************************************************************
			可以用下面的方法从配置文件中初始化
			qiniu.Config.InitFromAppConfig ();
			**********************************************************************/


			//======================================================================
			{
				QiniuFile qfile = new QiniuFile ("<input your bucket name>", "<input qiniu file key>", "<local disk file path");
				ManualResetEvent done = new ManualResetEvent (false);
				qfile.UploadCompleted += (sender, e) => {
					Console.WriteLine (e.RawString);
					done.Set ();
				};
				qfile.UploadFailed += (sender, e) => {
					Console.WriteLine (e.Error.ToString ());
					done.Set ();
				};
				qfile.UploadProgressChanged += (sender, e) => {
					int percentage = (int)(100 * e.BytesSent / e.TotalBytes);
					Console.Write (percentage);
				};
				// 上传为异步操作
				//上传本地文件到七牛云存储
				qfile.Upload ();
				done.WaitOne ();
			}

			//======================================================================
			{

				try {
					QiniuFile qfile = new QiniuFile ("<input your bucket Name>", "<input qiniu file key>");
					QiniuFileInfo finfo = qfile.Stat ();
					if (finfo != null) {
						//删除七牛云空间的文件
						qfile.Delete ();
					}
				} catch (QiniuWebException e) {
					Console.WriteLine (e.Error.HttpCode);
					Console.WriteLine (e.Error.ToString ());
				}
			}
		}	
	}	
}
