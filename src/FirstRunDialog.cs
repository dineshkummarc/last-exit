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

using Gtk;
using Mono.Unix;

namespace LastExit
{
        public class FirstRunDialog : Dialog {
		[Glade.Widget] private VBox first_run_contents;

		[Glade.Widget] private VBox password_container;
		[Glade.Widget] private Entry username_entry;
		
		private IconEntry password_entry;

		[Glade.Widget] private VBox signup_container;

		private Button signup_button;

		public string Username {
			get { return username_entry.Text; }
		}

		public string Password {
			get { return password_entry.Text; }
		}

	        public FirstRunDialog () {
			this.Title = Catalog.GetString("Last.fm Account Details");
			this.HasSeparator = false;
			this.Modal = true;

			Glade.XML glade_xml = new Glade.XML (null, "FirstRunDialog.glade", "first_run_contents", null);
			glade_xml.Autoconnect (this);
			this.VBox.Add (first_run_contents);

			// FIXME: Use stock?
			this.AddButton (Catalog.GetString("Quit"), ResponseType.Reject);
			this.AddButton (Catalog.GetString("Start Player"), ResponseType.Ok);

			signup_button = new Gnome.HRef ("http://www.last.fm/signup.php",
							Catalog.GetString("Sign up for Last.fm"));
			signup_container.Add (signup_button);
			signup_button.Visible = true;

			password_entry = new IconEntry ();
			password_entry.Visibility = false;
			password_entry.Visible = true;
			// This is the bullet char...
			password_entry.InvisibleChar = '•';
			password_entry.SetIcon (IconEntryPosition.Primary,
						new Image (null, "secure.png"));
			
			password_container.Add (password_entry);

			first_run_contents.Visible = true;
		}
	}
}
