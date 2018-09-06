using System;
using System.Collections.Generic;
using System.Linq;
using Value.Domain.Models.Valuations.Dto;
using Value.Domain.Models.Valuations.Response;

namespace Value.Infrastructure.Data.Queries
{
    public class ValuationQuery : UnitOfWork
    {
        public RequestDocumentDto GetQueryDocument(long documentId, long requestId, Entities dbContext)
        {
            return (from rd in dbContext.RequestDocument.AsNoTracking()
                    join createdBy in dbContext.User.AsNoTracking() on rd.CreatedByUserID equals createdBy.UserID
                    join dt in dbContext.DocumentType.AsNoTracking() on rd.DocumentTypeID equals dt.DocumentTypeID
                    where rd.RequestID == requestId
                    && rd.RequestDocumentID == documentId
                    && dt.DocumentTypeName != Domain.Models.Valuations.Enums.DocumentTypeName.DocumentTypePhoto
                    && rd.IsActive && dt.IsActive
                    select new RequestDocumentDto()
                    {
                        Description = rd.Description,
                        DocumentID = rd.RequestDocumentID,
                        DocumentTypeID = rd.DocumentTypeID,
                        DocumentTypeName = dt.DocumentTypeName,
                        DocumentPath = rd.DocumentPath,
                        OriginalName = rd.OriginalName,
                        CreatedByUser = createdBy.DisplayName,
                        CreatedByUserID = rd.CreatedByUserID,
                        CreatedDate = rd.CreatedDate,
                        RequestID = rd.RequestID,
                    }
             ).FirstOrDefault();
        }

        public IQueryable<RequestDocumentDto> GetQueryDocuments(long requestId, Entities dbContext)
        {
            return (from rd in dbContext.RequestDocument.AsNoTracking()
                    join createdBy in dbContext.User.AsNoTracking() on rd.CreatedByUserID equals createdBy.UserID
                    join dt in dbContext.DocumentType.AsNoTracking() on rd.DocumentTypeID equals dt.DocumentTypeID
                    where rd.RequestID == requestId
                    && dt.DocumentTypeName != Domain.Models.Valuations.Enums.DocumentTypeName.DocumentTypePhoto
                    && rd.IsActive && dt.IsActive
                    select new RequestDocumentDto()
                    {
                        Description = rd.Description,
                        DocumentID = rd.RequestDocumentID,
                        DocumentTypeID = rd.DocumentTypeID,
                        DocumentTypeName = dt.DocumentTypeName,
                        DocumentPath = rd.DocumentPath,
                        OriginalName = rd.OriginalName,
                        CreatedByUser = createdBy.DisplayName,
                        CreatedByUserID = rd.CreatedByUserID,
                        CreatedDate = rd.CreatedDate,
                        RequestID = rd.RequestID,
                    });
        }

        public IQueryable<RequestDocumentDto> GetQueryDocumentsThumbnails(long requestId, Entities dbContext)
        {
            return (from rd in dbContext.RequestDocument.AsNoTracking()
                    join dt in dbContext.DocumentType.AsNoTracking() on rd.DocumentTypeID equals dt.DocumentTypeID
                    where rd.RequestID == requestId
                    && dt.DocumentTypeName != Domain.Models.Valuations.Enums.DocumentTypeName.DocumentTypePhoto
                    && rd.IsActive && dt.IsActive
                    select new RequestDocumentDto()
                    {
                        DocumentPath = rd.DocumentPath,
                        DocumentTypeID = rd.DocumentTypeID,
                        DocumentTypeName = dt.DocumentTypeName,
                        OriginalName = rd.OriginalName,
                        DocumentID = rd.RequestDocumentID,
                        RequestID = rd.RequestID
                    });
        }

        public IDictionary<long, string> GetQueryLinkedRequests(long requestGroupId)
        {
            using (var dbContext = new Entities())
            {
                return (from req in dbContext.Request.AsNoTracking()
                        where req.RequestGroupID == requestGroupId && req.IsActive
                        select new { req.RequestID, req.SourceReference }).ToDictionary(x => x.RequestID, x => x.SourceReference);
            }
        }

