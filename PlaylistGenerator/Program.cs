namespace PlaylistGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Specify the step text and folder path of your feature files
            string stepText = "your_step_text_here";
            string featureFolderPath = "your_feature_folder_path_here";

            // Specify the path for the playlist file
            string playlistFilePath = "your_playlist_file_path_here";

            // Generate the test playlist
            PlaylistGenerator.GenerateTestPlaylist(stepText, featureFolderPath, playlistFilePath);
        }
    }
}