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

namespace LastExit {
	public class Tag {
		private int id;
		public int ID {
			get { return id; }
			set { id = value; }
		}

		private string name;
		public string Name {
			get { return name; }
			set { name = value; }
		}

		private double match;
		public double Match {
			get { return match; }
			set { match = value; }
		}

		private int count;
		public int Count {
			get { return count; }
			set { count = value; }
		}

		public Tag ()
		{
		}

		public Tag (int id, string name, double match) 
		{
			this.id = id;
			this.name = name;
			this.match = match;
		}
	}
}
