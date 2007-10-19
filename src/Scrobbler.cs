using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections;

namespace LastExit
{

	public class SongSort : IComparer
	{
		public int Compare (object x, object y) 
		{
			Song song1 = (Song) x;
			Song song2 = (Song) y;
			
			return DateTime.Compare (song1.StartTime, song2.StartTime);
		}
	}
	
	public class Scrobbler
	{
		private const string BaseUrl = "http://post.audioscrobbler.com/";
		private const string ProtocolVersion = "1.2";
		private const int MaxSongsPerPost = 50;
		private const string ClientId = "tst";
		private const string ClientVersion = "1.0";
	 

		private string username;
		private string password;
		
		private string session;
		private string now_playing_url;
		private string submission_url;

		private DateTime last_handshake;
		private int hard_failures;

		private ArrayList unsubmitted_songs;
		private Hashtable failed_submitted_songs;

		private bool connected;
		public bool Connected
		{
			get {
				return connected;
			}
		}

		private bool scrobbling_enabled;
		public bool ScrobblingEnabled
		{
			get {
				return scrobbling_enabled;
			}
			set {
				this.scrobbling_enabled = value;
			}
		}

		private bool now_playing_notification_enabled;
		public bool NowPlayingNotificationEnabled
		{
			get {
				return now_playing_notification_enabled;
			} 
			set {
				now_playing_notification_enabled = value;
			}
		}
			

		public Scrobbler (string username, string password)
		{
			this.unsubmitted_songs = new ArrayList ();
			this.failed_submitted_songs = new Hashtable ();
			this.last_handshake = new DateTime (0);
			this.hard_failures = 0;

			this.scrobbling_enabled = true;
			this.now_playing_notification_enabled = true;
			this.username = username;
			this.password = password;

			Handshake ();
		}

		

		public void Handshake ()
		{
			connected = false;
			last_handshake = DateTime.Now;
			
			decimal timestamp = Timestamp ();
			string auth = AuthHash (timestamp);
				
			string url = BaseUrl + "?hs=true&p=" + ProtocolVersion + "&c=" + 
				ClientId + "&v=" + ClientVersion + "&u=" + username + 
					"&t=" + timestamp + "&a=" + auth;
			
			FMRequest request = new FMRequest ();
			request.RequestCompleted += new FMRequest.RequestCompletedHandler (HandshakeRequestCompleted);
			request.DoRequest (url);
		}

		private void Reshake ()
		{
			// make shure to pause between handshakes
			if (DateTime.Now - last_handshake > new TimeSpan (0, 1, 0)) {
					Handshake ();
			}
		}

		private void HandshakeRequestCompleted (FMRequest request)
		{
			if (request.Data.Length > 1) {
				string content;

				content = request.Data.ToString ();
				ParseHandshake (content);

				if (Connected)
					ScrobbleAll ();
			}
		}

		private void ParseHandshake (string content) 
		{
			string[] lines = content.Split (new Char[] {'\n'});
			string[] response = lines[0].Split (new Char[] {' '});

			switch (response [0].Trim ().ToLower ()) {
			case "ok":
				Console.WriteLine ("Scrobbling enabled");
				this.connected = true;
				this.hard_failures = 0;
				
				this.session = lines[1].Trim ();
				this.now_playing_url = lines[2].Trim ();
				this.submission_url = lines[3].Trim ();
				break;

			case "banned":
				Console.WriteLine ("BANNED");
				break;

			case "badauth":
				Console.WriteLine ("BADAUTH");
				break;

			case "badtime":
				Console.WriteLine ("BADTIME");
				break;

			case "failed":
				Console.WriteLine ("FAILED: {0}", content);
				break;
				
			default:
				Console.WriteLine ("Unknown response: {0}", content);
				break;
			}
		}

		private static long Timestamp(DateTime time)
		{
		  DateTime unixEpoch = new DateTime(1970,1,1,0,0,0);
		  return (time.Ticks - unixEpoch.Ticks)/10000000;
		}

