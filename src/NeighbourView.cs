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

using Gtk;

namespace LastExit 
{
        public class NeighbourView : TreeView {
                public enum Column {
                        Avatar,
                        Name
		};

		private ListStore neighbourstore;

		public ArrayList Neighbours {
                        set { 
                                neighbourstore.Clear ();
                                if (value == null) {
                                        return;
                                }

                                foreach (Fan f in value) {
                                        neighbourstore.AppendValues (f.Image,
                                                                     f.Username);
                                }
                        }
		}

		public NeighbourView ()
		{
			neighbourstore = new ListStore (typeof (Gdk.Pixbuf),
                                                        typeof (string));
                        
			this.Model = neighbourstore;
			this.HeadersVisible = false;
			this.RulesHint = true;

                        this.AppendColumn ("avatar", new CellRendererPixbuf (),
                                           "pixbuf", (int) Column.Avatar);
			this.AppendColumn ("tag", new CellRendererText (), 
					   "markup", (int) Column.Name);
		}
	}
}
