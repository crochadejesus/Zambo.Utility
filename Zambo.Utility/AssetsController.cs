using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WS.WSI.Site.Extensions;
using WS.WSI.Site.Models;
using WSAsset.Api.Client;
using WSAsset.Api.RealEstateEntities;
using System.Web;

namespace WS.WSI.Site.Controllers
{
    public class AssetsController : BaseController
    {
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index(bool? isHighlighted, string mostViewed, string mostRecent, bool? reset)
        {
            if (isHighlighted.HasValue || !string.IsNullOrWhiteSpace(mostViewed) || !string.IsNullOrWhiteSpace(mostRecent) || reset.HasValue)
            {
                var searchCriteria = new SearchModel
                {
                    IsHighlighted = isHighlighted,
                    SortBy = (SortBy)Enum.Parse(typeof(SortBy), string.IsNullOrWhiteSpace(mostViewed) ? "MostRecent" : mostViewed),
                    MostRecentOptions = (MostRecentOptions)Enum.Parse(typeof(MostRecentOptions), string.IsNullOrWhiteSpace(mostRecent) ? MostRecentOptions.None.ToString() : MostRecentOptions.Last2Months.ToString())
                };
                SaveSearchCriteria(searchCriteria);
                return RedirectToAction("Index");
            }

            SearchModel model = GetSearchCriteria();

            return RenderList(model);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        [HttpPost]
        [ActionName("Index")]
        public ActionResult Index(SearchModel searchCriteria)
        {
            SaveSearchCriteria(searchCriteria);
            return RedirectToAction("Index");
        }

        public ActionResult MapSearch(SearchModel searchCriteria)
        {
            var assetList = RealEstateClient.GetAssetsLocations(searchCriteria.LanguageId);

            var model = new MapSearchViewModel
            {
                AssetLocations = assetList
            };

            return View("Map", model);
        }

        [HttpPost]
        public String GetMapInfo(int? id)
        {
            if (id.HasValue)
            {
                var asset = RealEstateClient.GetAssets(new List<int> { id.Value }, LanguageId).FirstOrDefault();

                return JsonConvert.SerializeObject(new {
                    Title = asset.RealEstateType + " " + asset.RealEstateTipology,
                    Price = TextHelper.FormatPrice(asset.CurrentPrice, asset.RealEstateStatus).ToHtmlString(),
                    Location = asset.District + ", " + asset.County + ", " + asset.Parish,
                    PhotoUrl = Url.Action("GetPhoto", "Photos", new { photoId = asset.MainPhotoId, width = 100, height = 100, crop = true })
                });
            }
            return String.Empty;
        }

        private ActionResult RenderList(SearchModel searchModel)
        {
            var model = new SearchPageViewModel();
            var searchViewModel = FillSearchViewModel();
            var searchFilters = new RealEstateSearchFilters();

            searchModel.LanguageId = LanguageId;

            searchViewModel.SearchCriteria = searchModel;
            searchFilters = AutoMapper.Mapper.Map<RealEstateSearchFilters>(searchModel);

            #region Set Default Values

            if(searchViewModel.SearchCriteria.PropertyType.HasValue)
                searchViewModel.PropertyType.FirstOrDefault(x=>x.Value == searchViewModel.SearchCriteria.PropertyType.Value.ToString()).Selected = true;

            searchViewModel.SortBy.FirstOrDefault(x => x.Value == searchViewModel.SearchCriteria.SortBy.ToString()).Selected = true;

            if (searchViewModel.SearchCriteria.District.HasValue)
                searchViewModel.District.FirstOrDefault(x => x.Value == searchViewModel.SearchCriteria.District.Value.ToString()).Selected = true;            
            
            searchViewModel.MostRecentOptions.FirstOrDefault(x => x.Value == searchViewModel.SearchCriteria.MostRecentOptions.ToString()).Selected = true;
            
            #endregion Set Default Values

            var realEstates = RealEstateClient.GetRealEstates(searchFilters);

            model.SearchViewModel = searchViewModel;
            model.AssetViewModel = new AssetViewModel { Assets = realEstates };
                        
            model.SugestedSearches = RealEstateClient.GetPopularSearches(10, LanguageId, searchModel.District, null, null);

            return View("Index", model);
        }

        [HttpPost]
        public ActionResult Search(SearchModel searchCriteria)
        {
            return PartialView("_List", GetAssets(searchCriteria, true));
        }

        [HttpPost]
        public ActionResult LoadMore(SearchModel searchCriteria)
        {
            var model = GetAssets(searchCriteria, false);
            
            ViewBag.EndOfList = model.Assets.PageSize > model.Assets.Rows.Count ? true : false;

            SavePageNumber(searchCriteria.PageNumber);

            return PartialView("_Asset", model);
        }

        [HttpPost]
        public ActionResult Reload()
        {
            SearchModel model = GetSearchCriteria();
            
            model.PageNumber = 1;
            model.PageSize = GetPageNumber() * 5;

            return PartialView("_List", GetAssets(model, false));
        }

        [HttpPost]
        public ActionResult LoadSimilar(int? id)
        {
            var model = new AssetViewModel();
            if (id.HasValue)
            {
                var asset = RealEstateClient.GetAssets(new List<int> { id.Value }, LanguageId).FirstOrDefault();
                if (asset != null)
                {
                    //set criteria and get similar assets
                    var criteria = new SearchModel();
                    criteria.PageNumber = 1;
                    criteria.PageSize = 6;
                    criteria.PropertyType = asset.AssetTypeID;
                    criteria.District = asset.DistrictID;
                    criteria.County = asset.CountyID;
                    //criteria.Parish = asset.ParishID;

                    var assetsList = GetAssets(criteria, false);
                    assetsList.Assets.Rows = assetsList.Assets.Rows.Where(x => x.AssetID != id.Value).ToList();
                    model = assetsList;
                }
            }
            return PartialView("_Similar", model);
        }

        private AssetViewModel GetAssets(SearchModel searchCriteria, bool saveCriteria)
        {
            //get search filters
            var searchFilters = AutoMapper.Mapper.Map<RealEstateSearchFilters>(searchCriteria);

            searchFilters.LanguageId = LanguageId;

            //get real estates
            var model = new AssetViewModel
            {
                Assets = RealEstateClient.GetRealEstates(searchFilters)
            };
            //store criteria
            if (saveCriteria)
            {
                SaveSearchCriteria(searchCriteria);
            }
            return model;
        }

        private SearchViewModel FillSearchViewModel()
        {
            var type = RealEstateClient.GetTipologyTypes(LanguageId).OrderBy(x=>x.Description).ToList();
            var districts = RealEstateClient.GetDistricts().OrderBy(x => x.Description).ToList();
            var status = RealEstateClient.GetRealEstateConditions(LanguageId).OrderBy(x => x.Description).ToList();
            //var size = client.GetTipologySizes(languageId);
            var searchViewModel = new SearchViewModel();

            searchViewModel.PropertyType = AutoMapper.Mapper.Map<List<SelectListItem>>(type);
            searchViewModel.District = AutoMapper.Mapper.Map<List<SelectListItem>>(districts);
            searchViewModel.PropertyCondition = AutoMapper.Mapper.Map<List<SelectListItem>>(status);
            searchViewModel.SortBy = EnumHelper.BuildSelectListItems(typeof(SortBy));
            searchViewModel.MostRecentOptions = EnumHelper.BuildSelectListItems(typeof(MostRecentOptions));

            return searchViewModel;
        }

        // Asset Details
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (id.HasValue)
            {
                var model = new AssetDetailsViewModel();

                var asset = RealEstateClient.GetAssets(new List<int> { id.Value }, LanguageId).FirstOrDefault();

                if (asset != null)
                {
                    model = AutoMapper.Mapper.Map<AssetDetailsViewModel>(asset);

                    if (id.HasValue)
                    {
                        try
                        {
                            model.Thumbnails = RealEstateClient.GetAssetThumbnails(id.Value, 160, 100);
                        }
                        catch (Exception){}

                        try
                        {
                            model.Features = RealEstateClient.GetAssetFeatures(id.Value, LanguageId);
                        }
                        catch (Exception){}
                    }                    

                    RealEstateClient.IncrementHitCountForAsset(id.Value);

                    model.EnergyRatingClass = GetEnergyRatingClass(model.EnergyRating);

                    SaveLastVisitedAsset(id.Value);
                }

                return View(model);
            }
            else
            {
                throw new HttpException(404, "Not Found");
            }
        }

