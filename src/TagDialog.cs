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

namespace LastExit {
        public class TagDialog : Dialog {
		enum ResponseType {
			Tag = 10
		};

		[Glade.Widget] private VBox tag_contents;

		[Glade.Widget] private Label tag_label;
		[Glade.Widget] private ScrolledWindow my_tags_container;
		[Glade.Widget] private ScrolledWindow global_tags_container;

		private TagSelector my_tags;
		private TagSelector global_tags;

	        public TagDialog (Window w) : base ("Tag A Song", w, DialogFlags.DestroyWithParent) {
			this.HasSeparator = false;
			this.SetDefaultSize (415, 225);
			
			Glade.XML glade_xml = new Glade.XML (null, "TagDialog.glade", "tag_contents", null);
			glade_xml.Autoconnect (this);
			this.VBox.Add (tag_contents);

			SetupUI ();

			this.AddButton ("New Tag", (int) ResponseType.Tag);
			this.AddButton ("Cancel", Gtk.ResponseType.Cancel);
			// FIXME: This really needs a better label
			this.AddButton ("Tag", Gtk.ResponseType.Ok);

			tag_contents.Visible = true;
		}

		private void SetupUI ()
		{
			my_tags = new TagSelector ();
			my_tags.Visible = true;
			my_tags_container.Add (my_tags);

			
			global_tags = new TagSelector ();
			global_tags.Visible = true;
			global_tags_container.Add (global_tags);
		}
	}
}
