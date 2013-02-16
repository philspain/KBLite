using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace KBDocumentConverter.Models
{
    class DirectoryModel
    {

        public DirectoryModel()
        {
            Subdirectories = new List<DirectoryModel>();
            Files = new Dictionary<string, string>();
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public List<DirectoryModel> Subdirectories { get; set; }
        public Dictionary<string, string> Files { get; set; }

        public static DirectoryModel GenerateDirectoryStructure(string path)
        {
            DirectoryModel directory = new DirectoryModel();
            directory.Path = path;
            directory.Name = System.IO.Path.GetDirectoryName(path);

            string[] files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => file.EndsWith(".htm") || file.EndsWith(".html"))
                .ToArray();

            foreach (string file in files)
            {
                if (!file.Contains("~$"))
                {
                    directory.Files.Add(file, System.IO.Path.GetFileNameWithoutExtension(file));
                }
            }

            string[] subdirectories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

            if (subdirectories.Count() > 0)
            {
                foreach (string subdirectory in subdirectories)
                {
                    if (Directory.GetFiles(directory.Path, "*", SearchOption.AllDirectories).Count() > 0)
                    {
                        DirectoryModel generatedDirectory = GenerateDirectoryStructure(subdirectory);
                        
                        if(!generatedDirectory.Path.EndsWith("_files"))
                            directory.Subdirectories.Add(generatedDirectory);
                    }
                }
            }

            return directory;
        } 
    }
}
