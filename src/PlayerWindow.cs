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
using Mono.Unix;
using Bacon;

namespace LastExit
{
      public class PlayerWindow : Window {
		Glade.XML glade_xml;

		// Widgets
		[Glade.Widget] private VBox player_contents;
		//[Glade.Widget] private Box menu_bar_box;

		[Glade.Widget] private Container volume_button_container;
		[Glade.Widget] private Container cover_image_container;

		[Glade.Widget] private ToggleButton toggle_play_button;
		[Glade.Widget] private Button next_button;
		[Glade.Widget] private Button love_button;
		[Glade.Widget] private Button hate_button;
		[Glade.Widget] private Button tag_button;
		[Glade.Widget] private Button journal_button;
		[Glade.Widget] private Button preferences_button;

		private VolumeButton volume_button;
		public int Volume {
			get {
				return volume_button.Volume;
			}
			set {
				volume_button.Volume = value;
			}
		}

		[Glade.Widget] private VBox title_label_container;
		[Glade.Widget] private VBox artist_label_container;
		private UrlLabel artist_label;
		private UrlLabel song_label;

		[Glade.Widget] private Button info_button;

		[Glade.Widget] private ComboBox station_combo;

		private StationStore stations;
		private Hashtable known_stations;
		private ArrayList custom_stations;

		private MagicCoverImage cover_image;
		private Image love_image;
		private Image hate_image;

		[Glade.Widget] private Gtk.Image tag_button_image;
		[Glade.Widget] private Gtk.Image info_button_image;

		private Gdk.Pixbuf user_image;
		private Gdk.Pixbuf neighbour_image;
		private Gdk.Pixbuf recommended_image;
		private Gdk.Pixbuf favourite_image;

		private Song current_song;

		private InfoWindow i_window = null;
       
		private TrayIcon trayicon; 
		enum Column {
			Pixbuf,
			Name,
			Sensitive,
			Path
		};

		private enum TargetType {
			UriList,
			Uri
		};

		private static TargetEntry uri_list = new TargetEntry ("text/uri-list", 0, (uint) TargetType.UriList);
		private static TargetEntry netscape = new TargetEntry ("_NETSCAPE_URL", 0, (uint) TargetType.Uri);
		private static TargetEntry [] drag_entries = {
			uri_list, netscape,
		};
        
		private int last_x = -1;
		private int last_y = -1;
		private bool window_visible;
		
		public bool WindowVisible {
			get { return window_visible; }
		}

		private SpecialKeys special_keys;
		
		public void SetWindowVisible (bool visible, uint time) {
			window_visible = visible;
			
			if (window_visible) {
				if (!Visible && last_x >= 0 && last_y >= 0)
					Move (last_x, last_y);
				Show ();
				GdkWindow.Focus (time);
			} else {
				GetPosition (out last_x, out last_y);
				Visible = false;
			}
			UpdateWindowVisibilityUI ();
		}
		
		public void UpdateWindowVisibilityUI () {
			((ToggleAction) Driver.Actions ["ToggleVisible"]).Active = Visible;
		}
		
		public void UpdatePlayingUI () {
			((ToggleAction) Driver.Actions ["TogglePlay"]).Active = Driver.player.Playing;
		}
		
		// Hack to stop the player playing when we change the combo.
		private bool internal_change;
		
		public String InitialStation;
		
		public PlayerWindow () : base (WindowType.Toplevel) {
			Title = "Last Exit";

			InitialStation = null;
			known_stations = new Hashtable ();
			custom_stations = new ArrayList (5);

			current_song = null;

			SetupPlayer ();

			// Build the interface
			glade_xml = new Glade.XML (null, "PlayerWindow.glade", "player_contents", null);
			glade_xml.Autoconnect (this);
			base.Add (player_contents);

			base.DeleteEvent += new DeleteEventHandler (OnDeleteEvent);

			base.DragDataReceived += new DragDataReceivedHandler (OnDragDataReceived);
			Gtk.Drag.DestSet (this, DestDefaults.All, drag_entries, Gdk.DragAction.Copy);

			SetupButtons ();
			trayicon = new TrayIcon (this);
			SetupUI ();

			// For MediaKeys support
			special_keys = new SpecialKeys();
			special_keys.Delay = new TimeSpan(500 * TimeSpan.TicksPerMillisecond);

			// Set event handler for MediaKey Press
			special_keys.RegisterHandler(OnSpecialKeysPressed,
				SpecialKey.AudioPlay,
				SpecialKey.AudioNext
			);
		}

