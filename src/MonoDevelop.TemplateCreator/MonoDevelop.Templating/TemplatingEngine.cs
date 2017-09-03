﻿//
// TemplatingEngine.cs
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Edge.Settings;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.Utils;
using System.Threading.Tasks;
using MonoDevelop.Ide.Projects;

namespace MonoDevelop.Templating
{
	class TemplatingEngine
	{
		TemplateCreator templateCreator;
		CustomTemplateEngineHost host;
		EngineEnvironmentSettings settings;
		Paths paths;
		bool loaded;
		List<CustomSolutionTemplate> templates = new List<CustomSolutionTemplate> ();

		void Initialize ()
		{
			host = new CustomTemplateEngineHost ();
			settings = new EngineEnvironmentSettings (host, s => new SettingsLoader (s));
			paths = new Paths (settings);
			templateCreator = new TemplateCreator (settings);

			ResetCache ();
		}

		void ResetCache ()
		{
			if (Directory.Exists (paths.User.BaseDir)) {
				paths.DeleteDirectory (paths.User.BaseDir);
			}
		}

		void EnsureInitialized ()
		{
			if (host == null) {
				Initialize ();
			}
		}

		public void LoadTemplates ()
		{
			if (loaded || !TemplatingServices.Options.TemplateFolders.Any ())
				return;

			EnsureInitialized ();

			var settingsLoader = (SettingsLoader)settings.SettingsLoader;

			foreach (string folder in TemplatingServices.Options.TemplateFolders) {
				if (Directory.Exists (folder)) {
					settingsLoader.UserTemplateCache.Scan (folder);
				}
			}

			// If there are no templates found then do not save the settings. This
			// avoids a null reference exception in the settingsLoader.Save method.
			if (!settingsLoader.MountPoints.Any ()) {
				loaded = true;
				return;
			}

			settingsLoader.Save ();
			paths.WriteAllText (paths.User.FirstRunCookie, string.Empty);

			foreach (TemplateInfo templateInfo in settingsLoader.UserTemplateCache.TemplateInfo) {
				var template = new CustomSolutionTemplate (templateInfo);
				templates.Add (template);
			}

			loaded = true;
		}

		public IEnumerable<CustomSolutionTemplate> Templates {
			get { return templates; }
		}

		public Task<TemplateCreationResult> Create (
			CustomSolutionTemplate template,
			NewProjectConfiguration config,
			IReadOnlyDictionary<string, string> parameters)
		{
			return templateCreator.InstantiateAsync (
				template.Info,
				config.ProjectName,
				config.GetValidProjectName (),
				config.ProjectLocation,
				parameters,
				true,
				false,
				null
			);
		}
	}
}
