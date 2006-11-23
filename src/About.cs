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
	public class About : Gtk.AboutDialog
	{
		// Strings
		private static readonly string string_lastexit =
			Catalog.GetString ("Last Exit");

		private static readonly string string_copyright =
			Catalog.GetString ("Copyright © 2006 Iain Holmes");

		private static readonly string string_description =
			Catalog.GetString ("A Last.fm radio player");
		
		// Authors
		private static readonly string [] authors = {
			Catalog.GetString ("Iain Holmes <iain@gnome.org>"),
			Catalog.GetString ("Baris Cicek <baris@teamforce.name.tr>"),
			Catalog.GetString ("Brandon Hale <brandon@ubuntu.com>"),
			null,
		};
		
		// Documenters
		private static readonly string [] documenters = {
		};

		// Icon
		private static readonly Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (null, "last-exit-16.png");
	
		// Variables
		private static string translators =
			Catalog.GetString ("translator-credits");
		
        public About (Gtk.Window parent)
        {
                TransientFor = parent;
                Name = string_lastexit;
                Authors = authors;
                Comments = string_description;
                Copyright = string_copyright;
		// FIXME: Remove this check when we have docs :)
		Documenters = (documenters.Length > 0) ? documenters : null;
		if (System.String.Compare (translators, 
					"translator-credits") != 0) {
                	TranslatorCredits = translators;
		}
                Logo = pixbuf;
                Website = "http://www.o-hand.com/~iain/last-exit/";

                Run ();
                Destroy ();
        }
	}
}
