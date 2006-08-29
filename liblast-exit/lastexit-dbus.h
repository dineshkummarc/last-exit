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

#define DBUS_API_SUBJECT_TO_CHANGE 1
#include <dbus/dbus.h>
#include <dbus/dbus-glib.h>

G_BEGIN_DECLS

#define DBUS_SERVICE "org.gnome.LastExit"
#define DBUS_OBJECT "/org/gnome/LastExit"
#define DBUS_INTERFACE "org.gnome.LastExit.interface"

typedef void (* DBusMessageHandler ) (const gchar * message , const gchar * content);

GObject *lastexit;
DBusGConnection *conn;
DBusGProxy *proxy;

static gboolean lastexit_change_station (GObject *obj, const gchar *station, GError **error);
static gboolean lastexit_focus_instance (GObject *obj, GError **error);


gboolean init_dbus(DBusMessageHandler handler);
guint check_lastexit (void);

G_END_DECLS

