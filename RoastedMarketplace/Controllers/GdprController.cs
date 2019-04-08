﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoastedMarketplace.Data.Entity.Gdpr;
using RoastedMarketplace.Factories.Gdpr;
using RoastedMarketplace.Infrastructure.Mvc;
using RoastedMarketplace.Infrastructure.Routing;
using RoastedMarketplace.Infrastructure.Security.Attributes;
using RoastedMarketplace.Models.Gdpr;
using RoastedMarketplace.Services.Gdpr;

namespace RoastedMarketplace.Controllers
{
    [Authorize]
    [Route("gdpr")]
    public class GdprController : FoundationController
    {
        private readonly IGdprService _gdprService;
        private readonly IConsentService _consentService;
        private readonly IGdprModelFactory _gdprModelFactory;
        public GdprController(IGdprService gdprService, IConsentService consentService, IGdprModelFactory gdprModelFactory)
        {
            _gdprService = gdprService;
            _consentService = consentService;
            _gdprModelFactory = gdprModelFactory;
        }

        [DualGet("~/account/privacy", Name = RouteNames.AccountGdpr)]
        public IActionResult Consents()
        {
            var userConsents = _gdprService.GetUserConsents(CurrentUser.Id);
            var allConsents = _consentService.Get(x => x.Published).GroupBy(x => x.ConsentGroup).ToList();
            var consentGroups = new List<ConsentGroupModel>();
            var acceptedIds = userConsents.Where(x => x.ConsentStatus == ConsentStatus.Accepted)
                .Select(x => x.ConsentId).ToArray();
            var deniedIds = userConsents.Where(x => x.ConsentStatus == ConsentStatus.Denied)
                .Select(x => x.ConsentId).ToArray();
            foreach (var ac in allConsents)
            {
                var group = ac.Key ?? new ConsentGroup()
                {
                    Consents = ac.ToList()
                };
                consentGroups.Add(_gdprModelFactory.Create(group, acceptedIds, deniedIds));
            }

            return R.Success.With("consentGroups", consentGroups).Result;
        }

        [DualPost("save-consents", Name = RouteNames.SaveGdprPreferences, OnlyApi = true)]
        [RejectForImitator]
        public IActionResult SaveConsents(IList<ConsentModel> consents)
        {
            if (consents == null)
                return BadRequest();
            var consentDictionary = consents.Where(x => x.Id != 0).ToDictionary(x => x.Id, x => x.ConsentStatus);
            _gdprService.SetUserConsents(CurrentUser.Id, consentDictionary);
            return R.Success.Result;
        }

    }
}