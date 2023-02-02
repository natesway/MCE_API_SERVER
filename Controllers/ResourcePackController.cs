using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
    [ServerHandleContainer]
    public static class ResourcePackController
    {
        [ServerHandle("/api/v1.1/resourcepacks/2020.1217.02/default")]
		public static byte[] Get(ServerHandleArgs args)
		{
			ResourcePackResponse response = new ResourcePackResponse
			{
				result = new List<ResourcePackResponse.Result>()
				{
					new ResourcePackResponse.Result
					{
						order = 0,
						parsedResourcePackVersion = new List<int>() {2020, 1214, 4},
						relativePath = "availableresourcepack/resourcepacks/dba38e59-091a-4826-b76a-a08d7de5a9e2-1301b0c257a311678123b9e7325d0d6c61db3c35", //Naming the endpoint the same thing for consistency. this might not be needed. 
						resourcePackVersion = "2020.1214.04",
						resourcePackId = "dba38e59-091a-4826-b76a-a08d7de5a9e2"
					}
				},
				updates = new Updates(),
				continuationToken = null,
				expiration = null
			};
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}
	}

	[ServerHandleContainer]
	public static class ResourcePackCdnController
	{
		[ServerHandle("/cdn/availableresourcepack/resourcepacks/dba38e59-091a-4826-b76a-a08d7de5a9e2-1301b0c257a311678123b9e7325d0d6c61db3c35")]
		public static byte[] Get(ServerHandleArgs args)
		{
			string resourcePackFilePath = "vanilla.zip";

			byte[] fileData;
			if (!LoadEmbededFile(resourcePackFilePath, out fileData)) {
				Log.Error("[Resourcepacks] Error! Resource pack file not found.");
				return BadRequest();
			}

            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition 
			{ 
				FileName = "dba38e59-091a-4826-b76a-a08d7de5a9e2-1301b0c257a311678123b9e7325d0d6c61db3c35", 
				Inline = true,
				Size = fileData.Length,
			};
			
			return File(args, fileData, "application/octet-stream", cd);
		}
	}
}