		public void Run () {
			Show ();
			SetWindowVisible (true, 0);
		}
		
		public void Quit () {
			ArrayList recent = new ArrayList ();
			
			foreach (string path in custom_stations) {
				RecentStations.Station s = new RecentStations.Station ();
				s.path = path;
				s.name = (string) known_stations[path];
				
				recent.Add (s);
			}
			
			Driver.config.Volume = volume_button.Volume;
			RecentStations.Recent = recent;
			Driver.Exit ();
		}

		private void SetupPlayer () {
			Driver.player.NewSong += new Player.NewSongHandler (OnNewSong);
		}

		private void SetupButtons () {
			toggle_play_button.Toggled += new EventHandler (OnTogglePlayButtonClicked);
			next_button.Clicked += new EventHandler (OnNextButtonClicked);
			love_button.Clicked += new EventHandler (OnLoveButtonClicked);
			hate_button.Clicked += new EventHandler (OnHateButtonClicked);
			tag_button.Clicked += new EventHandler (OnTagButtonClicked);
			journal_button.Clicked += new EventHandler (OnJournalButtonClicked);
			preferences_button.Clicked += new EventHandler (OnPreferencesButtonClicked);
			info_button.Clicked += new EventHandler (OnInfoButtonClicked);
			KeyPressEvent += OnKeyPressEvent;

			// Volume
			volume_button = new VolumeButton ();
			volume_button.Relief = ReliefStyle.Normal;
			volume_button_container.Add (volume_button);
			volume_button.Visible = true;
			volume_button.VolumeChanged += new VolumeButton.VolumeChangedHandler (OnVolumeChanged);

			volume_button.Volume = Driver.config.Volume;
		}

		
		private void SetupUI () {
	
			Driver.connection.ConnectionChanged += new FMConnection.ConnectionChangedHandler (OnConnectionChanged);
			Driver.connection.OperationStarted += new FMConnection.OperationHandler (OnOperationStarted);
			Driver.connection.OperationFinished += new FMConnection.OperationHandler (OnOperationFinished);
			Driver.connection.StationChanged += new FMConnection.StationChangedHandler (OnStationChanged);
			Driver.connection.MetadataLoaded += new FMConnection.MetadataCompletedHandler (OnMetadataCompleted);
			Driver.connection.Error += new FMConnection.ErrorHandler (OnError);

			artist_label = new UrlLabel ();
			artist_label.SetAlignment ((float) 0.0, (float) 0.5);
			artist_label.Ellipsize = Pango.EllipsizeMode.End;
			artist_label.UrlActivated += new UrlActivatedHandler (OnUrlActivated);
 			artist_label.Visible = true;
 			artist_label_container.Add (artist_label);

			song_label = new UrlLabel ();
			song_label.SetAlignment ((float) 0.0, (float) 0.5);
			song_label.Ellipsize = Pango.EllipsizeMode.End;
			song_label.UrlActivated += new UrlActivatedHandler (OnUrlActivated);
  			song_label.Visible = true;
  			title_label_container.Add (song_label);

			cover_image = new MagicCoverImage ();
			cover_image_container.Add (cover_image);
			cover_image.CoverImageChanged += new MagicCoverImage.CoverImageChangedHandler (set_height_limit);
			cover_image.Visible = true;

			love_image = new Image ();
			love_image.FromPixbuf = new Gdk.Pixbuf (null, "face-smile.png");
			love_button.Add (love_image);
			love_image.Visible = true;

			hate_image = new Image ();
			hate_image.FromPixbuf = new Gdk.Pixbuf (null, "face-sad.png");
			hate_button.Add (hate_image);
			hate_image.Visible = true;

			// Add additional search path
			IconTheme current_theme = IconTheme.Default;
			current_theme.AppendSearchPath(Defines.PREFIX+"/share/icons/");

			tag_button_image.SetFromStock ("tag-new", Gtk.IconSize.Button);
			info_button_image.SetFromStock ("show-info", Gtk.IconSize.Button);

			// Create images
			user_image = new Gdk.Pixbuf (null, "person-image.png");
			neighbour_image = new Gdk.Pixbuf (null, "people-image.png");
			favourite_image = new Gdk.Pixbuf (null, "favourites-image.png");
			recommended_image = new Gdk.Pixbuf (null, "recommended-image.png");

			
			// Create a store for the stations
			stations = new StationStore (typeof (Gdk.Pixbuf), typeof (String), typeof (bool), typeof (String));

			add_station (neighbour_image, Catalog.GetString("Neighbour Station"),
				     FMConnection.MakeUserRadio (Driver.connection.Username, 
								 "/neighbours"), true);
			add_station (user_image, Catalog.GetString("Personal Station"), 
				     FMConnection.MakeUserRadio (Driver.connection.Username,
								 "/personal"), 
				     Driver.connection.Subscriber);
			add_station (favourite_image, Catalog.GetString("Favourites Station"), 
				     FMConnection.MakeUserRadio (Driver.connection.Username,
								 "/loved"),
				     Driver.connection.Subscriber);
			add_station (recommended_image, Catalog.GetString("Recommended Music Station"),
				     FMConnection.MakeRecommendedRadio (Driver.connection.Username, Driver.config.RecommendationLevel), true);
			stations.AppendValues (null, "last-exit:separator", true, null);
			stations.AppendValues (null, Catalog.GetString("Other Station..."), 
					       true, null);

			// Add the recent stations too
			foreach (RecentStations.Station s in RecentStations.Recent) {
				add_custom_station (null, s.name, s.path, true);
			}

			if (Driver.connection.Connected == FMConnection.ConnectionState.Disconnected) {
 				station_combo.Sensitive = false;
				toggle_play_button.Sensitive = false;
				next_button.Sensitive = false;
				love_button.Sensitive = false;
				hate_button.Sensitive = false;
				tag_button.Sensitive = false;
				journal_button.Sensitive = false;
				info_button.Sensitive = false;
			}

			station_combo.Model = stations;
			station_combo.RowSeparatorFunc = station_separator_func;

			CellRenderer pb_render = new CellRendererPixbuf ();
			station_combo.PackStart (pb_render, false);
			station_combo.SetAttributes (pb_render, "pixbuf", Column.Pixbuf);
			station_combo.SetCellDataFunc (pb_render, station_sensitive_func);

			CellRenderer text_render = new CellRendererText ();
			station_combo.PackStart (text_render, true);
			station_combo.SetAttributes (text_render, "text", Column.Name);
			station_combo.SetCellDataFunc (text_render, station_sensitive_func);
			
			// Set default station
			station_combo.Active = 0;

			// Such a hassle to block emissions
			station_combo.Changed += new EventHandler (OnStationComboChanged);

			//Gdk.Pixbuf cover = new Gdk.Pixbuf (null, "unknown-cover.png", 66, 66);
			//cover_image.ChangePixbuf (cover);
			cover_image.ChangePixbuf(null);
		}
		
