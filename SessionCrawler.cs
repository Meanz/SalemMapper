using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SalemMapper
{
    class SessionCrawler
    {

        public static List<Session> CrawlSessions(string directory)
        {
            List<Session> sessions = new List<Session>();
            IEnumerable<string> files = Directory.EnumerateDirectories(directory);
            foreach (string file in files)
            {
                string file_name = file.Substring(file.LastIndexOf('\\') + 1);
                Session session = new Session();
                session.Name = file_name;
                session.Path = file;
                session.Tiles = CrawlDirectory(session, session.Path);
                sessions.Add(session);
            }
            return sessions;
        }

        /// <summary>
        /// Crawls the directory of a single session and returns all tiles in it
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static Tile[] CrawlDirectory(Session session, string directory)
        {
            List<Tile> tiles = new List<Tile>();
            IEnumerable<string> files = Directory.EnumerateFiles(directory);
            foreach (string file in files)
            {
                string file_name = file.Substring(file.LastIndexOf('\\') + 1);

                if (file_name.StartsWith("tile_") &&
                    file_name.ToLower().EndsWith(".png"))
                {
                    //Parse the filename
                    //tile_0_0.png
                    int index_of_first_slash = file_name.IndexOf('_');
                    string coords_first_part = file_name.Substring(index_of_first_slash + 1, file_name.Length - 1 - index_of_first_slash);
                    //x_y.png
                    int index_of_first_underscore = coords_first_part.IndexOf('_');
                    string x = coords_first_part.Substring(0, index_of_first_underscore);
                    string y = coords_first_part.Substring(index_of_first_underscore + 1, coords_first_part.IndexOf(".") - index_of_first_underscore - 1);
                    Tile tile = new Tile();
                    tile.LocalX = int.Parse(x);
                    tile.LocalY = int.Parse(y);
                    tile.Session = session;
                    tiles.Add(tile);
                    //Console.WriteLine("Added tile " + file_name);
                }
            }
            return tiles.ToArray();
        }

    }
}
