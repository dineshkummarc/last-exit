/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Jorn Baayen, Iain Holmes
 *
 *  Copyright 2004, 2005 Jorn Baayen <jorn@gnome.org>
 *  Copyright 2006 Iain Holmes <iain@gnome.org>
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
using System.Text;

namespace LastExit
{
	public sealed class StringUtils {
		public static string SecsToString (long secs) 
		{
			long h, m, s;

			h = secs / 3600;
			m = (secs % 3600) / 60;
			s = (secs % 3600) % 60;

			if (h > 0) {
				return String.Format ("{0}:{1}:{2}", h, m.ToString ("d2"), s.ToString ("d2"));
			} else {
				return String.Format ("{0}:{1}", m, s.ToString ("d2"));
			} 
		}

		public static string EscapeForPango (string s)
		{
			s = s.Replace ("&", "&amp;");
			s = s.Replace ("<", "&lt;");

			return s;
		}

		public static string StringToUTF8 (string s)
		{
			byte [] ba = (new UnicodeEncoding()).GetBytes(s);
			return System.Text.Encoding.UTF8.GetString (ba);
		}
	}
}