		private void set_height_limit (MagicCoverImage mci) {
			int height = 0;
			int width = 0;

			GetSize (out width, out height);
			
			Gdk.Geometry limits = new Gdk.Geometry ();
			limits.MinHeight = height;
			limits.MaxHeight = height;
			limits.MinWidth = SizeRequest ().Width;
			limits.MaxWidth = Gdk.Screen.Default.Width;

			SetGeometryHints (this, limits, 
					  Gdk.WindowHints.MaxSize |
					  Gdk.WindowHints.MinSize);
		}

		private void add_station (Gdk.Pixbuf image,
					  string name,
					  string path,
					  bool sensitive) 
		{
			stations.AppendValues (image, name, sensitive, path);
			known_stations.Add (path, name);
		}

		private void add_custom_station (Gdk.Pixbuf image,
						 string name,
						 string path,
						 bool sensitive)
		{
			if (custom_stations.Count == 0) {
				stations.InsertValues (4, null, "last-exit:separator", true, null);
			}

			if (known_stations.Contains (path) == false) {
				stations.InsertValues (5 + custom_stations.Count,
						       image, name, sensitive, path);
				custom_stations.Add (path);

				known_stations.Add (path, name);
			}
		}

		public void select_station (string path)
		{
			TreeIter iter;
			bool ret;
			int i = 0;

			ret = stations.GetIterFirst (out iter);
			while (ret) {
				string p = (string) stations.GetValue (iter, (int) Column.Path);
				if (p == path) {
					internal_change = true;
					station_combo.Active = i;
					internal_change = false;
					break;
				}

				i++;
				ret = stations.IterNext (ref iter);
			}
		}

