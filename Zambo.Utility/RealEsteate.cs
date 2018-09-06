using GhostscriptSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Value.Domain.Models.Common;
using Value.Domain.Models.Valuations.Dto;
using Value.Domain.Models.Valuations.Request;
using Value.Domain.Models.Valuations.Response;
using Value.Domain.Security.Interfaces;
using Value.Domain.Valuations.Interfaces;
using Value.Infrastructure.Data;
using Value.Infrastructure.Data.Queries;

namespace Value.Domain.Valuations.Services
{
    public partial class Valuation : ValuationQuery, IValuation
    {
        private IConverters _converters;
        private INotification _notification;

        public IConverters Converters
        {
            get
            {
                if (_converters == null)
                {
                    _converters = new Converters();
                    _converters.SecurityManagement = SecurityManagement;
                }

                return _converters;
            }
        }

        public INotification Notification
        {
            get
            {
                if (_notification == null)
                {
                    _notification = new Notification();
                    _notification.SecurityManagement = SecurityManagement;
                }

                return _notification;
            }
        }

        public ISecurityManagement SecurityManagement { get; set; }

        GeneralResponse<bool> IValuation.DeleteDocument(long requestId, long documentId)
        {
            GeneralResponse<bool> generalResponse = new GeneralResponse<bool>();

            using (var dbContext = new Entities())
            {
                var entity = FirstOrDefault<RequestDocument>(r => r.RequestID == requestId && r.RequestDocumentID == documentId, dbContext);
                entity.IsActive = false;
                entity.ModifiedByUserID = SecurityManagement.UserID;
                entity.ModifiedDate = SecurityManagement.ModifiedDate;
                entity.SessionID = SecurityManagement.SessionID;
                generalResponse = Update<RequestDocument>(entity, dbContext);
            }

            return generalResponse;
        }

        GeneralResponse<bool> IValuation.DeleteRequest(long requestId)
        {
            GeneralResponse<bool> generalResponse = new GeneralResponse<bool>();

            using (var dbContext = new Entities())
            {
                var entity = FirstOrDefault<Request>(r => r.RequestID == requestId, dbContext);
                entity.IsActive = false;
                entity.ModifiedByUserID = SecurityManagement.UserID;
                entity.ModifiedDate = SecurityManagement.ModifiedDate;
                entity.SessionID = SecurityManagement.SessionID;
                generalResponse = Update<Request>(entity, dbContext);
            }

            return generalResponse;
        }

        ActivityHistoryResponse[] IValuation.GetActivityHistories(long requestId)
        {
            var response = new ActivityHistoryResponse[] { };

            using (var dbContext = new Entities())
            {
                response = (from rst in dbContext.RequestStatusTransition.AsNoTracking()
                            join previus in dbContext.WorkFlowStatus.AsNoTracking() on rst.PreviousWorkFlowStatusID equals previus.WorkFlowStatusID into previusn
                            from previus in previusn.DefaultIfEmpty()
                            join current in dbContext.WorkFlowStatus.AsNoTracking() on rst.WorkFlowStatusID equals current.WorkFlowStatusID into currentn
                            from current in currentn.DefaultIfEmpty()
                            where rst.RequestID == requestId
                            select new ActivityHistoryResponse()
                            {
                                CurrentWorkFlowStatus = current.WorkFlowStatusName,
                                PreviousWorkFlowStatus = previus.WorkFlowStatusName,
                                TimeStamp = rst.WorkFlowStatusDate,
                                UserName = rst.User.DisplayName
                            }).ToArray();
            }

            return response;
        }

        CommentResponse[] IValuation.GetComment(CommentGetRequest param)
        {
            var response = new CommentResponse[] { };

            using (var dbContext = new Entities())
            {
                var iquery = (from rc in dbContext.RequestComment.AsNoTracking()
                              where rc.RequestID == param.RequestId && rc.IsActive
                              select new CommentResponse()
                              {
                                  Comment = rc.Comment,
                                  CreatedDate = rc.CreatedDate,
                                  ParentID = rc.ParentID,
                                  CommentTypeID = rc.CommentTypeID,
                                  RequestCommentID = rc.RequestCommentID,
                                  RequestID = rc.RequestID,
                                  ToUserID = rc.ToUserID,
                                  ToUserUserName = dbContext.User.FirstOrDefault(x => x.UserID == rc.ToUserID).DisplayName,
                                  UserID = rc.UserID,
                                  UserName = dbContext.User.FirstOrDefault(x => x.UserID == rc.UserID).DisplayName
                              }).Where(x => x.RequestCommentID > param.RequestCommentId && param.CommentTypeID.Contains(x.CommentTypeID));

                // Verify if the user is Appraiser or AppraiserTL
                // Verify if in the Role have Action DenyAccessOfInternalCommentsvar
                var authorizations = (Dictionary<string, string>[])SecurityManagement.GetAuthorizations(SecurityManagement.Token).Response;
                if (authorizations == null || authorizations.Any(x => x.ContainsValue("DenyAccessOfInternalComments")))
                {
                    response = iquery.Where(x => x.CommentTypeID == 3 || x.CommentTypeID == 1).ToArray();
                }
                else
                {
                    response = iquery.ToArray();
                }
            }

            return response;
        }

        DocumentResponse IValuation.GetDocument(long documentId, long requestId)
        {
            DocumentResponse response = null;

            using (var dbContext = new Entities())
            {
                var query = GetQueryDocument(documentId, requestId, dbContext);
                if (query != null)
                {
                    response = new DocumentResponse()
                    {
                        CreatedByUser = query.CreatedByUser,
                        CreatedDate = query.CreatedDate,
                        Description = query.Description,
                        Document = Infrastructure.Helpers.FileManager.RetrieveFile(Infrastructure.Helpers.FileManager.GetAbsolutePath(query.DocumentTypeName, query.RequestID, query.DocumentPath)),
                        DocumentID = query.DocumentID.Value,
                        DocumentTypeName = query.DocumentTypeName,
                        OriginalName = query.OriginalName,
                        RequestID = query.RequestID
                    };
                }
            }

            return response;
        }

        DocumentResponse[] IValuation.GetDocuments(long requestId)
        {
            var response = new DocumentResponse[] { };

            using (var dbContext = new Entities())
            {
                var query = GetQueryDocuments(requestId, dbContext).ToArray();
                response = new DocumentResponse[query.Count()];

                Parallel.For(0, query.Count(), i =>
                {
                    var thumbPath = new StringBuilder()
                        .Append(Infrastructure.Helpers.FileManager.CreateDirectory(Path.Combine(Infrastructure.Configuration.WebConfig.GetDocumentsThumbnailsPathKeyValue())))
                        .Append("\\")
                        .Append(query[i].RequestID.ToString())
                        .Append("\\")
                        .Append(query[i].DocumentPath)
                        .Append(".jpg").ToString();

                    response.SetValue(new DocumentResponse()
                    {
                        CreatedByUser = query[i].CreatedByUser,
                        CreatedDate = query[i].CreatedDate,
                        Description = query[i].Description,
                        Document = Infrastructure.Helpers.ImageManipulation.GetImage(150, 150, thumbPath, false),
                        DocumentID = query[i].DocumentID.Value,
                        DocumentTypeName = query[i].DocumentTypeName,
                        OriginalName = query[i].OriginalName,
                        RequestID = query[i].RequestID
                    }, i);
                });
            }

            return response;
        }

