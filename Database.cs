using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data.SQLite;
using System.IO;
using System.Threading;

namespace SalemMapper
{
    class Database
    {

        private static string _mapper_dir = "D:/SalemMapper/";

        private static Dictionary<int, SQLiteConnection> _layer_connections = new Dictionary<int, SQLiteConnection>();

        private static Mutex _db_mutex = new Mutex();

        public static void SetMapperDir(string mapper_dir)
        {
            _mapper_dir = mapper_dir;
        }

        public static void CloseAll()
        {
            _db_mutex.WaitOne();
            foreach (SQLiteConnection con in _layer_connections.Values)
            {
                con.Close();
            }
            _db_mutex.ReleaseMutex();
        }

        public void LoadTile()
        {

        }

        public static SQLiteConnection OpenLayer(int layer)
        {
            string db_file_name = _mapper_dir + "Layers/" + layer + "/layer.db";

            bool new_file = false;
            if(!File.Exists(db_file_name))
            {
                SQLiteConnection.CreateFile(db_file_name);
                new_file = true;
            }

            SQLiteConnection db_connection = new SQLiteConnection("Data Source=" + db_file_name + ";Version=3;");
            db_connection.Open();

            if(new_file)
            {
                string create_tiles_table = "create table tiles (session_name varchar(30), global_x int, global_y int, local_x int, local_y int)";
                SQLiteCommand cmd = new SQLiteCommand(create_tiles_table, db_connection);
                cmd.ExecuteNonQuery();
            }
            return db_connection;
        }

        public static SQLiteConnection GetLayerConnection(int layer)
        {
            SQLiteConnection con = null;
            _layer_connections.TryGetValue(layer, out con);
            if(con == null)
            {
                con = OpenLayer(layer);
                _layer_connections[layer] = con;
            }
            return con;
        }

        public static void SaveTile(int layer, LayerTile t)
        {
            Image bm = t.Image;
            bm.Save(_mapper_dir + "Layers/" + layer + "/Tiles/" + t.GlobalX + "_" + t.GlobalY + ".png", ImageFormat.Png);


            //Update database
            _db_mutex.WaitOne();
            SQLiteConnection con = GetLayerConnection(layer);
            string query = "insert or replace into tiles (session_name, global_x, global_y, local_x, local_y) VALUES(\"" + t.CurrentSession + "\", " + 
                t.GlobalX + "," + t.GlobalY + ", " + t.LocalX + ", " + t.LocalY + ")";
            SQLiteCommand cmd = new SQLiteCommand(query, con);
            cmd.ExecuteNonQuery();
            _db_mutex.ReleaseMutex();
        }

    }
}