		private static long Timestamp ()
		{
			return Timestamp (DateTime.UtcNow);
		}

		private string AuthHash (decimal timestamp)
		{
			MD5 hasher = MD5.Create();
			StringBuilder phash = new StringBuilder ();
			StringBuilder ahash = new StringBuilder ();
			byte[] Pass = hasher.ComputeHash(Encoding.Default.GetBytes (password));
			for (int i = 0; i < Pass.Length; ++i) {
				phash.Append (Pass[i].ToString ("x2"));
			}
			string AuthText = phash.ToString() + timestamp.ToString();
			
			byte[] Auth = hasher.ComputeHash(Encoding.Default.GetBytes (AuthText));
			for (int i = 0; i < Auth.Length; ++i) {
				ahash.Append (Auth[i].ToString ("x2"));
			}

			return ahash.ToString();
		}

		private void ScrobbleAll () 
		{
			ArrayList songs = new ArrayList ();

			foreach (DictionaryEntry entry in failed_submitted_songs) {
				Song[] more_songs = (Song[]) entry.Value;
				foreach (Song song in more_songs) {
					songs.Add (song);
				}
			}
			failed_submitted_songs.Clear ();

			foreach (Song song in unsubmitted_songs) {
				songs.Add (song);
			}
			unsubmitted_songs.Clear ();

			// Sort songs by StartTime because audioscrobbler will
			// drop songs submitted with dates before the last
			// sumbitted song
			songs.Sort (new SongSort ());

			Scrobble ((Song[]) songs.ToArray (typeof(Song)));
		}

		public void Scrobble (Song[] songs)
		{
			if (!Connected) {
				unsubmitted_songs.AddRange (songs);
			} else if (songs.Length > 0) {
				StringBuilder post_data = new StringBuilder ();
				
				post_data.Append ("s=" + session);
				
				int i = 0;	
				foreach (Song song in songs) {
					if (song.Length > 30000) {
						if (i<MaxSongsPerPost) {
							post_data.Append ("&a["+i+"]=" + Escape (song.Artist));
							post_data.Append ("&t["+i+"]=" + Escape (song.Track));
							post_data.Append ("&i["+i+"]=" + Timestamp (song.StartTime));
							post_data.Append ("&o["+i+"]=L" + song.TrackAuth);								 
							post_data.Append ("&r["+i+"]=");
							post_data.Append ("&l["+i+"]=" + (int) Math.Round ((double) song.Length / 1000));
							post_data.Append ("&b["+i+"]=" + Escape (song.Album));
							post_data.Append ("&n["+i+"]=");
							post_data.Append ("&m["+i+"]=");
							i++;
							} else {
							break;
						}
					}
					
				}
				
				FMRequest request = new FMRequest ();
				request.RequestCompleted += new FMRequest.RequestCompletedHandler (OnSubmissionCompleted);
				failed_submitted_songs.Add (request, songs);
				request.DoRequest (submission_url, post_data.ToString ());
				
				if (i>=MaxSongsPerPost) {
					Song[] s = new Song[songs.Length-MaxSongsPerPost];
					songs.CopyTo (s, i);
					Scrobble (s);
				}				
			}
		}
			


		public void Scrobble (Song song)
		{
			if (ScrobblingEnabled && song.Length > 30000) {
				if (!Connected || unsubmitted_songs.Count > 0 || 
					failed_submitted_songs.Count > 0) {
					unsubmitted_songs.Add (song);
					ScrobbleAll ();
				} else {
					Scrobble (new Song[] {song});
					/*
					StringBuilder post_data = new StringBuilder ();

					post_data.Append ("s=" + session);
					post_data.Append ("&a[0]=" + Escape (song.Artist));
					post_data.Append ("&t[0]=" + Escape (song.Track));
					post_data.Append ("&i[0]=" + Timestamp (song.StartTime));
					post_data.Append ("&o[0]=L" + song.TrackAuth);								 
					post_data.Append ("&r[0]=");
					post_data.Append ("&l[0]=" + (int) Math.Round ((double) song.Length / 1000));
					post_data.Append ("&b[0]=" + Escape (song.Album));
					post_data.Append ("&n[0]=");
					post_data.Append ("&m[0]=");

					FMRequest request = new FMRequest ();
					request.RequestCompleted += new FMRequest.RequestCompletedHandler (OnSubmissionCompleted);
					failed_submitted_songs.Add (request, song);
					
					request.DoRequest (submission_url, post_data.ToString ());*/
				}				
			}
		}


