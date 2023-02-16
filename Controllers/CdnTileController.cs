using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Utils;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
    [ServerHandleContainer]
    public static class CdnTileController
    {
        [ServerHandle("/cdn/tile/16/{_}/{tilePos1}_{tilePos2}_16.png")]
        public static byte[] Get(ServerHandleArgs args) // _ used because we dont care :|
        {
            int tilePos1;
            int tilePos2;
            if (!int.TryParse(args.UrlArgs["tilePos1"], out tilePos1) || !int.TryParse(args.UrlArgs["tilePos2"], out tilePos2))
                return Content(args, "hi!", "text/plain");

            string targetTilePath = SavePath_Server + $"tiles/16/{tilePos1}/{tilePos1}_{tilePos2}_16.png";

            if (!System.IO.File.Exists(targetTilePath)) {
                bool boo = Tile.DownloadTile(tilePos1, tilePos2, SavePath_Server + @"tiles/16/");

                //Lets download that lovely tile now, Shall we?
                if (boo == false) {
                    return Content(args, "hi!", "text/plain");
                } // Error 400 on Tile download error
            }

            //String targetTilePath = $"./data/tiles/creeper_tile.png";
            byte[] fileData = System.IO.File.ReadAllBytes(targetTilePath); //Namespaces
            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition { FileName = tilePos1 + "_" + tilePos2 + "_16.png", Inline = true };


            return File(args, fileData, "application/octet-stream", cd);
        }
    }
}
