/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *
 *  Copyright 2006 Iain Holmes
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
using GConf;
using Gnome.Keyring;
using Gtk;

namespace LastExit {
	public class Config {
		private const string GConfFirstRun = "/apps/lastexit/first_run";
		private const string GConfUsername = "/apps/lastexit/username";
		private const string GConfPassword = "/apps/lastexit/password";
		private const string GConfVolume = "/apps/lastexit/volume";
		private const string GConfRecommendationLevel = "/apps/lastexit/recommendation_level";

		private const string GConfShowNotifications = "/apps/lastexit/show_notifications";
		private const string GConfShowProgress = "/apps/lastexit/show_progress";
		private const string GConfShowDuration = "/apps/lastexit/show_duration";

		public static Client config;
		
		private string ring;

		private object try_with_default (string key, object fallback)
		{
			object o;
			// Check it exists, then a default if not
			try {
				o = config.Get (key);
			} catch (GConf.NoSuchKeyException) {
				config.Set (key, fallback);
				return fallback;
			}
			if (o == null) {
				return fallback;
			} else {
				return o;
			}
		}

		public bool FirstRun {
			get {
				return (bool) try_with_default (GConfFirstRun,
								(object) true);
			}
			set { config.Set (GConfFirstRun, (object) value); }
		}

		public string Username {
			get {
				return (string) try_with_default (GConfUsername,
								  (object) "username");
			}
			set { config.Set (GConfUsername, (object) value); }
		}

		public string Password {
			get {
				ItemData item = Ring.FindNetworkPassword(null, null, "last.fm", "last-exit", null, null, 0)[0];
				return item.Secret;
			}
			set {
				Hashtable attributes = new Hashtable();
				attributes ["server"] = "last.fm";
				attributes ["object"] = "last-exit";
				Ring.CreateItem(ring, ItemType.NetworkPassword, "last-exit", attributes, value, true);
			}
		}
		
		private string OldPassword {
			get {
				return (string) try_with_default (GConfPassword,
								  (object) String.Empty);
			}
			set { config.Set (GConfPassword, (object) value); }
		}

		public int Volume {
			get {
				return (int) try_with_default (GConfVolume,
								(object) 50);
			}
			set { config.Set (GConfVolume, (object) value); }
		}

		public int RecommendationLevel {
			get {
				return (int) try_with_default (GConfRecommendationLevel,
								(object) 100);
			}
			set { config.Set (GConfRecommendationLevel, (object) value); }
		}

		public bool ShowNotifications {
			get {
				return (bool) try_with_default (GConfShowNotifications,
								(object) true);
			}
            set { config.Set (GConfShowNotifications, (object) value); }
		}

		public bool ShowProgress {
			get {
				return (bool) try_with_default (GConfShowProgress,
								(object) false);
			}
            		set { config.Set (GConfShowProgress, (object) value); }
		}

		public bool ShowDuration {
			get {
				return (bool) try_with_default (GConfShowDuration,
								(object) false);
			}
            		set { config.Set (GConfShowDuration, (object) value); }
		}

		public void GConfAddNotify (string dir, NotifyEventHandler handler)
		{
			config.AddNotify (dir, handler);
		}

		public Config () {
			config = new GConf.Client ();
			if (Ring.Available) {
				ring = Ring.GetDefaultKeyring();
			} else {
				MessageDialog md = new MessageDialog (null, DialogFlags.Modal, MessageType.Error,
				                                         ButtonsType.Close,false,
				                                         "You need to have GNOME Keyring installed.");
				md.Run();
				md.Destroy();
				Driver.Exit();
			}
			/* Check if we have a password in stored in gconf - done once per app startup
			 * so it shouldn't be much of a performance impact...
			 * We should remove it after the next release together with the gconf key. */
			if (OldPassword != String.Empty) {
				Password = OldPassword;
				OldPassword = String.Empty;
			}
		}
	}
}
