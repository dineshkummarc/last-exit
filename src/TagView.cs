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
        public class TagView : TreeView {
		public enum Column {
			ID,
			Name,
			Match,
			PrettyText
		};

		private ListStore tagstore;

		public ArrayList Tags {
			set {
				tagstore.Clear ();
				if (value == null) {
					return;
				}

				foreach (Tag t in value) {
					string pretty = t.Name + "\n<span size=\"smaller\">Relevance: " + (t.Match * 100) + "%</span>";
					
					tagstore.AppendValues (t.ID, t.Name, t.Match, pretty);
				}
			}
		}

		public TagView ()
		{
			tagstore = new ListStore (typeof (int),
						  typeof (string),
						  typeof (double),
						  typeof (string));

			this.Model = tagstore;
			this.HeadersVisible = false;
			this.RulesHint = true;

			this.AppendColumn ("tag", new CellRendererText (), 
					   "markup", (int) Column.PrettyText);
		}
	}
}
