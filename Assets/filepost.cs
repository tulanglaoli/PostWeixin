using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

public class filepost : MonoBehaviour {

	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {
	
	}
	bool Show = false;
	void OnGUI()
	{
		if(GUI.Button (new Rect (0, 0, 100, 100), "Post"))
		{
			if(Show==false)
			{
				Show = true;
				StartCoroutine(MyCoroutine());
			}

		}
		if(Show)
		GUI.Label(new Rect(100,0,100,100),"Wait");
	}

	IEnumerator MyCoroutine()
	{

		NameValueCollection aaa = new NameValueCollection ();
		aaa.Add ("uploadHeader", "Img");
		
		UploadFile imagefile =new UploadFile();
		imagefile.Name = "uploadImg";
		imagefile.Filename = "1.jpg";
		imagefile.ContentType = "image/jpeg";
		imagefile.Data = System.IO.File.ReadAllBytes("1.jpg");
		UploadFile textfile =new UploadFile();
		textfile.Name = "uploadfile";
		textfile.Filename = "1.txt";
		textfile.ContentType = "text/plain";
		textfile.Data = System.IO.File.ReadAllBytes("1.txt");
		IEnumerable<UploadFile> files = new UploadFile[2]{imagefile,textfile};
		yield return new WaitForSeconds(2.0f);;
		byte[] response =  HttpPost.Post (new Uri( "http://2.wechatpicture.sinaapp.com/mystorage"),files,aaa);
		Debug.Log (Encoding.UTF8.GetString(response));
		Show = false;
		//Wait one frame, the 0 here is only because we need to return an IEnumerable
	}
}



public class HttpPost
{
	static void CopyTo(Stream source,Stream destination)
	{
		// 缓冲区默认大小
		InternalCopyTo(source,destination, 81920);
	}
	
	static void InternalCopyTo(Stream source,Stream destination, int bufferSize)
	{
		byte[] array = new byte[bufferSize];
		int count;
		while ((count = source.Read(array, 0, array.Length)) != 0)
		{
			destination.Write(array, 0, count);
		}
	}
	
	
	/// <summary>
	/// 以Post 形式提交数据到 uri
	/// </summary>
	/// <param name="uri"></param>
	/// <param name="files"></param>
	/// <param name="values"></param>
	/// <returns></returns>
	public static byte[] Post(Uri uri, IEnumerable<UploadFile> files, NameValueCollection values)
	{
		string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
		
		request.Referer = "http://2.wechatpicture.sinaapp.com/mystorage";
		request.ContentType = "multipart/form-data; boundary=" + boundary;
		request.Method = "POST";
		request.KeepAlive = true;
		request.Credentials = CredentialCache.DefaultCredentials;
		
		
		MemoryStream stream = new MemoryStream();
		
		byte[] line = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
		
		
		
		//提交文件
		if (files != null)
		{
			string fformat = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename={1}\r\nContent-Type: {2}\r\n\r\n";
			foreach (UploadFile file in files)
			{
				string s = string.Format(fformat, file.Name, file.Filename, file.ContentType);//
				byte[] data = Encoding.UTF8.GetBytes(s);
				stream.Write(data, 0, data.Length);
				
				stream.Write(file.Data, 0, file.Data.Length);
				//stream.Write(line, 0, line.Length);
			}
		}
		//提交文本字段
		if (values != null)
		{
			string format = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
			foreach (string key in values.Keys)
			{
				string s = string.Format(format, key, values[key]);
				byte[] data = Encoding.UTF8.GetBytes(s);
				stream.Write(data, 0, data.Length);
			}
			stream.Write(line, 0, line.Length);
		}
		
		
		request.ContentLength = stream.Length;
		
		
		Stream requestStream = request.GetRequestStream();
		
		stream.Position = 0L;
		
		CopyTo(stream, requestStream);
		
		stream.Close();
		
		requestStream.Close();
		
		//return new byte[0];
		
		using (var response = request.GetResponse())
			using (var responseStream = response.GetResponseStream())
				using (var mstream = new MemoryStream())
		{
			CopyTo(responseStream, mstream);
			
			return mstream.ToArray();
		}
	}
	
	
	
	
	
	
}

/// <summary>
/// 上传文件
/// </summary>
public class UploadFile
{
	
	
	public string Name { get; set; }
	public string Filename { get; set; }
	public string ContentType { get; set; }
	public byte[] Data { get; set; }
}
