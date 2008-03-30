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
	END_SONG,
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
	
	signals[END_SONG] = g_signal_new ("end-song",
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
	gint percent = 0;
	GstState old_state, new_state;

	switch (GST_MESSAGE_TYPE (message)) {
	case GST_MESSAGE_ERROR:
		gst_message_parse_error (message, &err, &debug);
		
		player_stop (player);
		g_signal_emit (player, signals[ERROR], 1, err->message);
		break;

	case GST_MESSAGE_EOS:
		player_stop (player);
		g_signal_emit (player, signals[END_SONG], 0);
		break;

	case GST_MESSAGE_STATE_CHANGED:
		break;

	case GST_MESSAGE_BUFFERING:
		gst_message_parse_buffering (message, &percent);
		if (percent != 100) 
			gst_element_set_state (player->priv->play, GST_STATE_PAUSED);
		else 
			gst_element_set_state (player->priv->play, GST_STATE_PLAYING);
			
		g_print ("Buffering (%.2u percent done) \r", percent);
		break;

	default:
		break;
	}

	return TRUE;
}


static void
player_construct (Player *player,
		  char **error)
{
	PlayerPrivate *priv = player->priv;
	GstElement *sink;

	gst_init (NULL, NULL);

	sink = gst_element_factory_make ("gconfaudiosink", "sink");
	priv->play = gst_element_factory_make ("playbin", "player");
	g_object_set (G_OBJECT (priv->play), 
		      "audio-sink", sink,
		      NULL);
		      
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
	g_signal_emit (player, signals[NEW_SONG], 0);
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

void
player_exit (Player *player)
{
	gst_element_set_state (player->priv->play,
			       GST_STATE_NULL);
}

gint64
player_get_stream_position (Player *player)
{
	GstFormat fmt = GST_FORMAT_TIME; // time in nanoseconds
	gint64 pos;

	if (gst_element_query_position (player->priv->play, 
	  &fmt, 
	  &pos))
		return pos;
	else
		return -1;
}
