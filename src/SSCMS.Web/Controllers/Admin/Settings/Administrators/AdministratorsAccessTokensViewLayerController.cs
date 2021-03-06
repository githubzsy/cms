﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Dto;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Settings.Administrators
{
    [OpenApiIgnore]
    [Authorize(Roles = Constants.RoleTypeAdministrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class AdministratorsAccessTokensViewLayerController : ControllerBase
    {
        private const string Route = "settings/administratorsAccessTokensViewLayer";
        private const string RouteRegenerate = "settings/administratorsAccessTokensViewLayer/actions/regenerate";

        private readonly ISettingsManager _settingsManager;
        private readonly IAuthManager _authManager;
        private readonly IAccessTokenRepository _accessTokenRepository;

        public AdministratorsAccessTokensViewLayerController(ISettingsManager settingsManager, IAuthManager authManager, IAccessTokenRepository accessTokenRepository)
        {
            _settingsManager = settingsManager;
            _authManager = authManager;
            _accessTokenRepository = accessTokenRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery]int id)
        {
            if (!await _authManager.HasSystemPermissionsAsync(Constants.AppPermissions.SettingsAdministratorsAccessTokens))
            {
                return Unauthorized();
            }

            var tokenInfo = await _accessTokenRepository.GetAsync(id);
            var accessToken = _settingsManager.Decrypt(tokenInfo.Token);

            return new GetResult
            {
                Token = tokenInfo,
                AccessToken = accessToken
            };
        }

        [HttpPost, Route(RouteRegenerate)]
        public async Task<ActionResult<RegenerateResult>> Regenerate([FromBody]IdRequest request)
        {
            if (!await _authManager.HasSystemPermissionsAsync(Constants.AppPermissions.SettingsAdministratorsAccessTokens))
            {
                return Unauthorized();
            }

            var accessTokenInfo = await _accessTokenRepository.GetAsync(request.Id);

            var accessToken = _settingsManager.Decrypt(await _accessTokenRepository.RegenerateAsync(accessTokenInfo));

            return new RegenerateResult
            {
                AccessToken = accessToken
            };
        }
    }
}
