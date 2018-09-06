using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using WS.WSI.Site.Models;

namespace WS.WSI.Site.Controllers
{
    public class FavoritesController : BaseController
    {
        // GET: Favorites
        public ActionResult Index()
        {
            var cookies = GetCookie("Favorites");

            var assetList = new List<int>();

            if (cookies != null)
            {
                assetList = JsonConvert.DeserializeObject<List<int>>(cookies.Value);
            }

            var model = new AssetViewModel();

            if (assetList.Any())
            {
                model.Assets.Rows.AddRange(RealEstateClient.GetAssets(assetList, LanguageId));
                model.Assets.TotalRows = assetList.Count;
            }

            return View(model);
        }

        public ActionResult ToggleFavorite(int id)
        {
            var key = "Favorites";
            var added = false;
            if (CookieExists(key))
            {
                var cookie = GetCookie(key);
                var assetList = JsonConvert.DeserializeObject<List<int>>(cookie.Value);
                var maxFavorites = Convert.ToInt32(ConfigurationManager.AppSettings["MaxFavorites"]);

                if (assetList.Count < maxFavorites)
                {
                    //If there are less favorites than the value defined in the web config add the asset id to the cookie
                    if (assetList.Contains(id))
                    {
                        assetList.Remove(id);
                        added = false;
                    }
                    else
                    {
                        assetList.Add(id);
                        added = true;
                    }
                    
                    AddCookie(key, JsonConvert.SerializeObject(assetList), DateTime.Now.AddDays(365));
                    return Json(new { added = added }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //if the favorites reached their limit then check if all the assets are still active 
                    var assets = RealEstateClient.GetAssets(assetList, LanguageId);

                    if (assets.Count <= maxFavorites - 1)
                    {
                        //if the result provides less assets then the existing ones in the cookie recreate cookie
                        RemoveCookie(key);

                        assetList = new List<int>();

                        //Add all active assets to the cookie list
                        assetList = assets.Select(x => x.AssetID).ToList();

                        //Add the new assetId to the cookie list
                        assetList.Add(id);

                        AddCookie(key, JsonConvert.SerializeObject(assetList), DateTime.Now.AddDays(365));
                        return Json(new { added = added }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (assetList.Contains(id))
                        {
                            assetList.Remove(id);
                            AddCookie(key, JsonConvert.SerializeObject(assetList), DateTime.Now.AddDays(365));
                            return Json(new { added = false, reachedLimit = false }, JsonRequestBehavior.AllowGet);
                        }
                        //Return message indicating the Favorite limit has been reached
                        return Json(new { added = added, reachedLimit = true }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            var list = new List<int> { id };

            AddCookie(key, JsonConvert.SerializeObject(list), DateTime.Now.AddDays(365));
            return Json(new { added = added }, JsonRequestBehavior.AllowGet);
        }

        private bool AddAssetToCookie(int id, List<int> assetList)
        {
            if (assetList.Contains(id))
            {
                assetList.Remove(id);
                return false;
            }

            assetList.Add(id);
            return true;
        }
    }
}
