using System;
using Encoding = System.Text.UTF8Encoding;

namespace qiniu
{
	public static class Base64URLSafe
	{
		public static string Encode (string text)
		{
			if (String.IsNullOrEmpty (text))
				return "";
			byte[] bs = Encoding.UTF8.GetBytes (text);
			string encodedStr = Convert.ToBase64String (bs);
			encodedStr = encodedStr.Replace ('+', '-').Replace ('/', '_');
			return encodedStr;
		}

		/// <summary>
		/// To the base64 safe URL.
		/// </summary>
		/// <returns>The base64 URL safe.</returns>
		/// <param name="str">String.</param>
		public static string ToBase64URLSafe (string str)
		{
			return Encode (str);
		}

		/// <summary>
		/// Encode the specified bs.
		/// </summary>
		/// <param name="bs">Bs.</param>
		public static string Encode (byte[] bs)
		{
			if (bs == null || bs.Length == 0)
				return "";
			string encodedStr = Convert.ToBase64String (bs);
			encodedStr = encodedStr.Replace ('+', '-').Replace ('/', '_');
			return encodedStr;
		}
	}
}

