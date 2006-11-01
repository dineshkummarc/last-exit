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
using System.IO;
using System.Net;

using Gdk;

namespace LastExit {
	public class ImageLoader {
		public delegate void ImageLoadedHandler (Pixbuf image);
		public event ImageLoadedHandler ImageLoaded;

		private string image_url;
		public string ImageUrl {
			set { image_url = value; }
		}

		private Pixbuf image;
		public Pixbuf Image {
			get { return image; }
		}

		private bool image_requested;
		
		public ImageLoader () {
			image = null;
			image_requested = false;
			image_url = null;

			BufferRead = new byte[BUFFER_SIZE];
			Request = null;
			ResponseStream = null;
			width = -1;
			height = -1;
		}

		public void RequestImage (int w, int h)
		{
			if (image != null) {
				// Already got the cover
				if (ImageLoaded != null) {
					ImageLoaded (image);
				}

				return;
			}

			if (image_requested) {
				return;
			}

			GetImage (image_url, w, h);
			image_requested = true;
		}

		public void RequestImage () 
		{
			RequestImage (-1, -1);
		}

		private const int BUFFER_SIZE = 1024;
		private byte[] BufferRead;
		private PixbufLoader loader;
		private HttpWebRequest Request;
		private HttpWebResponse Response;
		private Stream ResponseStream;

		private int width, height;

		private void GetImage (string url, int w, int h) 
		{
			HttpWebRequest request;

			width = w;
			height = h;

			try {
				request = (HttpWebRequest) WebRequest.Create (url);
			// FIXME: Any way to get rid of the duplicate stuff..?
			} catch (UriFormatException) {
				loader = null;
				GLib.Idle.Add (new GLib.IdleHandler (image_loaded_handler));
				return;
			} catch (ArgumentNullException) {
				loader = null;
				GLib.Idle.Add (new GLib.IdleHandler (image_loaded_handler));
				return;
			}

			Request = request;

			try {
				request.BeginGetResponse (new AsyncCallback (ImageDownloadRequestCallback), this);
			} catch (WebException e) {
				loader = null;
				GLib.Idle.Add (new GLib.IdleHandler (image_loaded_handler));
			}
		}

		private void ImageDownloadRequestCallback (IAsyncResult result) {
			ImageLoader cl = (ImageLoader) result.AsyncState;

			try {
				cl.Response = (HttpWebResponse) cl.Request.EndGetResponse (result);
			} catch (WebException e) {
				loader = null;
				GLib.Idle.Add (new GLib.IdleHandler (image_loaded_handler));
				return;
			}
			
			Stream stream = cl.Response.GetResponseStream ();
			cl.loader = new PixbufLoader ();
			if (width != -1 && height != -1) {
				cl.loader.SetSize (width, height);
			}
			
			cl.ResponseStream = stream;
			stream.BeginRead (cl.BufferRead, 0, 
					  BUFFER_SIZE, 
					  new AsyncCallback (ReadCallback), cl);
		}
		
		private void ReadCallback (IAsyncResult result) {
			ImageLoader cl = (ImageLoader) result.AsyncState;
			Stream stream = cl.ResponseStream;
			int read = stream.EndRead (result);
			
			if (read > 0) {
				cl.loader.Write (cl.BufferRead, (ulong) read);
				stream.BeginRead (cl.BufferRead, 0, 
						  BUFFER_SIZE, 
						  new AsyncCallback (ReadCallback), cl);
				return;
			} else {
				stream.Close ();
				cl.loader.Close ();

				GLib.Idle.Add (new GLib.IdleHandler (image_loaded_handler));
			}
		}

		private bool image_loaded_handler () 
		{
			if (ImageLoaded != null) {
				if (loader != null) {
					image = loader.Pixbuf;
					ImageLoaded (loader.Pixbuf);
				} else {
					image = null;
					ImageLoaded (null);
				}
			}

			image_requested = false;
			return false;
		}
	}
}
