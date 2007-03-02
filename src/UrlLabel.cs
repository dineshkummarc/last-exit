namespace LastExit {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	public  class UrlLabel : Gtk.Label {

		~UrlLabel()
		{
			Dispose();
		}

		public UrlLabel(IntPtr raw) : base(raw) {}

		[DllImport("libsexy")]
		static extern IntPtr sexy_url_label_new();

		public UrlLabel () : base (IntPtr.Zero)
		{
			if (GetType () != typeof (UrlLabel)) {
				CreateNativeObject (new string [0], new GLib.Value[0]);
				return;
			}
			Raw = sexy_url_label_new();
		}

		[GLib.CDeclCallback]
		delegate void UrlActivatedSignalDelegate (IntPtr arg0, IntPtr arg1, IntPtr gch);

		static void UrlActivatedSignalCallback (IntPtr arg0, IntPtr arg1, IntPtr gch)
		{
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			LastExit.UrlActivatedArgs args = new LastExit.UrlActivatedArgs ();
			args.Args = new object[1];
			args.Args[0] = GLib.Marshaller.Utf8PtrToString (arg1);
			LastExit.UrlActivatedHandler handler = (LastExit.UrlActivatedHandler) sig.Handler;
			handler (GLib.Object.GetObject (arg0), args);

		}

		[GLib.CDeclCallback]
		delegate void UrlActivatedVMDelegate (IntPtr url_label, IntPtr url);

		static UrlActivatedVMDelegate UrlActivatedVMCallback;

		static void urlactivated_cb (IntPtr url_label, IntPtr url)
		{
			UrlLabel url_label_managed = GLib.Object.GetObject (url_label, false) as UrlLabel;
			url_label_managed.OnUrlActivated (GLib.Marshaller.Utf8PtrToString (url));
		}

		private static void OverrideUrlActivated (GLib.GType gtype)
                        {
			if (UrlActivatedVMCallback == null)
				UrlActivatedVMCallback = new UrlActivatedVMDelegate (urlactivated_cb);
			OverrideVirtualMethod (gtype, "url_activated", UrlActivatedVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(LastExit.UrlLabel), ConnectionMethod="OverrideUrlActivated")]
		protected virtual void OnUrlActivated (string url)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (url);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("url_activated")]
		public event LastExit.UrlActivatedHandler UrlActivated {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "url_activated", new UrlActivatedSignalDelegate(UrlActivatedSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "url_activated", new UrlActivatedSignalDelegate(UrlActivatedSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[DllImport("libsexy")]
		static extern IntPtr sexy_url_label_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = sexy_url_label_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("libsexy")]
		static extern void sexy_url_label_set_markup(IntPtr raw, IntPtr markup);

		public new string Markup { 
			set {
				IntPtr markup_as_native = GLib.Marshaller.StringToPtrGStrdup (value);
				sexy_url_label_set_markup(Handle, markup_as_native);
				GLib.Marshaller.Free (markup_as_native);
			}
		}
	}
}
