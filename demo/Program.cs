using System;
using System.Collections;
using System.Collections.Generic;
using qiniu;
using System.Threading;

namespace demo
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// 初始化qiniu配置，主要是API Keys
			qiniu.Config.ACCESS_KEY = "IT9iP3J9wdXXYsT1p8ns0gWD-CQOdLvIQuyE0FOk";
			qiniu.Config.SECRET_KEY = "zUCzekBtEqTZ4-WJPCGlBrr2PeyYxsYn98LPaivM";

			/**********************************************************************
			可以用下面的方法从配置文件中初始化
			qiniu.Config.InitFromAppConfig ();
			**********************************************************************/

			string localfile = "/Users/icattlecoder/Movies/tzd.rmvb";
			string bucket = "icattlecoder";
			string qiniukey = "tzd.rmvb";
			
			//======================================================================
			{
				QiniuFile qfile = new QiniuFile (bucket, qiniukey, localfile);
//				ResumbleUploadEx puttedCtx = new ResumbleUploadEx (localfile);
				ManualResetEvent done = new ManualResetEvent (false);
				qfile.UploadCompleted += (sender, e) => {
					Console.WriteLine (e.key);
					Console.WriteLine (e.Hash);
					done.Set ();
				};
				qfile.UploadFailed += (sender, e) => {
					Console.WriteLine (e.Error.ToString ());
//					puttedCtx.Save();
					done.Set ();
				};

				qfile.UploadProgressChanged += (sender, e) => {
					int percentage = (int)(100 * e.BytesSent / e.TotalBytes);
					Console.Write (percentage);
				};
				qfile.UploadBlockCompleted += (sender, e) => {
//					puttedCtx.Add(e.Index,e.Ctx);
//					puttedCtx.Save();
				};
				qfile.UploadBlockFailed += (sender, e) => {
					//

				};

				//上传为异步操作
				//上传本地文件到七牛云存储
//				qfile.Upload (puttedCtx.PuttedCtx);
				qfile.Upload ();
				done.WaitOne ();
			}

			//======================================================================
			{
				/*

				try {
					QiniuFile qfile = new QiniuFile (bucket, qiniukey);
					QiniuFileInfo finfo = qfile.Stat ();
					if (finfo != null) {
						qfile.Move("cloudcomment","movetest");
						//删除七牛云空间的文件
						//qfile.Delete ();
					}
				} catch (QiniuWebException e) {
					Console.WriteLine (e.Error.HttpCode);
					Console.WriteLine (e.Error.ToString ());
				}
				*/
			}
		}	
	}	
}
