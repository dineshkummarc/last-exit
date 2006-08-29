/*
 * Copyright (C) 2005 Tamara Roberson <foxxygirltamara@gmail.com>
 * Copyright (C) 2003, 2004, 2005 Jorn Baayen <jbaayen@gnome.org>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as
 * published by the Free Software Foundation; either version 2 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public
 * License along with this program; if not, write to the
 * Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
 */

using System;
using System.IO;
using System.Collections;

using Mono.Unix;

using Gtk;

namespace LastExit
{
	public class Actions : ActionGroup
	{

		private static readonly string string_toggle_visible =
			Catalog.GetString ("Show _Window");

		private static readonly string string_toggle_play =
			Catalog.GetString ("_Play");

		private static readonly string string_next =
			Catalog.GetString ("_Next");

		private static readonly string string_about =
			Catalog.GetString ("_About");

		private static readonly string string_preferences =
			Catalog.GetString ("_Preferences");

		private static readonly string string_love =
			Catalog.GetString ("_Love Song");

		private static readonly string string_hate =
			Catalog.GetString ("_Hate Song");

		// Static
		// Static :: Objects
		// Static :: Objects :: Entries
		private static ActionEntry [] entries = {
                        new ActionEntry ("Quit", Stock.Quit, null,
			        "<control>Q", null, null),
			
                        new ActionEntry ("Next", "stock_media-next", string_next,
			        "N", null, null),
			
			new ActionEntry ("Love", "face-smile", string_love,
				"L", null, null),
			
			new ActionEntry ("Hate", "face-sad", string_hate,
				"H", null, null),

			new ActionEntry ("About", Gtk.Stock.About, string_about,
				null, null, null),
					
			new ActionEntry ("Preferences", Gtk.Stock.Properties, string_preferences,
				null, null, null)
		};

		// Static :: Objects :: Toggle Entries
		private static ToggleActionEntry [] toggle_entries = {
			new ToggleActionEntry ("TogglePlay", "stock_media-play", string_toggle_play,
				   "P", null, null, false),

			new ToggleActionEntry ("ToggleVisible", null, string_toggle_visible,
				"Escape", null, null, true),

		};

		// Static :: Properties :: Entries (get;)
		/// <summary>
		/// 	The defined actions.
		/// </summary>
		/// <returns>
		///	An array of <see cref="ActionEntry" />.
		/// </returns>
		public static ActionEntry [] Entries {
			get { return entries; }
		}
		
		// Static :: Properties :: ToggleEntries (get;)
		/// <summary>
		/// 	The defined toggle actions.
		/// </summary>
		/// <returns>
		///	An array of <see cref="ToggleActionEntry" />
		/// </returns>
		public static ToggleActionEntry [] ToggleEntries {
			get { return toggle_entries; }
		}

		// Objects
		private UIManager ui_manager = new UIManager ();

		// Constructor
		/// <summary>
		///	Create a new <see cref="Actions" /> object.
		/// </summary>
		/// <remarks>
		/// 	This object manages the actions for menu items.
		/// </remarks>
		public Actions () : base ("Actions")
		{
			Add (entries);
			Add (toggle_entries);

			ui_manager.InsertActionGroup (this, 0);
			//ui_manager.AddUiFromResource ("PlaylistWindow.xml");
			
			// Setup Callbacks
			this ["ToggleVisible"].Activated += new EventHandler (OnToggleVisible);
			this ["Quit"		 ].Activated += new EventHandler (OnQuit		 );
			this ["Next"		 ].Activated += new EventHandler (OnNext		 );
			this ["About"		].Activated += new EventHandler (OnAbout		);
			this ["Preferences"		].Activated += new EventHandler (OnPreferences		);
			this ["TogglePlay"   ].Activated += new EventHandler (OnTogglePlay   );
			this ["Love"   ].Activated += new EventHandler (OnLoved   );
			this ["Hate"   ].Activated += new EventHandler (OnHated   );
		}

