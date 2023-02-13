using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MCE_API_SERVER.Utils
{
	/// <summary>
	/// Contains Functions for converting long/lat -> tile pos, downloading tilees, and anything else that might come up
	/// </summary>
	public static class Tile
	{
		public static bool DownloadTile(int pos1, int pos2, string basePath)
		{
			HttpClient client = new HttpClient();
			try {
				Directory.CreateDirectory(Path.Combine(basePath, pos1.ToString()));
				string downloadUrl = StateSingleton.config.tileServerUrl + pos1 + "/" + pos2 + ".png";
				string savePath = Path.Combine(basePath, pos1.ToString(), $"{pos1}_{pos2}_16.png");

				HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
				req.Headers.UserAgent.Clear();
				req.Headers.UserAgent.Add(new ProductInfoHeaderValue("Mce_Api_Server", "1.0"));
				req.Headers.Remove("Cache-Control");
				req.Headers.Remove("Pragma");

				client = new HttpClient();
				HttpResponseMessage resp = client.SendAsync(req).Result;

				if (resp.StatusCode != HttpStatusCode.OK) {
					Log.Error($"Failed to download tile, StatusCode: {resp.StatusCode}");
					return false;
                }

				File.WriteAllBytes(savePath, resp.Content.ReadAsByteArrayAsync().Result);

				return true;
			}
			catch (Exception ex) {
				client?.Dispose();
				Log.Exception(ex);
				return false;
			}
		}

		//From https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames with slight changes

		public static string getTileForCoordinates(double lat, double lon)
		{
			//Adapted from java example. Zoom is replaced by the constant 16 because all MCE tiles are at zoom 16

			int xtile = (int)Math.Floor((lon + 180) / 360 * (1 << 16));
			int ytile = (int)Math.Floor((1 - Math.Log(Math.Tan(toRadians(lat)) + 1 / Math.Cos(toRadians(lat))) / Math.PI) / 2 * (1 << 16));

			if (xtile < 0)
				xtile = 0;
			if (xtile >= (1 << 16))
				xtile = ((1 << 16) - 1);
			if (ytile < 0)
				ytile = 0;
			if (ytile >= (1 << 16))
				ytile = ((1 << 16) - 1);

			return $"{xtile}_{ytile}";
		}

		//Helper
		static double toRadians(double angle)
		{
			return (Math.PI / 180) * angle;
		}
	}
}