		private void set_station_title (string path,
						string name)
		{
			TreeIter iter;
			bool ret;

			ret = stations.GetIterFirst (out iter);
			while (ret) {
				string p = (string) stations.GetValue (iter, (int) Column.Path);
				if (p == path) {
					stations.SetValue (iter, (int) Column.Name, name);
					break;
				}

				ret = stations.IterNext (ref iter);
			}
		}

		private bool station_separator_func (TreeModel model,
						     TreeIter iter)
		{
			string name = (string) model.GetValue (iter, (int) Column.Name);

			if (name == "last-exit:separator") {
				return true;
			} else {
				return false;
			}
		}

		private void station_sensitive_func (CellLayout cell_layout,
						     CellRenderer cell,
						     TreeModel model,
						     TreeIter iter)
		{
			bool sensitive = (bool) model.GetValue (iter, (int) Column.Sensitive);
			cell.Sensitive = sensitive;
		}
						     
		private void OnDeleteEvent (object o, DeleteEventArgs args) {
			Quit ();
		}

		private void OnDragDataReceived (object o, DragDataReceivedArgs args) {
			if (args.Info != (uint) TargetType.UriList &&
			    args.Info != (uint) TargetType.Uri) {
				Drag.Finish (args.Context, false, false, args.Time);
				return;
			}

			string path = System.Text.Encoding.UTF8.GetString (args.SelectionData.Data).Trim ();
			Driver.connection.ChangeStation (path);
		}

		private bool internal_toggle = false;
		private void OnTogglePlayButtonClicked (object o, EventArgs args) {
			ToggleButton t = o as ToggleButton;
			TreeIter iter;

			if (internal_toggle == true) {
				internal_toggle = false;
				return;
			}

			if (t.Active == false) {
                                Driver.player.Stop ();
				artist_label.Markup = "";
				song_label.Markup = "";
				this.Title = "Last Exit";

				trayicon.CanShowPopup = false;
				// FIXME: Need a blank cover.
				//Gdk.Pixbuf cover = new Gdk.Pixbuf (null, "unknown-cover.png", 66, 66);
				//cover_image.ChangePixbuf (cover);
				cover_image.ChangePixbuf(null);
				
				return;
			}

			if (station_combo.GetActiveIter (out iter)) {
				string path = (string) station_combo.Model.GetValue (iter, (int) Column.Path);
				if (path != null) {
					Driver.connection.ChangeStation (path);
				} else {
					FindStation dialog = new FindStation (this);
					dialog.Visible = true;
				}				
			}
		}

                public void TogglePlayButton ()
                {
			toggle_play_button.Activate ();
                }
		
                public void ActivateLoveButton ()
                {
			love_button.Activate ();
                }