        public RequestDocumentDto GetQueryPhoto(long documentId, long requestId, Entities dbContext)
        {
            return (from rd in dbContext.RequestDocument.AsNoTracking()
                    join createdBy in dbContext.User.AsNoTracking() on rd.CreatedByUserID equals createdBy.UserID
                    join dt in dbContext.DocumentType.AsNoTracking() on rd.DocumentTypeID equals dt.DocumentTypeID
                    where rd.RequestID == requestId
                    && rd.RequestDocumentID == documentId
                    && dt.DocumentTypeName == Domain.Models.Valuations.Enums.DocumentTypeName.DocumentTypePhoto
                    && rd.IsActive && dt.IsActive
                    select new RequestDocumentDto()
                    {
                        Description = rd.Description,
                        DocumentID = rd.RequestDocumentID,
                        DocumentTypeID = rd.DocumentTypeID,
                        DocumentTypeName = dt.DocumentTypeName,
                        DocumentPath = rd.DocumentPath,
                        OriginalName = rd.OriginalName,
                        CreatedByUser = createdBy.DisplayName,
                        CreatedByUserID = rd.CreatedByUserID,
                        CreatedDate = rd.CreatedDate,
                        RequestID = rd.RequestID,
                    }
             ).FirstOrDefault();
        }

        public IQueryable<RequestDocumentDto> GetQueryPhotosThumbnails(long requestId)
        {
            using (var dbContext = new Entities())
            {
                return (from rd in dbContext.RequestDocument.AsNoTracking()
                        join dt in dbContext.DocumentType.AsNoTracking() on rd.DocumentTypeID equals dt.DocumentTypeID
                        where rd.RequestID == requestId
                        && dt.DocumentTypeName == Domain.Models.Valuations.Enums.DocumentTypeName.DocumentTypePhoto
                        && rd.IsActive && dt.IsActive
                        select new RequestDocumentDto()
                        {
                            DocumentPath = rd.DocumentPath,
                            DocumentTypeID = rd.DocumentTypeID,
                            DocumentTypeName = dt.DocumentTypeName,
                            OriginalName = rd.OriginalName,
                            DocumentID = rd.RequestDocumentID,
                            RequestID = rd.RequestID
                        });
            }
        }

        public IQueryable<GetRequestsWithThumbnailDto> GetQueryRequestsWithThumbnail(short userId)
        {
            using (var dbContext = new Entities())
            {
                return (from req in dbContext.Request.AsNoTracking()
                                                   join val in dbContext.Valuation.AsNoTracking() on req.RequestID equals val.RequestID
                                                   join wst in dbContext.WorkFlowStatus.AsNoTracking() on req.WorkFlowStatusID equals wst.WorkFlowStatusID
                                                   join ast in dbContext.AssetSubType.AsNoTracking() on req.AssetSubTypeID equals ast.AssetSubTypeID
                                                   join at in dbContext.AssetType.AsNoTracking() on ast.AssetTypeID equals at.AssetTypeID
                                                   join rt in dbContext.RequestType.AsNoTracking() on req.RequestTypeID equals rt.RequestTypeID
                                                   join m in dbContext.Method.AsNoTracking() on rt.MethodID equals m.MethodID
                                                   where req.RequesterUserID == userId
                                                       && req.IsActive
                                                       && val.IsActive
                                                       && wst.IsActive
                                                       && ast.IsActive
                                                       && at.IsActive
                                                       && rt.IsActive
                                                       && m.IsActive
                                                   orderby req.CreatedDate descending
                                                   select new GetRequestsWithThumbnailDto()
                                                   {
                                                       AssetTypeName = at.AssetTypeName,
                                                       MethodName = m.MethodName,
                                                       OMV = val.OMV,
                                                       RequestID = req.RequestID,
                                                       SourceReference = req.SourceReference,
                                                       WorkFlowStatusAbrev = wst.WorkFlowStatusAbrev,
                                                       WorkFlowStatusName = wst.WorkFlowStatusName
                                                   });

            }
        }

