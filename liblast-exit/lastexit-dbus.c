/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
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

#include <lastexit-dbus.h>
#include <dbus/dbus.h>
#include <dbus/dbus-glib.h>
#include <lastexit-dbus-glue.h>

StationChangeHandler managed_callback; 


/* dbus method for "chat_station" */

static gboolean
lastexit_change_station (GObject     *obj, 
			 const gchar *station, 
			 GError     **error)
{
	managed_callback (station);
	return TRUE;
}

gboolean
init_dbus (StationChangeHandler handler)
{
	GError *err = NULL;
	guint request_name_result;
	
	/* setting managed callback */
	managed_callback = handler;
	
	dbus_g_object_type_install_info (G_TYPE_OBJECT, &dbus_glib_lastexit_object_info);
	
	if (conn == NULL) { 
		conn = dbus_g_bus_get (DBUS_BUS_SESSION, &err);
	}

	if (!conn) {
		g_printerr ("Error : %s\n", err->message);
		g_error_free (err);
		return FALSE;
	}
	
	lastexit = g_object_new (G_TYPE_OBJECT, NULL);
	dbus_g_connection_register_g_object (conn, DBUS_OBJECT, G_OBJECT (lastexit));
	return TRUE;
}

guint
check_lastexit (void)
{
	GError *err = NULL;
	DBusGProxy *proxy;
	guint request_name_result;
	
	if (!conn) { 
		conn = dbus_g_bus_get (DBUS_BUS_SESSION, &err);
	}
	
	if (!conn) {
		g_printerr ("Error : %s\n", err->message);
		g_error_free (err);
		return 0;
	}

	proxy = dbus_g_proxy_new_for_name (conn, "org.freedesktop.DBus",
					   "/org/freedesktop/DBus",
					   "org.freedesktop.DBus");
	
	if (!dbus_g_proxy_call (proxy, "RequestName", &err,
				G_TYPE_STRING, DBUS_SERVICE,
				G_TYPE_UINT, 0,
				G_TYPE_INVALID,
				G_TYPE_UINT, &request_name_result,
				G_TYPE_INVALID)) {
		g_printerr ("Error: %s\n", err->message);
		g_error_free (err);
		return 0;
	}
	
	return request_name_result;
	
}

/* dbus api for "change_station" message */
gboolean
dbus_change_station (const char *station) { 
	GError *err = NULL;
	DBusGProxy *proxy;
	
	if (!conn) { 
		conn = dbus_g_bus_get (DBUS_BUS_SESSION, &err);
	}

	if (!conn) {
		g_printerr ("Error : %s\n", err->message);
		g_error_free (err);
		return 0;
	}

	proxy = dbus_g_proxy_new_for_name (conn, DBUS_SERVICE,
					   DBUS_OBJECT,
					   DBUS_INTERFACE);
	
	if (!dbus_g_proxy_call (proxy, "change_station", &err,
				G_TYPE_STRING, station,
				G_TYPE_INVALID,
				G_TYPE_INVALID)) {
		g_printerr ("Error: %s\n", err->message);
		g_error_free (err);
		return FALSE;
	}
}  
