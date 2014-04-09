qiniu-csharp-sdk
================

七牛云存储SDK

此SDK实现了七牛云存储的核心部分，即文件上传，目的是简化文件上传并提供更加便捷的编程接口，从更高层面进行了抽象，而非官方SDK那样，仅是对API的一套封装。

项目地址: [https://github.com/icattlecoder/qiniu-csharp-sdk](https://github.com/icattlecoder/qiniu-csharp-sdk)

- [初始化](#init)
- [上传文件](#upload)
	- [上传事件](#event)
	- [上传结果](#result)
	- [续传](#resumble)
- [文件操作](#rsop)
	- [查看信息](#stat)
	- [删除](#delete)
- [问题反馈](#issue)

<a id="init"></a>
# 初始化

初始化工作包括对七牛的API Keys的赋值，如：
```c#
qiniu.Config.ACCESS_KEY = "IT9iP3J9wdXXYsT1p8ns0gWD-CQOdLvIQuyE0FOK";
qiniu.Config.SECRET_KEY = "zUCzekBtEqTZ4-WJPCGlBrr2PeyYxsYn98LPaivM";
```
<a id="upload"></a>
# 上传文件

```c# 
QiniuFile qfile = new QiniuFile ("<input your bucket name>", "<input qiniu file key>", "<local disk file path>");
qfile.Upload();
```

一个QiniuFile对象表示一个七牛云空间的文件,初始化QiniuFile提供以下三个参数：
- `bucketName`，七牛云空间名称
- `key`，七牛文件key
- `localfile`，本地文件。该参数为可选，如果要上传本地的文件至七牛云空间，则需要指定此参数的值。

注意上传为异步操作，上传的结果由事件通知。

<a id="event"></a>
## 上传事件

共包括五个事件

事件名称 | 说明
:--------- | :---------
UploadCompleted;       		| 上传完成
UploadFailed;       		| 上传失败
UploadProgressChanged;      | 上传进度
UploadBlockCompleted;       | 上传块完成
UploadBlockFailed;       	| 上传块失败

前三个事件比较容易理解，后两个事件是根据七牛的大文件上传机制衍生出来的，合理利用这两个事件可以完成大文件分块上传结果持久化，从而实现续传。

<a id="result"></a>
## 上传结果
成功上传一个文件后，结果通过事件`uploadCompleted`获取得到，包括文件的`Hash`和`Key`以及从七牛云存储返回的原始字符串（主要考虑到上传凭证中指定了自定义的returnBody）。

## 续传

类`QiniuResumbleUploadEx`可用于续传，见示例。

<a id="rsop"></a>
## 文件操作
简单的实现了文件的基本信息获取及删除操作，分别为`Stat`和`Delete`

# 完整示例

```c#
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
			qiniu.Config.ACCESS_KEY = "IT9iP3J9wdXXYsT1p8ns0gWD-CQOdLvIQuyE0FOi";
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

				ResumbleUploadEx puttedCtx = new ResumbleUploadEx (localfile); //续传

				ManualResetEvent done = new ManualResetEvent (false);
				qfile.UploadCompleted += (sender, e) => {
					Console.WriteLine (e.key);
					Console.WriteLine (e.Hash);
					done.Set ();
				};
				qfile.UploadFailed += (sender, e) => {
					Console.WriteLine (e.Error.ToString ());
					puttedCtx.Save();
					done.Set ();
				};
				qfile.UploadProgressChanged += (sender, e) => {
					int percentage = (int)(100 * e.BytesSent / e.TotalBytes);
					Console.Write (percentage);
				};
				qfile.UploadBlockCompleted += (sender, e) => {
					//上传结果持久化
					puttedCtx.Add(e.Index,e.Ctx);
					puttedCtx.Save();
				};
				qfile.UploadBlockFailed += (sender, e) => {
					//
				};

				//上传为异步操作
				//上传本地文件到七牛云存储
				qfile.Upload ();
				
				//如果要续传，调用下面的方法
				//qfile.Upload (puttedCtx.PuttedCtx);

				done.WaitOne ();
			}

			//======================================================================
			{

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
			}
		}	
	}	
}

```

<a id="issue"></a>
# 问题反馈
[https://github.com/icattlecoder/qiniu-csharp-sdk/issues/new](https://github.com/icattlecoder/qiniu-csharp-sdk/issues/new)