        ThumbnailsResponse[] IValuation.GetDocumentsThumbnails(ThumbnailsRequest param)
        {
            var response = new ThumbnailsResponse[] { };

            using (var dbContext = new Entities())
            {
                var query = GetQueryDocumentsThumbnails(param.RequestID, dbContext).ToArray();
                response = new ThumbnailsResponse[query.Count()];

                for (int i = 0; i < query.Count(); i++)
                {
                    var thumbFilePath = query[i].DocumentPath + "_thumb.jpg";
                    response.SetValue(new ThumbnailsResponse()
                    {
                        Document = Infrastructure.Helpers.ImageManipulation.GetImage(param.Width, param.Height, Infrastructure.Helpers.FileManager.GetAbsolutePath(query[i].DocumentTypeName, query[i].RequestID, thumbFilePath), true),
                        DocumentID = query[i].DocumentID.Value,
                        DocumentTypeName = query[i].DocumentTypeName,
                        RequestID = query[i].RequestID
                    }, i);
                };
            }

            return response;
        }

        DocumentResponse IValuation.GetPhoto(PhotoRequest param)
        {
            DocumentResponse response = null;

            using (var dbContext = new Entities())
            {
                var query = GetQueryPhoto(param.DocumentID.Value, param.RequestID, dbContext);
                if (query != null)
                {
                    response = new DocumentResponse()
                    {
                        CreatedByUser = query.CreatedByUser,
                        CreatedDate = query.CreatedDate,
                        Description = query.Description,
                        Document = Infrastructure.Helpers.ImageManipulation.GetImage(param.Width, param.Height, Infrastructure.Helpers.FileManager.GetAbsolutePath(query.DocumentTypeName, query.RequestID, query.DocumentPath), param.Crop),
                        DocumentID = query.DocumentID.Value,
                        DocumentTypeName = query.DocumentTypeName,
                        OriginalName = query.OriginalName,
                        RequestID = query.RequestID
                    };
                }
            }

            return response;
        }

        DocumentResponse[] IValuation.GetPhotos(PhotoRequest param)
        {
            var response = new DocumentResponse[] { };
            
            var query = GetQueryPhotos(param.RequestID);
            if (query != null)
            {
                response = query.ToArray();
            }

            return response;
        }

        ThumbnailsResponse[] IValuation.GetPhotosThumbnails(ThumbnailsRequest param)
        {
            var response = new ThumbnailsResponse[] { };
            
                var query = GetQueryPhotosThumbnails(param.RequestID).ToArray();
                response = new ThumbnailsResponse[query.Length];

                for (int i = 0; i < query.Length; i++)
                {
                    response.SetValue(new ThumbnailsResponse()
                    {
                        Document = Infrastructure.Helpers.ImageManipulation.GetImage(param.Width, param.Height, Infrastructure.Helpers.FileManager.GetAbsolutePath(query[i].DocumentTypeName, query[i].RequestID, query[i].DocumentPath), true),
                        DocumentID = query[i].DocumentID.Value,
                        DocumentTypeName = query[i].DocumentTypeName,
                        RequestID = query[i].RequestID
                    }, i);
                };

            return response;
        }
        
        ValuationDto IValuation.GetRequest(long requestId)
        {
            var dto = new ValuationDto();

            if (requestId > 0)
            {
                dto = Converters.Convert(requestId, ConvertValuation);
                
                // Authorized WorkflowActions
                dto.WorkflowActions = GetQueryWorkflowActions(userId: SecurityManagement.UserID, requestId: requestId);
            }

            return dto;
        }

