using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;


namespace GitHub.Unity
{
	class Utility
	{
		public static string GitRoot { get; protected set; }
		public static string UnityDataPath { get; protected set; }


		[InitializeOnLoadMethod]
		static void Prepare()
		{
			GitRoot = FindRoot(UnityDataPath = Application.dataPath);
		}


		// TODO: replace with libgit2sharp call
		static string FindRoot(string path)
		{
			if (string.IsNullOrEmpty(Path.GetDirectoryName(path)))
			{
				return UnityDataPath;
			}

			if (Directory.Exists(Path.Combine(path, ".git")))
			{
				return path;
			}

			return FindRoot(Directory.GetParent(path).FullName);
		}


		public static string RepositoryPathToAbsolute(string repositoryPath)
		{
			return Path.Combine(Utility.GitRoot, repositoryPath);
		}


		public static string RepositoryPathToAsset(string repositoryPath)
		{
			string localDataPath = UnityDataPath.Substring(GitRoot.Length + 1);
			return (repositoryPath.IndexOf (localDataPath) == 0) ?
				("Assets" + repositoryPath.Substring(localDataPath.Length)).Replace(Path.DirectorySeparatorChar, '/') :
				null;
		}


		public static string AssetPathToRepository(string assetPath)
		{
			string localDataPath = UnityDataPath.Substring(GitRoot.Length + 1);
			return Path.Combine(localDataPath.Substring(0, localDataPath.Length - "Assets".Length), assetPath.Replace('/', Path.DirectorySeparatorChar));
		}


		public static void ParseLines(StringBuilder buffer, Action<string> lineParser, bool parseAll)
		{
			int end = buffer.Length - 1;

			if(!parseAll)
			// Try to avoid partial lines unless asked not to
			{
				for(; end > 0 && buffer[end] != '\n'; --end);
			}

			if(end > 0)
			// Parse lines if we have any buffer to parse
			{
				for(int index = 0, last = -1; index <= end; ++index)
				{
					if(buffer[index] == '\n')
					{
						int start = last + 1;
						// TODO: Figure out how we get out of doing that ToString call
						string line = buffer.ToString(start, index - start);
						lineParser(line);
						last = index;
					}
				}

				buffer.Remove(0, end + 1);
			}
		}
	}
}