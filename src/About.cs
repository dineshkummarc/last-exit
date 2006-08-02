/*
 * Copyright (C) 2006 Brandon Hale <brandon@ubuntu.com>
 * Copyright (C) 2003, 2004, 2005 Jorn Baayen <jbaayen@gnome.org>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as
 * published by the Free Software Foundation; either version 2 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public
 * License along with this program; if not, write to the
 * Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
 */

using Gtk;
using Gdk;

using Mono.Unix;

namespace LastExit
{
	public class About : Gnome.About
	{
		// Strings
		private static readonly string string_translators = 
			Catalog.GetString ("translator-credits");

		private static readonly string string_lastexit =
			Catalog.GetString ("Last Exit");

		private static readonly string string_copyright =
			Catalog.GetString ("Copyright © 2006 Iain");

		private static readonly string string_description =
			Catalog.GetString ("A last.fm radio player");
		
		// Authors
		private static readonly string [] authors = {
			Catalog.GetString ("Iain <iain@gnome.org>"),
			Catalog.GetString ("Baris Cicek <foo@bar.com>"),
			Catalog.GetString ("Brandon Hale <brandon@ubuntu.com>"),
			"",
			null,
		};
		
		// Documenters
		private static readonly string [] documenters = {
			null,
		};

		// Icon
		private static readonly Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (null, "last-exit-16.png");
	
		// Variables
		private static string translators;
		
		// Static Constructor
		static About ()
		{
			// Translators
			if (string_translators == "translator-credits")
				translators = null;
			else
				translators = string_translators;
		}

		public About (Gtk.Window parent) : base (string_lastexit, 
							 "3.0", 
							 string_copyright, 
							 string_description,
							 authors, documenters, 
							 translators, pixbuf)
		{
			TransientFor = parent;
			
			Show ();
		}
	}
}