        private ValuationDto ConvertValuation(ICollection<Infrastructure.Data.Valuation> collection, RequestDto requestDto)
        {
            var valuationDto = new ValuationDto();

            if (collection != null && collection.Count > 0 && requestDto != null)
            {
                foreach (var item in collection)
                {
                    valuationDto.RequestDto = requestDto;

                    requestDto.ApproachComments = item.ApproachComments;
                    requestDto.AskingPrice = item.AskingPrice;
                    requestDto.AssetClassificationID = item.AssetClassificationID;
                    requestDto.AssetFoundID = item.AssetFoundID;
                    requestDto.AssetFoundInMarketID = item.AssetFoundInMarketID;
                    requestDto.AssetGrade = item.AssetGrade;
                    requestDto.AssetPurposeID = item.AssetPurposeID;
                    requestDto.BPV = item.BPV;
                    requestDto.BpvDate = item.BPVDate;
                    requestDto.BuiltConditionID = item.BuiltConditionID;
                    requestDto.BuiltQualityID = item.BuiltQualityID;
                    requestDto.CapexAmount = item.CapexAmount;
                    requestDto.CapexEstimated = item.CapexEstimated;
                    requestDto.CapexRequirements = item.CapexRequirements;
                    requestDto.CompleteBuiltID = item.CompleteBuiltID;
                    requestDto.CompleteBuiltPercent = item.CompleteBuiltPercent;
                    requestDto.ConditionID = item.ConditionID;
                    requestDto.ConstructionYear = item.ConstructionYear;
                    requestDto.CurrentMainUsageID = item.CurrentMainUsageID;
                    requestDto.DateOfInspection = item.DateOfInspection;
                    requestDto.DemandStatusID = item.DemandStatusID;
                    requestDto.DifPercentagePreviousValuation = item.DifPercentagePreviousValuation;
                    requestDto.DiscountFSV = item.DiscountFSV;
                    requestDto.EstimatedTimeToSell = item.EstimatedTimeToSell;
                    requestDto.FacadeConditionID = item.FacadeConditionID;
                    requestDto.HasBuildingPotentialID = item.HasBuildingPotentialID;
                    requestDto.HasBuildingRightsID = item.HasBuildingRightsID;
                    requestDto.HasValidProjectID = item.HasValidProjectID;
                    requestDto.InfrastructuresConditionID = item.InfrastructuresConditionID;
                    requestDto.InsertedInAllotmentID = item.InsertedInAllotmentID;
                    requestDto.InteriorConditionID = item.InteriorConditionID;
                    requestDto.IsRanRenID = item.IsRanRenID;
                    requestDto.LiquidityGrade = item.LiquidityGrade;
                    requestDto.LocationComments = item.LocationComments;
                    requestDto.LocationGrade = item.LocationGrade;
                    requestDto.LocationQualityID = item.LocationQualityID;
                    requestDto.LocationSubTypeID = item.LocationSubTypeID;
                    requestDto.LocationTypeID = item.LocationTypeID;
                    requestDto.MarketComments = item.MarketComments;
                    requestDto.MarketPerspectiveID = item.MarketPerspectiveID;
                    requestDto.MarketStatusID = item.MarketStatusID;
                    requestDto.NeedsWorkID = item.NeedsWorkID;
                    requestDto.OccupancyID = item.OccupancyID;
                    requestDto.OMV = item.OMV;
                    requestDto.PDMMainClassificationID = item.PDMMainClassificationID;
                    requestDto.ProbableUseID = item.ProbableUseID;
                    requestDto.ReconstructionCorrectionFactor = item.ReconstructionCorrectionFactor;
                    requestDto.RedFlags = item.RedFlags;
                    requestDto.RemValue = item.RemValue;
                    requestDto.RequestID = item.RequestID;
                    requestDto.SquattersID = item.SquattersID;
                    requestDto.StrategyToSell = item.StrategyToSell;
                    requestDto.SupplyStatusID = item.SupplyStatusID;
                    requestDto.TimeInMarket = item.TimeInMarket;
                    requestDto.TimeToFinish = item.TimeToFinish;
                    requestDto.TimeToSellMax = item.TimeToSellMax;
                    requestDto.TimeToSellMin = item.TimeToSellMin;
                    requestDto.TypologyID = item.TypologyID;
                    requestDto.UsageLicenseID = item.UsageLicenseID;
                    requestDto.UsageLicenseNumber = item.UsageLicenseNumber;
                    requestDto.ValidBuildPermitID = item.ValidBuildPermitID;
                    requestDto.ValidBuildPermitNumber = item.ValidBuildPermitNumber;
                    requestDto.ValuationExclusionsActiveFlags = item.ValuationExclusionsActiveFlags;
                    requestDto.ValuationID = item.ValuationID;
                    requestDto.ValueAfterWorks = item.ValueAfterWorks;
                    requestDto.ValueAsIs = item.ValueAsIs;
                    requestDto.WithInfrastructuresID = item.WithInfrastructuresID;
                    requestDto.WorksDescription = item.WorksDescription;

                    //ConvertToDto(item.ValuationCMVM, requestDto);

                    //requestDto.RequestBrokersOpinions = Convert(item.ValuationBrokersOpinion);
                    //requestDto.RequestCostApproach = Convert(item.ValuationCostApproach);
                    //requestDto.RequestIncomeMethod = Convert(item.ValuationIncomeMethod);
                    //requestDto.RequestLandMain = Convert(item.ValuationLandMain);
                    //requestDto.RequestLandMarketApproaches = Convert(item.ValuationLandMarketApproach);
                    //requestDto.RequestLandMarketComparables = Convert(item.ValuationLandMarketComparable);
                    //requestDto.RequestLandMethodCost = Convert(item.ValuationLandMethodCost);
                    //requestDto.RequestMarketComparables = Convert(item.ValuationMarketComparable);
                }
            }

            return valuationDto;
        }

        //async Task<RequestDto> IValuation.GetRequest(long requestId)
        //{
        //    var dto = new RequestDto();

        //    if (requestId > 0)
        //    {
        //        using (var dbContext = new Entities())
        //        {
        //            dto = Converters.ConvertRequest(FirstOrDefault<Request>(x => x.RequestID == requestId && x.IsActive, dbContext));
                    
        //            dto.AppraiserCompanyName = await Task.Run(() => (from c in dbContext.Company.AsNoTracking()
        //                                                                where c.CompanyID == dto.AppraiserCompanyID
        //                                                                select c.CompanyName).FirstOrDefault());
        //            dto.AppraiserUserName = await Task.Run(() => (from aun in dbContext.User.AsNoTracking()
        //                                                            where aun.UserID == dto.AppraiserUserID
        //                                                            select aun.DisplayName).FirstOrDefault());
        //            dto.CreatedByUserName = await Task.Run(() => (from aun in dbContext.User.AsNoTracking()
        //                                                            where aun.UserID == dto.CreatedByUserID
        //                                                            select aun.DisplayName).FirstOrDefault());
        //            dto.ModifiedByUserName = await Task.Run(() => (from aun in dbContext.User.AsNoTracking()
        //                                                            where aun.UserID == dto.ModifiedByUserID
        //                                                            select aun.DisplayName).FirstOrDefault());
        //            dto.RequesterUserName = await Task.Run(() => (from aun in dbContext.User.AsNoTracking()
        //                                                            where aun.UserID == dto.RequesterUserID
        //                                                            select aun.DisplayName).FirstOrDefault());
        //            dto.TechnicianUserName = await Task.Run(() => (from aun in dbContext.User.AsNoTracking()
        //                                                            where aun.UserID == dto.TechnicianUserID
        //                                                            select aun.DisplayName).FirstOrDefault());
        //            dto.InvestorName = await Task.Run(() => (from inv in dbContext.Investor.AsNoTracking()
        //                                                        where inv.InvestorID == dto.InvestorID
        //                                                        select inv.InvestorName).FirstOrDefault());

        //            // Total Documents
        //            dto.RequestDocument.TotalPhotos = await Task.Run(() => (from rd in dbContext.RequestDocument.AsNoTracking()
        //                                                                    join dt in dbContext.DocumentType.AsNoTracking() on rd.DocumentTypeID equals dt.DocumentTypeID
        //                                                                    where rd.RequestID == requestId && rd.IsActive
        //                                                                    && dt.DocumentTypeName == Models.Valuations.Enums.DocumentTypeName.DocumentTypePhoto
        //                                                                    select rd).Count());
        //            var documents = await Task.Run(() => (from rd in dbContext.RequestDocument.AsNoTracking()
        //                                                    join dt in dbContext.DocumentType.AsNoTracking() on rd.DocumentTypeID equals dt.DocumentTypeID
        //                                                    where rd.RequestID == requestId && dt.DocumentTypeName != Models.Valuations.Enums.DocumentTypeName.DocumentTypePhoto && rd.IsActive
        //                                                    select new { DocumentTypeName = dt.DocumentTypeName })
        //                                                    .GroupBy(o => o.DocumentTypeName)
        //                                                    .ToArray());
        //            //foreach (var item in documents)
        //            Parallel.ForEach(documents, item =>
        //            {
        //                dto.RequestDocument.TotalDocumentsPerType.Add(item.Key, item.ToArray().Select(x => x.DocumentTypeName == item.Key).Count());
        //                dto.RequestDocument.TotalDocuments += item.ToArray().Select(x => x.DocumentTypeName == item.Key).Count();
        //            });

        //            // Authorized WorkflowActions
        //            dto.WorkflowActions = GetQueryWorkflowActions(userId: SecurityManagement.UserID, requestId: requestId);
        //        }
        //    }

        //    return dto;
        //}

