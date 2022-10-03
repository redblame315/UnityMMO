﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MMOWorldServer
{
    public static class WorldDatabase
    {
        private static MySqlConnection conn;
        private static string connString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();

        public static String SetupConnection()
        {
            string status;
            try
            {
                conn = new MySqlConnection(connString);
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "TRUNCATE TABLE online_players; ALTER TABLE online_players AUTO_INCREMENT = 1;";
                command.ExecuteNonQuery();
                status = "OK";
            }
            catch (MySqlException e)
            {
                status = e.Message.ToString();
            }
            return status;
        }

        public static void RemoveFromOnlinePlayerList(uint characterId)
        {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "DELETE FROM online_players where charId=@charId";
            command.Parameters.AddWithValue("@charId", characterId);
            command.ExecuteNonQuery();
        }

        public static uint GetSessionId(uint characterId)
        {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "SELECT sessionId from online_players where charId=@characterId";
            command.Parameters.AddWithValue("@characterId", characterId);
            MySqlDataReader rdr = command.ExecuteReader();
            rdr.Read();
            if (rdr.HasRows)
            {
                var sessionId = rdr.GetUInt32(0);
                rdr.Close();

                return sessionId;
            }
            else
            {
                throw new Exception("Could not find session id for character id: " + characterId);
            }
        }

        public static void AddToOnlinePlayerList(uint characterId, string clientAddress)
        {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "INSERT INTO online_players(`charId`, `accountId`, `name`, `ipAddress`) SELECT id,accountId,name,@ipAddress from login.characters where id=@characterId;";
            command.Parameters.AddWithValue("@characterId", characterId);
            command.Parameters.AddWithValue("@ipAddress", clientAddress);
            command.ExecuteNonQuery();
        }

        public static void UpdateCharacterPosition(uint characterId, float xPos, float yPos, string zone)
        {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "UPDATE `character_positions` set`xPos`=@xPos,`yPos`=@yPos,`zone`=@zone WHERE `characterId`=@characterId";
            command.Parameters.AddWithValue("@characterId", characterId);
            command.Parameters.AddWithValue("@xPos", xPos);
            command.Parameters.AddWithValue("@yPos", yPos);
            command.Parameters.AddWithValue("@zone", zone);
            command.ExecuteNonQuery();
        }

        public static CharacterPositionsWrapper GetCharacterPosition(uint characterId)
        {
            CharacterPositionsWrapper wrapper = new CharacterPositionsWrapper()
            {
                CharacterId = characterId
            };
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "SELECT * from character_positions where characterId=@characterId";
            command.Parameters.AddWithValue("@characterId", characterId);
            MySqlDataReader rdr = command.ExecuteReader();
            rdr.Read();
            if (rdr.HasRows)
            {
                if (rdr.GetUInt32(0) != characterId)
                {
                    throw new Exception("Weird case, found record but does not match");
                }
                wrapper.XPos = rdr.GetFloat(1);
                wrapper.YPos = rdr.GetFloat(2);
                wrapper.Zone = rdr.GetString(3);
                rdr.Close();

                return wrapper;
            }
            else
            {
                throw new Exception("Could not find character_positions record for character id: " + characterId);
            }
        }
    }
}