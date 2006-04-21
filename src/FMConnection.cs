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
	public class FMConnection 
	{
		public enum ConnectionState {
			Connected,
			Disconnected
		};

		public enum TagMode {
			Artist,
			Song,
			Album
		};

		// Events
		public delegate void ConnectionChangedHandler (ConnectionState state);
		public event ConnectionChangedHandler ConnectionChanged;

		public delegate void OperationHandler ();
		public event OperationHandler OperationStarted;
		public event OperationHandler OperationFinished;

		public delegate void StationChangedHandler (string station);
		public event StationChangedHandler StationChanged;
		
		public delegate void MetadataCompletedHandler (Song song);
		public event MetadataCompletedHandler MetadataLoaded;

		public delegate void SearchCompletedHandler (object o,
							     FindStation.SearchType t);
		public event SearchCompletedHandler SearchCompleted;

		public delegate void ErrorHandler (int errno);
		public event ErrorHandler Error;

		public delegate void GetUserDataCompletedHandler (string data);
		public event GetUserDataCompletedHandler GetUserInfoCompleted;
		public event GetUserDataCompletedHandler GetUserTagsCompleted;

		private string username;
		public string Username {
			get { return username; }
		}

		// password is stored as an MD5 hash
		private string password;

		private string session;
		public string Session {
			get { return session; }
		}

		private string stream_url;
		public string StreamUrl {
			get { return stream_url; }
		}

		private bool subscriber;
		public bool Subscriber {
			set { subscriber = value; }
			get { return subscriber; }
		}

		// URL for updated client
		// private string update_url;
		// Is this client banned from using the network?
		// private bool banned;

		// I don't know what this is for
		// private bool framehack;

		private string base_url;
		private string base_path;

		// Handshake was successful
		private ConnectionState connected;
		public ConnectionState Connected {
			set { connected = value; }
			get { return connected; }
		}

		private string station = "Not listening to any station";
		public string Station {
			get { return station; }
		}
		private string station_location;
		public string StationLocation {
			get { return station_location; }
		}

		public FMConnection (string username,
				     string password) 
		{
			// Default to not subscribed
			subscriber = false;
			this.username = username;

			MD5 hasher = MD5.Create ();
			byte[] hash = hasher.ComputeHash (Encoding.Default.GetBytes (password));
			StringBuilder shash = new StringBuilder ();

			for (int i = 0; i < hash.Length; ++i) {
				shash.Append (hash[i].ToString ("x2"));
			}

			this.password = shash.ToString ();

			this.connected = ConnectionState.Disconnected;
		}

		public static string MakeUserRadio (string username,
						    string station) 
		{
			return "lastfm://user/" + username + station;
		}

		public static string MakeArtistRadio (string station)
		{
			return "lastfm://artist/" + station + "/similarartists";
		}

		public static string MakeFanRadio (string station)
		{
			return "lastfm://artist/" + station + "/fans";
		}

		public static string MakeTagRadio (string station)
		{
			return "lastfm://globaltags/" + station;
		}

		const int BUFFER_SIZE = 1024;

		private void DoOperationStarted ()
		{
			if (OperationStarted != null) {
				OperationStarted ();
			}
		}

		private void DoOperationFinished ()
		{
			if (OperationFinished != null) {
				OperationFinished ();
			}
		}

		public void Handshake () 
		{
			string handshake_url = "http://ws.audioscrobbler.com/radio/handshake.php?version=" + "1.1.1" + "&platform=" + "linux" + "&username=" + username + "&passwordmd5=" + password + "&debug=" + "0" + "&partner=";
			FMRequest fmr = new FMRequest ();

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (HandshakeCompleted);

			fmr.DoRequest (handshake_url);
			DoOperationStarted ();
		}

		private void HandshakeCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				string content;

				content = request.Data.ToString ();
				ParseHandshake (content);

				if (ConnectionChanged != null) {
					ConnectionChanged (connected);
				}

			}
			DoOperationFinished ();
		}

		private bool ParseHandshake (string content) 
		{
			string[] lines = content.Split (new Char[] {'\n'});

			foreach (string line in lines) {
				string[] opts = line.Split (new Char[] {'='});

				switch (opts[0].ToLower ()) {
				case "session":
					if (opts[1].ToLower () == "failed") {
						// Handshake failed
						session = "";
						connected = ConnectionState.Disconnected;
						Console.WriteLine ("Failed to connect");
						// don't need to parse anymore
						return false;
					}

					session = opts[1];
					connected = ConnectionState.Connected;
					break;

				case "stream_url":
					stream_url = opts[1];
					break;

				case "subscriber":
					if (opts[1] == "0") {
						subscriber = false;
					} else {
						subscriber = true;
					}
					break;

				case "framehack":
					// Does anyone know what this is for?
					break;

				case "base_url":
					base_url = opts[1];
					break;

				case "base_path":
					base_path = opts[1];
					break;
					
				case "update_url":
					// Don't think this can be used at the
					// moment.
					//update_url = opts[1];
					break;

				case "banned":
					/*
					if (opts[1] == "0") {
						banned = false;
					} else {
						banned = true;
					}
					*/
					break;

				default:
					Console.WriteLine ("Unknown key: " + opts[0]);
					break;
				}
			}

			return true;
		}

		public void ChangeStation (string station) 
		{
			FMRequest fmr = new FMRequest ();
			string url = "http://" + base_url + "/" + base_path + "/adjust.php?session=" + session + "&url=" + station + "&debug=0";

			Console.WriteLine ("station: " + url);
			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (StationChangeCompleted);

			fmr.DoRequest (url);
			DoOperationStarted ();
		}

		private void StationChangeCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				string content;
				int errno;

				content = request.Data.ToString ();
				if (ParseStation (content, out errno) == true) {
					if (StationChanged != null) {
						StationChanged (station_location);
					}
				} else {
					if (Error != null) {
						Error (errno);
					}
				}
			}

			DoOperationFinished ();
		}

		private bool ParseStation (string content, out int errno) 
		{
			string[] lines = content.Split (new Char[] {'\n'});
			bool ret = true;

			errno = 0;

			foreach (string line in lines) {
				string[] opts = line.Split (new Char[] {'='});

				Console.WriteLine (line);
				switch (opts[0].ToLower ()) {
				case "response":
					if (opts[1] == "OK") {
						ret = true;
					} else {
						ret = false;
					}
					break;

				case "error":
					errno = Int32.Parse (opts[1]);
					ret = false;
					break;
					
				case "url":
					station_location = opts[1];
					break;
					
				case "stationname":
					station = StringUtils.StringToUTF8 (opts[1]);
					break;

				default:
					break;
				}
			}

			return ret;
		}

		public void Skip () 
		{
			FMRequest fmr = new FMRequest ();
			string url = "http://" + base_url + "/" + base_path + "/control.php?session=" + session + "&command=skip&debug=0";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (SkipCompleted);
			fmr.DoRequest (url);
			DoOperationStarted ();
		}

		private void SkipCompleted (FMRequest request) 
		{
			DoOperationFinished ();
		}

		public void Love () 
		{
			FMRequest fmr = new FMRequest ();
			string url = "http://" + base_url + "/" + base_path + "/control.php?session=" + session + "&command=love&debug=0";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (LoveCompleted);
			fmr.DoRequest (url);
			DoOperationStarted ();
		}

		private void LoveCompleted (FMRequest request) 
		{
			DoOperationFinished ();
		}

		public void Hate () 
		{
			FMRequest fmr = new FMRequest ();
			string url = "http://" + base_url + "/" + base_path + "/control.php?session=" + session + "&command=ban&debug=0";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (HateCompleted);
			fmr.DoRequest (url);
			DoOperationStarted ();
		}

		private void HateCompleted (FMRequest request) 
		{
			DoOperationFinished ();
		}

		public void GetMetadata () 
		{
			FMRequest fmr = new FMRequest ();
			string url = "http://" + base_url + "/" + base_path + "/np.php?session=" + session + "&debug=0";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (MetadataCompleted);
			fmr.DoRequest (url);
			DoOperationStarted ();
		}

		private void MetadataCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				string content;
				Song song = new Song ();

				content = request.Data.ToString ();
				if (ParseMetadata (content, song) == true) {
					if (MetadataLoaded != null) {
						MetadataLoaded (song);
					}
				}
			}
			
			DoOperationFinished ();
		}

		private bool ParseMetadata (string content,
					    Song song) 
		{
			string[] lines = content.Split (new Char[] {'\n'});
			bool ret = true;

			foreach (string line in lines) {
				string[] opts = line.Split (new Char[] {'='});

				switch (opts[0].ToLower ()) {
				case "error":
					ret = false;
					break;
					
				case "station":
					song.Station = opts[1];
					break;

				case "station_url":
					song.StationUrl = opts[1];
					break;

				case "stationfeed":
					song.StationFeed = opts[1];
					break;

				case "stationfeed_url":
					song.StationFeedUrl = opts[1];
					break;

				case "artist":
					song.Artist = opts[1];
					break;

				case "album":
					song.Album = opts[1];
					break;

				case "track":
					song.Track = opts[1];
					break;

				case "albumcover_small":
					break;

					// We're only handling the medium cover
					// It's a good enough size to scale
					// to whatever size we need.
				case "albumcover_medium":
					song.ImageUrl = opts[1];
					break;

				case "albumcover_large":
					break;

				case "trackprogress":
					break;

				case "trackduration":
					song.Length = Int32.Parse (opts[1]);
					break;

				case "artist_url":
					song.ArtistUrl = opts[1];
					break;

				case "album_url":
					song.AlbumUrl = opts[1];
					break;

				case "track_url":
					song.TrackUrl = opts[1];
					break;

				case "discovery":
					break;

				default:
					break;
				}
			}

			return ret;
		}

		public void Search (FindStation.SearchType type,
				    string description) 
		{
			FMRequest fmr = new FMRequest ();
			string url;

			switch (type) {
			case FindStation.SearchType.SoundsLike:
				url = "http://" + base_url + "/1.0/get.php?resource=artist&document=similar&format=xml&artist=" + description;
				break;

			case FindStation.SearchType.TaggedAs:
				url = "http://" + base_url + "/1.0/tag/" + description + "/search.xml?showtop10=1";
				break;

			case FindStation.SearchType.FansOf:
				url = "http://" + base_url + "1.0/artist" + description + "/fans.xml";
				break;

			default:
				url = "";
				break;
			}

			fmr.Closure = (object) type;
			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (FindStationCompleted);
			fmr.DoRequest (url);

			DoOperationStarted ();
		}

		private void FindStationCompleted (FMRequest request) 
		{
			FindStation.SearchType t = (FindStation.SearchType) request.Closure;
			if (request.Data.Length > 1) {
				string content;

				content = request.Data.ToString ();
				switch (t) {
				case FindStation.SearchType.SoundsLike:
					Artist artist = ParseSimilar (content);
					
					if (SearchCompleted != null) {
						SearchCompleted ((object) artist, t);
					}
					break;

				case FindStation.SearchType.TaggedAs:
					ArrayList tags = ParseTag (content);

					if (SearchCompleted != null) {
						SearchCompleted ((object) tags, t);
					}
					break;

				case FindStation.SearchType.FansOf:
					ArrayList fans = ParseFans (content);

					if (SearchCompleted != null) {
						SearchCompleted ((object) fans, t);
					}
					break;
				}
			} else {
				Console.WriteLine ("There was an error");
				if (SearchCompleted != null) {
					SearchCompleted (null, t);
				}
			}

			DoOperationFinished ();
		}

		private string get_node_text (XmlNode node,
					      string name)
		{
			return node[name].InnerText;
		}

		private Artist ParseSimilar (string content)
		{
			XmlDocument xml = new XmlDocument ();
			XmlNodeList elemlist;
			
			xml.LoadXml (content);
			elemlist = xml.GetElementsByTagName ("similarartists");
			if (elemlist.Count == 0) {
				return null;
			}
			
			XmlNode artist_node = elemlist[0];
			Artist artist = new Artist ();
			artist.Name = artist_node.Attributes.GetNamedItem ("artist").InnerText;
			artist.Streamable = (artist_node.Attributes.GetNamedItem ("streamable").InnerText == "1");
			artist.ImageUrl = artist_node.Attributes.GetNamedItem ("picture").InnerText;
			artist.Mbid = artist_node.Attributes.GetNamedItem ("mbid").InnerText;
			
			elemlist = xml.GetElementsByTagName ("artist");
			
			// Loop over all the artists adding them as
			// similar artists
			IEnumerator ienum = elemlist.GetEnumerator ();
			while (ienum.MoveNext ()) {
				XmlNode a_node = (XmlNode) ienum.Current;
				SimilarArtist similar = new SimilarArtist ();
				
				similar.Name = get_node_text (a_node, "name");
				similar.Streamable = (get_node_text (a_node, "streamable") == "0");
				similar.Mbid = get_node_text (a_node, "mbid");
				similar.Url = get_node_text (a_node, "url");
				similar.Relevance = Int32.Parse (get_node_text (a_node, "match"));
				
				artist.AddSimilarArtist (similar);
			}
			
			return artist;
		}
		
		private ArrayList ParseTag (string content) 
		{
			XmlDocument xml = new XmlDocument ();
			XmlNodeList elemlist;
			ArrayList tags = new ArrayList ();

			xml.LoadXml (content);
			elemlist = xml.GetElementsByTagName ("tags");
			if (elemlist.Count == 0) {
				return tags;
			}

			elemlist = xml.GetElementsByTagName ("tag");
			IEnumerator ienum = elemlist.GetEnumerator ();
			while (ienum.MoveNext ()) {
				XmlNode t_node = (XmlNode) ienum.Current;
				string name = get_node_text (t_node, "name");
				int id = Int32.Parse (get_node_text (t_node, "id"));
				double match = Double.Parse (get_node_text (t_node, "match"));

				Tag t = new Tag (id, name, match);
				tags.Add (t);
			}

			return tags;
		}

		private ArrayList ParseFans (string content)
		{
			XmlDocument xml = new XmlDocument ();
			XmlNodeList elemlist;
			ArrayList fans = new ArrayList ();

			xml.LoadXml (content);
			elemlist = xml.GetElementsByTagName ("fans");
			if (elemlist.Count == 0) {
				return fans;
			}

			elemlist = xml.GetElementsByTagName ("user");
			IEnumerator ienum = elemlist.GetEnumerator ();
			while (ienum.MoveNext ()) {
				XmlNode f_node = (XmlNode) ienum.Current;
				string name = f_node.Attributes.GetNamedItem ("username").InnerText;
				string url = get_node_text (f_node, "url");
				string image = get_node_text (f_node, "image");
				int weight = Int32.Parse (get_node_text (f_node, "weight"));

				Fan f = new Fan (name, url, image, weight);
				fans.Add (f);
			}

			return fans;
		}

		public void GetUserInfo (string username)
		{
			FMRequest fmr = new FMRequest ();
			string url = "http://" + base_url + "/1.0/user/" + username + "/profile.xml";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (GetUserCompleted);
			fmr.DoRequest (url);

			DoOperationStarted ();
		}
			
		private void GetUserCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				string content;

				content = request.Data.ToString ();
				if (GetUserInfoCompleted != null) {
					GetUserInfoCompleted (content);
				}
			}

			DoOperationFinished ();
		}

		public void TagSong (Song song, 
				     TagMode mode,
				     string tag)
		{
			string modestr;
			
			switch (mode) {
			case TagMode.Artist:
				modestr = "artist=" + song.Artist;
				break;

			case TagMode.Song:
				modestr = "track=" + song.Track + "&artist=" + song.Artist;
				break;

			case TagMode.Album:
				modestr = "album=" + song.Album + "&artist=" + song.Artist;
				break;
			}

			FMRequest fmr = new FMRequest ();
			string url = "http://" + base_url + "/" + base_path + "/control.php?session=" + session + "&command=skip&debug=0";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (SkipCompleted);
			fmr.DoRequest (url);
		}			

		public void GetUserTags ()
		{
			FMRequest fmr = new FMRequest ();
			string url = "http://" + base_url + "/1.0/user/" + username + "/tags.xml";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (UserTagsCompleted);
			fmr.DoRequest (url);

			DoOperationStarted ();
		}
			
		private void UserTagsCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				string content;

				content = request.Data.ToString ();
				if (GetUserTagsCompleted != null) {
					GetUserTagsCompleted (content);
				}
			}

			DoOperationFinished ();
		}
		   
		private class FMRequest {
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

				byte [] post_data = Encoding.UTF8.GetBytes (data);
				request.Method = "POST";
				request.ContentLength = post_data.Length;
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
}
