//
//  Nautilus.cs
//
//  Author:
//       Roman M. Yagodin <roman.yagodin@gmail.com>
//
//  Copyright (c) 2014-2016 Roman M. Yagodin
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;

namespace R7.Scripting
{
    /* Environment variables
	NAUTILUS_SCRIPT_SELECTED_FILE_PATHS: список выделенных файлов, разделённых переводом строки (только в локальном случае)
	NAUTILUS_SCRIPT_SELECTED_URIS: список адресов (URI) выделенных файлов, разделённых переводом строки
	NAUTILUS_SCRIPT_CURRENT_URI: текущий адрес URI
	NAUTILUS_SCRIPT_WINDOW_GEOMETRY: положение и размер текущего окна 
	NAUTILUS_SCRIPT_NEXT_PANE_SELECTED_FILE_PATHS: список выделенных файлов, разделённых переводом строки, в неактивной панели окна раздельного вида (только в локальном случае)	
	NAUTILUS_SCRIPT_NEXT_PANE_SELECTED_URIS: список адресов (URI) выделенных файлов, разделённых переводом строки, в неактивной панели окна раздельного вида
	NAUTILUS_SCRIPT_NEXT_PANE_CURRENT_URI: текущий адрес URI в неактивной панели окна раздельного вида
	*/

    // THINK: Support Marlin?
    public enum FileManager { Unknown, Nautilus, Nemo, Caja }

    [Obsolete ("Use Nautilus class instead")]
    public class NauHelper : Nautilus
    {
    }

	/// <summary>
	/// Helper class for Nautilus and Nautilus-like filemanagers
	/// </summary>
	public class Nautilus
	{
		private static Version version = null;
		public static Version Version 
		{
            get {
			    if (version == null) {
					version = new Version (
						Regex.Match (Command.RunToString (FileManager.ToString ().ToLowerInvariant (), "--version"), @"\d+\.\d+\.\d+").Value
					);
				}

				return version;
			}
		}

		protected static FileManager fileManager = FileManager.Unknown;
		public static FileManager FileManager
		{
            get {
                if (fileManager == FileManager.Unknown) {
				    if (Environment.GetEnvironmentVariable ("NAUTILUS_SCRIPT_CURRENT_URI") != null) {
                        fileManager = FileManager.Nautilus;
                    }
                    else if (Environment.GetEnvironmentVariable ("NEMO_SCRIPT_CURRENT_URI") != null) {
                        fileManager = FileManager.Nemo;
                    }
                    else if (Environment.GetEnvironmentVariable ("CAJA_SCRIPT_CURRENT_URI") != null) {
                        fileManager = FileManager.Caja;
                    }
				}

				return fileManager;
			}
		}

		protected static string Env (string suffix)
		{
			return FileManager.ToString ().ToUpperInvariant () + "_" + suffix;
		}

		protected static string EnvValue (string suffix)
		{
			return Environment.GetEnvironmentVariable (Env (suffix));
		}

		public static string ScriptDirectory
		{
            get {
			    switch (FileManager) {
					case FileManager.Nautilus:
						return Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".local/share/nautilus/scripts");

					case FileManager.Nemo:
						return Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".local/share/nemo/scripts");

                    case FileManager.Caja:
                        return Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".config/caja/scripts");

					default:
						throw new NotSupportedException ("NauHelper.SciptDirectory supports only Nautilus, Nemo and Caja file managers");
				}
			}
		}

		[Obsolete ("Use Nautilus.FileManager property instead")]
		public static bool FromNau
        {
			get { return !string.IsNullOrWhiteSpace(EnvValue ("SCRIPT_CURRENT_URI")); }
			
		}

		public static string CurrentDirectory
		{
            get {
			    if (!string.IsNullOrWhiteSpace (EnvValue ("SCRIPT_CURRENT_URI"))) {
                    return UrlDecode (EnvValue ("SCRIPT_CURRENT_URI").Remove (0, "file://".Length));
                }

	            return Directory.GetCurrentDirectory ();
			}
		}
		
		public static bool IsSomethingSelected
		{
			get { return !string.IsNullOrWhiteSpace (EnvValue("SCRIPT_SELECTED_URIS")); }
			
		}
		
		public static bool IsNothingSelected
		{
			get { return !IsSomethingSelected; }
		}
		
		protected static string [] selectedFiles = new string [0];
		public static string [] SelectedFiles
		{
            get {
			    if (selectedFiles.Length == 0) {
					var filesVar = EnvValue ("SCRIPT_SELECTED_URIS");
					selectedFiles = filesVar.Split (new char [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    // TODO: Support not only local files
                    for (var i = 0; i < selectedFiles.Length; i++) {
                        selectedFiles [i] = UrlDecode (selectedFiles [i].Remove (0, "file://".Length));
                    }
				}

				return selectedFiles;
			}
		}
				
		public static string FixUrlEncoding (string url)
		{
            url = url.Replace ("+", "%2B");
			url = url.Replace ("$", "%24");
			url = url.Replace ("&", "%26");
			url = url.Replace (",", "%2C");
			url = url.Replace (":", "%3A");
			url = url.Replace (";", "%3B");
			url = url.Replace ("=", "%3D");
			url = url.Replace ("?", "%3F");
            url = url.Replace ("(", "%28");
            url = url.Replace (")", "%29");
            url = url.Replace ("[", "%5B");
            url = url.Replace ("]", "%5D");
            url = url.Replace ("{", "%7B");
            url = url.Replace ("}", "%7D");

			return url;
		}
			
		public static string UrlDecode (string url)
		{
			return HttpUtility.UrlDecode (FixUrlEncoding (url));
		} 
	}
}
