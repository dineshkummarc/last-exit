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
using System.Runtime.InteropServices;

namespace LastExit
{
	public sealed class DBus {
		public enum DBusState {
			Error,
			NotRunning,
			AlreadyRunning
		};

		public delegate void DBusMessageHandler (string message, string args);

		[DllImport ("dbus-glib-1")]
		public static extern void dbus_g_thread_init ();
		
		[DllImport ("liblastexit")]
		public static extern int check_lastexit ();

		public static DBusState CheckInstance () {
			int request_name_result;

			request_name_result = check_lastexit ();
			return (DBusState) request_name_result;
		}

		[DllImport ("liblastexit")]
		public static extern int init_dbus (DBusMessageHandler handler);

		public static void Init (DBusMessageHandler handler) {
			init_dbus (handler);
		}

		[DllImport ("liblastexit")]
		public static extern bool dbus_change_station (string station);

		public static bool ChangeStation (string station) {
			return dbus_change_station (station);
		}
		
		[DllImport ("liblastexit")]
		public static extern bool dbus_focus_instance ();
		
		public static bool FocusInstance () { 
			return dbus_focus_instance ();
		}
	}
}
