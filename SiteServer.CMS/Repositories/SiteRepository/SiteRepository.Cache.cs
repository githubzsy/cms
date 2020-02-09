using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiteServer.Abstractions;
using SiteServer.CMS.Context;
using SiteServer.CMS.Dto;
using SiteServer.CMS.Plugin;

namespace SiteServer.CMS.Repositories
{
    public partial class SiteRepository
    {
        public async Task<List<Site>> GetSiteListAsync()
        {
            var sites = new List<Site>();

            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                sites.Add(await GetAsync(summary.Id));
            }
            return sites;
        }

        private async Task<IEnumerable<SiteSummary>> GetCacheListAsync(int parentId)
        {
            var summaries = await GetSummariesAsync();
            return summaries.Where(x => x.ParentId == parentId);
        }

        private async Task<SiteSummary> GetCacheAsync(int siteId)
        {
            var summaries = await GetSummariesAsync();
            return summaries.FirstOrDefault(x => x.Id == siteId);
        }

        public async Task<int> GetParentSiteIdAsync(int siteId)
        {
            var parentSiteId = 0;
            var site = await GetAsync(siteId);
            if (site != null && site.Root == false)
            {
                parentSiteId = site.ParentId;
                if (parentSiteId == 0)
                {
                    parentSiteId = await GetIdByIsRootAsync();
                }
            }
            return parentSiteId;
        }

        public async Task<int> GetSiteLevelAsync(int siteId)
        {
            var level = 0;
            var site = await GetAsync(siteId);
            if (site.ParentId != 0)
            {
                level++;
                level += await GetSiteLevelAsync(site.ParentId);
            }
            return level;
        }

        public async Task<string> GetSiteNameAsync(Site site)
        {
            var padding = string.Empty;

            var level = await GetSiteLevelAsync(site.Id);
            string psLogo;
            if (site.Root)
            {
                psLogo = "siteHQ.gif";
            }
            else
            {
                psLogo = "site.gif";
                if (level > 0 && level < 10)
                {
                    psLogo = $"subsite{level + 1}.gif";
                }
            }
            psLogo = SiteServerAssets.GetIconUrl("tree/" + psLogo);

            for (var i = 0; i < level; i++)
            {
                padding += "��";
            }
            if (level > 0)
            {
                padding += "�� ";
            }

            return $"{padding}<img align='absbottom' border='0' src='{psLogo}'/>&nbsp;{site.SiteName}";
        }

        public async Task<string> GetTableNameAsync(int siteId)
        {
            var site = await GetAsync(siteId);
            return site?.TableName;
        }

