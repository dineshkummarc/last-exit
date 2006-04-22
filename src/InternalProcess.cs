using System;
using System.IO;
using System.Runtime.InteropServices;
using GLib;

namespace LastExit {
	[Flags]
	internal enum InternalProcessFlags {
		LeaveDescriptorsOpen =       1 << 0,
		DoNotReapChild =             1 << 1,
		SearchPath =                 1 << 2,
		StandardOutputToDevNull =    1 << 3,
		StandardErrorToDevNull =     1 << 4,
		ChildInheritsStandardInput = 1 << 5,
		FileAndArgvZero =            1 << 6
	}

	internal class InternalProcess {
		[DllImport("libglib-2.0-0.dll")]
		static extern bool g_spawn_async (string working_dir,
						  string [] argv,
						  string [] envp,
						  InternalProcessFlags flags,
						  IntPtr child_setup,
						  IntPtr child_data,
						  IntPtr pid,
						  out IntPtr error);
		
		public static void Spawn (string [] args)
		{
			IntPtr error;
			
			if (args[args.Length -1] != null) {
				string [] nargs = new string [args.Length + 1];
				Array.Copy (args, nargs, args.Length);
				args = nargs;
			}
			
			g_spawn_async (null, args, null, 
				       InternalProcessFlags.SearchPath, 
				       IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
				       out error);
			
			if (error != IntPtr.Zero)
				throw new GException (error);
		}
	}
}
