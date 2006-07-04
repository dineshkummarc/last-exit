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
using System.Collections;
using System.Xml;

using Gtk;

namespace LastExit {
        public class TagDialog : Dialog {
		enum ResponseType {
			Tag = 10
		};

		[Glade.Widget] private VBox tag_contents;

		[Glade.Widget] private ComboBox tag_type_combo;
		[Glade.Widget] private ScrolledWindow my_tags_container;
		[Glade.Widget] private ScrolledWindow global_tags_container;

		private TagSelector my_tags;
		private TagSelector global_tags;

		private Song song;

	        public TagDialog (Window w, Song s) : base ("Tag A Song", w, DialogFlags.DestroyWithParent) {
			this.HasSeparator = false;
			this.SetDefaultSize (415, 225);
			song = s;
			
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
			GetUserTags ();

			global_tags = new TagSelector ();
			global_tags.Visible = true;
			global_tags_container.Add (global_tags);
			GetGlobalTags ();

			tag_type_combo.Changed += new EventHandler (OnTagTypeChanged);
			tag_type_combo.Active = 0;
		}

		private void OnTagTypeChanged (object o, EventArgs args)
		{
			switch (tag_type_combo.Active) {
			case 0:
				this.Title = "Tag '" + song.Track + "'";
				break;

			case 1:
				this.Title = "Tag '" + song.Album + "'";
				break;

			case 2:
				this.Title = "Tag '" + song.Artist + "'";
				break;

			default:
				break;
			}
		}

		private void GetUserTags ()
		{
			FMRequest fmr = new FMRequest ();
			string base_url = Driver.connection.BaseUrl;
			string username = Driver.connection.Username;
			string url = "http://" + base_url + "/1.0/user/" + username + "/tags.xml";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (UserTagsCompleted);
			fmr.DoRequest (url);

			Driver.connection.DoOperationStarted ();
		}
			
		private void UserTagsCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				string content;

				content = request.Data.ToString ();
				my_tags.Tags = ParseUserTags (content);
			}

			Driver.connection.DoOperationFinished ();
		}

		private string get_node_text (XmlNode node,
					      string name)
		{
			return node[name].InnerText;
		}

		private ArrayList ParseUserTags (string content)
		{
			XmlDocument xml = new XmlDocument ();
			XmlNodeList elemlist;
			ArrayList tags = new ArrayList ();
			
			xml.LoadXml (content);
			elemlist = xml.GetElementsByTagName ("toptags");
			if (elemlist.Count == 0) {
				return tags;
			}

			elemlist = xml.GetElementsByTagName ("tag");
			IEnumerator ienum = elemlist.GetEnumerator ();
			while (ienum.MoveNext ()) {
				XmlNode f_node = (XmlNode) ienum.Current;
				
				Tag t = new Tag ();
				t.Name = get_node_text (f_node, "name");
				t.Count =  Int32.Parse (get_node_text (f_node, "count"));
				tags.Add (t);
			}

			return tags;
		}

		private void GetGlobalTags ()
		{
			FMRequest fmr = new FMRequest ();
			string base_url = Driver.connection.BaseUrl;
			string username = Driver.connection.Username;
			string url = "http://" + base_url + "/1.0/tag/toptags.xml";

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (GlobalTagsCompleted);
			fmr.DoRequest (url);

			Driver.connection.DoOperationStarted ();
		}
			
		private void GlobalTagsCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				string content;

				content = request.Data.ToString ();
				global_tags.Tags = ParseGlobalTags (content);
			}

			Driver.connection.DoOperationFinished ();
		}

		// Audioscrobbler rant #94 - They present the same data
		// in two completely different formats.
		private ArrayList ParseGlobalTags (string content)
		{
			XmlDocument xml = new XmlDocument ();
			XmlNodeList elemlist;
			ArrayList tags = new ArrayList ();
			
			xml.LoadXml (content);
			elemlist = xml.GetElementsByTagName ("toptags");
			if (elemlist.Count == 0) {
				return tags;
			}

			elemlist = xml.GetElementsByTagName ("tag");
			IEnumerator ienum = elemlist.GetEnumerator ();
			while (ienum.MoveNext ()) {
				XmlNode t_node = (XmlNode) ienum.Current;
				
				Tag t = new Tag ();
				t.Name = t_node.Attributes.GetNamedItem ("name").InnerText;
				t.Count =  Int32.Parse (t_node.Attributes.GetNamedItem ("count").InnerText);
				tags.Add (t);
			}

			return tags;
		}

	}
}
