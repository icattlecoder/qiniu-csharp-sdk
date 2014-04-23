using System;
using System.IO;

namespace demo
{
	public class jpegToBase64
	{
		string filename;
		long filesize;
		string base64Content;

		public string Filename {
			get { return filename; }
		}

		public long Filesize {
			get { return filesize; }
		}

		public string Base64Content {
			get { return base64Content; }
		}

		public jpegToBase64 (string filename)
		{
			if (!File.Exists (filename)) {
				throw new ArgumentException (filename + " not exists");
			}
			this.filename = filename;
			FileInfo finfo = new FileInfo (filename);
			this.filesize = finfo.Length;
			using (FileStream fs = finfo.OpenRead ()) {
				byte[] content = new byte[this.filesize]; 
				fs.Read (content, 0, (int)this.filesize);
				this.base64Content = Convert.ToBase64String (content);
			}
		}	
	}
}

