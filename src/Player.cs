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
		
		public delegate void NewSongHandler ();
		public event NewSongHandler NewSong;
		
		private SignalUtils.SignalDelegate new_song_cb;
		private bool playing;

		// Constructor
		[DllImport ("liblastexit")]
		private static extern IntPtr player_new ();

		public Player () : base (IntPtr.Zero) {
			Raw = player_new ();
			playing = false;
			new_song_cb = new SignalUtils.SignalDelegate (OnNewSong);
			SignalUtils.SignalConnect (Raw, "new-song", new_song_cb);
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
			player_play (Raw);
			playing = true;
			Driver.PlayerWindow.UpdatePlayingUI ();
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
				NewSong ();
			}
		}
	}
}

