/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
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

#define _GNU_SOURCE
#include <config.h>
#include <string.h>
#include <math.h>

#include <gst/gst.h>

#include "player.h"

enum {
	NEW_SONG,
	ERROR,
	LAST_SIGNAL
};
	
struct _PlayerPrivate {
	GstElement *play;
};

static GObjectClass *parent_class;
static guint signals[LAST_SIGNAL];

static void
finalize (GObject *object)
{
	Player *player = PLAYER (object);

	player_stop (player);

	g_object_unref (G_OBJECT (player->priv->play));
	g_free (player->priv);

	G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
player_class_init (PlayerClass *klass)
{
	GObjectClass *object_class;

	parent_class = g_type_class_peek_parent (klass);
	object_class = (GObjectClass *) klass;

	object_class->finalize = finalize;

	signals[NEW_SONG] = g_signal_new ("new-song",
					  G_TYPE_FROM_CLASS (klass),
					  G_SIGNAL_RUN_LAST,
					  0,
					  NULL, NULL,
					  g_cclosure_marshal_VOID__VOID,
					  G_TYPE_NONE, 0);
	
	signals[ERROR] = g_signal_new ("error",
				       G_TYPE_FROM_CLASS (klass),
				       G_SIGNAL_RUN_LAST,
				       0, 
				       NULL, NULL,
				       g_cclosure_marshal_VOID__STRING,
				       G_TYPE_NONE, 1, G_TYPE_STRING);
}

static void
player_init (Player *player)
{
	player->priv = g_new0 (PlayerPrivate, 1);
}

static gboolean
bus_message_cb (GstBus *bus,
		GstMessage *message,
		gpointer data)
{
	Player *player = (Player *) data;
	GError *err;
	char *debug;
	GstState old_state, new_state;

	switch (GST_MESSAGE_TYPE (message)) {
	case GST_MESSAGE_ERROR:
		gst_message_parse_error (message, &err, &debug);
		
		player_stop (player);
		break;

	case GST_MESSAGE_EOS:
		g_print ("Eos...\n");
		break;

	case GST_MESSAGE_STATE_CHANGED:
		break;

	case GST_MESSAGE_APPLICATION:
		g_signal_emit (player, signals[NEW_SONG], 0);
		break;

	default:
		break;
	}

	return TRUE;
}

static void
post_new_song_message (Player *player)
{
	GstMessage *msg;
	GstStructure *s;
	GstBus *bus;

	s = gst_structure_new ("new-song", NULL);
	msg = gst_message_new_application (GST_OBJECT (player->priv->play), s);

	bus = gst_pipeline_get_bus (GST_PIPELINE (player->priv->play));
	gst_bus_post (bus, msg);
}

static gboolean
have_data_cb (GstPad *pad,
	      GstBuffer *buffer,
	      Player *player)
{
	char *data = GST_BUFFER_DATA (buffer);
	char *s;
	static int req = 0;
	static char sync[5] = "SYNC";

	/* If there's a function that is ever screaming out for a g_* 
	   implementation its memmem...
	   Just read the man page for the list of possible bugs 
	   We need super_memmem: Like memmem, but doesn't suck */
	s = memmem (data, GST_BUFFER_SIZE (buffer), sync, 4);
	if (s != NULL) {
		post_new_song_message (player);
	} 
#if 0
	else if (req > 0) {
		if (strncmp (data, sync + (4 - req), req) == 0) {
			post_new_song_message (player);
		}

		// Reset req
		req = 0;
	} else {
		guint len = GST_BUFFER_SIZE (buffer);
		char *e = data + (len - 3);

		// Is it even possible for the SYNC to get split over two
		// different buffers? 

		// Check if there is any of SYN in the last three chars
		if (*(e + 2) == 'S') {
			req = 3;
		} else if ((*(e + 2) == 'Y') && (*(e + 1) == 'S')) {
			req = 2;
		} else if ((*(e + 2) == 'N') && (*(e + 1) == 'Y') && (*e == 'S')) {
			req = 1;
		} else {
			req = 0;
		}
	}
#endif

	return TRUE;
}

static void
src_setup (GObject *object,
	   GParamSpec *pspec,
	   Player *player)
{
	GstElement *src;
	GstPad *pad;

	g_object_get (G_OBJECT (player->priv->play),
		      "source", &src,
		      NULL);

	pad = gst_element_get_pad (src, "src");
	gst_pad_add_buffer_probe (pad, G_CALLBACK (have_data_cb), player);
}

static void
player_construct (Player *player,
		  char **error)
{
	PlayerPrivate *priv = player->priv;

	gst_init (NULL, NULL);

	priv->play = gst_element_factory_make ("playbin", "last-fm-player");
	g_signal_connect (G_OBJECT (priv->play), "notify::source",
			  G_CALLBACK (src_setup), player);

	gst_bus_add_watch (gst_pipeline_get_bus (GST_PIPELINE (priv->play)),
			   bus_message_cb, player);
}
	
GType
player_get_type (void)
{
	static GType type = 0;

	if (!type) {
		static const GTypeInfo info = {
			sizeof (PlayerClass), NULL, NULL,
			(GClassInitFunc) player_class_init,
			NULL, NULL, sizeof (Player), 0, 
			(GInstanceInitFunc) player_init,
		};

		type = g_type_register_static (G_TYPE_OBJECT, "Player",
					       &info, 0);
	}
	
	return type;
}

Player *
player_new (void)
{
	Player *player;

	player = g_object_new (PLAYER_TYPE, NULL);

	player_construct (player, NULL);

	return player;
}

gboolean
player_set_location (Player *player,
		     const char *location)
{
/* 	player_stop (player); */

	g_object_set (G_OBJECT (player->priv->play), 
		      "uri", location, 
		      NULL);

	return TRUE;
}

void
player_play (Player *player)
{
	gst_element_set_state (GST_ELEMENT (player->priv->play), 
			       GST_STATE_PLAYING);
}

void
player_stop (Player *player)
{
	gst_element_set_state (GST_ELEMENT (player->priv->play), 
			       GST_STATE_READY);
}

void
player_set_volume (Player *player,
		   int volume)
{
	double vol;

	vol = CLAMP (volume, 0, 100) / 100.0;

	g_object_set (G_OBJECT (player->priv->play),
		      "volume", vol,
		      NULL);
}

int
player_get_volume (Player *player)
{
	double vol;

	g_object_get (G_OBJECT (player->priv->play),
		      "volume", &vol,
		      NULL);

	return (int)(vol * 100);
}