        public async Task<Site> GetSiteBySiteNameAsync(string siteName)
        {
            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                if (StringUtils.EqualsIgnoreCase(summary.SiteName, siteName))
                {
                    return await GetAsync(summary.Id);
                }
            }
            return null;
        }

        public async Task<Site> GetSiteByIsRootAsync()
        {
            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                if (summary.Root)
                {
                    return await GetAsync(summary.Id);
                }
            }
            return null;
        }

        public async Task<bool> IsRootExistsAsync()
        {
            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                if (summary.Root)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<Site> GetSiteByDirectoryAsync(string siteDir)
        {
            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                if (StringUtils.EqualsIgnoreCase(summary.SiteDir, siteDir))
                {
                    return await GetAsync(summary.Id);
                }
            }
            return null;
        }

        public async Task<List<Cascade<int>>> GetSiteOptionsAsync(int parentId)
        {
            var optionList = new List<Cascade<int>>();
            var cacheList = await GetCacheListAsync(parentId);

            foreach (var cache in cacheList)
            {
                optionList.Add(new Cascade<int>
                {
                    Value = cache.Id,
                    Label = cache.SiteName,
                    Children = await GetSiteOptionsAsync(cache.Id)
                });
            }

            return optionList;
        }

        public async Task<IEnumerable<int>> GetSiteIdListAsync()
        {
            var summaries = await GetSummariesAsync();
            return summaries.Select(x => x.Id);
        }

        public async Task<List<int>> GetSiteIdListOrderByLevelAsync()
        {
            var retVal = new List<int>();

            var siteIdList = await GetSiteIdListAsync();
            var siteList = new List<Site>();
            var parentWithChildren = new Hashtable();
            var hqSiteId = 0;
            foreach (var siteId in siteIdList)
            {
                var site = await GetAsync(siteId);
                if (site.Root)
                {
                    hqSiteId = site.Id;
                }
                else
                {
                    if (site.ParentId == 0)
                    {
                        siteList.Add(site);
                    }
                    else
                    {
                        var children = new List<Site>();
                        if (parentWithChildren.Contains(site.ParentId))
                        {
                            children = (List<Site>)parentWithChildren[site.ParentId];
                        }
                        children.Add(site);
                        parentWithChildren[site.ParentId] = children;
                    }
                }
            }

            if (hqSiteId > 0)
            {
                retVal.Add(hqSiteId);
            }

            var list = siteList.OrderBy(site => site.Taxis == 0 ? int.MaxValue : site.Taxis).ToList();

            foreach (var site in list)
            {
                AddSiteIdList(retVal, site, parentWithChildren, 0);
            }
            return retVal;
        }

        private void AddSiteIdList(List<int> dataSource, Site site, Hashtable parentWithChildren, int level)
        {
            dataSource.Add(site.Id);

            if (parentWithChildren[site.Id] != null)
            {
                var children = (List<Site>)parentWithChildren[site.Id];
                level++;

                var list = children.OrderBy(child => child.Taxis == 0 ? int.MaxValue : child.Taxis).ToList();

                foreach (var subSite in list)
                {
                    AddSiteIdList(dataSource, subSite, parentWithChildren, level);
                }
            }
        }

        public async Task GetAllParentSiteIdListAsync(List<int> parentSiteIds, IEnumerable<int> siteIdCollection, int siteId)
        {
            var site = await GetAsync(siteId);
            var parentSiteId = -1;
            foreach (var psId in siteIdCollection)
            {
                if (psId != site.ParentId) continue;
                parentSiteId = psId;
                break;
            }
            if (parentSiteId == -1) return;

            parentSiteIds.Add(parentSiteId);
            await GetAllParentSiteIdListAsync(parentSiteIds, siteIdCollection, parentSiteId);
        }

        public async Task<List<int>> GetSiteIdListAsync(int parentId)
        {
            var siteIdList = new List<int>();

            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                if (summary.ParentId == parentId)
                {
                    siteIdList.Add(summary.Id);
                }
            }
            return siteIdList;
        }

        public async Task<List<string>> GetSiteTableNamesAsync()
        {
            return await GetTableNameListAsync(true, false);
        }

        public async Task<List<string>> GetAllTableNameListAsync()
        {
            return await GetTableNameListAsync(true, true);
        }

        private async Task<List<string>> GetTableNameListAsync(bool includeSiteTables, bool includePluginTables)
        {

            var tableNames = new List<string>();

            if (includeSiteTables)
            {
                var summaries = await GetSummariesAsync();
                foreach (var summary in summaries)
                {
                    if (!StringUtils.ContainsIgnoreCase(tableNames, summary.TableName))
                    {
                        tableNames.Add(summary.TableName);
                    }
                }
            }

            if (includePluginTables)
            {
                var pluginTableNames = await PluginContentManager.GetContentTableNameListAsync();
                foreach (var pluginTableName in pluginTableNames)
                {
                    if (!StringUtils.ContainsIgnoreCase(tableNames, pluginTableName))
                    {
                        tableNames.Add(pluginTableName);
                    }
                }
            }

            return tableNames;
        }

        public async Task<List<string>> GetTableNameListAsync(Site site)
        {
            var tableNames = new List<string> { site.TableName };
            var pluginTableNames = await PluginContentManager.GetContentTableNameListAsync();
            foreach (var pluginTableName in pluginTableNames)
            {
                if (!StringUtils.ContainsIgnoreCase(tableNames, pluginTableName))
                {
                    tableNames.Add(pluginTableName);
                }
            }
            return tableNames;
        }

        public async Task<int> GetIdByIsRootAsync()
        {
            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                if (summary.Root)
                {
                    return summary.Id;
                }
            }

            return 0;
        }

        private async Task<int> GetMaxTaxisAsync()
        {
            var summaries = await GetSummariesAsync();
            return summaries.Select(x => x.Taxis).DefaultIfEmpty().Max();
        }

        public async Task<IList<string>> GetSiteDirListAsync(int parentId)
        {
            var siteDirList = new List<string>();

            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                if (summary.ParentId == parentId && !summary.Root)
                {
                    siteDirList.Add(summary.SiteDir);
                }
            }

            return siteDirList;
        }

        public async Task<List<Select<int>>> GetSelectsAsync(List<int> includedSiteIds = null)
        {
            var selects = new List<Select<int>>();

            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                if (includedSiteIds != null && !includedSiteIds.Contains(summary.Id))
                {
                    continue;
                }
                selects.Add(new Select<int>(summary.Id, summary.SiteName));
            }

            return selects;
        }

        /// <summary>
        /// �õ�����ϵͳ�ļ��е��б�����Сд��ʾ��
        /// </summary>
        public async Task<IEnumerable<string>> GetLowerSiteDirListAsync(int parentId)
        {
            var list = await GetCacheListAsync(parentId);
            return list.Select(x => StringUtils.ToLower(x.SiteDir));
        }

        public async Task<int> GetIdBySiteDirAsync(string siteDir)
        {
            var summaries = await GetSummariesAsync();
            foreach (var summary in summaries)
            {
                if (StringUtils.EqualsIgnoreCase(summary.SiteDir, siteDir))
                {
                    return summary.Id;
                }
            }

            return 0;
        }
    }
}