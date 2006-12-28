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

using Gtk;
using Gdk;

namespace LastExit
{
        public class MagicCoverImage : Gtk.Image {
		public static readonly Pixbuf UnknownCover = new Pixbuf(null, "unknown-cover.png");
		private Pixbuf current;
		private Pixbuf next;

		public delegate void CoverImageChangedHandler (MagicCoverImage mci);
		public event CoverImageChangedHandler CoverImageChanged;

		private TimeSpan start;

		const double FADE_DURATION = 1500.0;

		public MagicCoverImage () {
			current = null;
			next = null;
		}

		public void ChangePixbuf (Pixbuf pb) {
			if(pb == null)
				pb = UnknownCover;

			if (current == null) {
				this.FromPixbuf = pb;
				current = pb;
			} else {
				next = pb;
				start_fade ();
			}
			if (CoverImageChanged != null) {
				CoverImageChanged (this);
			}
		}

		private void start_fade () {
			start = DateTime.Now.TimeOfDay;

			GLib.Idle.Add (new GLib.IdleHandler (do_fade));
		}

		private bool do_fade () {
			TimeSpan now = DateTime.Now.TimeOfDay;
			double elapsed = now.TotalMilliseconds - start.TotalMilliseconds;
			double percent = elapsed / FADE_DURATION;
			
			this.FromPixbuf = make_frame (percent);
			if (elapsed > FADE_DURATION) {
				current = next;
				return false;
			} else {
				return true;
			}
		}

		private Pixbuf make_frame (double percent) {
			if (percent > 1.0) {
				percent = 1.0;
			}

			Pixbuf frame = current.Copy ();
			next.Composite (frame, 0, 0, next.Width, next.Height,
					0, 0, 1.0, 1.0, InterpType.Nearest, 
					(int) (percent * 255));

			return frame;
		}
	}
}
