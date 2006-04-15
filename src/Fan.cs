/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *
 *  Copyright 2005 Iain Holmes
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

using System;
using System.Collections;

namespace LastExit {
        public class Fan : ImageLoader {
		private string username;
		public string Username {
			get { return username; }
			set { username = value; }
		}

		private string url;
		public string Url {
			get { return url; }
			set { url = value; }
		}

		private int weight;
		public int Weight {
			get { return weight; }
			set { weight = value; }
		}

		public Fan (string username, string url,
			    string image, int weight)
		{
			this.username = username;
			this.url = url;
			this.ImageUrl = image;
			this.weight = weight;
		}
	}
}
