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
using System.Text;

using Gtk;

namespace LastExit
{
        public class FindStation : Dialog {
		[Glade.Widget] private VBox search_contents;
		[Glade.Widget] private VBox search_combo_container;
		[Glade.Widget] private VBox search_entry_container;

		[Glade.Widget] private Button search_button;

		[Glade.Widget] private VBox results_container;

		[Glade.Widget] HBox similar_contents;
		[Glade.Widget] VBox image_container;
		[Glade.Widget] Label band_name_label;
		[Glade.Widget] Label similar_artist_label;

		[Glade.Widget] ScrolledWindow tag_container;

		private TagView tagview;
		private Image band_image;

		public enum SearchType {
			SoundsLike,
			TaggedAs
		};

		private ComboBox search_combo;
		private IconEntry search_entry;

		private Artist selected_artist;

	        public FindStation (Window w) : base ("Find A Station", w, DialogFlags.DestroyWithParent) {
			this.HasSeparator = false;
			this.SetDefaultSize (430, 290);
			
			Glade.XML glade_xml = new Glade.XML (null, "FindStation.glade", "search_contents", null);
			glade_xml.Autoconnect (this);
			this.VBox.Add (search_contents);
			
			Glade.XML similar_xml = new Glade.XML (null, "SimilarResults.glade", "similar_contents", null);
			similar_xml.Autoconnect (this);
			results_container.Add (similar_contents);

			Glade.XML tag_xml = new Glade.XML (null, "TagResults.glade", "tag_container", null);
			tag_xml.Autoconnect (this);
			tag_container.Visible = false;
			results_container.Add (tag_container);

			this.AddButton ("Cancel", ResponseType.Cancel);
			this.AddButton ("Change Station", ResponseType.Ok);
			this.SetResponseSensitive (ResponseType.Ok, false);

			SetupUI ();
			
			Driver.connection.SearchCompleted += new FMConnection.SearchCompletedHandler (OnSearchCompleted);

			search_button.Sensitive = false;
			
			similar_contents.Visible = false;
			search_contents.Visible = true;
		}
		
		protected override void OnResponse (ResponseType response_id) 
		{
			if (response_id == ResponseType.Ok) {
				if (search_combo.Active == 0) {
					string station = FMConnection.MakeArtistRadio (selected_artist.Name);
					Driver.player.Stop ();
					Driver.connection.ChangeStation (station);
				} else if (search_combo.Active == 1) {
					TreeSelection sel = tagview.Selection;
					TreeModel m;
					TreeIter iter;

					if (sel.GetSelected (out m, out iter)) {
						string name = (string) m.GetValue (iter, (int) TagView.Column.Name);
						string station = FMConnection.MakeTagRadio (name);
						Driver.player.Stop ();
						Driver.connection.ChangeStation (station);
					}
				}
			}
			
			this.Destroy ();
		}
		
		private void SetupUI () 
		{
			search_combo = ComboBox.NewText ();
			search_combo.AppendText ("Music that sounds like");
			search_combo.AppendText ("Music that is tagged as");
			search_combo.Active = 0;
			
			search_combo.Visible = true;
			
			search_combo_container.Add (search_combo);

			search_combo.Changed += new EventHandler (OnComboChanged);

			search_entry = new IconEntry ();
			search_entry.Visible = true;
			search_entry.AddClearButton ();
			search_entry_container.Add (search_entry);

			search_entry.Changed += new EventHandler (OnSearchChanged);
			search_entry.Activated += new EventHandler (OnSearchClicked);
			search_button.Clicked += new EventHandler (OnSearchClicked);

			band_image = new Image ();
			band_image.Visible = true;
			image_container.Add (band_image);

			tagview = new TagView ();
			tagview.Visible = true;
			tag_container.Add (tagview);
		}
		
		private void OnComboChanged (object obj, EventArgs args)
		{
			selected_artist = null;
			tagview.Tags = null;

			search_entry.Text = "";

			similar_contents.Visible = false;
			tag_container.Visible = false;

			this.SetResponseSensitive (ResponseType.Ok, false);
		}

		private void OnSearchChanged (object obj, EventArgs args) 
		{
			if (search_entry.Text == "") {
				search_button.Sensitive = false;
			} else {
				search_button.Sensitive = true;
			}
		}

		private void OnSearchClicked (object obj, EventArgs args) 
		{
			SearchType t;

			search_button.Sensitive = false;

			switch  (search_combo.Active) {
			case 0:
				t = SearchType.SoundsLike;
				break;

			case 1:
				t = SearchType.TaggedAs;
				break;

			default:
				t = SearchType.SoundsLike;
				break;
			}

			Driver.connection.Search (t, search_entry.Text);
		}

		private void OnSearchCompleted (object o, SearchType t)
		{
			switch (t) {
			case SearchType.SoundsLike:
				FillArtistDetails ((Artist) o);

				similar_contents.Visible = true;
				tag_container.Visible = false;
				break;

			case SearchType.TaggedAs:
				FillTagDetails ((ArrayList) o);

				TreeSelection sel = tagview.Selection;
				sel.Changed += new EventHandler (OnSelectionChanged);
				similar_contents.Visible = false;
				tag_container.Visible = true;
				break;

			default:
				break;
			}

			search_button.Sensitive = true;
		}

		private void OnSelectionChanged (object o, EventArgs args)
		{
			TreeSelection sel = (TreeSelection) o;
			TreeIter iter;
			TreeModel model;
			bool selection;

			selection = sel.GetSelected (out model, out iter);
			this.SetResponseSensitive (ResponseType.Ok, selection);
		}

		private void FillArtistDetails (Artist artist) 
		{
			selected_artist = artist;

			if (artist == null) {
				band_name_label.Markup = "Artist not found";
				similar_artist_label.Text = "";
				similar_contents.Visible = true;
				search_button.Sensitive = true;
				this.SetResponseSensitive (ResponseType.Ok, false);
				return;
			}

			if (artist.Streamable == false) {
				band_name_label.Markup = artist.Name + " is not streamable.";
				similar_artist_label.Text = "";
				similar_contents.Visible = true;
				search_button.Sensitive = true;
				this.SetResponseSensitive (ResponseType.Ok, false);
				return;
			}

			this.SetResponseSensitive (ResponseType.Ok, true);
			band_name_label.Markup = "Music that sounds like <b>" + StringUtils.EscapeForPango (artist.Name) + "</b>";
			artist.ImageLoaded += new Artist.ImageLoadedHandler (OnImageLoaded);
			artist.RequestImage ();

			artist.SimilarArtists.Sort ();
			StringBuilder sim = new StringBuilder ("Featuring: \n");
			int i = 0;
			foreach (SimilarArtist sa in artist.SimilarArtists) {
				if (sa.Streamable) {
					// Don't count any artist that
					// can't be played.
					continue;
				}

				i++;
				if (i < 9) {
					sim.Append (sa.Name + ", ");
				} else if (i == 9) {
					sim.Append (sa.Name);
				}
			}
			sim.Append (" and " + (i - 10) + " others.");

			similar_artist_label.Text = sim.ToString ();

			similar_contents.Visible = true;
		}

		private void FillTagDetails (ArrayList tags)
		{
			tagview.Tags = tags;
		}

		private void OnImageLoaded (Gdk.Pixbuf image)
		{
			band_image.FromPixbuf = image;
		}
	}
}