		// Properties
		// Properties :: UIManager (get;)
		/// <summary>
		/// 	The UIManager.
		/// </summary>
		/// <returns>
		///	A <see cref="UIManager" />.
		/// </returns>
		public UIManager UIManager {
			get { return ui_manager; }
		}
		
		// Properties :: MenuBar (get;)
		//	TODO: Change return type to Gtk.MenuBar?
		/// <summary>
		/// 	Contains the MenuBar.
		/// </summary>
		/// <returns>
		///	A <see cref="Gtk.Widget" />.
		/// </returns>
		public Gtk.Widget MenuBar {
			get { return ui_manager.GetWidget ("/MenuBar"); }
		}
		
		
		// Handlers :: OnToggleWindowVisible
		/// <summary>
		/// 	Handler called when the ToggleVisible action is activated.
		/// </summary>
		/// <remarks>
		///	This calls <see cref="PlayerWindow.ToggleVisible" />.
		/// </remarks>
		/// <param name="o">
		///	The calling object.
		/// </param>
		/// <param name="args">
		///	The <see cref="EventArgs" />.
		/// </param>

		private void OnToggleVisible (object o, EventArgs args)
		{
        	        ToggleAction a = (ToggleAction) o;

		        if (a.Active == Driver.PlayerWindow.Visible)
		                return;

		        Driver.PlayerWindow.SetWindowVisible (!Driver.PlayerWindow.WindowVisible,
		        Gtk.Global.CurrentEventTime);
		}

		// Handlers :: OnQuit
		/// <summary>
		/// 	Handler called when the Quit action is activated.
		/// </summary>
		/// <remarks>
		///	This calls <see cref="PlaylistWindow.Quit" />.
		/// </remarks>
		/// <param name="o">
		///	The calling object.
		/// </param>
		/// <param name="args">
		///	The <see cref="EventArgs" />.
		/// </param>
		private void OnQuit (object o, EventArgs args)
		{
                        Driver.PlayerWindow.Quit ();
                }

		// Handlers :: OnNext
		/// <summary>
		/// 	Handler called when the Next action is activated.
		/// </summary>
		/// <remarks>
		///	This calls <see cref="PlaylistWindow.Next" />.
		/// </remarks>
		/// <param name="o">
		///	The calling object.
		/// </param>
		/// <param name="args">
		///	The <see cref="EventArgs" />.
		/// </param>
		private void OnNext (object o, EventArgs args)
		{
                        Driver.connection.Skip ();
		}

		private void OnLoved (object o, EventArgs args)
		{
                        Driver.PlayerWindow.ActivateLoveButton ();
		}
		
		private void OnHated (object o, EventArgs args)
		{
			Driver.PlayerWindow.ActivateHateButton ();
		}
		
		// Handlers :: OnAbout
		/// <summary>
		/// 	Handler called when the About action is activated.
		/// </summary>
		/// <remarks>
		///	This creates a new <see cref="Muine.About" /> window.
		/// </remarks>
		/// <param name="o">
		///	The calling object.
		/// </param>
		/// <param name="args">
		///	The <see cref="EventArgs" />.
		/// </param>
		private void OnAbout (object o, EventArgs args)
		{
			new LastExit.About (Driver.PlayerWindow);
		}

		private void OnPreferences (object o, EventArgs args)
		{
			PreferencesDialog prefs = new PreferencesDialog ();
		}

		// Handlers :: OnTogglePlay
		/// <summary>
		/// 	Handler called when the TogglePlay action is activated.
		/// </summary>
		/// <remarks>
		///	This sets <see cref="PlaylistWindow.Playing" /> to the
		///	state of the TogglePlay action.
		/// </remarks>
		/// <param name="o">
		///	The calling object.
		/// </param>
		/// <param name="args">
		///	The <see cref="EventArgs" />.
		/// </param>
		private void OnTogglePlay (object o, EventArgs args)
		{
		        ToggleAction a = (ToggleAction) o;

                        if (a.Active == Driver.player.Playing)
			        return;
			
                        Driver.PlayerWindow.TogglePlayButton ();
		}
	}
}
