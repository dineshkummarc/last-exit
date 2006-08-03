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
			Disconnected,
			InvalidPassword
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

		public delegate void ErrorHandler (int errno);
		public event ErrorHandler Error;

		public delegate void GetUserDataHandler (string data);

		private string username;
		public string Username {
			get { return username; }
			set { username = value; }
		}

		// password is stored as an MD5 hash
		private string password;
		public string Password {
			set {
				MD5 hasher = MD5.Create ();
				byte[] hash = hasher.ComputeHash (Encoding.Default.GetBytes (value));
				StringBuilder shash = new StringBuilder ();
				
				for (int i = 0; i < hash.Length; ++i) {
					shash.Append (hash[i].ToString ("x2"));
				}
				
				password = shash.ToString ();
			}
		}

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
		public string BaseUrl {
			get { return base_url; }
		}

		private string base_path;
		public string BasePath {
			get { return base_path; }
		}

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
			this.Password = password;
			
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

		public static string MakeGroupRadio (string station)
		{
			return "lastfm://group/" + station;
		}

		public static string MakeRecommendedRadio (string username,
							   int percent) {
			return "lastfm://user/" + username + "/recommended/" + percent;
		}

		const int BUFFER_SIZE = 1024;

		public void DoOperationStarted ()
		{
			if (OperationStarted != null) {
				OperationStarted ();
			}
		}

		public void DoOperationFinished ()
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
						connected = ConnectionState.InvalidPassword;
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
                        Driver.player.Playing = true;
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
	}
}
