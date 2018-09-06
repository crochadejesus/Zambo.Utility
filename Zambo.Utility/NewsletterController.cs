using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using WS.WSI.Site.Models;
using WSAsset.Api.Client;

namespace WS.WSI.Site.Controllers
{
    public class NewsletterController : BaseController
    {
        // GET: Newsletter
        public ActionResult Subscribe()
        {
            var districts = RealEstateClient.GetDistricts().OrderBy(x => x.Description).ToList();
            var model = new NewsletterViewModel();
            model.District = AutoMapper.Mapper.Map<List<SelectListItem>>(districts);
            return PartialView("_SubscriptionForm", model);
        }

        // POST: Newsletter/Subscribe
        [HttpPost]
        public ActionResult Subscribe(NewsletterModel newsletterViewModel)
        {
            bool result = false;
            string resultMessage = "";

            if (IsValidEmail(newsletterViewModel.Email))
            {
                try
                {
                    var client = new NewsletterClient();
                    result = client.AddNewsletterSubscription(newsletterViewModel.Email, newsletterViewModel.District, newsletterViewModel.County, 169, Convert.ToInt32(GetCookie("Language").Value));

                    if (result)
                    {
                        var registeredNewsletter = client.GetNewsletterForActivation(newsletterViewModel.Email);

                        var body = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~\\Views\\_Templates\\EmailSubscripitionConfirmation.html"));

                        body = body.Replace("#SITEURL#", ConfigurationManager.AppSettings["SiteUrl"]);
                        body = body.Replace("#EMAIL#", newsletterViewModel.Email);
                        body = body.Replace("#UNIQUEGUID#", registeredNewsletter.UniqueId.ToString());
                        body = body.Replace("#NewsletterSubscriptionTitleText#", LanguageResources.Resources.SubscriptionNewsletterTitleText);
                        body = body.Replace("#ConfirmEmailAddressText#", LanguageResources.Resources.SubscriptionConfirmEmailAddressText);
                        body = body.Replace("#SubscribeMeText#", LanguageResources.Resources.SubscriptionSubscribeMeText);
                        body = body.Replace("#SubscriptionExplanationText#", LanguageResources.Resources.SubscriptionExplanationText);


                        result = client.SendNewsletterActivationEmail(body, newsletterViewModel.Email);
                    }
                }
                catch (Exception e)
                {
                    resultMessage = e.Message;
                    result = false;
                }
            }
            return Json(new { success = result, message = resultMessage });
        }

        [HttpGet]
        public ActionResult ActivateNewsletter(string email, string uniqueId)
        {
            bool result = false;
            string resultMessage = "";
            try
            {
                var client = new NewsletterClient();
                result = client.ActivateNewsletter(email, uniqueId);
            }
            catch (Exception e)
            {
                resultMessage = e.Message;
            }
            return View(result);
        }


        [HttpGet]
        public ActionResult Unsubscribe(string uniqueId)
        {
            var client = new NewsletterClient();
            var result = client.CancelNewsletterSubscription(uniqueId);
            return View(result);
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Account(string email, string uniqueId)
        {
            //check required parameters
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(uniqueId)) { return RedirectToAction("BadRequest", "Error"); }
            try
            {
                //get subscription
                var subscription = new NewsletterClient().GetSubscription(uniqueId);
                if (subscription == null || subscription.Email != email) {
                    return RedirectToAction("NotFound", "Error");
                }
                //get model
                var model = new SubscriptionViewModel
                {
                    AccountEmail = subscription.Email,
                    AccountUniqueId = subscription.UniqueId.ToString(),
                    SubscriptionDate = subscription.SubscriptionDate,
                    LanguageId = subscription.LanguageId,
                    DistrictId = subscription.DistrictId,
                    CountyId = subscription.CountyId
                };
                //get languages
                var languages = RealEstateClient.GetAvailableLanguages().ToList();
                model.Language = AutoMapper.Mapper.Map<List<SelectListItem>>(languages);
                //get districts
                var districts = RealEstateClient.GetDistricts().OrderBy(x => x.Description).ToList();
                model.District = AutoMapper.Mapper.Map<List<SelectListItem>>(districts);
                //get counties
                if (model.DistrictId.GetValueOrDefault() > 0)
                {
                    var counties = RealEstateClient.GetCountiesByDistrict(model.DistrictId.Value);
                    model.County = AutoMapper.Mapper.Map<List<SelectListItem>>(counties);
                }
                //render
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("BadRequest", "Error");
            }            
        }
                
