using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using MyToolkit.Collections;


namespace NuGetReferenceSwitcher.Presentation.Models
{
    public class SolutionModel
    {
        private readonly Solution _solution;

        /// <summary>Initializes a new instance of the <see cref="SolutionModel"/> class. </summary>
        /// <param name="solution">The native solution object.</param>
        public SolutionModel(Solution solution)
        {
            _solution = solution;
        }

        public string ConfigurationPath
        {
            get { return GetConfigurationPath(".nugetreferenceswitcher"); }
        }

        public List<string> SearchDirectories
        {
            get { return LoadSearchDirectoriesFromFile(ConfigurationPath); }
        }



        /// <summary>Save the serach directories to disk</summary>
        /// <param name="searchDirectories">The latest search directories.</param>
        public void SaveSearchDirectories(IEnumerable<string> searchDirectories)
        {
            File.WriteAllLines(ConfigurationPath, searchDirectories);
        }

        /// <summary>Build a map from package id to project path for all found projects in the serach directories</summary>
        /// <param name="searchDirectories">The directories to search.</param>
        public IDictionary<string, string> BuildPackageMap(IEnumerable<string> searchDirectories)
        {
            var map = new Dictionary<string, string>();
            foreach (var projectFile in searchDirectories
                    .Where(Directory.Exists)
                    .SelectMany(path => Directory.EnumerateFiles(path, "*.csproj", SearchOption.AllDirectories)))
            {
                // TODO: If we have a nuspec file at the same level, we should use the id from that.

                // Assume that the package id is equal to the project file name
                var packageId = Path.GetFileNameWithoutExtension(projectFile);
                if (!map.ContainsKey(packageId))
                {
                    map.Add(packageId, projectFile);
                }
            }
            return map;
        }

        private string GetConfigurationPath(string fileExtension)
        {
            return PathUtilities.GetFilePathWithAlternateExtension(_solution.FullName, fileExtension);
        }


        private List<string> LoadSearchDirectoriesFromFile(string configurationPath)
        {
            var list = new List<string>();
            if (File.Exists(configurationPath))
            {
                list.AddRange(File.ReadAllLines(configurationPath));
            }
            return list;
        }
    }    
}