        NearbyLocationResponse[] IValuation.GetRequestLocations(long requestId)
        {
            NearbyLocationResponse[] response = null;

            if (requestId > 0)
            {
                response = GetQueryRequestLocations(requestId, DateTime.Today.AddDays(-180));
            }

            return response;
        }

        async Task<PagedList<RequestGridResponse>> IValuation.GetRequests(SearchFiltersRequest filters)
        {
            PagedList<RequestGridResponse> response = new PagedList<RequestGridResponse>();
            
            using (var dbContext = new Entities())
            {
                var iquery = await Task.Run(() => (from req in dbContext.Request.AsNoTracking()
                              join rt in dbContext.RequestType.AsNoTracking() on req.RequestTypeID equals rt.RequestTypeID
                              join mth in dbContext.Method.AsNoTracking() on rt.MethodID equals mth.MethodID into mthn
                              from mth in mthn.DefaultIfEmpty()
                              join at in dbContext.AssetType.AsNoTracking() on rt.AssetTypeID equals at.AssetTypeID into atn
                              from at in atn.DefaultIfEmpty()
                              join ast in dbContext.AssetSubType.AsNoTracking() on req.AssetSubTypeID equals ast.AssetSubTypeID into astn
                              from ast in astn.DefaultIfEmpty()
                              join c in dbContext.Company.AsNoTracking() on req.PartnerCompanyID equals c.CompanyID into cn
                              from c in cn.DefaultIfEmpty()
                              join appraiser in dbContext.User.AsNoTracking() on req.PartnerUserID equals appraiser.UserID into appraisern
                              from appraiser in appraisern.DefaultIfEmpty()
                              join creator in dbContext.User.AsNoTracking() on req.CreatedByUserID equals creator.UserID into creatorn
                              from creator in creatorn.DefaultIfEmpty()
                              join requester in dbContext.User.AsNoTracking() on req.RequesterUserID equals requester.UserID into requestern
                              from requester in requestern.DefaultIfEmpty()
                              join ms in dbContext.MemberShip.AsNoTracking() on req.RequesterUserID equals ms.UserID into msn
                              from ms in msn.DefaultIfEmpty()
                              join technician in dbContext.User.AsNoTracking() on req.AssigneeUserID equals technician.UserID into techniciann
                              from technician in techniciann.DefaultIfEmpty()
                              join rg in dbContext.RequestGroup.AsNoTracking() on req.RequestGroupID equals rg.RequestGroupID into rgn
                              from rg in rgn.DefaultIfEmpty()
                              join wfs in dbContext.WorkFlowStatus.AsNoTracking() on req.WorkFlowStatusID equals wfs.WorkFlowStatusID into wfsn
                              from wfs in wfsn.DefaultIfEmpty()
                              join rre in dbContext.Valuation.AsNoTracking() on req.RequestID equals rre.RequestID into rren
                              from rre in rren.DefaultIfEmpty()
                              join county in dbContext.County.AsNoTracking() on req.CountyID equals county.CountyID into countyn
                              from county in countyn.DefaultIfEmpty()
                              where req.IsActive == true
                              select new
                              {
                                  AppraiserCompanyID = req.PartnerCompanyID,
                                  AppraiserCompanyName = c.CompanyName,
                                  AppraiserDueDate = req.PartnerDueDate,
                                  AppraiserUserID = req.PartnerUserID,
                                  AppraiserUserName = appraiser.DisplayName,
                                  AssetSubTypeID = req.AssetSubTypeID,
                                  AssetSubTypeName = ast.AssetSubTypeName,
                                  AssetTypeID = at.AssetTypeID,
                                  AssetTypeName = at.AssetTypeName,
                                  CountyID = req.CountyID,
                                  CountyName = county.County1,
                                  CreationDate = req.CreatedDate,
                                  CreatedByUserID = req.CreatedByUserID,
                                  CreatedByUserName = creator.DisplayName,
                                  MethodID = rt.MethodID,
                                  ModifiedDate = req.ModifiedDate,
                                  RequestDueDate = req.RequestDueDate,
                                  RequesterUserID = req.RequesterUserID,
                                  RequesterUserName = requester.DisplayName,
                                  RequestGroupID = req.RequestGroupID,
                                  RequestGroupName = rg.RequestGroupName,
                                  RequestID = req.RequestID,
                                  RequestTypeID = req.RequestTypeID,
                                  RequestTypeName = rt.RequestTypeName,
                                  SourceReference = req.SourceReference,
                                  UserDepartmentID = requester.UserDepartmentID,
                                  TechnicianUserID = req.AssigneeUserID,
                                  TechnicianUserName = technician.DisplayName,
                                  ValuationDate = req.RequestDate,
                                  ValuationMethod = mth.MethodName,
                                  ValuationMethodName = mth.MethodName,
                                  WorkFlowStatusID = req.WorkFlowStatusID,
                                  WorkflowStatusName = wfs.WorkFlowStatusName,
                                  WorkFlowStatusAbrev = wfs.WorkFlowStatusAbrev,
                              }));

                if (iquery.Any())
                {
                    // Find the roles of user
                    var roles = SecurityManagement.GetRoles(SecurityManagement.UserID);
                    if (roles.Any())
                    {
                        foreach (var role in roles)
                        {
                            if (role.RoleCode.Contains(Models.Common.Enums.RoleName.Requester))
                            {
                                iquery = iquery.Where(x => x.RequesterUserID == SecurityManagement.UserID);
                            }
                            else if (role.RoleCode.Contains(Models.Common.Enums.RoleName.AppraiserLeader)) // IMPORTANT: this condition MUST PRECEDE the condition bellow !!
                            {
                                var companyID = FirstOrDefault<User>(u => u.UserID == SecurityManagement.UserID).CompanyID;
                                iquery = iquery.Where(x => x.AppraiserCompanyID == companyID);
                            }
                            else if (role.RoleCode.Contains(Models.Common.Enums.RoleName.Appraiser))   // IMPORTANT: this condition MUST FOLLOW the condition above !!
                            {
                                var companyID = FirstOrDefault<User>(u => u.UserID == SecurityManagement.UserID).CompanyID;
                                iquery = iquery.Where(x => x.AppraiserCompanyID == companyID && x.AppraiserUserID == SecurityManagement.UserID);
                            }
                        }
                    }

                    // Header Filters
                    iquery = (filters.AppraiserCompanyId.HasValue && filters.AppraiserCompanyId.Value > 0) ? iquery.Where(x => x.AppraiserCompanyID == filters.AppraiserCompanyId.Value) : iquery;
                    iquery = (filters.AppraiserDueDate.HasValue && filters.AppraiserDueDate.Value > DateTime.MinValue) ? iquery.Where(x => x.AppraiserDueDate == filters.AppraiserDueDate.Value) : iquery;
                    iquery = (filters.AppraiserUserId.HasValue && filters.AppraiserUserId.Value > 0) ? iquery.Where(x => x.AppraiserUserID == filters.AppraiserUserId.Value) : iquery;
                    iquery = (filters.AssetSubTypeId.HasValue && filters.AssetSubTypeId.Value > 0) ? iquery.Where(x => x.AssetSubTypeID == filters.AssetSubTypeId.Value) : iquery;
                    iquery = (filters.AssetTypeId.HasValue && filters.AssetTypeId.Value > 0) ? iquery.Where(x => x.AssetTypeID == filters.AssetTypeId.Value) : iquery;
                    iquery = (filters.CreationDate.HasValue && filters.CreationDate.Value > DateTime.MinValue) ? iquery.Where(x => x.CreationDate == filters.CreationDate.Value) : iquery;
                    iquery = (filters.CountyId.HasValue && filters.CountyId.Value > 0) ? iquery.Where(x => x.CountyID == filters.CountyId.Value) : iquery;
                    iquery = (filters.MethodId.HasValue && filters.MethodId.Value > 0) ? iquery.Where(x => x.MethodID == filters.MethodId.Value) : iquery;
                    iquery = (filters.RequestId.HasValue && filters.RequestId.Value > 0) ? iquery.Where(x => x.RequestID == filters.RequestId.Value) : iquery;
                    iquery = (filters.WorkflowStatusId.HasValue && filters.WorkflowStatusId.Value > 0) ? iquery.Where(x => x.WorkFlowStatusID == filters.WorkflowStatusId.Value) : iquery;
                    iquery = (filters.TechnicianUserId.HasValue && filters.TechnicianUserId.Value > 0) ? iquery.Where(x => x.TechnicianUserID == filters.TechnicianUserId.Value) : iquery;
                    iquery = (filters.RequesterUserId.HasValue && filters.RequesterUserId.Value > 0) ? iquery.Where(x => x.RequesterUserID == filters.RequesterUserId.Value) : iquery;

                    // RequestDueDate
                    if (filters.RequestDueDate.HasValue && filters.RequestDueDate.Value > DateTime.MinValue)
                    {
                        var dateStart = Infrastructure.Helpers.Format.AdjustDateStart(filters.RequestDueDate.Value);
                        var dateEnd = Infrastructure.Helpers.Format.AdjustDateEnd(filters.RequestDueDate.Value);
                        iquery = iquery.Where(x => x.RequestDueDate >= dateStart && x.RequestDueDate <= dateEnd);
                    }

                    // SourceReference
                    if (!string.IsNullOrWhiteSpace(filters.SourceReference) && !string.IsNullOrWhiteSpace(filters.SourceReferenceAdvanced))
                    {
                        var pquery = iquery.Where(x => x.SourceReference == filters.SourceReference);
                        var p2query = iquery.Where(x => x.SourceReference == filters.SourceReferenceAdvanced);
                        iquery = pquery.Concat(p2query);
                    }
                    else
                    {
                        iquery = !string.IsNullOrWhiteSpace(filters.SourceReference) ? iquery.Where(x => x.SourceReference == filters.SourceReference) : iquery;
                        iquery = !string.IsNullOrWhiteSpace(filters.SourceReferenceAdvanced) ? iquery.Where(x => x.SourceReference == filters.SourceReferenceAdvanced) : iquery;
                    }

                    #region Advanced Filters
                    iquery = (filters.AppraiserCompanyIds != null && filters.AppraiserCompanyIds.Length > 0) ? iquery.Where(x => filters.AppraiserCompanyIds.Contains(x.AppraiserCompanyID.Value)) : iquery;
                    iquery = (filters.AppraiserUserIds != null && filters.AppraiserUserIds.Length > 0) ? iquery.Where(x => filters.AppraiserUserIds.Contains(x.AppraiserUserID.Value)) : iquery;
                    iquery = (filters.AssetSubTypeIds != null && filters.AssetSubTypeIds.Length > 0) ? iquery.Where(x => filters.AssetSubTypeIds.Contains(x.AssetSubTypeID.Value)) : iquery;
                    iquery = (filters.AssetTypeIds != null && filters.AssetTypeIds.Length > 0) ? iquery.Where(x => filters.AssetTypeIds.Contains(x.AssetTypeID)) : iquery;
                    iquery = (filters.CountyIds != null && filters.CountyIds.Length > 0) ? iquery.Where(x => filters.CountyIds.Contains(x.CountyID.Value)) : iquery;
                    iquery = (filters.MethodIds != null && filters.MethodIds.Length > 0) ? iquery.Where(x => filters.MethodIds.Contains(x.MethodID)) : iquery;
                    iquery = (filters.WorkflowStatusIds != null && filters.WorkflowStatusIds.Length > 0) ? iquery.Where(x => filters.WorkflowStatusIds.Contains(x.WorkFlowStatusID)) : iquery;
                    iquery = (filters.TechnicianUserIds != null && filters.TechnicianUserIds.Length > 0) ? iquery.Where(x => filters.TechnicianUserIds.Contains(x.TechnicianUserID.Value)) : iquery;
                    iquery = (filters.RequesterUserIds != null && filters.RequesterUserIds.Length > 0) ? iquery.Where(x => filters.RequesterUserIds.Contains(x.RequesterUserID.Value)) : iquery;
                    iquery = (filters.UserDepartmentIds != null && filters.UserDepartmentIds.Length > 0) ? iquery.Where(x => filters.UserDepartmentIds.Contains(x.UserDepartmentID.Value)) : iquery;

                    // CompletionStatus
                    if (filters.GoalCompletionStatus.HasValue)
                    {
                        switch (filters.GoalCompletionStatus)
                        {
                            // OnTrack = é quando ainda esta dentro do prazo, porém, até 3 dias antes do prazo final. Porque quando estiver a 3 dias já se torna Behind.
                            // not WorkflowStatus.ValuationFinish && today less than (AppraiserDueDate - 3)
                            case Models.Valuations.Enums.GoalCompletionStatus.OnTrack:
                                {
                                    iquery = iquery.Where(x => (x.WorkFlowStatusAbrev != Models.Valuations.Enums.DocumentType.FINISH
                                                            && x.WorkFlowStatusAbrev != Models.Valuations.Enums.DocumentType.CANCELED)
                                                            && DateTime.Today < DbFunctions.AddDays(x.AppraiserDueDate, -3));
                                    break;
                                }
                            // Behind = é quando esta próximo do prazo final. Neste caso considera-se 3 dias antes do AppraiserDueDate
                            // not WorkflowStatus.ValuationFinish && today greater than or equal to (AppraiserDueDate - 3) && today less than or equal AppraiserDueDate
                            case Models.Valuations.Enums.GoalCompletionStatus.Behind:
                                {
                                    iquery = iquery.Where(x => (x.WorkFlowStatusAbrev != Models.Valuations.Enums.DocumentType.FINISH
                                                            && x.WorkFlowStatusAbrev != Models.Valuations.Enums.DocumentType.CANCELED)
                                                            && DateTime.Today >= DbFunctions.AddDays(x.AppraiserDueDate, -3)
                                                            && DateTime.Today <= x.AppraiserDueDate);
                                    break;
                                }
                            // Overdue = not WorkflowStatus.ValuationFinish && today greater than AppraiserDueDate
                            case Models.Valuations.Enums.GoalCompletionStatus.Overdue:
                                {
                                    iquery = iquery.Where(x => (x.WorkFlowStatusAbrev != Models.Valuations.Enums.DocumentType.FINISH
                                                            && x.WorkFlowStatusAbrev != Models.Valuations.Enums.DocumentType.CANCELED)
                                                            && DateTime.Today > x.AppraiserDueDate);
                                    break;
                                }
                            // Complete = WorkflowStatus.ValuationFinish
                            case Models.Valuations.Enums.GoalCompletionStatus.Complete:
                                {
                                    iquery = iquery.Where(x => x.WorkFlowStatusAbrev == Models.Valuations.Enums.DocumentType.FINISH
                                                            || x.WorkFlowStatusAbrev == Models.Valuations.Enums.DocumentType.CANCELED);
                                    break;
                                }
                            default:
                                break;
                        }
                    }

                    // MostRecentOptions
                    if (filters.MostRecentOptions.HasValue)
                    {
                        switch (filters.MostRecentOptions)
                        {
                            case MostRecentOptions.ClosedRecently:
                                iquery = iquery.Where(x => x.WorkFlowStatusAbrev == Models.Valuations.Enums.DocumentType.FINISH
                                                        && x.ModifiedDate >= DbFunctions.AddDays(DateTime.Today, -5));
                                break;

                            case MostRecentOptions.UpdatedRecently:
                                iquery = iquery.Where(x => x.ModifiedDate >= DbFunctions.AddDays(DateTime.Today, -5));
                                break;

                            case MostRecentOptions.ViewedRecently:
                                break;

                            default:
                                break;
                        }
                    }

                    // AppraiserDueDate
                    if (filters.AppraiserDueDateMin.HasValue && filters.AppraiserDueDateMin.Value > DateTime.MinValue
                        && filters.AppraiserDueDateMax.HasValue && filters.AppraiserDueDateMax.Value > DateTime.MinValue)
                    {
                        var dateStart = Infrastructure.Helpers.Format.AdjustDateStart(filters.AppraiserDueDateMin.Value);
                        var dateEnd = Infrastructure.Helpers.Format.AdjustDateEnd(filters.AppraiserDueDateMax.Value);
                        iquery = iquery.Where(x => x.AppraiserDueDate.Value >= dateStart && x.AppraiserDueDate.Value <= dateEnd);
                    }
                    else if (filters.AppraiserDueDateMin.HasValue && filters.AppraiserDueDateMin.Value > DateTime.MinValue)
                    {
                        var dateStart = Infrastructure.Helpers.Format.AdjustDateStart(filters.AppraiserDueDateMin.Value);
                        iquery = iquery.Where(x => x.AppraiserDueDate >= dateStart);
                    }
                    else if (filters.AppraiserDueDateMax.HasValue && filters.AppraiserDueDateMax.Value > DateTime.MinValue)
                    {
                        var dateEnd = Infrastructure.Helpers.Format.AdjustDateEnd(filters.AppraiserDueDateMax.Value);
                        iquery = iquery.Where(x => x.AppraiserDueDate <= dateEnd);
                    }

                    // CreationDate
                    if (filters.CreationDateMin.HasValue && filters.CreationDateMin.Value > DateTime.MinValue
                        && filters.CreationDateMax.HasValue && filters.CreationDateMax.Value > DateTime.MinValue)
                    {
                        var dateStart = Infrastructure.Helpers.Format.AdjustDateStart(filters.CreationDateMin.Value);
                        var dateEnd = Infrastructure.Helpers.Format.AdjustDateEnd(filters.CreationDateMax.Value);
                        iquery = iquery.Where(x => x.CreationDate >= dateStart && x.CreationDate <= dateEnd);
                    }
                    else if (filters.CreationDateMin.HasValue && filters.CreationDateMin.Value > DateTime.MinValue)
                    {
                        var dateStart = Infrastructure.Helpers.Format.AdjustDateStart(filters.CreationDateMin.Value);
                        iquery = iquery.Where(x => x.CreationDate >= dateStart);
                    }
                    else if (filters.CreationDateMax.HasValue && filters.CreationDateMax.Value > DateTime.MinValue)
                    {
                        var dateEnd = Infrastructure.Helpers.Format.AdjustDateEnd(filters.CreationDateMax.Value);
                        iquery = iquery.Where(x => x.CreationDate <= dateEnd);
                    }

                    // RequestDueDate
                    if (filters.RequestDueDateMin.HasValue && filters.RequestDueDateMin.Value > DateTime.MinValue
                        && filters.RequestDueDateMax.HasValue && filters.RequestDueDateMax.Value > DateTime.MinValue)
                    {
                        var dateStart = Infrastructure.Helpers.Format.AdjustDateStart(filters.RequestDueDateMin.Value);
                        var dateEnd = Infrastructure.Helpers.Format.AdjustDateEnd(filters.RequestDueDateMax.Value);
                        iquery = iquery.Where(x => x.RequestDueDate.Value >= dateStart && x.RequestDueDate.Value <= dateEnd);
                    }
                    else if (filters.RequestDueDateMin.HasValue && filters.RequestDueDateMin.Value > DateTime.MinValue)
                    {
                        var dateStart = Infrastructure.Helpers.Format.AdjustDateStart(filters.RequestDueDateMin.Value);
                        iquery = iquery.Where(x => x.RequestDueDate >= dateStart);
                    }
                    else if (filters.RequestDueDateMax.HasValue && filters.RequestDueDateMax.Value > DateTime.MinValue)
                    {
                        var dateEnd = Infrastructure.Helpers.Format.AdjustDateEnd(filters.RequestDueDateMax.Value);
                        iquery = iquery.Where(x => x.RequestDueDate <= dateEnd);
                    }
                    # endregion Advanced Filters

                    // General Search
                    if (!string.IsNullOrWhiteSpace(filters.AllFields) && filters.AllFields.Length >= 2)
                    {
                        int requestId = 0;
                        int.TryParse(filters.AllFields, out requestId);

                        iquery = (requestId > 0) ? iquery.Where(x => x.RequestID == requestId) : iquery;
                        iquery = iquery.Where(x => x.AppraiserCompanyName.Contains(filters.AllFields)).Any() == true ? iquery.Where(x => x.AppraiserCompanyName.Contains(filters.AllFields)) : iquery;
                        iquery = iquery.Where(x => x.AppraiserUserName.Contains(filters.AllFields)).Any() == true ? iquery.Where(x => x.AppraiserUserName.Contains(filters.AllFields)) : iquery;
                        iquery = iquery.Where(x => x.RequestGroupName.Contains(filters.AllFields)).Any() == true ? iquery.Where(x => x.RequestGroupName.Contains(filters.AllFields)) : iquery;
                        iquery = iquery.Where(x => x.SourceReference.Contains(filters.AllFields)).Any() == true ? iquery.Where(x => x.SourceReference.Contains(filters.AllFields)) : iquery;
                        iquery = iquery.Where(x => x.TechnicianUserName.Contains(filters.AllFields)).Any() == true ? iquery.Where(x => x.TechnicianUserName.Contains(filters.AllFields)) : iquery;
                        iquery = iquery.Where(x => x.WorkflowStatusName.Contains(filters.AllFields)).Any() == true ? iquery.Where(x => x.WorkflowStatusName.Contains(filters.AllFields)) : iquery;
                    }

                    var queryRequestGridResponse = await Task.Run(() => (from req in iquery
                                                                         select new RequestGridResponse()
                                                                         {
                                                                             AppraiserCompanyName = req.AppraiserCompanyName,
                                                                             AppraiserDueDate = req.AppraiserDueDate,
                                                                             AppraiserUserName = req.AppraiserUserName,
                                                                             AssetSubTypeName = req.AssetSubTypeName,
                                                                             AssetType = req.AssetTypeName,
                                                                             CountyID = req.CountyID,
                                                                             CountyName = req.CountyName,
                                                                             CreatedByDate = req.CreationDate,
                                                                             CreatedByUserName = req.CreatedByUserName,
                                                                             RequestDueDate = req.RequestDueDate,
                                                                             RequesterUserName = req.RequesterUserName,
                                                                             RequestGroupName = req.RequestGroupName,
                                                                             RequestId = req.RequestID,
                                                                             RequestType = req.RequestTypeName,
                                                                             SourceReference = req.SourceReference,
                                                                             TechnicianUserName = req.TechnicianUserName,
                                                                             ValuationDate = req.ValuationDate,
                                                                             ValuationMethod = req.ValuationMethodName,
                                                                             WorkflowStatusID = req.WorkFlowStatusID,
                                                                             WorkflowStatusName = req.WorkflowStatusName,
                                                                             WorkFlowStatusAbrev = req.WorkFlowStatusAbrev
                                                                         }).Distinct());

                    var orderField = string.Empty;
                    Func<RequestGridResponse, Object> orderByFunc = null;
                    string direction = string.Empty;
                    IOrderedEnumerable<RequestGridResponse> requestGridResponseOrdered = null;

                    for (int i = 0; i < filters.SortBy.Length; i++)
                    {
                        switch (filters.SortBy[i].CollumnName)
                        {
                            case "RequestId":
                                {
                                    orderByFunc = item => item.RequestId;
                                    direction = filters.SortBy[i].Direction;
                                    break;
                                }
                            case "SourceReference":
                                {
                                    orderByFunc = item => item.SourceReference;
                                    direction = filters.SortBy[i].Direction;
                                    break;
                                }
                            case "AssetType":
                                {
                                    orderByFunc = item => item.AssetType;
                                    direction = filters.SortBy[i].Direction;
                                    break;
                                }
                            case "ValuationMethod":
                                {
                                    orderByFunc = item => item.ValuationMethod;
                                    direction = filters.SortBy[i].Direction;
                                    break;
                                }
                            case "WorkflowStatusName":
                                {
                                    orderByFunc = item => item.WorkflowStatusName;
                                    direction = filters.SortBy[i].Direction;
                                    break;
                                }
                            case "TechnicianUserName":
                                {
                                    orderByFunc = item => item.TechnicianUserName;
                                    direction = filters.SortBy[i].Direction;
                                    break;
                                }
                            case "AppraiserUserName":
                                {
                                    orderByFunc = item => item.AppraiserUserName;
                                    direction = filters.SortBy[i].Direction;
                                    break;
                                }
                            case "AppraiserCompanyName":
                                {
                                    orderByFunc = item => item.AppraiserCompanyName;
                                    direction = filters.SortBy[i].Direction;
                                    break;
                                }
                            default:
                                {
                                    orderByFunc = item => item.AppraiserDueDate;
                                    direction = filters.SortBy[i].Direction;
                                    break;
                                }
                        }
                    }

                    if (orderByFunc == null || direction == null)
                    {
                        orderByFunc = item => item.AppraiserDueDate;
                        direction = "asc";
                    }

                    switch (direction)
                    {
                        case "desc":
                            {
                                requestGridResponseOrdered = queryRequestGridResponse.OrderByDescending(orderByFunc);
                                break;
                            }

                        default:
                            {
                                requestGridResponseOrdered = queryRequestGridResponse.OrderBy(orderByFunc);
                                break;
                            }
                    }

                    response.Rows = await Task.Run(() => requestGridResponseOrdered.Skip((filters.PageNumber - 1) * filters.PageSize).Take(filters.PageSize).Distinct().ToArray());
                    response.TotalRows = iquery.Count();
                    response.PageNumber = filters.PageNumber;
                    response.PageSize = filters.PageSize;
                }
            }

            return response;
        }

