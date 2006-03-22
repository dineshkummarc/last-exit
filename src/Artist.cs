/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *
 *  Copyright 2005 Iain Holmes
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
using Gdk;

namespace LastExit 
{
	public class Artist {
		public delegate void ImageLoadedHandler (Pixbuf image);
		public event ImageLoadedHandler ImageLoaded;

		private string name;
		public string Name {
			get { return name; }
			set { name = value; }
		}

		private bool streamable;
		public bool Streamable {
			get { return streamable; }
			set { streamable = value; }
		}

		private Pixbuf artist_image;
		public Pixbuf ArtistImage {
			get { return artist_image; }
		}

		private string image_url;
		public string ImageUrl {
			set { image_url = value; }
		}
		
		private string mbid;
		public string Mbid {
			get { return mbid; }
			set { mbid = value; }
		}
		
		private ArrayList similar_artists;
		public ArrayList SimilarArtists {
			get { return similar_artists; }
		}

		public Artist () {
			artist_image = null;
			similar_artists = new ArrayList ();
		}

		public void AddSimilarArtist (SimilarArtist artist) {
			similar_artists.Add (artist);
		}

		public void RequestImage () 
		{
			ImageLoader il = new ImageLoader ();
			il.ImageLoaded += new ImageLoader.ImageLoadedHandler (OnImageLoaded);
			il.GetImage (image_url);
		}

		private void OnImageLoaded (Pixbuf image)
		{
			artist_image = image;

			if (ImageLoaded != null) {
				ImageLoaded (image);
			}
		}
	}

        public class SimilarArtist : IComparable{
		private string name;
		public string Name {
			get { return name; }
			set { name = value; }
		}

		private string url;
		public string Url {
			get { return url; }
			set { url = value; }
		}

		private bool streamable;
		public bool Streamable {
			get { return streamable; }
			set { streamable = value; }
		}

		private int relevance;
		public int Relevance {
			get { return relevance; }
			set { relevance = value; }
		}

		private string mbid;
		public string Mbid {
			get { return mbid; }
			set { mbid = value; }
		}

		public SimilarArtist () 
		{
		}

		// IComparable::CompareTo
		public int CompareTo (Object rhs)
		{
			SimilarArtist sa = (SimilarArtist) rhs;

			// This is negative because we want more relevant first
			return -this.relevance.CompareTo (sa.Relevance);
		}
	}
}
