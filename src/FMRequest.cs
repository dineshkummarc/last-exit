/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *
 *  Copyright 2005, 2006 Iain Holmes
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of version 2 of the GNU General Public License as 
 *  published by the Free Software Foundation.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Street #330, Boston, MA 02111-1307, USA.
 *
 */

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace LastExit
{
	public class FMRequest {
		const int BUFFER_SIZE = 1024;
		public StringBuilder Data;
		public byte[] BufferRead;
		public HttpWebRequest Request;
		public HttpWebResponse Response;
		public Stream ResponseStream;
		public object Closure;
		public string PostData;
		
		public delegate void RequestCompletedHandler (FMRequest req);
		public event RequestCompletedHandler RequestCompleted;
		
		public FMRequest () 
		{
			BufferRead = new byte[BUFFER_SIZE];
			Data = new StringBuilder ("");
			Request = null;
			ResponseStream = null;
		}
		
		public void DoRequest (string url) 
		{
			HttpWebRequest request;
			request = (HttpWebRequest) WebRequest.Create (url);
			request.Headers.Set ("Accept-Language", "en");
			Request = request;
			
			// Start the handshake async
			request.BeginGetResponse (new AsyncCallback (RequestCallback), this);
		}
		
		// POST version
		public void DoRequest (string url, string data)
		{
				HttpWebRequest request;
				request = (HttpWebRequest) WebRequest.Create (url);
				Request = request;
				PostData = data;
				
				request.Method = "POST";
				request.ContentLength = data.Length;
				request.ContentType = "application/x-www-form-urlencoded";
				
				// Start the handshake async
				request.BeginGetRequestStream (new AsyncCallback (PostRequestCallback), this);
		}
		
		private void PostRequestCallback (IAsyncResult result)
		{
			FMRequest req = (FMRequest) result.AsyncState;
			Stream stream = Request.EndGetRequestStream (result);
			
			// Write the data to the stream.
			byte [] data = Encoding.UTF8.GetBytes (req.PostData);
			stream.Write(data, 0, PostData.Length);
			stream.Close();
			
			Request.BeginGetResponse (new AsyncCallback (RequestCallback), this);
		}
		
		private void RequestCallback (IAsyncResult result) 
		{
			FMRequest req = (FMRequest) result.AsyncState;
			req.Response = (HttpWebResponse) req.Request.EndGetResponse (result);
			
			Stream stream = req.Response.GetResponseStream ();
			req.ResponseStream = stream;
			
			stream.BeginRead (req.BufferRead, 0, BUFFER_SIZE, new AsyncCallback (ReadCallback), req);
		}
		
		private void ReadCallback (IAsyncResult result) 
		{
			FMRequest req = (FMRequest) result.AsyncState;
			Stream stream = req.ResponseStream;
			int read = stream.EndRead (result);
			
			if (read > 0) {
				req.Data.Append (Encoding.UTF8.GetString (req.BufferRead, 0, read));
				stream.BeginRead (req.BufferRead, 0, BUFFER_SIZE, new AsyncCallback (ReadCallback), req);
				return;
			} else {
				stream.Close ();
				
				GLib.Idle.Add (new GLib.IdleHandler (request_completed_idle));
			}
		}
		
		private bool request_completed_idle ()
		{
			if (RequestCompleted != null) {
				RequestCompleted (this);
			}
			
			return false;
		}
	}
}
