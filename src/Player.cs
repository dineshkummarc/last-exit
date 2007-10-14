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
using System.Runtime.InteropServices;

using Mono.Posix;

namespace LastExit
{
	public class PlayerException : Exception {
		public PlayerException (IntPtr p) : base (GLib.Marshaller.PtrToStringGFree (p)) {
		}
	}

	public class Player : GLib.Object {
		
		public delegate void NewSongHandler (Song song);
		public event NewSongHandler NewSong;
		public event NewSongHandler SongEnded;
		private SignalUtils.SignalDelegate new_song_cb;
		private SignalUtils.SignalDelegate end_song_cb;
		private SignalUtils.SignalDelegateStr error_cb;
		private bool playing;

		private Song current_song;
		private Playlist playlist;
		public Playlist Playlist {
			get { return playlist; }
		}

		// Constructor
		[DllImport ("liblastexit")]
		private static extern IntPtr player_new ();

		public Player (Playlist playlist) : base (IntPtr.Zero) {
			this.playlist = playlist; 
			playlist.SongReady += new Playlist.SongReadyHandler (OnSongReady);

			Raw = player_new ();
			playing = false;
			new_song_cb = new SignalUtils.SignalDelegate (OnNewSong);
			end_song_cb = new SignalUtils.SignalDelegate (OnEndSong);
			error_cb = new SignalUtils.SignalDelegateStr (OnError);
			SignalUtils.SignalConnect (Raw, "new-song", new_song_cb);
			SignalUtils.SignalConnect (Raw, "end-song", end_song_cb);
			SignalUtils.SignalConnect (Raw, "error", error_cb);
		}

		~Player () {
			Dispose ();
		}

		[DllImport ("liblastexit")]
		private static extern bool player_set_location (IntPtr player, 
								string filename);
		public string Location {
			set {
				player_set_location (Raw, value);
			}
		}

		public bool Playing {
			set { 
				playing = value;
				Driver.PlayerWindow.UpdatePlayingUI ();
			}
			get { return playing; }
		}

		[DllImport ("liblastexit")]
		private static extern void player_play (IntPtr player);
		
		public void Play () {
			Console.WriteLine ("Player: Requesting Song");
			playing = true;
			playlist.RequestNextSong ();			
			Driver.PlayerWindow.UpdatePlayingUI ();
		}
		
		private void OnSongReady (Song song)
		{
			Console.WriteLine ("Player: Got Song");
			current_song = song;
			Console.WriteLine ("Now playing: " + song.Location);
			Driver.connection.InvokeMetadataLoaded (song);
			this.Location = song.Location;
			player_play (Raw);
			
			song.StartTime = DateTime.UtcNow;
		}
		
		[DllImport ("liblastexit")]
		private static extern void player_stop (IntPtr player);

		public void Stop () {
			player_stop (Raw);
			playing = false;
			Driver.PlayerWindow.UpdatePlayingUI ();
		}

		[DllImport ("liblastexit")]
		private static extern void player_set_volume (IntPtr player, int volume);

		public int Volume {
			set { player_set_volume (Raw, value); }
		}
		
		private void OnNewSong (IntPtr obj) {
			if (NewSong != null) {
				NewSong (current_song);
			}
		}

		private void OnEndSong (IntPtr obj) {
			Console.WriteLine ("EndSong");
			
			if (SongEnded != null) {
				SongEnded (current_song);
			}
			current_song = null;
			this.Play ();
		}

		private void OnError (IntPtr obj, string error) {
			Console.Error.WriteLine ("GST error: " + error);
			this.Stop ();
		}

		public void SkipSong ()
		{
			if (Playing) {
				player_stop (Raw);
				Play ();
			}				
		}
		
		
		[DllImport ("liblastexit")]
		private static extern long player_get_stream_position (IntPtr player);
		
		public long StreamPosition {
			get {
				if (Playing) {
					return player_get_stream_position (Raw);
				} else {
					return -1;
				}
			}
		}
	}
}

