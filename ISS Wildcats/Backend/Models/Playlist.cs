﻿namespace ISS_Wildcats.Backend.Models;
using System;
using MySql.Data.MySqlClient;
public class Playlist
{
    public int PlaylistID { get; set; }
    public string Name { get; set; }
    public int[] SongIDs { get; set; }
    public string Creator { get; set; }

    private readonly string connectionString;

    public Playlist(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public Playlist(string connectionString, int playlistID)
    {
        this.connectionString = connectionString;
        LoadPlaylistFromDatabase(playlistID);
    }

    private void LoadPlaylistFromDatabase(int playlistID)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            var query = "SELECT * FROM Playlists WHERE PlaylistID = @PlaylistID";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@PlaylistID", playlistID);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        PlaylistID = Convert.ToInt32(reader["PlaylistID"]);
                        Name = reader["Name"].ToString();
                        // Assuming SongIDs are stored as a comma-separated string in the database
                        var songIDsString = reader["SongIDs"].ToString();
                        SongIDs = Array.ConvertAll(songIDsString.Split(','), int.Parse);
                        Creator = reader["Creator"].ToString();
                    }
                    else
                    {
                        throw new ArgumentException("Playlist not found in the database.");
                    }
                }
            }
        }
    }

    public void Add(int[] songIDs)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            var query = "INSERT INTO Playlists (Name, SongIDs, Creator) " +
                        "VALUES (@Name, @SongIDs, @Creator)";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Name", Name);
                // Convert SongIDs array to comma-separated string for database storage
                cmd.Parameters.AddWithValue("@SongIDs", string.Join(",", songIDs));
                cmd.Parameters.AddWithValue("@Creator", Creator);
                cmd.ExecuteNonQuery();
            }
        }
    }

    
    public static Playlist Get(int playlistID, string connectionString)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            var query = "SELECT * FROM Playlists WHERE PlaylistID = @PlaylistID";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@PlaylistID", playlistID);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Playlist(connectionString)
                        {
                            PlaylistID = Convert.ToInt32(reader["PlaylistID"]),
                            Name = reader["Name"].ToString(),
                            // Assuming SongIDs are stored as a comma-separated string in the database
                            SongIDs = Array.ConvertAll(reader["SongIDs"].ToString().Split(','), int.Parse),
                            Creator = reader["Creator"].ToString()
                        };
                    }
                    else
                    {
                        return null; // Playlist not found
                    }
                }
            }
        }
    }
    
    public void Delete()
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            var query = "DELETE FROM Playlists WHERE PlaylistID = @PlaylistID";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@PlaylistID", PlaylistID);
                cmd.ExecuteNonQuery();
            }
        }
    }
    
    public void Update()
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            var query = "UPDATE Playlists SET Name = @Name, SongIDs = @SongIDs, Creator = @Creator " +
                        "WHERE PlaylistID = @PlaylistID";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Name", Name);
                // Convert SongIDs array to comma-separated string for database storage
                cmd.Parameters.AddWithValue("@SongIDs", string.Join(",", SongIDs));
                cmd.Parameters.AddWithValue("@Creator", Creator);
                cmd.Parameters.AddWithValue("@PlaylistID", PlaylistID);
                cmd.ExecuteNonQuery();
            }
        }
    }
    
    
    public void AddSong(int songID)
    {
        // Check if the song is already in the playlist
        if (Array.IndexOf(SongIDs, songID) == -1)
        {
            // Add the new song ID to the SongIDs array
            int[] updatedSongIDs = new int[SongIDs.Length + 1];
            Array.Copy(SongIDs, updatedSongIDs, SongIDs.Length);
            updatedSongIDs[updatedSongIDs.Length - 1] = songID;
            SongIDs = updatedSongIDs;

            Update();
        }
        else
        {
            throw new ArgumentException("Song already exists in the playlist.");
        }
    }

    public void RemoveSong(int songID)
    {
        // Check if the song exists in the playlist
        int index = Array.IndexOf(SongIDs, songID);
        if (index != -1)
        {
            // Remove the song from the SongIDs array
            int[] updatedSongIDs = new int[SongIDs.Length - 1];
            Array.Copy(SongIDs, 0, updatedSongIDs, 0, index);
            Array.Copy(SongIDs, index + 1, updatedSongIDs, index, SongIDs.Length - index - 1);
            SongIDs = updatedSongIDs;

            Update();
        }
        else
        {
            throw new ArgumentException("Song does not exist in the playlist.");
        }
    }
    
    
}