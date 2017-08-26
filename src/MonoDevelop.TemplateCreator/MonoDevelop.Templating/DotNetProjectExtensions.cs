﻿//
// DotNetProjectExtensions.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2017 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.Templating
{
	static class DotNetProjectExtensions
	{
		public static FilePath GetTemplateJsonFilePath (this DotNetProject project)
		{
			FilePath templateConfigDirectory = project.GetTemplateConfigDirectory ();
			return templateConfigDirectory.Combine ("template.json");
		}

		public static FilePath GetTemplateConfigDirectory (this DotNetProject project)
		{
			return project.BaseDirectory.Combine (".template.config");
		}

		public static bool HasTemplateJsonFile (this DotNetProject project)
		{
			FilePath fileName = project.GetTemplateJsonFilePath ();
			return File.Exists (fileName);
		}

		public static bool HasTemplateConfigDirectory (this DotNetProject project)
		{
			FilePath templateConfigDirectory = project.GetTemplateConfigDirectory ();
			return Directory.Exists (templateConfigDirectory);
		}

		public static string GetTemplateLanguageName (this DotNetProject project)
		{
			return project.LanguageName.Replace ("#", "Sharp");
		}

		public static bool TemplateConfigDirectoryExistsInProject (this DotNetProject project)
		{
			FilePath directory = project.GetTemplateConfigDirectory ();

			foreach (ProjectFile file in project.Files) {
				if (ExistsInDirectory (directory, file)) {
					return true;
				}
			}

			return false;
		}

		static bool ExistsInDirectory (string basePath, ProjectFile file)
		{
			StringComparison comparison = GetStringComparison ();
			return file.Name.StartsWith (basePath, comparison) &&
				(file.Name.Length == basePath.Length || file.Name [basePath.Length] == Path.DirectorySeparatorChar);
		}

		static StringComparison GetStringComparison ()
		{
			if (Platform.IsWindows || Platform.IsMac)
				return StringComparison.OrdinalIgnoreCase;

			return StringComparison.Ordinal;
		}
	}
}
