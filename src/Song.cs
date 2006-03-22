/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *
 *  Copyright 2005 Iain Holmes
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License version 2 as 
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

namespace LastExit 
{
	public class Song {
		public delegate void CoverLoadedHandler (Pixbuf cover);
		public event CoverLoadedHandler CoverLoaded;

		private string artist;
		public string Artist {
			set { artist = value; }
			get { return artist; }
		}

		private string artist_url;
		public string ArtistUrl {
			set { artist_url = value; }
			get { return artist_url; }
		}

		private string album;
		public string Album {
			set { album = value; }
			get { return album; }
		}

		private string album_url;
		public string AlbumUrl {
			set { album_url = value; }
			get { return album_url; }
		}

		private string track;
		public string Track {
			set { track = value; }
			get { return track; }
		}

		private string track_url;
		public string TrackUrl {
			set { track_url = value; }
			get { return track_url; }
		}

		private string station;
		public string Station {
			set { station = value; }
			get { return station; }
		}

		private string station_url;
		public string StationUrl {
			set { station_url = value; }
			get { return station_url; }
		}
		
		private string station_feed;
		public string StationFeed {
			set { station_feed = value; }
			get { return station_feed; }
		}

		private string station_feed_url;
		public string StationFeedUrl {
			set { station_feed_url = value; }
			get { return station_feed_url; }
		}

		private bool cover_requested;

		private Pixbuf cover;
		public Pixbuf Cover {
			get { return cover; }
		}

		private string cover_url;
		public string CoverUrl {
			set { 
				cover_url = value; 
				// Having new cover_url means Cover is invalid
				cover = null;
				cover_requested = false;
			}
		}
				

		private int progress;
		public int Progress {
			set { progress = value; }
			get { return progress; }
		}

		private int length;
		public int Length {
			set { length = value; }
			get { return length; }
		}

		private bool discovery;
		public bool Discovery {
			set { discovery = value; }
			get { return discovery; }
		}

		private bool hated;
		public bool Hated {
			set { hated = value; }
			get { return hated; }
		}

		private bool loved;
		public bool Loved {
			set { loved = value; }
			get { return loved; }
		}

		public Song () {
			cover = null;
			cover_requested = false;
		}

		public void RequestCover () {
			if (cover != null) {
				// Already got the cover
				if (CoverLoaded != null) {
					CoverLoaded (cover);
				}

				return;
			}

			if (cover_url == null) {
				// In an ideal bug free world where it isn't
				// 1am, this would fire a signal with a No 
				// Cover image...but I'm tired.
				// Oh yeah, FIXME!
				
				return;
			}

			if (cover_requested) {
				return;
			}

			ImageLoader cl = new ImageLoader ();

			cl.ImageLoaded += new ImageLoader.ImageLoadedHandler (OnCoverLoaded);
			cl.GetImage (cover_url, Driver.CoverSize, Driver.CoverSize);
			cover_requested = true;
		}

		private void OnCoverLoaded (Pixbuf cover) 
		{
			this.cover = cover;

			if (CoverLoaded != null) {
				CoverLoaded (cover);
			}

			cover_requested = false;
		}
	}
}