                public void ActivateHateButton ()
                {
			hate_button.Activate ();
                }
 
		private void OnStationComboChanged (object o, EventArgs args) {
			ComboBox combo = o as ComboBox;
			TreeIter iter;

			if (internal_change == true) {
				return;
			}

			if (combo.GetActiveIter (out iter)) {
				string path = (string) combo.Model.GetValue (iter, (int) Column.Path);
				if (path != null) {
					Driver.connection.ChangeStation (path);
				} else {
					FindStation dialog = new FindStation (this);
					dialog.Visible = true;
				}
			}
		}
			
		private void OnNextButtonClicked (object o, EventArgs args) {
			Driver.connection.Skip ();
		}
		
		private void OnLoveButtonClicked (object o, EventArgs args) {
			love_button.Sensitive = false;
			hate_button.Sensitive = false;
                        CurrentSong.Loved = true;
                        XmlRpc xmlrpc = new XmlRpc ();
                        xmlrpc.loveTrack (current_song.Artist, current_song.Track);
		}

		private void OnHateButtonClicked (object o, EventArgs args) {
			love_button.Sensitive = false;
			hate_button.Sensitive = false;
                        CurrentSong.Hated = true;
                        XmlRpc xmlrpc = new XmlRpc ();
                        xmlrpc.banTrack (current_song.Artist, current_song.Track);
			Driver.connection.Skip ();
		}

		private void OnTagButtonClicked (object o, EventArgs args) 
		{
			TagDialog d = new TagDialog (this, current_song);
			d.Visible = true;
		}

		private void OnJournalButtonClicked (object o, EventArgs args) 
		{
			string url = "http://www.last.fm/user/" + Driver.connection.Username + "/journal/?action=create&artistname=" + current_song.Artist + "&trackname=" + current_song.Track;
			Driver.OpenUrl (url);
		}

		private void OnPreferencesButtonClicked (object o, EventArgs args) {
			PreferencesDialog prefs = new PreferencesDialog ();
		}
		
		private void OnInfoWindowResponse (object o, ResponseArgs args)
		{
			i_window.Destroy ();
			i_window = null;
		}

		private void OnInfoButtonClicked (object o, EventArgs args) 
		{
			if (i_window != null) {
				i_window.Present ();
				return;
			}

			i_window = new InfoWindow (current_song, this);
			i_window.Visible = true;
			i_window.Response += new ResponseHandler (OnInfoWindowResponse);
		}

		private void OnVolumeChanged (int vol) 
		{
			Driver.player.Volume = vol;
		}

		private void OnConnectionChanged (FMConnection.ConnectionState state) 
		{
			bool connected = (state == FMConnection.ConnectionState.Connected);

			if (state == FMConnection.ConnectionState.InvalidPassword) {
				this.Visible = false;

				FirstRunDialog frd = new FirstRunDialog ();

				int response = frd.Run ();
				
				frd.Visible = false;
				switch (response) {
				case (int) ResponseType.Reject:
					Environment.Exit (0);
					break;
					
				case (int) ResponseType.Ok:
					Driver.config.Username = frd.Username;
					Driver.config.Password = frd.Password;
					break;
					
				default:
					break;
				}
				
				Driver.connection.Username = Driver.config.Username;
				Driver.connection.Password = Driver.config.Password;
				Driver.connection.Handshake ();

				frd.Destroy ();
				this.Visible = true;
			}
			
			station_combo.Sensitive = connected;
			toggle_play_button.Sensitive = connected;
			next_button.Sensitive = connected;

			if (current_song != null) {
				love_button.Sensitive = connected;
				hate_button.Sensitive = connected;
				tag_button.Sensitive = connected;
				journal_button.Sensitive = connected;
				info_button.Sensitive = connected;
			}

			string personal = FMConnection.MakeUserRadio (Driver.connection.Username, "/personal");
			string loved = FMConnection.MakeUserRadio (Driver.connection.Username, "/loved");

			TreeIter iter;
			bool ret = stations.GetIterFirst (out iter);
			while (ret) {
				string p = (string) stations.GetValue (iter, (int) Column.Path);
				if ( p == personal || p == loved ) {
					stations.SetValue (iter, (int) Column.Sensitive, connected & Driver.connection.Subscriber);
				}
				ret = stations.IterNext (ref iter);
			}

			if (InitialStation != null) {
				Driver.connection.ChangeStation (InitialStation);

				// Don't have an initial station anymore...
				InitialStation = null;
			}
		}

