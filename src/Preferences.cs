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

namespace LastExit
{    
	public class PreferencesDialog : Dialog
	{ 
	
		[Glade.Widget] private Window preferences;

		[Glade.Widget] private Entry username_entry;
		[Glade.Widget] private Entry password_entry;

		[Glade.Widget] private Button signup_button;
		[Glade.Widget] private Button join_button;
		[Glade.Widget] private Button close_button;

		[Glade.Widget] private Scale recommendation_scale;
		[Glade.Widget] private CheckButton notification_checkbutton;

		public string Username {
			get { return username_entry.Text; }
		}

		public string Password {
			get { return password_entry.Text; }
		}

		public PreferencesDialog () {
			Glade.XML glade_xml = new Glade.XML (null, "Preferences.glade", "preferences", null);
			glade_xml.Autoconnect (this);
			SetupUI ();
		}

		private void SetupUI ()
		{
			recommendation_scale.ValueChanged += new EventHandler (OnRecommendationScaleChanged);
			notification_checkbutton.Toggled += new EventHandler (OnNotificationCheckboxToggled);
			username_entry.Changed += new EventHandler (OnUsernameEntryChanged);
			password_entry.Changed += new EventHandler (OnPasswordEntryChanged);
			close_button.Clicked += new EventHandler (OnCloseButtonClicked);
			signup_button.Clicked += new EventHandler (OnSignupButtonClicked);
			join_button.Clicked += new EventHandler (OnJoinButtonClicked);
		
			notification_checkbutton.Active = Driver.config.ShowNotifications;
			recommendation_scale.Value = Driver.config.RecommendationLevel;
			username_entry.Text = Driver.config.Username;
			password_entry.InvisibleChar = '•';
			password_entry.Text = Driver.config.Password;
		}

		private void OnRecommendationScaleChanged (object o, EventArgs args)
		{
			Driver.config.RecommendationLevel = ( int)recommendation_scale.Value;
		}
		
		private void OnNotificationCheckboxToggled (object o, EventArgs args)
		{
			Driver.config.ShowNotifications = notification_checkbutton.Active;
		}
	
		private void OnUsernameEntryChanged (object o, EventArgs args)
		{
			Driver.config.Username = username_entry.Text;
		}
		
		private void OnPasswordEntryChanged (object o, EventArgs args)
		{
			Driver.config.Password = password_entry.Text;
		}
		
		private void OnCloseButtonClicked (object o, EventArgs args) 
		{
			preferences.Destroy ();
		}
		
		private void OnSignupButtonClicked (object o, EventArgs args) 
		{
			Gnome.Url.Show("https://www.last.fm/join/");
		}

		private void OnJoinButtonClicked (object o, EventArgs args) 
		{
			Gnome.Url.Show("http://www.last.fm/group/Last%2BExit");
		}
	}
}