        /// <summary>
        /// Find all Requests created by user where status is open
        /// If haven't, find the requests is closed
        /// </summary>
        /// <returns></returns>
        async Task<RequestWithThumbnailResponse[]> IValuation.GetRequestsWithThumbnail(RequestWithThumbnailsRequest param)
        {
            var response = new RequestWithThumbnailResponse[] { };

            var iquery = await Task.Run(() => GetQueryRequestsWithThumbnail(SecurityManagement.UserID));
            
            if (iquery.Any())
            {
                iquery = iquery.Where(wst => wst.WorkFlowStatusAbrev != Models.Valuations.Enums.DocumentType.FINISH && wst.WorkFlowStatusAbrev != Models.Valuations.Enums.DocumentType.CANCELED).Take(param.PageSize);

                if (!iquery.Any())
                {
                    iquery = iquery.Where(wst => wst.WorkFlowStatusAbrev == Models.Valuations.Enums.DocumentType.FINISH && wst.WorkFlowStatusAbrev == Models.Valuations.Enums.DocumentType.CANCELED).Take(param.PageSize);
                }
            }

            if (iquery.Any())
            {
                response = await Task.Run(() => (from query in iquery
                                                    select new RequestWithThumbnailResponse
                                                    {
                                                        AssetTypeName = query.AssetTypeName,
                                                        MethodName = query.MethodName,
                                                        OMV = query.OMV,
                                                        RequestID = query.RequestID,
                                                        SourceReference = query.SourceReference,
                                                        WorkFlowStatusName = query.WorkFlowStatusName,
                                                    }).ToArray());

                foreach (var item in response)
                {
                    var images = await Task.Run(() => GetQueryPhotosThumbnails(item.RequestID).FirstOrDefault());

                    item.MainPhoto = images == null ? null : Infrastructure.Helpers.ImageManipulation.GetImage(param.Width, param.Height, Infrastructure.Helpers.FileManager.GetAbsolutePath(images.DocumentTypeName, item.RequestID, images.DocumentPath), true);
                }
            }

            return response;
        }

