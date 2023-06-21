using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gherkin;
using Gherkin.Ast;

namespace PlaylistGenerator
{
    public static class PlaylistGenerator
    {
        public static void GenerateTestPlaylist(string stepText, string featureFolderPath, string playlistFilePath)
        {
            // Get all feature files in the specified folder
            var featureFiles = Directory.GetFiles(featureFolderPath, "*.feature", SearchOption.AllDirectories);

            // Initialize a list to store the scenario names
            var scenarioNames = new List<string>();

            foreach (var featureFile in featureFiles)
            {
                // Parse the feature file
                var feature = ParseFeatureFile(featureFile);

                if(feature == null)
                {
                    continue;
                }

                // Get all scenarios that contain the specified step text
                var scenariosWithStep = feature.Children.OfType<Scenario>()
                    .Where(scenario => scenario.Steps.Any(step => step.Text.Contains(stepText)))
                    .ToList();

                // Add the scenario names to the list
                foreach (var scenario in scenariosWithStep)
                {
                    // Get the folder structure relative to the feature folder
                    var folderPath = Path.GetDirectoryName(featureFile);
                    var relativePath = GetRelativePath(folderPath, featureFolderPath);

                    // Prepend the folder structure with dots and sanitize the scenario name
                    var fullyQualifiedName = $"{relativePath}.{feature.Name}Feature.{SanitizeScenarioName(scenario.Name)}";
                    scenarioNames.Add(fullyQualifiedName);
                }
            }

            // Generate the playlist file
            GeneratePlaylistFile(playlistFilePath, scenarioNames);
        }

        private static Feature? ParseFeatureFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                var parser = new Parser();
                var gherkinDocument = parser.Parse(reader);
                var feature = gherkinDocument.Feature;
                return feature;
            }
        }

        private static void GeneratePlaylistFile(string filePath, IEnumerable<string> scenarioNames)
        {
            try
            {
                // Create the root element for the playlist
                var playlist = new XElement("Playlist", new XAttribute("Version", "2.0"));

                // Create a rule element for matching any scenario
                var rule = new XElement("Rule", new XAttribute("Match", "Any"));

                // Create a property element for each scenario name
                foreach (var scenarioName in scenarioNames)
                {
                    var property = new XElement("Property", new XAttribute("Name", "TestWithNormalizedFullyQualifiedName"), new XAttribute("Value", scenarioName));
                    rule.Add(property);
                }

                // Add the rule to the playlist
                playlist.Add(rule);

                // Create the playlist document
                var playlistDocument = new XDocument(new XDeclaration("1.0", "utf-8", null), playlist);

                // Save the playlist file
                playlistDocument.Save(filePath);

                Console.WriteLine($"Test playlist generated successfully at: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating test playlist: {ex.Message}");
            }
        }

        private static string SanitizeScenarioName(string scenarioName)
        {
            // Remove whitespace and punctuation from the scenario name
            var sanitizedName = Regex.Replace(scenarioName, @"\s+|[^a-zA-Z0-9]+", "");
            return sanitizedName;
        }

        private static string SanitizeNamespace(string name)
        {
            name = name.Replace('/', '.');
            // Remove whitespace from namespace
            var sanitizedName = Regex.Replace(name, @"\s", "");
            return sanitizedName;
        }

        private static string GetRelativePath(string fullPath, string basePath)
        {
            var baseUri = new Uri(basePath);
            var fullUri = new Uri(fullPath);
            var relativeUri = baseUri.MakeRelativeUri(fullUri);
            return Uri.UnescapeDataString(relativeUri.ToString());
        }
    }
}