    public NearbyLocationResponse[] GetQueryRequestLocations(long requestId, DateTime dateRange)
        {
            using (var dbContext = new Entities())
            {
                return (from req in dbContext.Request.AsNoTracking()
                        where req.PartnerCompanyID == (from reqin in dbContext.Request
                                                       where reqin.RequestID == requestId && reqin.IsActive
                                                       select reqin.PartnerCompanyID).FirstOrDefault()
                        && req.CreatedDate >= dateRange
                        && (!string.IsNullOrEmpty(req.Latitude) && !string.IsNullOrEmpty(req.Longitude))
                        && req.IsActive
                        select new NearbyLocationResponse
                        {
                            RequestID = req.RequestID,
                            Latitude = req.Latitude,
                            Longitude = req.Longitude
                        }
                ).ToArray();
            }
        }

        public IQueryable<DocumentResponse> GetQueryPhotos(long requestId)
        {
            using (var dbContext = new Entities())
            {
                var response = (from rd in dbContext.RequestDocument.AsNoTracking()
                                join createdBy in dbContext.User.AsNoTracking() on rd.CreatedByUserID equals createdBy.UserID
                                join dt in dbContext.DocumentType.AsNoTracking() on rd.DocumentTypeID equals dt.DocumentTypeID
                                where rd.RequestID == requestId
                                && dt.DocumentTypeName == Domain.Models.Valuations.Enums.DocumentTypeName.DocumentTypePhoto
                                && rd.IsActive && dt.IsActive
                                select new DocumentResponse()
                                {
                                    CreatedByUser = createdBy.DisplayName,
                                    CreatedDate = rd.CreatedDate,
                                    Description = rd.Description,
                                    DocumentID = rd.RequestDocumentID,
                                    DocumentTypeName = dt.DocumentTypeName,
                                    OriginalName = rd.OriginalName,
                                    RequestID = rd.RequestID,
                                });

                return response;
            }
        }

        public WorkflowActionsDto[] GetQueryWorkflowActions(short userId, long requestId)
        {
            WorkflowActionsDto[] response = null;
            
            using (var dbContext = new Entities())
            {
                response = (from ms in dbContext.MemberShip.AsNoTracking()
                                join auth in dbContext.Authorization.AsNoTracking() on ms.RoleID equals auth.RoleID
                                join act in dbContext.Action.AsNoTracking() on auth.ActionID equals act.ActionID
                                join wfa in dbContext.WorkFlowAction.AsNoTracking() on act.ActionID equals wfa.ActionID
                                join st in dbContext.StatusTransition.AsNoTracking() on wfa.WorkFlowActionID equals st.WorkFlowActionID
                                join req in dbContext.Request.AsNoTracking() on st.InicialWorkFlowStatusID equals req.WorkFlowStatusID
                                join wfsi in dbContext.WorkFlowStatus.AsNoTracking() on st.InicialWorkFlowStatusID equals wfsi.WorkFlowStatusID
                                join wfsf in dbContext.WorkFlowStatus.AsNoTracking() on st.FinalWorkFlowStatusID equals wfsf.WorkFlowStatusID
                                where ms.IsActive
                                    && auth.IsActive
                                    && act.IsActive
                                    && wfa.IsActive
                                    && req.IsActive
                                    && ms.UserID == userId && req.RequestID == requestId
                                select new WorkflowActionsDto
                                {
                                    CurrentWorkFlowStatusID = st.InicialWorkFlowStatusID,
                                    CurrentWorkFlowStatusName = wfsi.WorkFlowStatusName,
                                    NextWorkFlowStatusID = st.FinalWorkFlowStatusID,
                                    NextWorkFlowStatusName = wfsf.WorkFlowStatusName,
                                    StatusTransitionID = st.StatusTransitionID,
                                    StatusTransitionName = st.StatusTransitionName,
                                    WorkFlowActionID = st.WorkFlowActionID,
                                    WorkFlowActionName = wfa.WorkFlowActionName,
                                    IconClassName = wfa.IconClassName,
                                    Confirm = wfa.Confirm,
                                    Validate = wfa.Validate,
                                }).Distinct()
                                .ToArray();
            }

            return response;
        }

        public Request GetRequest(long requestId, Entities dbContext)
        {
            Request entity = null;

            if (requestId > 0 && dbContext != null)
            {
                entity = FirstOrDefault<Request>(x => x.RequestID == requestId && x.IsActive, dbContext);
            }

            return entity;
        }
    }
}