        RequestTypeResponse IValuation.GetRequestType(long requestTypeId)
        {
            RequestTypeResponse response = null;

            using (var dbContext = new Entities())
            {
                var query = FirstOrDefault<RequestType>(r => r.RequestTypeID == requestTypeId && r.IsActive == true, dbContext);
                response = new RequestTypeResponse
                {
                    RequestTypeID = query.RequestTypeID,
                    RequestTypeName = query.RequestTypeName,
                    AssetTypeID = query.AssetTypeID,
                    AssetTypeName = query.AssetType.AssetTypeName,
                    MethodID = query.MethodID,
                    TemplateID = query.TemplateID,
                    WorkFlowID = query.WorkFlowID
                };
            }

            return response;
        }

        GeneralResponse<bool> IValuation.SaveComment(CommentSaveRequest param)
        {
            var generalResponse = new GeneralResponse<bool>();

            using (var dbContext = new Entities())
            {
                var entity = new RequestComment()
                {
                    Comment = param.Comment,
                    CommentTypeID = param.CommentTypeID,
                    CreatedByUserID = SecurityManagement.CreatedByUserID,
                    CreatedDate = SecurityManagement.CreatedDate,
                    IsActive = true,
                    ParentID = param.ParentID,
                    RequestID = param.RequestID,
                    SessionID = SecurityManagement.SessionID,
                    ToUserID = param.ToUserID,
                    UserID = SecurityManagement.UserID,
                };
                generalResponse = Add(entity, dbContext);
            }

            if (generalResponse.Success)
            {
                Notification.AddNotificationCommentMade += new AddNotificationCommentHandler(Notification.AddNotificationComment);
                Notification.AddNotificationComment(param.RequestID, param.CommentTypeID);
            }

            return generalResponse;
        }

