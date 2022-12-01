using System.IO;
using static ApiClient.App.Models.AppContext;

namespace ApiClient.App.Controllers
{
	public static class Management
	{
		public static class Files
		{
			public static string SearchForPath(string fileName)
			{
				string CheckDir = Globals.Dir.Value as string;
				string filePath = @"C:\";
				do
				{
					filePath = CheckDir + @"\" + fileName;
					if (System.IO.File.Exists(filePath))
					{
						return (filePath);
					}
					else
					{
						CheckDir = Directory.GetParent(CheckDir).FullName;
					}
				} while (CheckDir.Contains(Globals.ProjectName.Value as string));
				return null;
			}
			public static Ambience.File Get(string fileName)
			{
				string fullPath = SearchForPath(fileName);
				return new Ambience.File()
				{
					Content = System.IO.File.ReadAllText(fullPath),
					Path = fullPath.Replace(fileName, ""),
					Name = fileName
				};
			}

		}
	}
}