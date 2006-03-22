/*
 * Copyright (C) 2004 Ross Girshick <ross.girshick@gmail.com>
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

using GLib;
using Gtk;
using System;

namespace LastExit
{
	public class VolumeButton : ToggleButton {
		// Constants
		// 	TODO: GDK_CURRENT_TIME doesn't seem to have an equiv in gtk# yet.
		private const uint CURRENT_TIME = 0;
		
		// Events
		public delegate void VolumeChangedHandler (int vol);
		public event         VolumeChangedHandler VolumeChanged;
		
		// Widgets
		private Image icon;
		private Window popup;
		
		// Variables
		private int volume;
		
		// Variables :: revert_volume
		// 	Volume to restore to if the user rejects the slider changes.
		private int revert_volume;
		
		// Constructor
		public VolumeButton () : base () {
			icon = new Image ();
			icon.Show ();
			Add (icon);
			
			popup = null;
			
			base.ScrollEvent += new ScrollEventHandler (OnScrollEvent);
			base.Toggled     += new EventHandler       (OnToggled    );
			
			base.WidgetFlags |= WidgetFlags.NoWindow;
		}
		
		// Destructor
		~VolumeButton () {
			Dispose ();
		}

		// Properties
		// Properties :: Volume (set; get;)
		public int Volume {
			set {
				volume = value;
				
				string id = "stock_volume-";
				
				if      (volume <= 000/3) id += "0"  ;
				else if (volume <= 100/3) id += "min";
				else if (volume <= 200/3) id += "med";
				else                      id += "max";

				icon.SetFromStock (id, IconSize.LargeToolbar);

				VolumeChanged (Volume);
			}

			get { return volume; }
		}

		// Methods
		// Methods :: Private
		// Methods :: Private :: ShowScale
		private void ShowScale () {
			revert_volume = this.Volume;
			
			// Popup
			popup = new Window (WindowType.Popup);
			popup.Screen = base.Screen;
			popup.ButtonPressEvent += new ButtonPressEventHandler (OnPopupButtonPressEvent);

			// Frame
			Frame frame = new Frame ();
			frame.Shadow = ShadowType.Out;
			frame.Show ();

			popup.Add (frame);

			// Box
			VBox box = new VBox (false, 0);
			box.Show();
			frame.Add (box);

			// +
			Gtk.Label plus_label = new Gtk.Label ("+");
			plus_label.Show ();
			box.PackStart (plus_label, false, true, 0);

			// Adjustment
			Adjustment adj = new Adjustment (volume, 0, 100, 5, 10, 0);		

			// Scale
			VScale scale = new VScale (adj);

			scale.ValueChanged  += new EventHandler         (OnScaleValueChanged );
			scale.KeyPressEvent += new KeyPressEventHandler (OnScaleKeyPressEvent);

			scale.Adjustment.Upper = 100.0;
			scale.Adjustment.Lower =   0.0;
			scale.DrawValue = false;
			scale.Inverted  = true ;
			scale.UpdatePolicy = UpdateType.Continuous;

			scale.Show ();

			box.PackStart (scale, true, true, 0);

			// -
			Gtk.Label minus_label = new Gtk.Label ("-");
			minus_label.Show ();
			box.PackEnd (minus_label, false, true, 0);


			Requisition req = SizeRequest ();

			int x, y;
			GdkWindow.GetOrigin (out x, out y);

			scale.SetSizeRequest (-1, 100);
			popup.SetSizeRequest (req.Width, -1);

			popup.Move (x + Allocation.X, y + Allocation.Y + req.Height);
			popup.Show ();

			popup.GrabFocus ();

			Grab.Add (popup);

			Gdk.GrabStatus grabbed = Gdk.Pointer.Grab (popup.GdkWindow, true, 
				Gdk.EventMask.ButtonPressMask   | 
				Gdk.EventMask.ButtonReleaseMask | 
				Gdk.EventMask.PointerMotionMask, 
				null, null, CURRENT_TIME);

			if (grabbed == Gdk.GrabStatus.Success) {
				grabbed = Gdk.Keyboard.Grab (popup.GdkWindow, true, CURRENT_TIME);

				if (grabbed == Gdk.GrabStatus.Success)
					return;
					
				Grab.Remove (popup);
				popup.Destroy ();
				popup = null;

				return;
			}

			Grab.Remove (popup);
			popup.Destroy ();
			popup = null;
		}

		// Methods :: Private :: HideScale
		private void HideScale () {
			Active = false;

			if (popup == null)
				return;

			Grab.Remove (popup);
			Gdk.Pointer .Ungrab (CURRENT_TIME);
			Gdk.Keyboard.Ungrab (CURRENT_TIME);

			popup.Destroy ();
			popup = null;
		}

		// Handlers
		// Handlers :: OnToggled
		private void OnToggled (object obj, EventArgs args) {
			if (Active) ShowScale ();
			else        HideScale ();
		}

		// Handlers :: OnScrollEvent
		private void OnScrollEvent (object obj, ScrollEventArgs args) {
			int v = this.Volume;
			
			switch (args.Event.Direction) {
			case Gdk.ScrollDirection.Up:
				v += 10;
				break;

			case Gdk.ScrollDirection.Down:
				v -= 10;
				break;

			default:
				break;
			}

			// Ensure volume is between 0 and 100
			v = Math.Min (100, v);
			v = Math.Max (  0, v);

			this.Volume = v;
		}

		// Handlers :: OnScaleValueChanged
		private void OnScaleValueChanged (object obj, EventArgs args) {
			VScale scale = (VScale) obj;

			this.Volume = (int) scale.Value;
		}

		// Handlers :: OnScaleKeyPressEvent
		private void OnScaleKeyPressEvent (object obj, KeyPressEventArgs args) {
			switch (args.Event.Key) {
			case Gdk.Key.Escape:
				HideScale ();
				this.Volume = revert_volume;
				
				break;

			case Gdk.Key.KP_Enter:
			case Gdk.Key.ISO_Enter:
			case Gdk.Key.Key_3270_Enter:
			case Gdk.Key.Return:
			case Gdk.Key.space:
			case Gdk.Key.KP_Space:
				HideScale ();

				break;

			default:
				break;
			}
		}

		// Handlers :: OnPopupButtonPressEvent
		private void OnPopupButtonPressEvent (object obj, ButtonPressEventArgs args) {
			if (popup == null)
				return;

			HideScale ();
		}
	}
}
