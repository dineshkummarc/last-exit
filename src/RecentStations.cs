/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 *  Authors: Iain Holmes <iain@gnome.org>
 *
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

using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace LastExit {
	public class RecentStations
	{
		public struct Station {
			public string name;
			public string path;
		};

		private static ArrayList recent;
		public static ArrayList Recent {
			get { return RecentStations.LoadRecent (); }
			set { RecentStations.SaveRecent (value); }
		}

		private static ArrayList LoadRecent ()
		{
			XmlDocument xml = new XmlDocument ();
			ArrayList rs = new ArrayList ();

			string name = Driver.ConfigDirectory + "/recent.xml";
			if (File.Exists (name) == false) {
				return rs;
			}

			try {
				xml.Load (name);
			} catch (XmlException) {
				return rs;
			}

			XmlNodeList r = xml.GetElementsByTagName ("station");
			if (r.Count == 0) {
				return rs;
			}

			IEnumerator ienum = r.GetEnumerator ();
			while (ienum.MoveNext ()) {
				XmlNode r_node = (XmlNode) ienum.Current;
				Station s = new Station ();

				s.name = r_node["name"].InnerText;
				s.path = r_node["path"].InnerText;

				rs.Add (s);
			}

			return rs;
		}

		private static void SaveRecent (ArrayList stations)
		{
			XmlDocument xml = new XmlDocument ();
			XmlTextWriter tw;
			XmlNode node;
			XmlElement root;

			node = xml.CreateNode (XmlNodeType.XmlDeclaration, "", "");
			xml.AppendChild (node);

			root = xml.CreateElement ("", "recent-files", "");
			xml.AppendChild (root);

			foreach (Station s in stations) {
				XmlElement elem, name, path;

				elem = xml.CreateElement ("station");
				name = xml.CreateElement ("name");
				name.InnerText = s.name;
				
				path = xml.CreateElement ("path");
				path.InnerText = s.path;

				elem.AppendChild (name);
				elem.AppendChild (path);

				root.AppendChild (elem);
			}

			Stream stream = new FileStream (Driver.ConfigDirectory + "/recent.xml", FileMode.Create);
			tw = new XmlTextWriter (stream, null);
			tw.Formatting = Formatting.Indented;
			xml.WriteTo (tw);
			tw.Flush ();
			tw.Close ();
		}
	}
}
