using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Nwc.XmlRpc;

namespace LastExit
{
	public class XmlRpc
	{
		private string Password { get { return Driver.config.Password; } }
		private string Username { get { return Driver.config.Username; } }
		private Decimal Challenge { get { return UnixTime(); } }
		private string AuthHash {
			get {
				MD5 hasher = MD5.Create();
				StringBuilder phash = new StringBuilder ();
				StringBuilder ahash = new StringBuilder ();
				byte[] Pass = hasher.ComputeHash(Encoding.Default.GetBytes (Password));
				for (int i = 0; i < Pass.Length; ++i) {
					phash.Append (Pass[i].ToString ("x2"));
				}
				string AuthText = phash.ToString() + Challenge.ToString();
				byte[] Auth = hasher.ComputeHash(Encoding.Default.GetBytes (AuthText));
				for (int i = 0; i < Auth.Length; ++i) {
					ahash.Append (Auth[i].ToString ("x2"));
				}
				return ahash.ToString();
			}

		}
		private String xmlrpc_base = "/1.0/rw/xmlrpc.php";
		private XmlRpcRequest client;
		
		private static long UnixTime()
		{
		  DateTime unixEpoch = new DateTime(1970,1,1,0,0,0);
		  return (DateTime.Now.Ticks - unixEpoch.Ticks)/10000000;
		}
		public String MethodName {
			set { 			
					client.MethodName = value;
					client.Params.Clear ();
					client.Params.Add (Username);
					client.Params.Add (Challenge.ToString());
					client.Params.Add (AuthHash);
				}
		}
			
		public XmlRpc () {
			this.client = new XmlRpcRequest();
		}
		
		public string Response () {
			try 
			{
				XmlRpcResponse response = client.Send("http://" + Driver.connection.BaseUrl + xmlrpc_base);
				if (response.IsFault) { 
					Console.WriteLine("Fault {0}: {1}", response.FaultCode, response.FaultString);
					return "error - " + response.FaultString;
				} else {
					return response.Value.ToString();
				}
			} 
			catch (XmlRpcException serverException)
			  {
			  	//TODO: show errors in window
			    Console.WriteLine("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString);
			    return "error -" + serverException.FaultString;
			  }
			catch (Exception e)
			  {
			    Console.WriteLine("Exception " + e);
			    return "error";
			  }
		}
		
		public void TagTrack (string Artist, string Song, IList Tag) {			
			IList Tags = new ArrayList();
			for (int i=0; i < Tag.Count; i++) { 
				Tags.Add(HttpUtility.HtmlEncode(Tag[i].ToString()));
			}
			MethodName = "tagTrack";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(Song));
			client.Params.Add (Tags);
			client.Params.Add ("append");	
			Response ();
		}
		
		public void TagArtist (string Artist, IList Tag) {			
			IList Tags = new ArrayList();
			for (int i=0; i < Tag.Count; i++) { 
				Tags.Add(HttpUtility.HtmlEncode(Tag[i].ToString()));
			}
			MethodName = "tagAlbum";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (Tags);
			client.Params.Add ("append");	
			Response ();
		}
		
		public void TagAlbum (string Artist, string Album, IList Tag) {			
			IList Tags = new ArrayList();
			for (int i=0; i < Tag.Count; i++) { 
				Tags.Add(HttpUtility.HtmlEncode(Tag[i].ToString()));
			}
			MethodName = "tagAlbum";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(Album));
			client.Params.Add (Tags);
			client.Params.Add ("append");	
			Response ();
		}
		
		public void loveTrack (string Artist, string Song) {			
			MethodName = "loveTrack";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(Song));
			Response ();
		}

		public void unLoveTrack (string Artist, string Song) {			
			MethodName = "unLoveTrack";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(Song));
			Response ();
		}
		public void banTrack (string Artist, string Song) {			
			MethodName = "banTrack";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(Song));
			Response ();
		}

		public void unBanTrack (string Artist, string Song) {			
			MethodName = "unBanTrack";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(Song));
			Response ();
		}

		public void removeFriend (string User) {			
			MethodName = "removeFriend";
			client.Params.Add (HttpUtility.HtmlEncode(User));
			Response ();
		}
		
		public void recommendTrack (string Artist, string Song, string User, string Message){			
			MethodName = "recommendTrack";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(Song));
			client.Params.Add (HttpUtility.HtmlEncode(User));
			client.Params.Add (HttpUtility.HtmlEncode(Message));
			Response ();
		}
		
		public void recommendAlbum (string Artist, string Album, string User, string Message) {			
			MethodName = "recommendAlbum";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(Album));
			client.Params.Add (HttpUtility.HtmlEncode(User));
			client.Params.Add (HttpUtility.HtmlEncode(Message));
			Response ();
		}
		
		public void recommendArtist (string Artist, string User, string Message) {			
			MethodName = "recommendArtist";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(User));
			client.Params.Add (HttpUtility.HtmlEncode(Message));
			Response ();
		}
		
		public void removeRecentlyListenedTrack (string Artist, string Song) {			
			MethodName = "removeRecentlyListenedTrack";
			client.Params.Add (HttpUtility.HtmlEncode(Artist));
			client.Params.Add (HttpUtility.HtmlEncode(Song));
			Response ();
		}
	}
}