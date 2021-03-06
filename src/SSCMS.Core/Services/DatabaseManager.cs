﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core;
using Dapper;
using Datory;
using SSCMS.Core.Utils;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Core.Services
{
    public partial class DatabaseManager : IDatabaseManager
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ICacheManager<object> _cacheManager;
        public IAccessTokenRepository AccessTokenRepository { get; }
        public IAdministratorRepository AdministratorRepository { get; }
        public IAdministratorsInRolesRepository AdministratorsInRolesRepository { get; }
        public IChannelGroupRepository ChannelGroupRepository { get; }
        public IChannelRepository ChannelRepository { get; }
        public IConfigRepository ConfigRepository { get; }
        public IContentCheckRepository ContentCheckRepository { get; }
        public IContentGroupRepository ContentGroupRepository { get; }
        public IContentRepository ContentRepository { get; }
        public IContentTagRepository ContentTagRepository { get; }
        public IDbCacheRepository DbCacheRepository { get; }
        public IErrorLogRepository ErrorLogRepository { get; }
        public ILibraryFileRepository LibraryFileRepository { get; }
        public ILibraryGroupRepository LibraryGroupRepository { get; }
        public ILibraryImageRepository LibraryImageRepository { get; }
        public ILibraryTextRepository LibraryTextRepository { get; }
        public ILibraryVideoRepository LibraryVideoRepository { get; }
        public ILogRepository LogRepository { get; }
        public IPermissionsInRolesRepository PermissionsInRolesRepository { get; }
        public IPluginConfigRepository PluginConfigRepository { get; }
        public IPluginRepository PluginRepository { get; }
        public IRelatedFieldItemRepository RelatedFieldItemRepository { get; }
        public IRelatedFieldRepository RelatedFieldRepository { get; }
        public IRoleRepository RoleRepository { get; }
        public ISiteLogRepository SiteLogRepository { get; }
        public ISitePermissionsRepository SitePermissionsRepository { get; }
        public ISiteRepository SiteRepository { get; }
        public ISpecialRepository SpecialRepository { get; }
        public ITableStyleRepository TableStyleRepository { get; }
        public ITemplateLogRepository TemplateLogRepository { get; }
        public ITemplateRepository TemplateRepository { get; }
        public IUserGroupRepository UserGroupRepository { get; }
        public IUserLogRepository UserLogRepository { get; }
        public IUserMenuRepository UserMenuRepository { get; }
        public IUserRepository UserRepository { get; }

        public DatabaseManager(ISettingsManager settingsManager, ICacheManager<object> cacheManager, IAccessTokenRepository accessTokenRepository, IAdministratorRepository administratorRepository, IAdministratorsInRolesRepository administratorsInRolesRepository, IChannelGroupRepository channelGroupRepository, IChannelRepository channelRepository, IConfigRepository configRepository, IContentCheckRepository contentCheckRepository, IContentGroupRepository contentGroupRepository, IContentRepository contentRepository, IContentTagRepository contentTagRepository, IDbCacheRepository dbCacheRepository, IErrorLogRepository errorLogRepository, ILibraryFileRepository libraryFileRepository, ILibraryGroupRepository libraryGroupRepository, ILibraryImageRepository libraryImageRepository, ILibraryTextRepository libraryTextRepository, ILibraryVideoRepository libraryVideoRepository, ILogRepository logRepository, IPermissionsInRolesRepository permissionsInRolesRepository, IPluginConfigRepository pluginConfigRepository, IPluginRepository pluginRepository, IRelatedFieldItemRepository relatedFieldItemRepository, IRelatedFieldRepository relatedFieldRepository, IRoleRepository roleRepository, ISiteLogRepository siteLogRepository, ISitePermissionsRepository sitePermissionsRepository, ISiteRepository siteRepository, ISpecialRepository specialRepository, ITableStyleRepository tableStyleRepository, ITemplateLogRepository templateLogRepository, ITemplateRepository templateRepository, IUserGroupRepository userGroupRepository, IUserLogRepository userLogRepository, IUserMenuRepository userMenuRepository, IUserRepository userRepository)
        {
            _settingsManager = settingsManager;
            _cacheManager = cacheManager;
            AccessTokenRepository = accessTokenRepository;
            AdministratorRepository = administratorRepository;
            AdministratorsInRolesRepository = administratorsInRolesRepository;
            ChannelGroupRepository = channelGroupRepository;
            ChannelRepository = channelRepository;
            ConfigRepository = configRepository;
            ContentCheckRepository = contentCheckRepository;
            ContentGroupRepository = contentGroupRepository;
            ContentRepository = contentRepository;
            ContentTagRepository = contentTagRepository;
            DbCacheRepository = dbCacheRepository;
            ErrorLogRepository = errorLogRepository;
            LibraryFileRepository = libraryFileRepository;
            LibraryGroupRepository = libraryGroupRepository;
            LibraryImageRepository = libraryImageRepository;
            LibraryTextRepository = libraryTextRepository;
            LibraryVideoRepository = libraryVideoRepository;
            LogRepository = logRepository;
            PermissionsInRolesRepository = permissionsInRolesRepository;
            PluginConfigRepository = pluginConfigRepository;
            PluginRepository = pluginRepository;
            RelatedFieldItemRepository = relatedFieldItemRepository;
            RelatedFieldRepository = relatedFieldRepository;
            RoleRepository = roleRepository;
            SiteLogRepository = siteLogRepository;
            SitePermissionsRepository = sitePermissionsRepository;
            SiteRepository = siteRepository;
            SpecialRepository = specialRepository;
            TableStyleRepository = tableStyleRepository;
            TemplateLogRepository = templateLogRepository;
            TemplateRepository = templateRepository;
            UserGroupRepository = userGroupRepository;
            UserLogRepository = userLogRepository;
            UserMenuRepository = userMenuRepository;
            UserRepository = userRepository;
        }

        public List<IRepository> GetAllRepositories()
        {
            var list = new List<IRepository>
            {
                AccessTokenRepository,
                AdministratorRepository,
                AdministratorsInRolesRepository,
                ChannelGroupRepository,
                ChannelRepository,
                ConfigRepository,
                ContentCheckRepository,
                ContentGroupRepository,
                ContentRepository,
                ContentTagRepository,
                DbCacheRepository,
                ErrorLogRepository,
                LibraryFileRepository,
                LibraryGroupRepository,
                LibraryImageRepository,
                LibraryTextRepository,
                LibraryVideoRepository,
                LogRepository,
                PermissionsInRolesRepository,
                PluginConfigRepository,
                PluginRepository,
                RelatedFieldItemRepository,
                RelatedFieldRepository,
                RoleRepository,
                SiteLogRepository,
                SitePermissionsRepository,
                SiteRepository,
                SpecialRepository,
                TableStyleRepository,
                TemplateLogRepository,
                TemplateRepository,
                UserGroupRepository,
                UserLogRepository,
                UserMenuRepository,
                UserRepository
            };

            return list;
        }

        public Database GetDatabase(string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _settingsManager.Database.ConnectionString;
            }

            return new Database(_settingsManager.Database.DatabaseType, connectionString);
        }

        private IDbConnection GetConnection(string connectionString = null)
        {
            var database = GetDatabase(connectionString);
            return database.GetConnection();
        }

        public async Task DeleteDbLogAsync()
        {
            if (_settingsManager.Database.DatabaseType == DatabaseType.MySql)
            {
                using var connection = _settingsManager.Database.GetConnection();
                await connection.ExecuteAsync("PURGE MASTER LOGS BEFORE DATE_SUB( NOW( ), INTERVAL 3 DAY)");
            }
            else if (_settingsManager.Database.DatabaseType == DatabaseType.SqlServer)
            {
                var databaseName = await _settingsManager.Database.GetDatabaseNamesAsync();

                using var connection = _settingsManager.Database.GetConnection();
                var versions = await connection.QueryFirstAsync<string>("SELECT SERVERPROPERTY('productversion')");

                var version = 8;
                var arr = versions.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 0)
                {
                    version = TranslateUtils.ToInt(arr[0], 8);
                }
                if (version < 10)
                {
                    await connection.ExecuteAsync($"BACKUP LOG [{databaseName}] WITH NO_LOG");
                }
                else
                {
                    await connection.ExecuteAsync($@"ALTER DATABASE [{databaseName}] SET RECOVERY SIMPLE;DBCC shrinkfile ([{databaseName}_log], 1); ALTER DATABASE [{databaseName}] SET RECOVERY FULL; ");
                }
            }
        }

        public int GetIntResult(string connectionString, string sqlString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _settingsManager.Database.ConnectionString;
            }

            int count;

            var database = new Database(_settingsManager.Database.DatabaseType, connectionString);
            using (var conn = database.GetConnection())
            {
                count = conn.ExecuteScalar<int>(sqlString);
                //conn.Open();
                //using (var rdr = ExecuteReader(conn, sqlString))
                //{
                //    if (rdr.Read())
                //    {
                //        count = GetInt(rdr, 0);
                //    }
                //    rdr.Close();
                //}
            }
            return count;
        }

        public int GetIntResult(string sqlString)
        {
            return GetIntResult(_settingsManager.Database.ConnectionString, sqlString);
        }

        public string GetString(string connectionString, string sqlString)
        {
            string value;

            using (var connection = GetConnection(connectionString))
            {
                value = connection.ExecuteScalar<string>(sqlString);
            }

            return value;
        }

        private string GetString(string sqlString)
        {
            string value;

            using (var connection = GetConnection())
            {
                value = connection.ExecuteScalar<string>(sqlString);
            }

            return value;
        }

        public IEnumerable<IDictionary<string, object>> GetRows(string connectionString, string sqlString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _settingsManager.Database.ConnectionString;
            }

            if (string.IsNullOrEmpty(sqlString)) return null;

            IEnumerable<IDictionary<string, object>> rows;

            using (var connection = GetConnection(connectionString))
            {
                rows = connection.Query(sqlString).Cast<IDictionary<string, object>>();
            }

            return rows;
        }

        public int GetPageTotalCount(string sqlString)
        {
            var temp = sqlString.ToLower();
            var pos = temp.LastIndexOf("order by", StringComparison.Ordinal);
            if (pos > -1)
                sqlString = sqlString.Substring(0, pos);

            // Add new ORDER BY info if SortKeyField is specified
            //if (!string.IsNullOrEmpty(sortField) && addCustomSortInfo)
            //    SelectCommand += " ORDER BY " + SortField;

            var cmdText = _settingsManager.Database.DatabaseType == DatabaseType.Oracle
                ? $"SELECT COUNT(*) FROM ({sqlString})"
                : $"SELECT COUNT(*) FROM ({sqlString}) AS T0";
            return GetIntResult(cmdText);
        }

        public string GetStlPageSqlString(string sqlString, string orderString, int totalCount, int itemsPerPage, int currentPageIndex)
        {
            var retVal = string.Empty;

            var temp = sqlString.ToLower();
            var pos = temp.LastIndexOf("order by", StringComparison.Ordinal);
            if (pos > -1)
                sqlString = sqlString.Substring(0, pos);

            var recordsInLastPage = itemsPerPage;

            // Calculate the correspondent number of pages
            var lastPage = totalCount / itemsPerPage;
            var remainder = totalCount % itemsPerPage;
            if (remainder > 0)
                lastPage++;
            var pageCount = lastPage;

            if (remainder > 0)
                recordsInLastPage = remainder;

            var recsToRetrieve = itemsPerPage;
            if (currentPageIndex == pageCount - 1)
                recsToRetrieve = recordsInLastPage;

            orderString = orderString.ToUpper();
            var orderStringReverse = orderString.Replace(" DESC", " DESC2");
            orderStringReverse = orderStringReverse.Replace(" ASC", " DESC");
            orderStringReverse = orderStringReverse.Replace(" DESC2", " ASC");

            if (_settingsManager.Database.DatabaseType == DatabaseType.MySql)
            {
                retVal = $@"
SELECT * FROM (
    SELECT * FROM (
        SELECT * FROM ({sqlString}) AS t0 {orderString} LIMIT {itemsPerPage * (currentPageIndex + 1)}
    ) AS t1 {orderStringReverse} LIMIT {recsToRetrieve}
) AS t2 {orderString}";
            }
            else if (_settingsManager.Database.DatabaseType == DatabaseType.SqlServer)
            {
                retVal = $@"
SELECT * FROM (
    SELECT TOP {recsToRetrieve} * FROM (
        SELECT TOP {itemsPerPage * (currentPageIndex + 1)} * FROM ({sqlString}) AS t0 {orderString}
    ) AS t1 {orderStringReverse}
) AS t2 {orderString}";
            }
            else if (_settingsManager.Database.DatabaseType == DatabaseType.PostgreSql)
            {
                retVal = $@"
SELECT * FROM (
    SELECT * FROM (
        SELECT * FROM ({sqlString}) AS t0 {orderString} LIMIT {itemsPerPage * (currentPageIndex + 1)}
    ) AS t1 {orderStringReverse} LIMIT {recsToRetrieve}
) AS t2 {orderString}";
            }
            else if (_settingsManager.Database.DatabaseType == DatabaseType.Oracle)
            {
                retVal = $@"
SELECT * FROM (
    SELECT * FROM (
        SELECT * FROM ({sqlString}) WHERE ROWNUM <= {itemsPerPage * (currentPageIndex + 1)} {orderString}
    ) WHERE ROWNUM <= {recsToRetrieve} {orderStringReverse}
) {orderString}";
            }

            //            if (WebConfigUtils.DatabaseType == DatabaseType.MySql)
            //            {
            //                return $@"
            //SELECT * FROM (
            //    SELECT * FROM (
            //        SELECT * FROM ({sqlString}) AS t0 {orderString} LIMIT {itemsPerPage * (currentPageIndex + 1)}
            //    ) AS t1 {orderStringReverse} LIMIT {recsToRetrieve}
            //) AS t2 {orderString}";
            //            }
            //            else
            //            {
            //                return $@"
            //SELECT * FROM (
            //    SELECT TOP {recsToRetrieve} * FROM (
            //        SELECT TOP {itemsPerPage * (currentPageIndex + 1)} * FROM ({sqlString}) AS t0 {orderString}
            //    ) AS t1 {orderStringReverse}
            //) AS t2 {orderString}";
            //            }

            return retVal;
        }

        public string GetSelectSqlString(string tableName, int totalNum, string columns, string whereString, string orderByString)
        {
            return GetSelectSqlString(_settingsManager.Database.ConnectionString, tableName, totalNum, columns, whereString, orderByString);
        }

        private string GetSelectSqlString(string connectionString, string tableName, int totalNum, string columns, string whereString, string orderByString)
        {
            return GetSelectSqlString(connectionString, tableName, totalNum, columns, whereString, orderByString, string.Empty);
        }

        private string GetSelectSqlString(string connectionString, string tableName, int totalNum, string columns, string whereString, string orderByString, string joinString)
        {
            if (!string.IsNullOrEmpty(whereString))
            {
                whereString = StringUtils.ReplaceStartsWith(whereString.Trim(), "AND", string.Empty);
                if (!StringUtils.StartsWithIgnoreCase(whereString, "WHERE "))
                {
                    whereString = "WHERE " + whereString;
                }
            }

            if (!string.IsNullOrEmpty(joinString))
            {
                whereString = joinString + " " + whereString;
            }

            return SqlUtils.ToTopSqlString(_settingsManager.Database.DatabaseType, tableName, columns, whereString, orderByString, totalNum);
        }

        public int GetCount(string tableName)
        {
            int count;

            using (var conn = _settingsManager.Database.GetConnection())
            {
                count = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM {SqlUtils.GetQuotedIdentifier(_settingsManager.Database.DatabaseType, tableName)}");
            }
            return count;

            //return GetIntResult();
        }

        public IEnumerable<dynamic> GetObjects(string tableName)
        {
            IEnumerable<dynamic> objects;
            var sqlString = $"select * from {tableName}";

            using (var connection = _settingsManager.Database.GetConnection())
            {
                objects = connection.Query(sqlString, null, null, false).ToList();
            }

            return objects;
        }

        public IEnumerable<dynamic> GetPageObjects(string tableName, string identityColumnName, int offset, int limit)
        {
            IEnumerable<dynamic> objects;
            var sqlString = GetPageSqlString(tableName, "*", string.Empty, $"ORDER BY {identityColumnName} ASC", offset, limit);

            using (var connection = _settingsManager.Database.GetConnection())
            {
                objects = connection.Query(sqlString, null, null, false).ToList();
            }

            return objects;
        }

        private decimal? _sqlServerVersion;

        private decimal SqlServerVersion
        {
            get
            {
                if (_settingsManager.Database.DatabaseType != DatabaseType.SqlServer)
                {
                    return 0;
                }

                if (_sqlServerVersion == null)
                {
                    try
                    {
                        _sqlServerVersion =
                            TranslateUtils.ToDecimal(
                                GetString("select left(cast(serverproperty('productversion') as varchar), 4)"));
                    }
                    catch
                    {
                        _sqlServerVersion = 0;
                    }
                }

                return _sqlServerVersion.Value;
            }
        }

        private bool IsSqlServer2012 => SqlServerVersion >= 11;

        public string GetPageSqlString(string tableName, string columnNames, string whereSqlString, string orderSqlString, int offset, int limit)
        {
            var retVal = string.Empty;

            if (string.IsNullOrEmpty(orderSqlString))
            {
                orderSqlString = "ORDER BY Id DESC";
            }

            if (offset == 0 && limit == 0)
            {
                return $@"SELECT {columnNames} FROM {tableName} {whereSqlString} {orderSqlString}";
            }

            if (_settingsManager.Database.DatabaseType == DatabaseType.MySql)
            {
                if (limit == 0)
                {
                    limit = int.MaxValue;
                }
                retVal = $@"SELECT {columnNames} FROM {tableName} {whereSqlString} {orderSqlString} LIMIT {limit} OFFSET {offset}";
            }
            else if (_settingsManager.Database.DatabaseType == DatabaseType.SqlServer && IsSqlServer2012)
            {
                retVal = limit == 0
                    ? $"SELECT {columnNames} FROM {tableName} {whereSqlString} {orderSqlString} OFFSET {offset} ROWS"
                    : $"SELECT {columnNames} FROM {tableName} {whereSqlString} {orderSqlString} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
            }
            else if (_settingsManager.Database.DatabaseType == DatabaseType.SqlServer && !IsSqlServer2012)
            {
                if (offset == 0)
                {
                    retVal = $"SELECT TOP {limit} {columnNames} FROM {tableName} {whereSqlString} {orderSqlString}";
                }
                else
                {
                    var rowWhere = limit == 0
                        ? $@"WHERE [row_num] > {offset}"
                        : $@"WHERE [row_num] BETWEEN {offset + 1} AND {offset + limit}";

                    retVal = $@"SELECT * FROM (
    SELECT {columnNames}, ROW_NUMBER() OVER ({orderSqlString}) AS [row_num] FROM [{tableName}] {whereSqlString}
) as T {rowWhere}";
                }
            }
            else if (_settingsManager.Database.DatabaseType == DatabaseType.PostgreSql)
            {
                retVal = limit == 0
                    ? $@"SELECT {columnNames} FROM {tableName} {whereSqlString} {orderSqlString} OFFSET {offset}"
                    : $@"SELECT {columnNames} FROM {tableName} {whereSqlString} {orderSqlString} LIMIT {limit} OFFSET {offset}";
            }
            else if (_settingsManager.Database.DatabaseType == DatabaseType.Oracle)
            {
                retVal = limit == 0
                    ? $"SELECT {columnNames} FROM {tableName} {whereSqlString} {orderSqlString} OFFSET {offset} ROWS"
                    : $"SELECT {columnNames} FROM {tableName} {whereSqlString} {orderSqlString} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
            }

            return retVal;
        }
    }
}

