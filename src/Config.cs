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
using GConf;

namespace LastExit {
	public class Config {
		private const string GConfFirstRun = "/apps/lastexit/first_run";
		private const string GConfUsername = "/apps/lastexit/username";
		private const string GConfPassword = "/apps/lastexit/password";
		private const string GConfVolume = "/apps/lastexit/volume";
		private const string GConfRecommendationLevel = "/apps/lastexit/recommendation_level";

		private const string GConfShowNotifications = "/apps/lastexit/show_notifications";

		public static Client config;

		public bool FirstRun {
			get { 
				object o;
				// Check it exists, then a default if not
				try {
					o = config.Get (GConfFirstRun);
				} catch (GConf.NoSuchKeyException) {
					config.Set (GConfFirstRun, 
						    (object) true);
					return true;
				}

				if (o == null) {
					return true;
				} else {
					return (bool) o;
				}
			}
			set { config.Set (GConfFirstRun, (object) value); }
		}

		public string Username {
			get { 
				object o;
				try {
					o = config.Get (GConfUsername);
				} catch (GConf.NoSuchKeyException) {
					config.Set (GConfUsername, 
						    (object) "username");
					return "username";
				}

				return (string) o; 
			}
			set { config.Set (GConfUsername, (object) value); }
		}

		public string Password {
			get { 
				object o;
				try {
					o = config.Get (GConfPassword);
				} catch (GConf.NoSuchKeyException) {
					config.Set (GConfPassword, 
						    (object) "password");
					return "password";
				}

				return (string) o;
			}
			set { config.Set (GConfPassword, (object) value); }
		}

		public int Volume {
			get { 
				object o;
				try {
					o = config.Get (GConfVolume); 
				} catch (GConf.NoSuchKeyException) {
					config.Set (GConfVolume, (object) 50);
					return 50;
				}

				return (int) o;
			}
			set { config.Set (GConfVolume, (object) value); }
		}

		public int RecommendationLevel {
			get { 
				object o;
				try {
					o = config.Get (GConfRecommendationLevel); 
				} catch (GConf.NoSuchKeyException) {
					config.Set (GConfRecommendationLevel, (object) 100);
					return 100;
				}

				return (int) o;
			}
			set { config.Set (GConfRecommendationLevel, (object) value); }
		}

		public bool ShowNotifications {
			get { 
				object o;
				// Check it exists, then a default if not
				try {
					o = config.Get (GConfShowNotifications);
				} catch (GConf.NoSuchKeyException) {
					config.Set (GConfShowNotifications, 
						    (object) true);
					return true;
				}

				if (o == null) {
					return true;
				} else {
					return (bool) o;
				}
			}
            set { config.Set (GConfShowNotifications, (object) value); }
		}

        public void GConfAddNotify (string dir, NotifyEventHandler handler)
        {
            config.AddNotify (dir, handler);
        }

		public Config () {
			config = new GConf.Client ();
		}
	}
}