        [HttpGet]
        public ActionResult GetDocument(int? id, string filename)
        {
            if (id.HasValue)
            {
                filename = String.IsNullOrEmpty(filename) ? id.ToString() : filename;
                var pdfData = RealEstateClient.GetDocument(id.Value).Document;
                return File(pdfData, "application/pdf", filename + ".pdf");
            }
            else
            {
                throw new HttpException(404, "Not Found");
            }
        }

        private string GetEnergyRatingClass(string value)
        {
            if (value.Contains("-"))
            {
                return string.Format("class{0}{1}", value.Substring(0,1),"Minus");
            }
            else if (value.Contains("+"))
            {
                return string.Format("class{0}{1}", value.Substring(0, 1), "Plus");
            }
            else if (value.Replace("+","").Replace("-", "").Length > 1)
            {
                return "classExempt";
            }
            else
                return string.Format("class{0}",value);
        }

        public ActionResult Simulator(Decimal AssetPrice)
        {
            return PartialView("_Simulator", AssetPrice);
        }

        [HttpPost]
        public ActionResult GetMapResults(List<int> ids)
        {
            var assets = RealEstateClient.GetRealEstatesSummary(ids, 100, 100, LanguageId);
            var model = AutoMapper.Mapper.Map<List<AssetSummaryModel>>(assets);
            return PartialView("_MapResults", model);
        }

        /* Session Management */
        
        private void SaveSearchCriteria(SearchModel searchCriteria)
        {
            //change/create search criteria session
            Session["SearchCriteria"] = JsonConvert.SerializeObject(searchCriteria);
            //reset last page number
            SavePageNumber(1);
            //reset last visited asset
            SaveLastVisitedAsset(0);
        }

        private SearchModel GetSearchCriteria()
        {
            return (Session["SearchCriteria"] != null) ?
                JsonConvert.DeserializeObject<SearchModel>(Session["SearchCriteria"].ToString()) :
                new SearchModel();
        }

        private void SavePageNumber(int pageNumber)
        {
            Session["LoadMorePageNumber"] = pageNumber;
        }

        private int GetPageNumber()
        {
            int pageNumber = 1;
            if (Session["LoadMorePageNumber"] != null)
            {
                int.TryParse(Session["LoadMorePageNumber"].ToString(), out pageNumber);
            }
            return pageNumber;
        }

        private void SaveLastVisitedAsset(int assetId)
        {
            Session["LastVisitedAssetID"] = assetId;
        }

    }
}
