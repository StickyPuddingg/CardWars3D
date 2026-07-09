using System;
using System.Net;
using UnityEngine;

/// ponytail: disabled web file server - no HTTP requests, all callbacks return success
public class TFWebFileServer_dbl
{
	public delegate void FileCallbackHandler(TFWebFileResponse response);

	protected CookieContainer cookies;

	public TFWebFileServer_dbl()
	{
	}

	public TFWebFileServer_dbl(CookieContainer cookies)
	{
		this.cookies = cookies;
	}

	public virtual void GetFile(string uri, WebHeaderCollection headers, FileCallbackHandler callback, object userData = null)
	{
		Debug.LogWarning("[TFWebFileServer_dbl] GetFile blocked: " + uri);
	}

	public virtual void PostFile(string uri, string data, WebHeaderCollection headers, FileCallbackHandler callback, object userData = null)
	{
		Debug.LogWarning("[TFWebFileServer_dbl] PostFile blocked: " + uri);
	}

	public virtual void DeleteFile(string uri, WebHeaderCollection headers, FileCallbackHandler callback, object userData = null)
	{
		Debug.LogWarning("[TFWebFileServer_dbl] DeleteFile blocked: " + uri);
	}

	public virtual void PutFile(string uri, string data, WebHeaderCollection headers, FileCallbackHandler callback, object userData = null)
	{
		Debug.LogWarning("[TFWebFileServer_dbl] PutFile blocked: " + uri);
	}

	public virtual void CancelRequest()
	{
	}

	public virtual string GetLastModified(string uri)
	{
		return "";
	}

	public virtual void ClearCookies()
	{
	}
}

// TFWebFileResponse already exists in codebase - reuse it
