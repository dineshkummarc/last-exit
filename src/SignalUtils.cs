/*
 * Copyright (C) 2004 Tamara Roberson <foxxygirltamara@gmail.com>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as
 * published by the Free Software Foundation; either version 2 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public
 * License along with this program; if not, write to the
 * Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.  */

using System;
using System.Runtime.InteropServices; 

namespace LastExit
{
	public sealed class SignalUtils {
		public delegate void SignalDelegate (IntPtr obj);
		public delegate void SignalDelegateStr (IntPtr obj, string arg);
		
		[DllImport ("libgobject-2.0-0.dll")]
		private static extern uint g_signal_connect_data (IntPtr obj,
								  string name,
								  SignalDelegate cb,
								  IntPtr data,
								  IntPtr p, 
								  int flags);
		public static uint SignalConnect (IntPtr obj, string name, SignalDelegate cb) {
			return SignalConnect (obj, name, cb, IntPtr.Zero, IntPtr.Zero, 0);
		}

		public static uint SignalConnect (IntPtr obj, string name, 
						  SignalDelegate cb,
						  IntPtr data, IntPtr p,
						  int flags) {
			return g_signal_connect_data (obj, name, cb, data, p, flags);
		}

		[DllImport ("libgobject-2.0-0.dll")]
		private static extern uint g_signal_connect_data (IntPtr obj, 
								  string name,
								  SignalDelegateStr cb, 
								  IntPtr data,
								  IntPtr p, 
								  int flags);
		public static uint SignalConnect (IntPtr obj, string name, SignalDelegateStr cb)
		{
			return SignalConnect (obj, name, cb, IntPtr.Zero, IntPtr.Zero, 0);
		}

		public static uint SignalConnect (IntPtr obj, string name, 
						  SignalDelegateStr cb,
						  IntPtr data, IntPtr p,
						  int flags) {
			return g_signal_connect_data (obj, name, cb, data, p, flags);
		}
						  
	}
}