        [HttpPost]
        public ActionResult UpdateSubscription(string accountUniqueId, int languageId, int? districtId, int? countyId)
        {
            var result = false;
            try
            {
                var client = new NewsletterClient();
                result = client.UpdateSubscription(accountUniqueId, languageId, districtId, countyId);
            }
            catch (Exception){}
            return Json(new { done = result });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}



using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WSAsset.Api.Interfaces.Response;
using WSAsset.Api.RealEstateEntities.Request;
using WSAsset.Api.RealEstateEntities.Response;
using WSAsset.DataModel;

namespace WSAsset.Api.Controllers
{
    [RoutePrefix("Newsletter")]
    public class NewsletterController : BaseApiController// ApiController
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(NewsletterController));

        [System.Web.Http.HttpGet]
        [Route("GetRegisteredNewsletters")]
        public List<RegisteredNewslettersResponse> GetRegisteredNewsletters()
        {
            try
            {
                using (var model = new WSAssetModel())
                {
                    _log.Info("Getting Active newsletters");
                    return model.NewsletterSubscription
                        .Where(x=>!x.IsCanceled && x.IsActive)
                        .Select(x => new RegisteredNewslettersResponse {  UniqueId = x.UniqueID, CountyId = x.CountyID, DistrictId = x.DistrictID, Email = x.Email, LanguageId = x.LanguageID, SubscriptionDate = x.SubscriptionDate })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("An Error Occured : {0} - Inner exception - {1}", ex.Message, ex.InnerException);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        [Route("GetNewsletterForActivation")]
        public RegisteredNewslettersResponse GetNewsletterForActivation(GetNewsletterForActivationRequest request)
        {
            try
            {
                using (var model = new WSAssetModel())
                {
                    _log.Info("Getting registered but inactive newsletters");
                    return model.NewsletterSubscription
                        .Where(x => !x.IsActive && x.Email == request.Email)
                        .Select(x => new RegisteredNewslettersResponse { UniqueId = x.UniqueID, CountyId = x.CountyID, DistrictId = x.DistrictID, Email = x.Email, LanguageId = x.LanguageID, SubscriptionDate = x.SubscriptionDate })
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("An Error Occured : {0} - Inner exception - {1}", ex.Message, ex.InnerException);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        [Route("GetNewsletterInfo")]
        public List<NewsletterInfoResponse> GetNewsletterInfo(List<NewsletterInfoRequest> request)
        {
            try
            {
                using (var model = new WSAssetModel())
                {
                    var responseList = new List<NewsletterInfoResponse>();

                    foreach (var newsletterInfo in request)
                    {
                        var list = responseList;

                        if (!newsletterInfo.DistrictId.HasValue)
                        {
                            list = list.Where(x => !x.DistrictId.HasValue && !x.CountyId.HasValue).ToList();

                        }
                        else if (!newsletterInfo.CountyId.HasValue)
                        {
                            list = list.Where(x => x.DistrictId == newsletterInfo.DistrictId && !x.CountyId.HasValue).ToList();
                        }
                        else
                        {
                            list = list.Where(x => x.DistrictId == newsletterInfo.DistrictId && x.CountyId == newsletterInfo.CountyId).ToList();
                        }

                        if (!list.Any())
                        {
                            var info = new NewsletterInfoResponse();

                            var assetQuery = model.Asset.Where(x => x.IsActive).AsQueryable();

                            //Should only return Assets that are for sale
                            var defaultQuery = model.RealEstate.Where(x=>x.RealEstateStatusID == 1).AsQueryable();

                            var realEstateQuery = defaultQuery;
                            realEstateQuery = newsletterInfo.DistrictId.HasValue ? realEstateQuery.Where(x => x.DistrictID == newsletterInfo.DistrictId) : realEstateQuery;
                            realEstateQuery = newsletterInfo.CountyId.HasValue ? realEstateQuery.Where(x => x.CountyID == newsletterInfo.CountyId) : realEstateQuery;

                            info.DistrictId = newsletterInfo.DistrictId;
                            info.CountyId = newsletterInfo.CountyId;

                            if (newsletterInfo.DistrictId.HasValue)
                            {
                                info.District = model.District.FirstOrDefault(x => x.DistrictID == newsletterInfo.DistrictId).Description;

                                if (newsletterInfo.CountyId.HasValue)
                                    info.County = model.County.FirstOrDefault(x => x.DistrictID == newsletterInfo.DistrictId && x.CountyID == newsletterInfo.CountyId).Description;
                            }
                            //New Assets
                            info.NewAssets = GetMergedResults(assetQuery, realEstateQuery,model, newsletterInfo.LanguageId)
                                                    .OrderByDescending(x => x.PublishedDate)
                                                    .Take(9)
                                                    .ToList();

                            //HighLight Assets
                            info.HighlightAssets = GetMergedResults(assetQuery.Where(x => x.IsHighlighted).AsQueryable(), realEstateQuery,model, newsletterInfo.LanguageId)
                                                    .OrderByDescending(x => x.PublishedDate)
                                                    .Take(9)
                                                    .ToList();

                            //Changed Price Assets
                            info.PriceChangeAssets = GetMergedResults(assetQuery.Where(x => x.CurrentPrice != x.PreviousPrice).AsQueryable(), realEstateQuery, model, newsletterInfo.LanguageId)
                                                    .OrderByDescending(x => x.ChangedPriceDate)
                                                    .Take(9)
                                                    .ToList();

                            //Alternative Content: Highlights
                            info.AlternativeAssets = GetMergedResults(assetQuery.Where(x => x.IsHighlighted).AsQueryable(), defaultQuery, model, newsletterInfo.LanguageId)
                                                    .OrderByDescending(x => x.PublishedDate)
                                                    .Take(18)
                                                    .ToList();

                            responseList.Add(info);
                        }
                    }
                    
                    return responseList;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("An Error occurred in GetNewsletterInfo : {0}, INNER EXCEPTION : {1} ", ex.Message, ex.InnerException);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        [Route("NewsletterSubscription")]
        public bool NewsletterSubscription(NewsletterSubscriptionRequest subscriptionInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subscriptionInfo.Email))
                    throw new ArgumentException("Provided value (" + subscriptionInfo.Email + ") is invalid. ", "Email" );

                _log.Info("Creating Newsletter Subscription");

                using (var model = new WSAssetModel())
                {
                    var newsletterSubscription = model.NewsletterSubscription.Where(x => x.Email == subscriptionInfo.Email).FirstOrDefault();

                    if (newsletterSubscription == null)
                    {
                        _log.Info("Creating new entry for Newsletter Subscription");
                        var subscription = AutoMapper.Mapper.Map<NewsletterSubscription>(subscriptionInfo);

                        model.NewsletterSubscription.Add(subscription);
                    }
                    else
                    {
                        if (!newsletterSubscription.IsActive)
                        {
                            _log.Info("Updating entry for Newsletter Subscription");
                            newsletterSubscription.LanguageID = subscriptionInfo.LanguageId;
                            newsletterSubscription.DistrictID = subscriptionInfo.DistrictId;
                            newsletterSubscription.CountyID = subscriptionInfo.CountyId;
                            newsletterSubscription.SubscriptionDate = DateTime.Now;

                            model.SaveChanges();
                            return true;
                        }
                        else
                        {
                            _log.Info("Entry for Newsletter Subscription already activated");
                            return false;
                        }
                    }

                    model.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("An Error occurred in NewsletterSubscription : {0}, INNER EXCEPTION : {1} ", ex.Message, ex.InnerException);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        [Route("NewsletterActivation")]
        public bool NewsletterActivation(NewsletterActivationRequest newsletterActivationRequest)
        {
            try
            {
                using (var model = new WSAssetModel())
                {
                    _log.Info("Activating newsletter subscription");
                    var uniqueId = Guid.Parse(newsletterActivationRequest.UniqueId);

                    var newsletterSubscription = model
                                                .NewsletterSubscription
                                                .Where(x => x.Email == newsletterActivationRequest.Email 
                                                            && !x.IsActive
                                                            && x.UniqueID == uniqueId)
                                                .FirstOrDefault();

                    if (newsletterSubscription != null)
                    {
                        newsletterSubscription.IsCanceled = false;
                        newsletterSubscription.CancelDate = null;
                        newsletterSubscription.IsActive = true;
                    }
                    else
                    {
                        return false;
                    }

                    model.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("An Error occurred in NewsletterActivation : {0}, INNER EXCEPTION : {1} ", ex.Message, ex.InnerException);
                throw;
            }
        }

        [HttpPost]
        [Route("CancelNewsletterSubscription")]
        public bool CancelNewsletterSubscription([FromBody]string uniqueId)
        {
            try
            {
                Guid uniqueGuid;

                if (!Guid.TryParse(uniqueId, out uniqueGuid))
                    throw new ArgumentException("uniqueId", "Provided value is invalid.");

                _log.Info("Cancel newsletter subscription");

                using (var model = new WSAssetModel())
                {
                    var subscription = model.NewsletterSubscription.Where(x => x.UniqueID == uniqueGuid).FirstOrDefault();
                    if (subscription != null)
                    {
                        subscription.IsCanceled = true;
                        subscription.CancelDate = DateTime.Now;
                        subscription.IsActive = false;

                        model.SaveChanges();

                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("An Error occurred in CancelNewsletterSubscription : {0}, INNER EXCEPTION : {1} ", ex.Message, ex.InnerException);
                throw;
            }
        }

        [HttpPost]
        [Route("SendNewsletterActivationEmail")]
        public bool SendNewsletterActivationEmail(SendNewsletterActivationEmailRequest request)
        {
            try
            {
                _log.Info("Sending newsletter Activation email");
                return SendEmailCommand(request.EmailTemplate, request.To, "WSRE - Newsletter Activation", "Whitestar Assets newsletter activation");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("An Error occurred in SendNewsletterActivationEmail : {0}, INNER EXCEPTION : {1} ", ex.Message,ex.InnerException);
                throw;

                //return false;
            }
        }
        
        private static IQueryable<RealEstateInformationResponse> GetMergedResults(IQueryable<Asset> assetQuery, IQueryable<RealEstate> realEstateQuery, WSAssetModel model, int languageId)
        {
            IQueryable<RealEstateInformationResponse> mergedResults;

            mergedResults = (from a in assetQuery
                             join r in realEstateQuery on a.AssetID equals r.AssetID
                             join rc in model.RealEstateConditionLocalized on r.RealEstateConditionID equals rc.RealEstateConditionID
                             //join rt in model.RealEstateTipologyLocalized on r.RealEstateTipologyID equals rt.RealEstateTipologyID
                             from rt in model.RealEstateTipologyLocalized
                                .Where(x => x.RealEstateTipologyID == r.RealEstateTipologyID && x.LanguageID == languageId)
                                .DefaultIfEmpty()
                             join rs in model.RealEstateStatusLocalized on r.RealEstateStatusID equals rs.RealEstateStatusID
                             join rType in model.RealEstateTypeLocalized on r.RealEstateTypeID equals rType.RealEstateTypeID
                             //join photo in model.AssetPhoto on a.AssetID equals photo.AssetID
                             from photo in model.AssetPhoto
                                .Where(x => x != null &&  x.AssetID == r.AssetID && x.IsHighlighted)
                                .DefaultIfEmpty()
                             where rc.LanguageID == languageId
                             && rs.LanguageID == languageId
                             && rType.LanguageID == languageId
                             //&& (photo != null ? photo.IsHighlighted : true)
                             //orderby (orderByHitCount? a.HitCount : a.PublishDate) descending
                             select new RealEstateInformationResponse
                             {
                                 AssetID = a.AssetID,
                                 AssetTypeID = r.RealEstateTypeID,
                                 ConstructionYear = r.ConstructionYear,
                                 County = r.County.Description,
                                 CurrentPrice = a.CurrentPrice,
                                 Description = a.Description,
                                 District = r.District.Description,
                                 GrossBuildingArea = r.GrossBuildingArea,
                                 HasParking = r.HasParking,
                                 IsHighlighted = a.IsHighlighted,
                                 Latitude = r.Latitude,
                                 Longitude = r.Longitude,
                                 Parish = r.Parish.Description,
                                 OriginatorAssetID = a.SourceAssetID,
                                 RealEstateStatus = rs.Description,
                                 RealEstateCondition = rc.Description,
                                 RealEstateTipology = rt.Description,
                                 RealEstateType = rType.Description,
                                 Reference = a.Reference,
                                 PreviousPrice = a.PreviousPrice,
                                 ChangedPriceDate = a.PriceChangeDate,
                                 PublishedDate = a.PublishDate,
                                 HitCount = a.HitCount,
                                 EnergyCertificate = r.EnergyCertificateNumber,
                                 EnergyRating = r.RealEstateEnergyCertificate.Description,
                                 MainPhotoId = (photo==null ? 0: photo.AssetPhotoID)
                             });

            return mergedResults;
        }
        
        [System.Web.Http.HttpPost]
        [Route("GetSubscription")]
        public RegisteredNewslettersResponse GetSubscription([FromBody]string uniqueId)
        {
            try
            {
                using (var model = new WSAssetModel())
                {
                    Guid uniqueGuid;

                    if (!Guid.TryParse(uniqueId, out uniqueGuid))
                        throw new ArgumentException("uniqueId", "Provided value is invalid.");

                    return model.NewsletterSubscription
                        .Where(x => !x.IsCanceled && x.IsActive && x.UniqueID == uniqueGuid)
                        .Select(x => new RegisteredNewslettersResponse
                        {
                            UniqueId = x.UniqueID,
                            CountyId = x.CountyID,
                            DistrictId = x.DistrictID,
                            Email = x.Email,
                            LanguageId = x.LanguageID,
                            SubscriptionDate = x.SubscriptionDate
                        })
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("An Error Occured in GetSubscription : {0} - Inner exception - {1}", ex.Message, ex.InnerException);
                throw;
            }
        }

        [System.Web.Http.HttpPost]
        [Route("UpdateSubscription")]
        public bool UpdateSubscription(NewsletterSubscriptionRequest request)
        {
            try
            {
                Guid uniqueGuid;

                if (!Guid.TryParse(request.UniqueId, out uniqueGuid))
                    throw new ArgumentException("uniqueId", "Provided value is invalid.");

                using (var model = new WSAssetModel())
                {
                    var subscription = model.NewsletterSubscription.Where(x => x.UniqueID == uniqueGuid).FirstOrDefault();
                    if (subscription != null)
                    {
                        subscription.LanguageID = request.LanguageId;
                        subscription.DistrictID = request.DistrictId;
                        subscription.CountyID = request.CountyId;
                        model.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("An Error occurred in UpdateSubscription : {0}, INNER EXCEPTION : {1} ", ex.Message, ex.InnerException);
                throw;
            }
        }
    }
}
