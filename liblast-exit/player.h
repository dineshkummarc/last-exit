/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *
 *  Copyright 2005 Iain Holmes
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
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

#ifndef __PLAYER_H__
#define __PLAYER_H__

#include <glib-object.h>

#define PLAYER_TYPE (player_get_type ())
#define PLAYER(obj) (G_TYPE_CHECK_INSTANCE_CAST ((obj), PLAYER_TYPE, Player))
#define PLAYER_CLASS(klass) (G_TYPE_CHECK_CLASS_CAST ((klass), PLAYER_TYPE, PlayerClass))

typedef struct _Player Player;
typedef struct _PlayerClass PlayerClass;
typedef struct _PlayerPrivate PlayerPrivate;

struct _Player {
	GObject parent;
	PlayerPrivate *priv;
};

struct _PlayerClass {
	GObjectClass parent_class;
};

GType player_get_type (void);
Player *player_new (void);
gboolean player_set_location (Player *player,
			      const char *location);
void player_play (Player *player);
void player_stop (Player *player);

#endif
