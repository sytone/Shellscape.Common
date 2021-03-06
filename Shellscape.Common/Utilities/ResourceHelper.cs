﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Shellscape.Utilities {

	public static class ResourceHelper {

		//#region .    Win32

		//private const uint RT_BITMAP = 0x00000002;
		//private const int LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

		//[DllImport("kernel32.dll", SetLastError = true)]
		//static extern bool FreeLibrary(IntPtr hModule);

		//[DllImport("kernel32.dll", SetLastError = true)]
		//public extern static IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, int dwFlags);

		//[DllImport("kernel32.dll", SetLastError = true)]
		//public extern static bool EnumResourceNames(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpszType, EnumResNameProc lpEnumFunc, IntPtr lParam);

		//public delegate bool EnumResNameProc(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

		//[DllImport("kernel32.dll", SetLastError = true)]
		//static extern IntPtr FindResource(IntPtr hModule, string lpName, IntPtr lpType);

		//[DllImport("kernel32.dll", SetLastError = true)]
		//static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

		//[DllImport("Kernel32.dll")]
		//public static extern int FreeResource(IntPtr hglbResource);

		//[DllImport("kernel32.dll", SetLastError = true)]
		//static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

		//[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
		//static extern void CopyMemory(IntPtr dest, IntPtr src, int Length);

		//[DllImport("Gdi32", EntryPoint = "GdiFlush")]
		//private extern static void GdiFlush();

		//#endregion

		private static String _resourcePrefix = String.Empty;

		static ResourceHelper() {

			Assembly assembly = Assembly.GetEntryAssembly();

			if (assembly == null) {
				return;
			}

			String[] resourceNames = assembly.GetManifestResourceNames();
			String root = String.Empty;
			String find = ".Resources.";

			foreach (String res in resourceNames) {
				if (res.Contains(find)) {
					root = res;
					break;
				}
			}

			root = root.Substring(0, root.LastIndexOf(find) + find.Length);

			_resourcePrefix = root;
		}

		/// <summary>
		/// Returns the contents of a file which has been embedded as a resource.
		/// This functional has only been setup to handle the following data types: String
		/// </summary>
		/// <typeparam name="T">Datatype of the data you want returned.</typeparam>
		/// <param name="assembly"></param>
		/// <param name="fileName">Filename of the embedded resource.</param>
		/// <returns></returns>
		public static T Get<T>(Assembly assembly, String fileName) {

			Type type = typeof(T);

			if (type == typeof(Stream)) {
				return (T)Convert.ChangeType(assembly.GetManifestResourceStream(String.Concat(_resourcePrefix, fileName)), type);
			}

			using (Stream dataStream = assembly.GetManifestResourceStream(String.Concat(_resourcePrefix, fileName))) {

				if (dataStream == null) {
					return default(T);
				}

				if (type == typeof(String)) {
					using (StreamReader sr = new StreamReader(dataStream)) {
						return (T)Convert.ChangeType(sr.ReadToEnd(), type);
					}
				}
				else if (type == typeof(Icon) || type == typeof(Image) || type == typeof(Bitmap)) {
					try {
						ConstructorInfo constructor = typeof(T).GetConstructor(new System.Type[] { typeof(Stream) });
						T result = (T)constructor.Invoke(new object[] { dataStream });

						return result;

					}
					catch { }
				}
			}

			return default(T);

		}

		/// <summary>
		/// Returns the contents of a file which has been embedded as a resource.
		/// This functional has only been setup to handle the following data types: String
		/// </summary>
		/// <typeparam name="T">Datatype of the data you want returned.</typeparam>
		/// <param name="fileName">Filename of the embedded resource.</param>
		/// <returns></returns>
		public static T Get<T>(String fileName) {

			Assembly assembly = Assembly.GetEntryAssembly(); // Assembly.GetCallingAssembly(); may want to revisit this method to allow for loading from other sources.

			return Get<T>(assembly, fileName);
		}

		public static Stream GetResourceStream(String fileName) {
			Assembly assembly = Assembly.GetEntryAssembly();
			return assembly.GetManifestResourceStream(String.Concat(_resourcePrefix, fileName));
		}

		public static Icon GetIcon(string iconName) {
			iconName = String.Concat("Icons.", iconName);

			return Get<Icon>(iconName);
		}

		public static Icon GetIcon(string iconName, int size) {
			iconName = String.Concat("Icons.", iconName);

			using (Stream stream = GetResourceStream(iconName)) {
				if (stream == null) {
					return default(Icon);
				}

				return new Icon(stream, size, size);
			}

		}

		public static Bitmap GetImage(string imageName) {
			imageName = String.Concat("Images.", imageName);

			return Get<Bitmap>(imageName);
		}

		public static Stream GetStream(string resourceName) {
			return Get<Stream>(resourceName);
		}

		//public static Locale GetLocale(String language) {

		//  String fileName = String.Concat("Locales.", language, ".xml");
		//  String xml = String.Empty;

		//  // we have to load the xml via System.Xml so that character encoding is enforced and we get the right unicode output.
		//  // otherwise, the Deserializer just craps out poop characters.
		//  using (Stream stream = GetResourceStream(fileName)) {
		//    if (stream == null) {
		//      return null;
		//    }				

		//    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

		//    try {
		//      doc.Load(stream);
		//    }
		//    catch (Exception) {
		//      return null;
		//    }

		//    xml = doc.OuterXml;

		//    if (String.IsNullOrEmpty(xml)) {
		//      return null;
		//    }
		//  }

		//  Locale locale = Utilities.Serializer.Deserialize<Locale>(xml);

		//  return locale;
		//}

		//private static Dictionary<String, String> _AvailableLocales = null;

		//public static Dictionary<String, String> AvailableLocales {
		//  get {

		//    if (_AvailableLocales == null) {
		//      _AvailableLocales = new Dictionary<String, String>();

		//      Assembly a = Assembly.GetExecutingAssembly();
		//      String[] resNames = a.GetManifestResourceNames();
		//      String prefix = String.Concat(_resourcePrefix, "Locales.");

		//      foreach (String name in resNames) {
		//        if(!name.StartsWith(prefix)){
		//          continue;
		//        }

		//        String lang = name.Replace(".xml", String.Empty).Replace(prefix, String.Empty);
		//        Locale locale = GetLocale(lang);

		//        if (locale != null) {
		//          _AvailableLocales.Add(locale.Name, lang);
		//        }
		//      }

		//      if (_AvailableLocales.Keys.Count > 0) {
		//        _AvailableLocales.OrderBy(o => o.Key);
		//      }

		//    }

		//    return _AvailableLocales;

		//  }
		//}

		public static Bitmap GetStandardResourceBitmap(String dllName, String resourceId) {
			Bitmap result = null;

			using (ResourceLibrary library = new ResourceLibrary() { Filename = dllName }) {
				IntPtr hDib = library.GetResource(resourceId, ResourceLibrary.ImageType.IMAGE_BITMAP, ResourceLibrary.ImageLoadOptions.LR_CREATEDIBSECTION);
				if (!hDib.Equals(IntPtr.Zero)) {
					result = ImageUtility.DibToBitmap(hDib);
					ImageUtility.DeleteObject(hDib);
				}
			}

			return result;
		}

		//public static Bitmap GetResourcePNG(String moduleName, string resourceID) {

		//  const int FILE_HEADER_BYTES = 40; // the standard 40 bytes of BITMAPHEADERINFO).

		//  Bitmap bitmap = null;
		//  IntPtr hModule = IntPtr.Zero;
		//  IntPtr hResource = IntPtr.Zero;

		//  try {

		//    // load the bitmap resource normally to get dimensions etc.

		//    hModule = LoadLibraryEx(moduleName, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
		//    hResource = FindResource(hModule, "#" + resourceID, (IntPtr)RT_BITMAP);
		//    int resourceSize = (int)SizeofResource(hModule, hResource);

		//    // initialize 32bit alpha bitmap (target)
		//    using (Bitmap tmpNoAlpha = Bitmap.FromResource(hModule, "#" + resourceID)) {
		//      bitmap = new Bitmap(tmpNoAlpha.Width, tmpNoAlpha.Height, PixelFormat.Format32bppArgb);
		//    }

		//    // load the resource (preserves alpha)
		//    IntPtr hLoadedResource = LoadResource(hModule, hResource);

		//    // copy bitmap data into byte array directly
		//    byte[] bitmapBytes = new byte[resourceSize];
		//    IntPtr firstCopyElement = Marshal.UnsafeAddrOfPinnedArrayElement(bitmapBytes, 0);
		//    // nb. we only copy the actual PNG data (no header)

		//    CopyMemory(firstCopyElement, hLoadedResource, resourceSize);
		//    FreeResource(hLoadedResource);

		//    // copy the byte array contents back to a handle to the alpha bitmap (use lockbits)
		//    Rectangle copyArea = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		//    BitmapData alphaBits = bitmap.LockBits(copyArea, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

		//    // copymemory to bitmap data (Scan0)
		//    IntPtr scan0 = Marshal.UnsafeAddrOfPinnedArrayElement(bitmapBytes, FILE_HEADER_BYTES);
		//    CopyMemory(alphaBits.Scan0, scan0, resourceSize - FILE_HEADER_BYTES);

		//    // complete operation
		//    bitmap.UnlockBits(alphaBits);
		//    GdiFlush();

		//    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY); // image is inverted when using LoadResource
		//  }
		//  catch (Exception e) {

		//  }
		//  finally {
		//    if (hModule != IntPtr.Zero) {
		//      FreeLibrary(hModule);
		//    }
		//  }

		//  return bitmap;
		//}

	}
}

