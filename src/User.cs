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
using System.Text;
using System.Xml; 

namespace LastExit
{
	public class User {
		public delegate void UserLoadedHandler (User user);
		public event UserLoadedHandler UserLoaded;

		private string username;
		public string Username {
			get { return username; }
			set { username = value; }
		}

		private string realname;
		public string RealName {
			get { return realname; }
			set { realname = value; }
		}

		private string homepage;
		public string Homepage {
			get { return homepage; }
			set { homepage = value; }
		}

		private string url;
		public string Url {
			get { return url; }
			set { url = value; }
		}

		private int age;
		public int Age {
			get { return age; }
			set { age = value; }
		}

		private string gender;
		public string Gender {
			get { return gender; }
			set { gender = value; }
		}

		private string country;
		public string Country {
			get { return country; }
			set { country = value; }
		}

		private int playcount;
		public int PlayCount {
			get { return playcount; }
			set { playcount = value; }
		}

		private string registered;
		public string Registered {
			get { return registered; }
			set { registered = value; }
		}

		public User (string name) {
			this.username = name;

			FMRequest fmr = new FMRequest ();
			string base_url = Driver.connection.BaseUrl;
			string url = "http://" + base_url + "/1.0/user/" + name + "/profile.xml";
			
			fmr.RequestCompleted += new FMRequest.RequestCompletedHandler (GetUserCompleted);
			fmr.DoRequest (url);
			
			Driver.connection.DoOperationStarted ();
		}

		private void GetUserCompleted (FMRequest request) 
		{
			if (request.Data.Length > 1) {
				string content;

				content = request.Data.ToString ();
				OnGetUserInfoCompleted (content);
			}

			Driver.connection.DoOperationFinished ();
		}

		private string GetXmlString (XmlNode node)
		{
			if (node != null) {
				return node.InnerText;
			} else {
				return "";
			}
		}

		private void OnGetUserInfoCompleted (string content) 
		{
			XmlDocument xml = new XmlDocument ();
			XmlNodeList elemlist;

			xml.LoadXml (content);
			elemlist = xml.GetElementsByTagName ("profile");
			if (elemlist.Count == 0) {
				return;
			}
			
			XmlNode profile = elemlist[0];
			string username = profile.Attributes.GetNamedItem("username").InnerText;
			
			string url = GetXmlString (profile["url"]);
			string realname = GetXmlString (profile["realname"]);
			string homepage = GetXmlString (profile["homepage"]);
			string registered = GetXmlString (profile["registered"]);
			string age = GetXmlString (profile["age"]);
			string gender = GetXmlString (profile["gender"]);
			string country = GetXmlString (profile["country"]);
			string playcount = GetXmlString (profile["playcount"]);
		}			
		
	}
}
