/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *  Authors: Brandon Hale <brandon@ubuntu.com>
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
using NDesk.DBus;

namespace LastExit
{

	// FIXME: Move this to liblastexit, see Banshee
	[Interface ("org.gnome.LastExit.Core")]
	public interface IDBusPlayer
	{
		void PresentWindow();
		void ChangeStation (string station);
	}

	public class DBusPlayer : IDBusPlayer
	{
		const string ServicePath = "org.gnome.LastExit";
		static ObjectPath CorePath = new ObjectPath ("/org/gnome/LastExit/Core");

		public static IDBusPlayer FindInstance()
		{
			BusG.Init ();

			if(!Bus.Session.NameHasOwner(DBusRemote.BusName)) {
				return null;
			}

			return Bus.Session.GetObject<IDBusPlayer>(
				DBusRemote.BusName, new ObjectPath(DBusRemote.ObjectRoot + "/Player"));
		}

		public void PresentWindow()
		{
			Driver.PlayerWindow.SetWindowVisible (true, 0);
		}
		
		public void ChangeStation (string station)
		{
			Driver.player.Stop ();
			Driver.connection.ChangeStation	(station);
			Driver.player.Play ();
			Driver.connection.DoOperationFinished ();
			Driver.PlayerWindow.Present ();
		}
	}
}
