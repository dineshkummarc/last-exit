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

		public static Client config;

		public bool FirstRun {
			get { 
				// Check it exists, then a default if not
				object o = config.Get (GConfFirstRun);
				if (o == null) {
					return true;
				} else {
					return (bool) o;
				}
			}
			set { config.Set (GConfFirstRun, (object) value); }
		}

		public string Username {
			get { return (string) config.Get (GConfUsername); }
			set { config.Set (GConfUsername, (object) value); }
		}

		public string Password {
			get { return (string) config.Get (GConfPassword); }
			set { config.Set (GConfPassword, (object) value); }
		}

		public int Volume {
			get { return (int) config.Get (GConfVolume); }
			set { config.Set (GConfVolume, (object) value); }
		}

		public Config () {
			config = new GConf.Client ();
		}
	}
}