		private void OnStationChanged (string station_id) 
		{
			if (toggle_play_button.Active == false) {
				internal_toggle = true;
				toggle_play_button.Active = true;
			}

			Driver.player.Location = Driver.connection.StreamUrl + "=" + Driver.connection.Session;
			//Console.WriteLine (Driver.connection.StreamUrl + "=" + Driver.connection.Session);
			Driver.player.Play ();
		}

		private void OnNewSong () 
		{
			
			Driver.connection.GetMetadata ();
		}

		private void OnCoverLoaded (Gdk.Pixbuf cover_pb) 
		{
			cover_image.ChangePixbuf (cover_pb);
			trayicon.UpdateCover (cover_pb);
			if(i_window != null)
				i_window.UpdateCover (cover_pb);
		}

		private void OnMetadataCompleted (Song song) 
		{
			string station_id = Driver.connection.StationLocation;
			current_song = song;

			love_button.Sensitive = true;
			hate_button.Sensitive = true;
			tag_button.Sensitive = true;
			journal_button.Sensitive = true;
			info_button.Sensitive = true;

			if (station_id != null) {
				if (known_stations.Contains (station_id) == false) {
					string name;
					
					if (station_id.StartsWith ("lastfm://play")) {
						// This is a 30 second preview
						// but the station name isn't very
						// descriptive..Make it better.
						
						name = song.Artist + " (" + song.Station + ")";
					} else {
						name = song.Station;
					}
					
					add_custom_station (null, name,
							    station_id, true);
					select_station (station_id);
				} else {
					// Irritating code to deal with 
					// the payola adverts on last.fm now.
					set_station_title (station_id, song.Station);
				}
			}

			trayicon.FillPopup (song);
			
			song.ImageLoaded += new Song.ImageLoadedHandler (OnCoverLoaded);
			if (song.Track != null) {
				song_label.Markup = "<span weight=\"bold\"><a href=\"" + song.TrackUrl + "\">" + StringUtils.EscapeForPango (song.Track) + "</a></span>";
			} else {
				song_label.Markup = "<span weight=\"bold\"> </span>";
			}
			
			if (song.Album != null && song.Artist != null) {
				artist_label.Markup = String.Format(Catalog.GetString("<span size=\"smaller\">From <a href=\"{0}\">{1}</a> by <a href=\"{2}\">{3}</a></span>"), song.AlbumUrl, StringUtils.EscapeForPango (song.Album), song.ArtistUrl, StringUtils.EscapeForPango (song.Artist));
			} else if (song.Album == null) {
				artist_label.Markup = String.Format(Catalog.GetString("<span size=\"smaller\">By <a href=\"{0}\">{1}</a></span>"), song.ArtistUrl, StringUtils.EscapeForPango (song.Artist));
			} else if (song.Artist == null) {
				artist_label.Markup = String.Format(Catalog.GetString("<span size=\"smaller\">From <a href=\"{0}\">{1}</a></span>"), song.AlbumUrl, StringUtils.EscapeForPango (song.Album));
			} else {
				artist_label.Markup = "<span size=\"smaller\"> </span>";
			}

			song.RequestImage (Driver.CoverSize, Driver.CoverSize);
			
			this.Title = String.Format(Catalog.GetString("{0} by {1}"), song.Track, song.Artist);

			love_button.Sensitive = true;
			hate_button.Sensitive = true;

			if (i_window != null) {
				i_window.SetSong (song);
			}
		}

