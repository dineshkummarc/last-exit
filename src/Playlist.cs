using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Web;

namespace LastExit
{
	public class Playlist
	{
		private const int PLAYLIST_VERSION = 1;

		private ArrayList songs;
		private int nextSong;
		private string station;
		private FMConnection connection;
		private bool getting_playlist;
		private bool song_requested;

		public delegate void SongReadyHandler (Song song);
		public event SongReadyHandler SongReady;

		public delegate void NewPlaylistReadyHandler (Playlist playlist);
		public event NewPlaylistReadyHandler NewPlaylistReady;

		public Playlist (FMConnection connection)
		{
			this.connection = connection;
			this.songs = new ArrayList ();
			connection.StationChanged += new FMConnection.StationChangedHandler (OnStationChanged);
		}

		private void OnStationChanged (string station)
		{
			GetNewPlaylist ();
		}

		private void GetNewPlaylist () 
		{
			Console.WriteLine ("Playlist: New playlist requested");
			if (getting_playlist) {
				Console.WriteLine ("Playlist: Already getting playlist");
			} else {
				getting_playlist = true;
				string url = String.Format ("http://{0}{1}/xspf.php?sk={2}&discovery=0&desktop=1",
											connection.BaseUrl,
											connection.BasePath,
											connection.Session);
				FMRequest fmr = new FMRequest ();
				fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (GetNewPlaylistCompleted);
				fmr.DoRequest (url);
				connection.DoOperationStarted ();
			}
		}

		private void GetNewPlaylistCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				ParsePlaylist (request.Data.ToString ());
				
				Console.WriteLine ("Playlist: Got new playlist");
				getting_playlist = false;

				if (song_requested) {
					song_requested = false;
					if (SongReady != null)
						SongReady (GetNextSong());
				}
				
				if (NewPlaylistReady != null) {
					NewPlaylistReady (this);
				}
			}
			
			connection.DoOperationFinished ();
		}

		public void ParsePlaylist (string content)
		{
			this.songs.Clear ();
			this.nextSong = 0;
			
			XmlTextReader reader = new XmlTextReader (content, XmlNodeType.Element,
													  null);
			
			while (reader.Read ()) {
				reader.MoveToElement ();
				
				switch (reader.LocalName) {
				case "playlist":
					int version = Convert.ToInt32 (reader.GetAttribute ("version"));
					if (version > PLAYLIST_VERSION) {
						Console.WriteLine ("WARNING: Remote playlist version is: {0} - local version is {1}", 
										   version, PLAYLIST_VERSION);
					}
					break;
					
				case "title":
					string title = reader.ReadString ();
					station = HttpUtility.UrlDecode (title);
					break;

				case "creator":
					break;
					
				case "link":
					break;
					
				case "track":
					string xml = reader.ReadInnerXml ();
					this.songs.Add (ParseSong (xml, station));
					break;
				}
			}
		}

		public Song ParseSong (string xml, string station)
		{
			Song song = new Song ();
			XmlTextReader reader = new XmlTextReader (xml, XmlNodeType.Element,
													  null);
			song.Station = station;

			song.StationUrl = "station_url?";
			song.StationFeed = "station_feed?";
			song.StationFeedUrl = "station_feed_url?";
			
			song.Discovery = false;
			song.Hated = false;
			song.Loved = false;		

			while (reader.Read ()) {
				reader.MoveToElement ();
				switch (reader.LocalName.ToLower ()) {
				case "location":
					song.Location = ReadString (reader);
					break;
				case "title":
					song.Track = ReadString (reader);
					break;
				case "id":
					song.Id = Convert.ToInt32 (ReadString (reader));
					break;
				case "album":
					song.Album = ReadString (reader);
					break;
				case "creator":
					song.Artist = ReadString (reader);
					break;
				case "duration":
					song.Length = Convert.ToInt32 (ReadString (reader));
					break;
				case "image":
					song.ImageUrl = ReadString (reader);
					break;
				case "trackauth":
					song.TrackAuth = ReadString (reader);
					break;
				case "albumid":
					song.AlbumId = Convert.ToInt32 (ReadString (reader));
					break;
				case "artistid":
					song.ArtistId = Convert.ToInt32 (ReadString (reader));
					break;
				case "link":
					switch (reader.GetAttribute ("rel")) {
					case "http://www.last.fm/artistpage":
						song.ArtistUrl = ReadString (reader);
						break;
					case "http://www.last.fm/albumpage":
						song.AlbumUrl = ReadString (reader);
						break;
					case "http://www.last.fm/trackpage":
						song.TrackUrl = ReadString (reader);
						break;		
					}
					break;

				default:
					if (reader.LocalName.Trim ().Length > 0)
						Console.WriteLine ("Unknown element '{0}' in playlist", reader.LocalName);
					break;
				}
			}

			return song;
		}
		

		public void Refresh ()
		{
			GetNewPlaylist ();
		}

		public void RequestNextSong ()
		{
			Console.WriteLine ("Playlist: Song Requested");
			Console.WriteLine ("Playlist: Getting Playlist? " + getting_playlist);
			if (getting_playlist) {
				song_requested = true;
			} else {
				if (SongReady != null) {
					SongReady (GetNextSong ());
				}
			}
		}		

		private Song GetNextSong () 
		{
			Console.WriteLine ("Playlist: GetNextSong (): Returning song number " + this.nextSong);
			Song song = (Song) this.songs[this.nextSong];
			
			if (this.nextSong < this.songs.Count-1) {
				this.nextSong++;
			} else {
				GetNewPlaylist ();
			}
			
			return song;
		}

		private string ReadString (XmlTextReader reader)
		{
			string s = reader.ReadString ();
			if (s.Trim ().Length == 0) 
				s = null;
			
			return s;
		}
		
	}
}
