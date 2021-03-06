﻿using System.Collections.Generic;
using SSCMS.Plugins;

namespace SSCMS.Web.Controllers.Admin.Plugins
{
    public partial class ManageController
    {
        public class GetResult
        {
            public bool IsNightly { get; set; }
            public string Version { get; set; }
            public IEnumerable<IPackageMetadata> EnabledPackages { get; set; }
            public string PackageIds { get; set; }
            public IEnumerable<IPluginMetadata> EnabledPlugins { get; set; }
            public IEnumerable<string> PluginIds { get; set; }
        }
    }
}