        GeneralResponse<bool> IValuation.SaveDocument(DocumentRequest param)
        {
            var generalResponse = new GeneralResponse<bool>();
            var documentTypeName = string.Empty;

            var entity = new RequestDocument
            {
                SessionID = SecurityManagement.SessionID,
                Description = param.Description,
                DocumentTypeID = param.DocumentTypeID,
                OriginalName = param.OriginalName,
                RequestID = param.RequestID
            };

            using (var dbContext = new Entities())
            {
                documentTypeName = dbContext.DocumentType.FirstOrDefault(x => x.DocumentTypeID == param.DocumentTypeID).DocumentTypeName;

                // Save informations in data base. Save with false and without file name
                if (entity.RequestDocumentID > 0) // Update
                {
                    entity.IsActive = false;
                    entity.ModifiedByUserID = SecurityManagement.UserID;
                    entity.ModifiedDate = SecurityManagement.ModifiedDate;
                    generalResponse = Update(entity, dbContext);
                }
                else if (entity.RequestDocumentID == 0) // Insert
                {
                    entity.IsActive = false;
                    entity.CreatedByUserID = SecurityManagement.CreatedByUserID;
                    entity.CreatedDate = SecurityManagement.CreatedDate;
                    generalResponse = Add(entity, dbContext);
                }

                if (generalResponse.Success)
                {
                    // Depois de o registro salvo recupera o último ID gravado para compor o nome do ficheiro
                    // Montagem do nome do ficheiro no seguinte formato:
                    // RequestDocumentID + RequestID + UserId + SessionId + DocumentTypeID + TimeStamp
                    var firstPortion = new StringBuilder().Append(entity.RequestDocumentID).Append(param.RequestID).Append(SecurityManagement.UserID).Append(SecurityManagement.SessionID).Append(param.DocumentTypeID).ToString();
                    var timestamp = new StringBuilder().Append(DateTime.Now.Year).Append(DateTime.Now.Month).Append(DateTime.Now.Day).Append(DateTime.Now.Hour).Append(DateTime.Now.Minute).Append(DateTime.Now.Millisecond).ToString();
                    var fileName = new StringBuilder().Append(firstPortion).Append(timestamp).Append(param.FileExtension).ToString();

                    // Mounting absolut path
                    var path = new StringBuilder().Append(Infrastructure.Helpers.FileManager.GetAbsolutePath(documentTypeName, param.RequestID, fileName)).ToString();

                    // Save file in the disc
                    Infrastructure.Helpers.FileManager.ByteArrayToFile(path, param.Document);

                    try
                    {
                        // Create Thumbnail (pdf only)
                        if (param.FileExtension == ".pdf")
                        {
                            var fileNameThumbnail = new StringBuilder().Append(Path.Combine(Infrastructure.Configuration.WebConfig.GetDocumentsThumbnailsPathKeyValue())).Append("\\").Append(param.RequestID.ToString()).ToString();
                            var directory = Infrastructure.Helpers.FileManager.CreateDirectory(fileNameThumbnail);
                            var thumbPath = new StringBuilder().Append(directory).Append("\\").Append(fileName).Append(".jpg").ToString();
                            GhostscriptWrapper.GeneratePageThumb(path, thumbPath, 1, 150, 150);
                        }
                    }
                    catch (Exception ex)
                    {
                        generalResponse.Message = string.Format("An Error Occured : {0} - Inner exception - {1}", ex.Message, ex.InnerException);
                    }

                    // After save file in the disc, make a data base update
                    entity.IsActive = true;
                    entity.DocumentPath = fileName;
                    var result = Update(entity, dbContext);
                    if (result.Success)
                    {
                        generalResponse.Success = result.Success;
                    }
                    else
                    {
                        generalResponse.Success = result.Success;
                        generalResponse.Message = result.Message;
                    }
                }
            }

            return generalResponse;
        }

        GeneralResponse<long> IValuation.SaveRequest(RequestDto dto)
        {
            var generalResponse = new GeneralResponse<long>();

            if (dto != null)
            {
                using (var dbContext = new Entities())
                {
                    try
                    {
                        var entity = GetRequest(dto.RequestID, dbContext);
                        Converters.ConvertDtoToEntity(dto, ref entity, dbContext);

                        dbContext.Entry(entity).State = dto.RequestID == 0 ? EntityState.Added : EntityState.Modified;

                        generalResponse.Success = dbContext.SaveChanges() > 0 ? true : false;

                        if (generalResponse.Success == false)
                        {
                            generalResponse.Message = "The Request could not be saved!";
                        }
                        generalResponse.Response = entity.RequestID;
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                System.Diagnostics.Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        generalResponse.Message = new StringBuilder().AppendLine().Append(Infrastructure.Helpers.Format.MSGEXCECAO).AppendLine(ex.Message).AppendLine().Append(ex.InnerException).AppendLine(ex.StackTrace).ToString();
                        generalResponse.Success = false;
                    }
                }
            }

            return generalResponse;
        }
    }
}
