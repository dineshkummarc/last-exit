/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *
 *  Copyright 2006 Iain Holmes
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

#ifndef __LAST_FM_H__
#define __LAST_FM_H__

#include <gst/gst.h>
#include <gst/base/gstbasetransform.h>

G_BEGIN_DECLS

#define LAST_FM_TYPE (last_fm_get_type ())
#define LAST_FM(obj) (G_TYPE_CHECK_INSTANCE_CAST ((obj), LAST_FM_TYPE, LastFM))
#define LAST_FM_CLASS(klass) (G_TYPE_CHECK_CLASS_CAST ((klass), LAST_FM_TYPE, LastFMClass))

typedef struct _LastFM LastFM;
typedef struct _LastFMClass LastFMClass;
typedef struct _LastFMPrivate LastFMPrivate;

struct _LastFM {
	GstBaseTransform element;

	LastFMPrivate *priv;
};

struct _LastFMClass {
	GstBaseTransformClass parent_class;
};

GType last_fm_get_type (void);

G_END_DECLS

#endif
