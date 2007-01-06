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

#include <string.h>

#include <last-fm.h>

enum {
	LAST_SIGNAL
};

static GstStaticPadTemplate sink_factory =
GST_STATIC_PAD_TEMPLATE ("sink",
			 GST_PAD_SINK,
			 GST_PAD_ALWAYS,
			 GST_STATIC_CAPS_ANY);
static GstStaticPadTemplate src_factory =
GST_STATIC_PAD_TEMPLATE ("src",
			 GST_PAD_SRC,
			 GST_PAD_ALWAYS,
			 GST_STATIC_CAPS_ANY);

GST_DEBUG_CATEGORY_STATIC (last_fm_debug);
#define GST_CAT_DEFAULT last_fm_debug

static GstElementDetails last_fm_details = {
	"Last FM filter",
	"Filter",
	"Filters the SYNC tag out of Last.FM streams",
	"Iain Holmes <iain@gnome.org>",
};

static GstElementClass *parent_class = NULL;

static gboolean
last_fm_get_unit_size (GstBaseTransform *base,
		       GstCaps *caps,
		       guint *size)
{
	*size = 4096;
	g_print ("Getting unit size\n");
	return TRUE;
}

static GstFlowReturn
last_fm_transform (GstBaseTransform *btrans,
		   GstBuffer *inbuf,
		   GstBuffer *outbuf)
{
	char *data = (char *) GST_BUFFER_DATA (inbuf);
	char *loc;

	loc = strstr (data, "SYNC");
	if (loc == NULL) {
		memcpy (GST_BUFFER_DATA (outbuf), data, GST_BUFFER_SIZE (outbuf));
		gst_buffer_stamp (outbuf, inbuf);
	} else {
		g_print ("Got sync at %p offset %d\n", loc, (int) (loc - data));
		memcpy (GST_BUFFER_DATA (outbuf), data, GST_BUFFER_SIZE (outbuf));
		gst_buffer_stamp (outbuf, inbuf);
	}
		
	return GST_FLOW_OK;
}


{
	/* 
	 * Explicitly load the plugins.
	 * This is to avoid Mono JIT loosing the ability to catch SEGV.
	 * http://bugzilla.gnome.org/show_bug.cgi?id=391777
	*/
	GstElement *element = gst_parse_launch("audioconvert", NULL);
	element = gst_parse_launch("audioconvert", NULL);
	gst_object_unref(element);

	GstElementClass *element_class = GST_ELEMENT_CLASS (klass);

	gst_element_class_add_pad_template (element_class, gst_static_pad_template_get (&src_factory));
	gst_element_class_add_pad_template (element_class, gst_static_pad_template_get (&sink_factory));
	gst_element_class_set_details (element_class, &last_fm_details);
}

static void
last_fm_class_init (LastFMClass *klass)
{
	GObjectClass *object_class;
	GstElementClass *element_class;
	GstBaseTransformClass *transform_class;

	object_class = (GObjectClass *) klass;
	element_class = (GstElementClass *) klass;
	transform_class = (GstBaseTransformClass *) klass;

	parent_class = g_type_class_ref (GST_TYPE_BASE_TRANSFORM);

/* 	transform_class->get_unit_size = last_fm_get_unit_size; */
	transform_class->transform = last_fm_transform;

	GST_DEBUG_CATEGORY_INIT (last_fm_debug, "last-fm", 0,
				 "Last Exit element");
}

static void
last_fm_init (LastFM *lastfm)
{
	/* Should do something? */
}

GType
last_fm_get_type (void)
{
	static GType type = 0;

	if (!type) {
		static const GTypeInfo info = {
			sizeof (LastFMClass),
			(GBaseInitFunc) last_fm_base_init,
			NULL,
			(GClassInitFunc) last_fm_class_init,
			NULL,
			NULL,
			sizeof (LastFM),
			0,
			(GInstanceInitFunc) last_fm_init,
		};

		type = g_type_register_static (GST_TYPE_BASE_TRANSFORM,
					       "LastFM", &info, 0);
	}

	return type;
}