		public void NowPlayingNotification (Song song)
		{
			if (NowPlayingNotificationEnabled) {
				if (Connected) {
					StringBuilder post_data = new StringBuilder ();

					post_data.Append ("s=" + session);
					post_data.Append ("&a=" + Escape (song.Artist));
					post_data.Append ("&t=" + Escape (song.Track));
					post_data.Append ("&b=" + Escape (song.Album));
					post_data.Append ("&l=" + (int) Math.Round ((double) song.Length / 1000));
					post_data.Append ("&n=");
					post_data.Append ("&m=");

					FMRequest request = new FMRequest ();
					request.RequestCompleted += new FMRequest.RequestCompletedHandler (OnNotificationCompleted);
					
					request.DoRequest (now_playing_url, post_data.ToString ());
				}
			}
		}

		private string Escape (string str)
		{
			string ret = "";
			if (str != null && str.Length > 0) {
				//byte[] utf8 = Encoding.UTF8.GetBytes(str);
				//ret = Encoding.GetEncoding ("ISO-8859-1").GetString (utf8);
				ret = Uri.EscapeDataString (str);
			}
			return ret;
		}

		private void OnSubmissionCompleted (FMRequest request)
		{
			string[] response = request.Data.ToString ().Split (new Char[] {' '});

			switch (response[0].Trim ().ToLower ()) {
			case "ok":
				failed_submitted_songs.Remove (request);
				Console.WriteLine ("OK: {0}", request.Request.RequestUri.ToString ());
				break;
			case "badsession":
				Reshake ();
				break;
			case "failed":
				// Maybe it't last-exit's fault, better remove it
				//failed_submitted_songs.Remove (request);
				Console.WriteLine (GetErrorMsg (request.PostData, 
												request.Request.RequestUri.ToString (),
												request.Data.ToString ()));
				break;
			default:
				// Maybe it't last-exit's fault, better remove it
				//failed_submitted_songs.Remove (request);
				
				Console.WriteLine (GetErrorMsg (request.PostData, 
												request.Request.RequestUri.ToString (),
												request.Data.ToString ()));
				
				if (hard_failures >= 3) {
					Handshake ();
				} else {
					hard_failures++;
				}
				
				break;
			}
		}

		private void OnNotificationCompleted (FMRequest request)
		{
			string[] response = request.Data.ToString ().Split (new Char[] {' '});

			switch (response[0].Trim ().ToLower ()) {
			case "ok":
				Console.WriteLine ("OK: {0}", request.Request.RequestUri.ToString ());
				break;
			case "badsession":
				Reshake ();
				break;
			case "failed":
				Console.WriteLine (GetErrorMsg (request.PostData, 
												request.Request.RequestUri.ToString (),
												request.Data.ToString ()));
				break;
			default:
				Console.WriteLine (GetErrorMsg (request.PostData, 
												request.Request.RequestUri.ToString (),
												request.Data.ToString ()));
				break;
			}
		}

		private string GetErrorMsg (string post_data, string uri, string reason)
		{
			StringBuilder error_msg = new StringBuilder ();
			error_msg.AppendFormat ("Error: Post '{0}' to '{1}' failed",
									post_data, uri);

			if (reason.Trim ().Length > 0) {
				error_msg.AppendFormat (" - reason: {0}", reason);
			} else {
				error_msg.Append (".");
			}
			
			return error_msg.ToString ();
		}
		

	}
}
