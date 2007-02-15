/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/* vim:tabstop=8:noexpandtab:shiftwidth=8:
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using NDesk.DBus;

using Gtk;
using Mono.Unix;

namespace LastExit
{
	public sealed class Driver
	{
		// For the SEGV trap hack (see below)
		[System.Runtime.InteropServices.DllImport("libc")]
			private static extern int sigaction(Mono.Unix.Native.Signum sig, IntPtr act, IntPtr oact);

		// The size of the cover images.
		public static int CoverSize = 66;

		private static Actions actions;
		private static PlayerWindow player_window;

		public static FMConnection connection;
		public static Player player;

		private static DBusRemote dbus_remote;
		private static DBusPlayer dbus_player;

		private static string config_directory;
		public static string ConfigDirectory {
			get { return config_directory; }
		}

		public static Config config;

		public static void Main (string[] args) {

			// Work around DBus locking issues
			DBusPlayer.dbus_g_thread_init ();

			BusG.Init();

			// Search for existing DBus server
			IDBusPlayer dbus_core = DetectInstanceAndDbus();
			HandleDbusCommands (dbus_core, args);

			// If we are the only instance, start a new DBus server
			Console.WriteLine("Starting new Last Exit server");
			try {
				dbus_remote = new DBusRemote();
				dbus_player = new DBusPlayer();
				dbus_remote.RegisterObject(dbus_player, "Player");
			} catch {}

			string username;
			string password;

			Driver.SetProcessName ("last-exit");
			Catalog.Init("last-exit", Defines.LOCALE_DIR);
			Application.Init("last-exit", ref args);

			StockIcons.Initialize ();

			SetDefaultWindowIcon ();

			config = new Config ();

			if (config.FirstRun) {
				FirstRunDialog frd = new FirstRunDialog ();

				int response = frd.Run ();

				frd.Visible = false;

				switch (response) {
					case (int) ResponseType.Reject:
						Gtk.Application.Quit();
						Environment.Exit (0);
						break;

					case (int) ResponseType.Ok:
						config.Username = frd.Username;
						config.Password = frd.Password;
						break;

					default:
						break;
				}

				frd.Destroy ();
				config.FirstRun = false;
			}

			Driver.SetUpConfigDirectory ();

			//TrayIcon.ShowNotifications = config.ShowNotifications;

			username = config.Username;
			password = config.Password;

			// We must get a reference to the JIT's SEGV handler because
			// GStreamer will set its own and not restore the previous, which
			// will cause what should be NullReferenceExceptions to be unhandled
			// segfaults for the duration of the instance, as the JIT is powerless!
			// FIXME: http://bugzilla.gnome.org/show_bug.cgi?id=391777
			IntPtr mono_jit_segv_handler = System.Runtime.InteropServices.Marshal.AllocHGlobal(512);
			sigaction(Mono.Unix.Native.Signum.SIGSEGV, IntPtr.Zero, mono_jit_segv_handler);

			connection = new FMConnection (username, password);
			connection.Handshake ();

			player = new Player ();

			actions = new Actions ();
			player_window = new PlayerWindow ();

			if (args.Length > 0) {
				player_window.InitialStation = args[0];
			}

			player_window.Run ();

			Application.Run ();

			// Reset the SEGV handle to that of the JIT again (SIGH!)
			sigaction(Mono.Unix.Native.Signum.SIGSEGV, mono_jit_segv_handler, IntPtr.Zero);
			System.Runtime.InteropServices.Marshal.FreeHGlobal(mono_jit_segv_handler);
		}

		private static void HandleDbusCommands(IDBusPlayer remote_player, string[] args)
		{
			if(remote_player == null) {
				return;
			}

			bool present = true;

			if (args.Length > 0) {
				// FIXME: Do some better argument checking
				remote_player.ChangeStation (args[0]);
				Present(remote_player);
				present = false;
			}

			if(present) {
				try {
					Present(remote_player);
				} catch (Exception e) {
					Console.WriteLine(e);
					return;
				}
			}

			Gdk.Global.NotifyStartupComplete();
			System.Environment.Exit(0);
		}

		private static void Present(IDBusPlayer remote_player)
		{
			remote_player.PresentWindow();
		}

		public static void Exit () {
			Environment.Exit (0);
		}

		// FIXME: Needs to handle no gnome-open.
		public static void OpenUrl (string url)
		{
			string [] argv = new string [] { "gnome-open", url };
			InternalProcess.Spawn (argv);
		}

		private static void SetDefaultWindowIcon ()
		{
			Gdk.Pixbuf [] default_icon_list = { new Gdk.Pixbuf (null, "last-exit-16.png") };
			Gtk.Window.DefaultIconList = default_icon_list;
		}

		[DllImport ("libc")]
			private static extern int prctl (int option,  
					byte[] arg2,
					ulong arg3,
					ulong arg4,
					ulong arg5);

		private static void SetProcessName (string name)
		{
			if (prctl (15, Encoding.ASCII.GetBytes (name + "\0"), 0, 0, 0) != 0) {
				throw new ApplicationException ("Error setting process name: " + Mono.Unix.Native.Stdlib.GetLastError ());
			}
		}

		private static void SetUpConfigDirectory ()
		{
			config_directory = Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "last-exit");
			DirectoryInfo dinfo = new DirectoryInfo (config_directory);
			if (dinfo.Exists) {
				return;
			}

			dinfo.Create ();
		}

		public static Actions Actions {
			get { return actions; }
		}

		public static PlayerWindow PlayerWindow {
			get { return player_window; }
		}

		private static IDBusPlayer DetectInstanceAndDbus()
		{
			try {
				return DBusPlayer.FindInstance();
			} catch (Exception e) {
				Console.Error.WriteLine(e);
				return null;
			}
		}
	}
}
