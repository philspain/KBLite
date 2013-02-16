using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Word;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using KBDocumentConverter.Models;
using HtmlAgilityPack;
using System.Text;
using KBDocumentConverter.DataAccess;

namespace KBDocumentConverter.Converters
{
    public class ConvertToHtml
    {
        // Directory for files converted from Word docs to Html.
        static readonly string _htmlKnowledgeBaseDir = 
            Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)),
            "KnowledgebaseFiles\\HTML");

        // Directory for Word files to be converted.
        static readonly string _docKnowledgeBaseDir =
            Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)),
            "KnowledgebaseFiles\\DOCS");

        // Directory for holding the Content List which represents the knowledgebase
        // directory and file structure. Will be used to create menu for user to browse
        // articles.
        static readonly string _htmlContentDir = Path.Combine(
            Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)),
            "KnowledgebaseFiles\\Content");

        // Path to upload files "Uploaded"
        static string strPathToUpload;

        // Path to convert uploaded files and save
        static string strPathToConvert;

        // For filtered HTML Output
        static object fltDocFormat = 10;

        // Placeholder values for parameters in Word object that will not
        // be used during conversions
        static object missing = System.Reflection.Missing.Value;

        static object readOnly = false;

        //The process has to be in invisible mode
        static object isVisible = false;

        // List of files that have already been converted to html
        static List<string> _htmlKnowledgebaseFiles = new List<string>();

        // List of files contained in the folder structure that represents
        // a knowledgebase; these files will be converted to html to be
        // displayed on a website.
        static List<string> _docKnowledgebaseFiles = new List<string>();

        static void ConvertDocToHTML(string fileName)
        {
            try
            {
                //To check the file extension if it is word document or something else
                string strFileName = Path.GetFileNameWithoutExtension(fileName);
                string strExt = Path.GetExtension(fileName);

                //Map-path to the folder where html to be saved
                strPathToConvert = Path.GetDirectoryName(fileName).Replace("KnowledgebaseFiles\\DOCS",
                                                                            "KnowledgebaseFiles\\HTML");

                object FileName = fileName.Clone();
                object FileToSave = strPathToConvert + "\\" + strFileName + ".htm";

                if (!File.Exists((string)FileToSave))
                { 
                    if (strExt.ToLower().Equals(".doc") || strExt.ToLower().Equals(".docx"))
                    {
                        Microsoft.Office.Interop.Word._Application objWord; objWord = new Microsoft.Office.Interop.Word.Application();

                        //Do the background activity
                        objWord.Visible = false;

                        //open the file internally in word. In the method all the parameters should be passed by object reference
                        Microsoft.Office.Interop.Word.Document oDoc = objWord.Documents.Open(ref FileName, ref readOnly, ref missing, 
                            ref missing, ref missing, ref missing, ref missing, ref  missing, ref missing, ref missing, 
                            ref isVisible, ref missing, ref missing, ref missing, ref missing, ref missing);

                        oDoc.Activate();

                        //Save to Html format
                        oDoc.SaveAs(ref FileToSave, ref fltDocFormat, ref missing, ref missing, ref missing, ref missing,
                        ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing,
                        ref missing, ref missing);

                        //Close/quit word
                        objWord.Quit();

                        //Add file to list of existing html files.
                        _htmlKnowledgebaseFiles.Add((string)FileToSave);

                        FixEncodingErrors((string)FileToSave);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                Logger.LogError(message);
            }
        }

        /// <summary>
        /// Method to fix error that manifest in rendered html
        /// due to Office's method of converting documents to 
        /// html.
        /// </summary>
        /// <param name="filePath">Path of file to fix.</param>
        static void FixEncodingErrors(string filePath)
        {
            bool isFixed = false;
            int attempts = 0;

            // Check file exist.
            if(File.Exists(filePath))
            {
                while (!isFixed && attempts <= 3)
                {
                    try
                    {
                        // Get directory file is contained in.
                        string dir = System.IO.Path.GetDirectoryName(filePath);
                        
                        // Read file contents
                        string content = File.ReadAllText(filePath, Encoding.GetEncoding(1252));

                        // Get relative directory so that images etc. display in rendered page.
                        string relativeDir = dir.Replace(dir.Substring(0, dir.IndexOf("KnowledgebaseFiles")), "../../../").Replace("\\", "/");
                        content = content.Replace("src=\"", "src=\"" + relativeDir + "/");

                        // Correct issues with headers.
                        content = content.Replace("h3\r\n\t{", "#article h3\r\n\t{");

                        // Write corrections.
                        using (FileStream stream = new FileStream(filePath, FileMode.Open))
                        {
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                writer.Write(content);
                            }
                        }

                        isFixed = true;
                    }
                    catch (IOException ioEX)
                    {
                        string message = "There has been an input/output error, check permissions for containing directory\n" + 
                            ioEX.Message + "\n" + ioEX.InnerException + "\n" + ioEX.Source + "\n" + ioEX.StackTrace;
                        Logger.LogError(message);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                        Logger.LogError(message);
                    }

                    attempts++;
                }
            }
        }

        // Check that knowledgebase folders exist.
        static void InitialiseFolders()
        {

            if (!Directory.Exists(_docKnowledgeBaseDir))
            {
                try
                {
                    Directory.CreateDirectory(_docKnowledgeBaseDir);
                }
                catch (IOException ioEX)
                {
                    string message = "There has been an input/output error, check permissions for containing directory\n" + 
                        ioEX.Message + "\n" + ioEX.InnerException + "\n" + ioEX.Source + "\n" + ioEX.StackTrace;
                    Logger.LogError(message);
                }
                catch (Exception ex)
                {
                    string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                    Logger.LogError(message);
                }
            }

            if (!Directory.Exists(_htmlKnowledgeBaseDir))
            {
                try
                {
                    Directory.CreateDirectory(_htmlKnowledgeBaseDir);
                }
                catch (IOException ioEX)
                {
                    string message = "There has been an input/output error, check permissions for containing directory\n" +
                        ioEX.Message + "\n" + ioEX.InnerException + "\n" + ioEX.Source + "\n" + ioEX.StackTrace;
                    Logger.LogError(message);
                }
                catch (Exception ex)
                {
                    string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                    Logger.LogError(message);
                }
            }

            if (!Directory.Exists(_htmlContentDir))
            {
                try
                {
                    Directory.CreateDirectory(_htmlContentDir);
                }
                catch (IOException ioEX)
                {
                    string message = "There has been an input/output error, check permissions for containing directory\n" +
                        ioEX.Message + "\n" + ioEX.InnerException + "\n" + ioEX.Source + "\n" + ioEX.StackTrace;
                    Logger.LogError(message);
                }
                catch (Exception ex)
                {
                    string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                    Logger.LogError(message);
                }
            }
        }

        //Initialise lists that contain files to be converted, and converted files.
        static void InitialiseFileCollections()
        {
            // Check if converted document's source still exists. If it does, add to convertedFileNames list,
            // otherwise delete it so that the "HTMLKnowledgeBase" structure is consistent with the 
            // "DOCKnowledgeBase" structure.
            try
            {
                // Get all HTML and HTM files.
                string[] convertedFiles = Directory.GetFiles(_htmlKnowledgeBaseDir, "*.*", SearchOption.AllDirectories)
                    .Where(file => file.ToLower().EndsWith("htm") || file.ToLower().EndsWith("html"))
                    .ToArray();

                // Delete converted file if source does not exist
                foreach (string file in convertedFiles)
                {
                    string docFileToCheck = file.Replace("KnowledgebaseFiles\\HTML", "KnowledgebaseFiles\\DOCS")
                        .Replace(".htm", ".doc");
                    string docxFileToCheck = file.Replace("KnowledgebaseFiles\\HTML", "KnowledgebaseFiles\\DOCS")
                        .Replace(".htm", ".docx");

                    if (!File.Exists(docFileToCheck) && !File.Exists(docxFileToCheck))
                    {
                        File.Delete(file);

                        // Delete folder that contains the content(images etc) for the html files.
                        if (Directory.Exists(Path.GetDirectoryName(file) + "\\" +
                            Path.GetFileNameWithoutExtension(file) + "_files"))
                        {
                            bool deleteFiles = true;
                            Directory.Delete(Path.GetDirectoryName(file) + "\\" +
                                Path.GetFileNameWithoutExtension(file) + "_files", deleteFiles);
                        }
                    }
                    else
                    {
                        _htmlKnowledgebaseFiles.Add(file);
                    }
                }
            }
            catch (UnauthorizedAccessException uaEx)
            {
                string message = @"There was an Unauthorized Access error; perhaps check read/write permissions or
                                   group policy settings for applications install folder"  
                    + "\n" + uaEx.Message + "\n" + uaEx.InnerException + "\n" + uaEx.Source + "\n" + uaEx.StackTrace;

                Logger.LogError(message);
            }
            catch (IOException ioEx)
            {
                string message = @"There was an input/output error; perhaps check read/write permissions or
                                   group policy settings for applications install folder"  
                    + "\n" + ioEx.Message + "\n" + ioEx.InnerException + "\n" + ioEx.Source + "\n" + ioEx.StackTrace;

                Logger.LogError(message);
            }
            catch (Exception ex)
            {
                string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;

                Logger.LogError(message);
            }

            // Get all files in the "DOCKnowledgeBase" folder.
            string[] filesToConvert = Directory.GetFiles(_docKnowledgeBaseDir, "*.*", SearchOption.AllDirectories)
                .Where(file => !file.Contains("~$") && (file.ToLower().EndsWith("doc") || file.ToLower().EndsWith("docx")))
                .ToArray();

            // Check each file to be converted, if there is no corresponding converted file, add
            // to list.
            foreach (string file in filesToConvert)
            {
                if (!_docKnowledgebaseFiles.Contains(file)) _docKnowledgebaseFiles.Add(file);
            }
        }

        // Traverse list of files to be converted and convert.
        static void ConvertFiles()
        {
            foreach (string file in _docKnowledgebaseFiles)
            {
                if (!File.Exists(file.Replace("KnowledgebaseFiles\\DOCS", "KnowledgebaseFiles\\HTML").Replace(".doc", ".htm")) &&
                    !File.Exists(file.Replace("KnowledgebaseFiles\\DOCS", "KnowledgebaseFiles\\HTML").Replace(".docx", ".htm")))
                {

                    if (!Directory.Exists(Path.GetDirectoryName(file).Replace("KnowledgebaseFiles\\DOCS", "KnowledgebaseFiles\\HTML")))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(file).Replace("KnowledgebaseFiles\\DOCS", "KnowledgebaseFiles\\HTML"));
                    }

                    ConvertDocToHTML(file);
                }
            }
        }

        /// <summary>
        /// Traverse the directory structure present in provided DirectoryModel and
        /// create HTML that will represent the directories and files that are found.
        /// </summary>
        /// <param name="directory">Directory containing subdirectories and articles.</param>
        /// <param name="listFileDoc">Object representing HTML document.</param>
        /// <param name="directoryDiv">Container representing div that will contain categories and articles.</param>
        /// <param name="isSourceDir">Bool representing whether directoryDiv, is parent directory of article directory structure.</param>
        static void GenerateHTML(DirectoryModel directory, HtmlDocument listFileDoc, HtmlNode directoryContainerDiv, bool isSourceDir, ref int index)
        {

            // Hash directory path to be used as unique identifier for
            // folder div's id attribute
            index++;
            string encryptedPath = EncryptStrings.EncryptToMD5String(directory.Path);

            // Create div that will hold folder icon and directory name
            HtmlNode directoryDiv = listFileDoc.CreateElement("div");

            // Container for categories (subdirectories) and article links.
            directoryDiv.SetAttributeValue("class", "directory");
            directoryDiv.SetAttributeValue("onclick", "expandFileList(this.id)");
            directoryDiv.SetAttributeValue("id", encryptedPath);

            // Check whether or not current directory, is the parent node
            // of article directory structure (as this is first container to be seen)
            string style = isSourceDir ? String.Format("z-index: {0};", index) : String.Format("z-index: -{0}; display: none", index);
            directoryDiv.SetAttributeValue("style", style);

            HtmlNode directoryHeader = listFileDoc.CreateElement("h3");
            directoryHeader.SetAttributeValue("class", "directory-headers");

            HtmlNode categoryHeaderText = listFileDoc.CreateTextNode("Categories");
            directoryHeader.AppendChild(categoryHeaderText);
            directoryDiv.AppendChild(directoryHeader);

            HtmlNode categoryRule = listFileDoc.CreateElement("hr");
            categoryRule.SetAttributeValue("class", "header-rule");
            directoryDiv.AppendChild(categoryRule);

            // Process subdirectories' contents and generate relevant
            // html.
            if (directory.Subdirectories.Count > 0)
            {
                foreach (DirectoryModel subdirectory in directory.Subdirectories)
                {
                    isSourceDir = false;

                    HtmlNode subDirectoryDiv = listFileDoc.CreateElement("div");
                    subDirectoryDiv.SetAttributeValue("class", "subdirectory");
                    subDirectoryDiv.SetAttributeValue("name", EncryptStrings.EncryptToMD5String(subdirectory.Path));
                    subDirectoryDiv.SetAttributeValue("onclick", "bringToFront(this)");

                    HtmlNode folderParagraph = listFileDoc.CreateElement("p");
                    folderParagraph.SetAttributeValue("class", "folder-name");

                    HtmlTextNode text = listFileDoc.CreateTextNode(Path.GetFileName(subdirectory.Path));
                    folderParagraph.AppendChild(text);

                    subDirectoryDiv.AppendChild(folderParagraph);

                    directoryDiv.AppendChild(subDirectoryDiv);

                    GenerateHTML(subdirectory, listFileDoc, directoryContainerDiv, isSourceDir, ref index);
                }
            }

            // Container for links to show articles.
            HtmlNode clearFloatDiv = listFileDoc.CreateElement("div");
            clearFloatDiv.SetAttributeValue("style", "clear: both; width: 100%;");
            directoryDiv.AppendChild(clearFloatDiv);

            HtmlNode articleHeader = listFileDoc.CreateElement("h3");
            articleHeader.SetAttributeValue("class", "directory-headers");

            HtmlNode articleHeaderText = listFileDoc.CreateTextNode("Articles");
            articleHeader.AppendChild(articleHeaderText);
            directoryDiv.AppendChild(articleHeader);

            HtmlNode articleRule = listFileDoc.CreateElement("hr");
            articleRule.SetAttributeValue("class", "header-rule");
            directoryDiv.AppendChild(articleRule);

            // Check that html files exist in directory and create representative
            // html links.
            if (directory.Files.Keys.Count > 0)
            {
                HtmlNode linkContainerNode = listFileDoc.CreateElement("ul");
                linkContainerNode.SetAttributeValue("class", "article-list");

                foreach (string key in directory.Files.Keys)
                {
                    HtmlNode listNode = listFileDoc.CreateElement("li");

                    HtmlNode linkNode = listFileDoc.CreateElement("a");

                    HtmlTextNode textNode = listFileDoc.CreateTextNode(directory.Files[key]);
                    linkNode.AppendChild(textNode);

                    string file = key;

                    linkNode.SetAttributeValue("id", EncryptStrings.EncryptToAESString(file));
                    linkNode.SetAttributeValue("href", "#");
                    linkNode.SetAttributeValue("onclick", "setContent(this.id)");

                    listNode.AppendChild(linkNode);

                    linkContainerNode.AppendChild(listNode);
                }

                directoryDiv.AppendChild(linkContainerNode);
            }

            directoryContainerDiv.AppendChild(directoryDiv);
        }

        /// <summary>
        /// Create and populate html file to represent the existing knowledgebase 
        /// file and directory structure.
        /// </summary>
        static void CreateContentList()
        {
            try
            {
                string listFilePath = String.Empty;

                // If no content list file exists, create it. if one does exist, create an alternative, this version 
                // will be pulled up by the site, and renamed to replace the orginal content list file. This is to ensure
                // there are no errors with the site and this class trying to open the same file.
                if (!File.Exists(_htmlContentDir + "\\content_list.htm"))
                {
                    FileStream fs = File.Create(_htmlContentDir + "\\content_list.htm");
                    listFilePath = _htmlContentDir + "\\content_list.htm";
                    fs.Close();
                }
                else
                {
                    FileStream fs = File.Create(_htmlContentDir + "\\content_list_new.htm");
                    listFilePath = _htmlContentDir + "\\content_list_new.htm";
                    fs.Close();
                }

                //Create html document
                HtmlDocument listFileDoc = new HtmlDocument();
                listFileDoc.Load(listFilePath);

                HtmlNode articleContainerDiv = listFileDoc.CreateElement("div");
                articleContainerDiv.SetAttributeValue("id", "article-container");
                articleContainerDiv.SetAttributeValue("class", "directory");
                articleContainerDiv.SetAttributeValue("style", "display: none");

                HtmlNode articleDiv = listFileDoc.CreateElement("div");
                articleDiv.SetAttributeValue("id", "article");

                articleContainerDiv.AppendChild(articleDiv);
                listFileDoc.DocumentNode.AppendChild(articleContainerDiv);

                // Create instance of DirectoryModel which will hold knowledgebase directory and file structure
                DirectoryModel rootDirectory = DirectoryModel.GenerateDirectoryStructure(_htmlKnowledgeBaseDir);

                // Number that is used to create a unique identifier for each
                // knowledgebase category's containing div; it is the div's
                // z-index value.
                int index = 0;

                // Whether or not the directory that is being process is
                // the top level directory for knowledgebase directory tree.
                bool isSourceDir = true;
                GenerateHTML(rootDirectory, listFileDoc, listFileDoc.DocumentNode, isSourceDir, ref index);

                listFileDoc.Save(listFilePath);
            }
            catch (Exception ex)
            {
                string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                Logger.LogError(message);
            }
        }

        public static void RunConversion()
        {
            try
            {
                InitialiseFolders();
                InitialiseFileCollections();
                ConvertFiles();
                CreateContentList();
            }
            catch (Exception ex)
            {
                string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                Logger.LogError(message);
            }
        }
    }
}