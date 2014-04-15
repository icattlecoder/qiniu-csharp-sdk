using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace qiniu
{
	/// <summary>
	/// 断点续传.
	/// </summary>
	public class ResumbleUploadEx
	{
		private string puttedCtxDir;
		private string fileName;
		private string puttedCtxFileName;
		private Dictionary<int,BlkputRet> puttedCtx;

		/// <summary>
		/// 获取已上传的结果.
		/// </summary>
		/// <value>The putted context.</value>
		public BlkputRet[] PuttedCtx {
			get {
				if (this.puttedCtx == null || this.puttedCtx.Count == 0)
					return null;
				BlkputRet[] result = new BlkputRet[this.puttedCtx.Count];
				for (int i = 0; i < this.puttedCtx.Count; i++) {
					if (this.puttedCtx.ContainsKey (i)) {
						result [i] = this.puttedCtx [i];
					} else {
						this.Clear ();
						return null;
					}
				}
				return result;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="demo.ResumbalePutEx"/> class.
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <param name="puttedCtxDir">断点续传上传结果持久化存放目录. 默认存放在系统临时文件夹。</param>
		public ResumbleUploadEx (string filename, string puttedCtxDir=null){
			if (!File.Exists (filename)) {
				throw new Exception(string.Format("{0} does not exist", filename));
			}
			this.fileName = filename;
			if (puttedCtxDir != null) {
				if (!Directory.Exists (puttedCtxDir)) {
					try{
						Directory.CreateDirectory (puttedCtxDir);
					}
					catch(Exception e){
						throw e;
					}
				}
				this.puttedCtxDir = puttedCtxDir;
			} else {
				this.puttedCtxDir = Path.GetTempPath ();
			}
			string ctxfile = getFileBase64Sha1 (this.fileName);
			this.puttedCtxFileName = Path.Combine (this.puttedCtxDir, ctxfile);
			if (!File.Exists (this.puttedCtxFileName)) {
				File.Open (this.puttedCtxFileName, FileMode.Create);
				this.puttedCtx = new Dictionary<int, BlkputRet> ();
			} else {
				this.puttedCtx = initloadPuttedCtx ();
			}
		}

		private Dictionary<int,BlkputRet> initloadPuttedCtx(){

			Dictionary<int,BlkputRet> result = new Dictionary<int, BlkputRet> ();
			string[] lines = File.ReadAllLines(this.puttedCtxFileName);
			foreach (string line in lines) 
			{
				string[] fields = line.Split(',');
				BlkputRet ret = new BlkputRet();
				ret.offset = ulong.Parse(fields[1]);
				ret.ctx = fields[2];
				int idx = int.Parse(fields[0]);
				result.Add (idx, ret);
			}
			return result;
		}

		/// <summary>
		/// 保存持久化结果至磁盘文件
		/// </summary>
		public void Save(){
			StringBuilder sb = new StringBuilder ();
			foreach (int i in this.puttedCtx.Keys) {
				string content = i + "," + this.puttedCtx[i].offset + "," + this.puttedCtx[i].ctx + "\n";
				sb.Append (content);
			}
			File.WriteAllText (this.puttedCtxFileName, sb.ToString ());
		}

		/// <summary>
		/// 保存持久化结果至内存
		/// </summary>
		/// <param name="tempFileName"></param>
		/// <param name="idx"></param>
		/// <param name="ret"></param>
		public void Add(int idx, BlkputRet ret)
		{
			this.puttedCtx [idx] = ret;
		}

		/// <summary>
		/// 保存持久化结果至内存和文件.
		/// </summary>
		/// <param name="idx">Index.</param>
		/// <param name="ret">Ret.</param>
		public void AddAndSave(int idx,BlkputRet ret){
			this.Add (idx, ret);
			this.Save ();
		}

		/// <summary>
		/// 清除上传持久化结果
		/// </summary>
		public void Clear(){
			if (File.Exists (this.puttedCtxFileName)) {
				this.puttedCtx.Clear ();
				File.Delete (this.puttedCtxFileName);
			}
		}

		/// <summary>
		/// 获取文件的SHA1值
		/// </summary>
		/// <param name="filename"></param>
		/// <returns>base64编码的sha1值</returns>
		private static string getFileBase64Sha1(string filename) 
		{
			SHA1 sha1 = new SHA1CryptoServiceProvider();
			using (Stream reader = System.IO.File.OpenRead(filename))
			{
				byte[] result = sha1.ComputeHash(reader);
				return BitConverter.ToString(result);
			}
		}
	}
}

