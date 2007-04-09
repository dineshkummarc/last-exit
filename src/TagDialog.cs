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
using System.Web;
using System.Xml;

using Gtk;
using Mono.Unix;

namespace LastExit {
        public class TagDialog : Dialog {
		[Glade.Widget] private VBox tag_contents;

		[Glade.Widget] private ComboBox tag_type_combo;
		[Glade.Widget] private ScrolledWindow my_tags_container;
		[Glade.Widget] private VBox extra_tags_container;
		[Glade.Widget] private Button tag_button;

		private TagSelector my_tags;
		private ComboBoxEntry extra_tags;

		private Song song;

	        public TagDialog (Window w, Song s) : base (Catalog.GetString("Tag A Song"), w, DialogFlags.DestroyWithParent) {
			this.HasSeparator = false;
			this.SetDefaultSize (415, 225);
			song = s;
			
			Glade.XML glade_xml = new Glade.XML (null, "TagDialog.glade", "tag_contents", null);
			glade_xml.Autoconnect (this);
			this.VBox.Add (tag_contents);

			SetupUI ();

			GetTagsForSong (song);

			this.AddButton ("gtk-close", ResponseType.Close);

			tag_contents.Visible = true;
		}

		protected override void OnResponse (ResponseType response_id) {
			this.Destroy ();
		}

		private void SetupUI ()
		{
			my_tags = new TagSelector ();
			my_tags.Visible = true;
			my_tags_container.Add (my_tags);
			GetUserTags ();

			tag_type_combo.Changed += new EventHandler (OnTagTypeChanged);
			tag_type_combo.Active = 0;

			tag_button.Clicked += new EventHandler (OnTagButtonClicked);

			extra_tags = ComboBoxEntry.NewText ();
			extra_tags.Visible = true;
			extra_tags_container.Add (extra_tags);
		}

		private void OnTagTypeChanged (object o, EventArgs args)
		{
			switch (tag_type_combo.Active) {
			case 0:
				this.Title = String.Format(Catalog.GetString("Tag '{0}'"), song.Track);
				break;

			case 1:
				this.Title = String.Format(Catalog.GetString("Tag '{0}'"), song.Album);
				break;

			case 2:
				this.Title = String.Format(Catalog.GetString("Tag '{0}'"), song.Artist);
				break;

			default:
				break;
			}

			GetTagsForSong (song);
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
				foreach (Tag t in ParseUserTags (content)) {
					extra_tags.AppendText (t.Name);
				}
				
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

		private void GetTagsForSong (Song s) {
			FMRequest fmr = new FMRequest ();
			string base_url = Driver.connection.BaseUrl;
			string username = Driver.connection.Username;

			string mode;

			switch (tag_type_combo.Active) {
			case 0:
				mode = "/tracktags.xml?artist=" + s.SafeArtist + "&track=" + s.SafeTrack;
				break;

			case 1:
				mode = "/albumtags.xml?artist=" + s.SafeArtist + "&album=" + s.SafeAlbum;
				break;

			case 2:
				mode = "/artisttags.xml?artist=" + s.SafeArtist;
				break;

			default:
				mode = "error";
				break;
			}
			
 			string url = "http://" + base_url + "/1.0/user/" + username + mode;

			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (SongTagsCompleted);
			fmr.DoRequest (url);

			Driver.connection.DoOperationStarted ();
		}

		private void SongTagsCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				string content;

				content = request.Data.ToString ();
				my_tags.Tags = ParseSongTags (content);
			}

			Driver.connection.DoOperationFinished ();
		}

		private ArrayList ParseSongTags (string content)
		{
			XmlDocument xml = new XmlDocument ();
			XmlNodeList elemlist;
			ArrayList tags = new ArrayList ();
			
			xml.LoadXml (content);

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

		private void OnTagButtonClicked (object o, EventArgs args) {
			XmlRpc xmlrpc = new XmlRpc ();
			string tagname = extra_tags.Entry.Text;
			switch (tag_type_combo.Active) {
			case 0:
				xmlrpc.TagTrack (song.Artist, song.Track, tagname.Split(','));
				break;

			case 1:
				xmlrpc.TagAlbum (song.Artist, song.Album, tagname.Split(','));
				break;

			case 2:
				xmlrpc.TagArtist (song.Artist, tagname.Split(','));
				break;

			default:
				break;
			}
			
			GetTagsForSong (song);
		}

		private void SetTagCompleted (FMRequest request) {
			Driver.connection.DoOperationFinished ();
			// Update the list
			GetTagsForSong (song);
		}
	}
}
