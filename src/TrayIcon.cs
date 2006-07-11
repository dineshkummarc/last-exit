/*
 *  Authors: Baris Cicek <baris@teamforce.name.tr>
 *
 *  Copyright 2006 Baris Cicek
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

using Gtk;
using System;

namespace LastExit
{
	public class TrayIcon : Gtk.EventBox {
		private NotificationArea trayicon;
		private EventBox event_box;
		private Gtk.Window player_window;
		private Image trayicon_image;
		private TrackInfoPopup popup;
		private bool can_show_popup = false;
		private bool cursor_over_trayicon = false;
		
                private void OnNotificationAreaIconClick (object o, ButtonPressEventArgs args) {
			switch (args.Event.Button) {
                        case 1:
                                player_window.Visible = !player_window.Visible;
                                break;
                        case 3:
                                break;
			}
                }

                private void PositionWidget (Widget widget, out int x, out int y, int yPadding) {
			int button_y, panel_width, panel_height;
			Requisition requisition = widget.SizeRequest ();
                        
			event_box.GdkWindow.GetOrigin (out x, out button_y);
			(event_box.Toplevel as Window).GetSize (out panel_width, out panel_height);
                        
			y = (button_y + panel_height + requisition.Height >= event_box.Screen.Height) 
                                ? button_y - requisition.Height - yPadding
                                : button_y + panel_height + yPadding;
                }
                
		private void PositionPopup () {
			int x, y;

			Gtk.Requisition event_box_req = event_box.SizeRequest ();
			Gtk.Requisition popup_req = popup.SizeRequest ();
			PositionWidget (popup, out x, out y, 5);

			x = x - (popup_req.Width / 2) + (event_box_req.Width / 2);     
			if (x + popup_req.Width >= event_box.Screen.Width) { 
                                x = event_box.Screen.Width - popup_req.Width - 5;
			}
                        
			popup.Move (x, y);
                }
                
                private void OnLeaveNotifyEvent (object o, LeaveNotifyEventArgs args) {
			cursor_over_trayicon = false;
			HidePopup ();
                }
                
                private void OnEnterNotifyEvent (object o, EnterNotifyEventArgs args) {
			cursor_over_trayicon = true;
			if (can_show_popup) {
                                // only show the popup when the cursor is still over the
                                // tray icon after 500ms
                                GLib.Timeout.Add (500, delegate {
                                        if ((cursor_over_trayicon) && (can_show_popup)) {
                                                ShowPopup ();
                                        }
                                        return false;
                                });
			}
                }
                
		private void HidePopup () {
                        popup.Hide ();
                }
                
                private void ShowPopup () {
			PositionPopup ();
			popup.Show ();
                }
                
		public void FillPopup (Song song) {
			if (song.Track != null) {
				popup.TrackTitle = "<span weight=\"bold\">" + StringUtils.EscapeForPango (song.Track) + "</span>";
			} else {
				popup.TrackTitle = "";
			}
                        
			if (song.Album != null && song.Artist != null) {
				popup.Artist = "<span size=\"smaller\">By <span weight=\"bold\">" + StringUtils.EscapeForPango (song.Artist) + "</span></span>";
				popup.Album = "<span size=\"smaller\">From <span weight=\"bold\">" + StringUtils.EscapeForPango (song.Album) + "</span></span>";
			} else if (song.Album == null) {
				popup.Artist = "<span size=\"smaller\">By <span weight=\"bold\">" + StringUtils.EscapeForPango (song.Artist) + "</span></span>";
				popup.Album = "";
			} else if (song.Artist == null) {
				popup.Album = "<span size=\"smaller\">From <span weight=\"bold\">" + StringUtils.EscapeForPango (song.Album) + "</span></span>";
				popup.Artist = "";
			} else {
				popup.Album = "";
				popup.Artist = "";
			}
			
			SetShowPopup (true);
                }
                
                public void SetShowPopup (bool value) { 
                        can_show_popup = value;
                }
                
		public TrayIcon (Gtk.Window window) {
			player_window = window;
			
			popup = new TrackInfoPopup ();
			trayicon = new NotificationArea ("last-exit");
			
			event_box = new EventBox ();
			trayicon_image = new Image ();
			trayicon_image.FromPixbuf = new Gdk.Pixbuf (null, "last-exit-16.png");
			event_box.Add (trayicon_image);
			trayicon_image.Visible = true;
			trayicon.Add (event_box);
                        trayicon.ShowAll ();
                        
                        // Setting callback procs 
                        event_box.ButtonPressEvent += OnNotificationAreaIconClick;
			event_box.EnterNotifyEvent += OnEnterNotifyEvent;
			event_box.LeaveNotifyEvent += OnLeaveNotifyEvent;
		}
	}
	
        public class TrackInfoPopup : Gtk.Window {
                private string artist;
                private string album;
                private string title;
                
                private Label artist_album_label;
                private Label title_label;        
                private VBox box;
                
                public TrackInfoPopup () : base (Gtk.WindowType.Popup) {
                        BorderWidth = 8;
                        AppPaintable = true;
                        Resizable = false;
                        
                        box = new VBox ();
                        box.Spacing = 2;
                        
                        artist_album_label = new Label (album + artist);
                        artist_album_label.Show ();
                        artist_album_label.Xalign = 0.0f;
                        artist_album_label.Yalign = 0.5f;
                        artist_album_label.Selectable = false;
                        
                        title_label = new Label (title);
                        title_label.Show ();            
                        title_label.Xalign = 0.0f;
                        title_label.Yalign = 0.5f;
                        title_label.Selectable = false;
                        
                        box.PackStart (title_label, false, false, 0);
                        box.PackStart (artist_album_label, false, false, 0);
                        
                        
                        Add (box);
                        box.ShowAll ();
                        //Name = "gtk-tooltips";
                }
                
                protected override bool OnExposeEvent (Gdk.EventExpose evnt) {
                        GdkWindow.DrawRectangle (Style.ForegroundGC(StateType.Normal), true, Allocation);
                        GdkWindow.DrawRectangle (Style.BackgroundGC(StateType.Normal), true, 
                                                 Allocation.X + 1, 
                                                 Allocation.Y + 1, 
                                                 Allocation.Width - 2, 
                                                 Allocation.Height - 2);
                        base.OnExposeEvent (evnt);
                        return true;
                }
                
                private void UpdateSize () {
                        QueueResize ();
                }
                
                
                public string Artist {
                        set {
                                artist = value;
                                artist_album_label.Markup = this.Album + " " + value;
                                UpdateSize ();
                        }
                        get {
                                return artist;
                        }
                }
                
                public string Album {
                        set {
                                album = value;
                                artist_album_label.Markup = value + " " + this.Artist;
                                UpdateSize ();
                        }
                        get {
                                return album;
                        }
                }
                
                public string TrackTitle {
                        set {
                                title_label.Markup = value;
                                UpdateSize ();
                        }
                }
        }        
}