		private void show_error_message (string message)
		{
			MessageDialog md;
			
			md = new MessageDialog (this, 
						DialogFlags.DestroyWithParent,
						MessageType.Error, 
						ButtonsType.Close, message);
			
			md.Run ();
			md.Destroy();
		}

		private void OnError (int errno)
		{
			switch (errno) {
			case 1:
				show_error_message (Catalog.GetString("There is not enough content to play this station."));
				break;

			case 2:
				show_error_message (Catalog.GetString("This group does not have enough members for radio."));
				break;

			case 3:
				show_error_message (Catalog.GetString("This artist does not have enough fans for radio."));
				break;

			case 4:
				show_error_message (Catalog.GetString("This item is not available for streaming."));
				break;
				
			case 5:
				show_error_message (Catalog.GetString("This feature is only available to subscribers."));
				break;

			case 6:
				show_error_message (Catalog.GetString("There are not enough neighbours for this radio."));
				break;

			case 7:
				show_error_message (Catalog.GetString("This stream has stopped."));
				break;
				
			case 8:
				show_error_message (Catalog.GetString("There is no such a group."));
				break;
			
			case 9:
				show_error_message (Catalog.GetString("There is no such an artist."));
				break;

			default:
				break;
			}

			// All these errors require a new station to be selected
			FindStation dialog = new FindStation (this);
			dialog.Visible = true;
		}

		static void OnUrlActivated (object o, UrlActivatedArgs args)
		{
			Driver.OpenUrl (args.Url);
		}

		private void OnOperationStarted ()
		{
			Gdk.Cursor busy = new Gdk.Cursor (Gdk.CursorType.Watch);
			Gdk.Window w = this.GdkWindow;

			w.Cursor = busy;
		}

		private void OnOperationFinished ()
		{
			Gdk.Window w = this.GdkWindow;

			w.Cursor = null;
		}

                public Song CurrentSong
                { 
			get { return current_song; }
                }

		private void OnSpecialKeysPressed(object o, SpecialKey key) {
			switch (key) {
				case SpecialKey.AudioPlay:
					if(Driver.player.Playing == true) {
						// We are playing so stop playing
						Driver.player.Stop();
						toggle_play_button.Active = false;
						artist_label.Markup = "";
						song_label.Markup = "";
						this.Title = "Last Exit";

						trayicon.CanShowPopup = false;
						// FIXME: Need a blank cover.
						//Gdk.Pixbuf cover = new Gdk.Pixbuf (null, "unknown-cover.png", 66, 66);
						//cover_image.ChangePixbuf (cover);
						cover_image.ChangePixbuf(null);
					} else {
						// We were paused so play
						TreeIter iter;

						if (station_combo.GetActiveIter (out iter)) {
							string path = (string) station_combo.Model.GetValue (iter, (int) Column.Path);
							if (path != null) {
								Driver.connection.ChangeStation (path);
							} else {
								FindStation dialog = new FindStation (this);
								dialog.Visible = true;
							}				
						}
					}
					break;
				case SpecialKey.AudioNext:
					Driver.connection.Skip();
					break;
			}
		}

		private void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs args) {
			switch(args.Event.Key) {
			case Gdk.Key.n:
			case Gdk.Key.N:
				next_button.Activate();
				break;
			case Gdk.Key.Escape:
				this.Visible = !this.Visible;
				break;
			case Gdk.Key.h:
			case Gdk.Key.H:
				hate_button.Activate();
				break;
			case Gdk.Key.l:
			case Gdk.Key.L:
				love_button.Activate();
				break;
			case Gdk.Key.p:
			case Gdk.Key.P:
				toggle_play_button.Activate();
				break;
			case Gdk.Key.plus:
				volume_button.Volume += 5;
				break;
			case Gdk.Key.minus:
				volume_button.Volume -= 5;
				break;
			case Gdk.Key.Key_0:
				volume_button.ToggleMute ();
				break;
			default:
				break;
			}
		}
      }
}
