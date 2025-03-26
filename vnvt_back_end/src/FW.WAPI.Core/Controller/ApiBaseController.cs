using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.Configuration;
using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
///
/// </summary>
namespace FW.WAPI.Core.BaseController
{
    public class ApiBaseController<TDataContext, TEntity> : ControllerBase
    {
        public readonly IBaseService<TDataContext, TEntity> _baseService;
        public readonly ResultCodeTable _resultCode;
        public DataLevel _dataLevel = DataLevel.COMPANY;

        public ApiBaseController(IBaseService<TDataContext, TEntity> baseService
            , IOptions<ResultCodeTable> resultCodeSettings)
        {
            _baseService = baseService;
            _resultCode = resultCodeSettings.Value;
        }

        /// <summary>
        /// Get all data of entity
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> GetAll([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();
            try
            {
                if (request is null && _dataLevel != DataLevel.ALL || request is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    //Check company code exist
                    string companyCode = null;

                    if (request.PostObject is string)
                    {
                        companyCode = request.PostObject as string;
                    }

                    var sortBy = request.SortDTO;

                    //check if server paging
                    if (request.PageIndex != null && request.PageSize != null)
                    {
                        if (request.Fields is null)
                        {
                            var data = await _baseService.GetAllPagingAsync(
                            request.PageIndex.Value, request.PageSize.Value, companyCode, sortBy, _dataLevel, request.Fields);

                            response.Data = data.Data;
                            response.Total = data.Total;
                        }
                        else
                        {
                            var data = await _baseService.GetAllPagingFieldsAsync(
                                request.PageIndex.Value, request.PageSize.Value, request.Fields, companyCode, sortBy, _dataLevel);

                            response.Data = data.Data;
                            response.Total = data.Total;
                        }
                    }
                    else
                    {
                        if (request.Fields is null)
                        {
                            IEnumerable<TEntity> result = await _baseService.GetAllAsync(companyCode, sortBy, _dataLevel);
                            response.Data = result;
                            response.Total = result.Count();
                        }
                        else
                        {
                            dynamic resultField = await _baseService.GetAllFieldsAsync(request.Fields, companyCode, sortBy, _dataLevel);
                            response.Data = resultField;
                            response.Total = resultField.Count;
                        }
                    }

                    response.Code = _resultCode.Ok.Code;
                    response.Message = _resultCode.Ok.Message;
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> GetAllIncludeChildren([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();
            try
            {
                if (request.PostObject is null && _dataLevel != DataLevel.ALL)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    string companyCode = null;

                    if (request.PostObject is string)
                    {
                        companyCode = request.PostObject as string;
                    }

                    var sortBy = request.SortDTO;

                    if (request.PageIndex != null && request.PageSize != null)
                    {
                        var data = await _baseService.GetAllPagingIncludeChildren(request.PageIndex.Value,
                            request.PageSize.Value, companyCode, sortBy, _dataLevel);

                        response.Data = data.Data;
                        response.Total = data.Total;
                    }
                    else
                    {
                        if (request.Fields is null)
                        {
                            IEnumerable<TEntity> result = await _baseService.GetAllIncludeChildrenAsync(companyCode, sortBy, _dataLevel,
                                request.Fields);
                            response.Data = result;
                            response.Total = result.Count();
                        }
                        else
                        {
                            dynamic resultFields = await _baseService.GetAllIncludeChildrenFieldsAsync(request.Fields, companyCode, sortBy, _dataLevel);
                            response.Data = resultFields;
                            response.Total = resultFields.Count;
                        }

                    }

                    response.Code = _resultCode.Ok.Code;
                    response.Message = _resultCode.Ok.Message;
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> Insert([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();
            try
            {
                if (request.PostObject is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var objectToInsert = JsonUtilities.ConvertJsonToObject<TEntity>(request.PostObject.ToString());

                    AsyncRetryPolicy policyRetry;
                    var code = PropertyUtilities.GetPropValueOfObject(objectToInsert, TableColumnConst.CODE_COL);
                    //Check code is generated by autosettingvalue
                    if (code == null)
                    {
                        //create retry policy
                        policyRetry = Policy.HandleInner<SqlException>(ex => ex.Number == 2627).RetryAsync(1);
                        await policyRetry.ExecuteAsync(async () =>
                        {
                            try
                            {
                                var result = await _baseService.Insert(objectToInsert);

                                response.Code = _resultCode.Ok.Code;
                                response.Message = _resultCode.Ok.Message;
                                response.Data = result;
                                response.Total = 1;
                            }
                            catch (Exception ex)
                            {
                                if (ex is DbUpdateException)
                                {
                                    //throw to retry insert
                                    throw;
                                }
                                else if (ex is DuplicateCodeException)
                                {
                                    response.Code = _resultCode.DuplicateKey.Code;
                                    response.Message = _resultCode.DuplicateKey.Message;
                                }
                                else if (ex is FriendlyException)
                                {
                                    response.Code = _resultCode.Friendly.Code;
                                    response.Message = ex.Message;
                                }
                                else
                                {
                                    response.Code = _resultCode.HandleRequestFail.Code;
                                    response.Message = ex.Message;
                                }
                            }
                        });
                    }
                    else
                    {
                        var result = await _baseService.Insert(objectToInsert);

                        response.Code = _resultCode.Ok.Code;
                        response.Message = _resultCode.Ok.Message;
                        response.Data = result;
                        response.Total = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is DuplicateCodeException)
                {
                    response.Code = _resultCode.DuplicateKey.Code;
                    response.Message = _resultCode.DuplicateKey.Message;
                }
                else if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = ex.Message;
                }

            }

            return Ok(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> Update([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();

            try
            {
                if (request.PostObject is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var objectToUpdate = JsonUtilities.ConvertJsonToObject<TEntity>(request.PostObject.ToString());

                    var result = await _baseService.Update(objectToUpdate);

                    response.Code = _resultCode.Ok.Code;
                    response.Message = _resultCode.Ok.Message;
                    response.Data = result;
                    response.Total = 1;
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else if (ex is DbUpdateConcurrencyException)
                {
                    response.Code = _resultCode.ConncurrencyUpdate.Code;
                    response.Message = _resultCode.ConncurrencyUpdate.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = ex.Message;
                }
            }

            return Ok(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> Delete([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();

            try
            {
                if (request.PostObject is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var jsonObject = request.PostObject as JObject;

                    var istExist = jsonObject.Children().FirstOrDefault(x => ((JProperty)x).Name == TableColumnConst.COMPANY_CODE_COL);

                    var code = request.PostObject.Code.Value.ToString();

                    string companyCode = null;

                    if (istExist != null)
                    {
                        companyCode = request.PostObject.CompanyCode.Value.ToString();
                    }

                    var result = await _baseService.Delete(code, companyCode);

                    if (result)
                    {
                        response.Code = _resultCode.Ok.Code;
                        response.Message = _resultCode.Ok.Message;
                        response.Data = result;
                        response.Total = 1;
                    }
                    else
                    {
                        response.Code = _resultCode.DeleteDbFail.Code;
                        response.Message = _resultCode.DeleteDbFail.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else if (ex is DbUpdateConcurrencyException)
                {
                    response.Code = _resultCode.ConncurrencyUpdate.Code;
                    response.Message = _resultCode.ConncurrencyUpdate.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = ex.Message;
                }
            }

            return Ok(response);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Remove([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();

            try
            {
                if (request.PostObject is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var jsonObject = request.PostObject as JObject;

                    var istExist = jsonObject.Children().FirstOrDefault(x => ((JProperty)x).Name == TableColumnConst.COMPANY_CODE_COL);

                    var code = request.PostObject.Code.Value.ToString();

                    string companyCode = null;

                    if (istExist != null)
                    {
                        companyCode = request.PostObject.CompanyCode.Value.ToString();
                    }

                    var result = await _baseService.Remove(code, companyCode);

                    if (result)
                    {
                        response.Code = _resultCode.Ok.Code;
                        response.Message = _resultCode.Ok.Message;
                        response.Data = result;
                        response.Total = 1;
                    }
                    else
                    {
                        response.Code = _resultCode.DeleteDbFail.Code;
                        response.Message = _resultCode.DeleteDbFail.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else if (ex is DbUpdateConcurrencyException)
                {
                    response.Code = _resultCode.ConncurrencyUpdate.Code;
                    response.Message = _resultCode.ConncurrencyUpdate.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> GetByCode([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();

            try
            {
                if (request.PostObject is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var jsonObject = request.PostObject as JObject;
                    string code = null;
                    string companyCode = null;

                    if (jsonObject != null)
                    {
                        companyCode = request.PostObject.CompanyCode.Value.ToString();
                        code = request.PostObject.Code.Value.ToString();
                    }
                    else
                    {
                        code = request.PostObject.ToString();
                    }

                    var result = request.Fields == null ? await _baseService.GetByCode(code, companyCode, _dataLevel)
                        : await _baseService.GetByCodeFields(code, companyCode, _dataLevel, request.Fields, null);

                    response.Code = _resultCode.Ok.Code;
                    response.Message = _resultCode.Ok.Message;
                    response.Data = result;
                    response.Total = 1;
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> GetByCodeIncludeChildren([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();

            try
            {
                if (request.PostObject is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var jsonObject = request.PostObject as JObject;
                    string code = null;
                    string companyCode = null;

                    if (jsonObject != null)
                    {
                        companyCode = request.PostObject.CompanyCode.Value.ToString();
                        code = request.PostObject.Code.Value.ToString();
                    }
                    else
                    {
                        code = request.PostObject.ToString();
                    }

                    var result = request.Fields == null ?
                        await _baseService.GetByCodeIncludeChildrenAsync(code, companyCode, request.Fields) :
                        await _baseService.GetByCodeIncludeChildrenFieldAsync(code, companyCode, _dataLevel, request.Fields);

                    response.Code = _resultCode.Ok.Code;
                    response.Message = _resultCode.Ok.Message;
                    response.Data = result;
                    response.Total = 1;
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> Search([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();

            try
            {
                if (request.Searching is null && request.SortDTO is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var searchConditions = request.Searching;

                    string companyCode = null;

                    if (request.PostObject != null)
                    {
                        companyCode = request.PostObject is string ? request.PostObject as string : null;
                    }

                    var sortDTO = request.SortDTO;

                    if (searchConditions != null && searchConditions.Count > 0)
                    {
                        if (request.PageIndex != null && request.PageSize != null)
                        {
                            if (request.Fields is null)
                            {
                                var result = await _baseService.SearchPaging(searchConditions, request.PageSize.Value, request.PageIndex.Value,
                                 sortDTO, companyCode, _dataLevel);

                                var total = result.Total;

                                var results = result.Data;

                                response.Data = results;

                                response.Total = total;
                            }
                            else
                            {
                                var result = await _baseService.SearchPagingFieldsAsync(searchConditions, request.PageSize.Value, request.PageIndex.Value,
                                 request.Fields, sortDTO, companyCode, _dataLevel);

                                var total = result.Total;
                                var results = result.Data;

                                response.Data = results;

                                response.Total = total;
                            }
                        }
                        else
                        {
                            dynamic results = request.Fields is null ? await _baseService.Search(searchConditions, sortDTO, companyCode, _dataLevel)
                                : await _baseService.SearchFieldsAysnc(searchConditions, request.Fields, sortDTO, companyCode, _dataLevel);

                            var total = results.Count;
                            response.Data = results;
                            response.Total = total;
                        }

                        response.Code = _resultCode.Ok.Code;
                        response.Message = _resultCode.Ok.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> SearchIncludeChildren([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();

            try
            {
                if (request.Searching is null && request.SortDTO is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var searchConditions = request.Searching;

                    string companyCode = null;

                    if (request.PostObject != null)
                    {
                        companyCode = request.PostObject is string ? request.PostObject.ToString() : null;
                    }

                    var sortDTO = request.SortDTO;

                    if (searchConditions != null && searchConditions.Count > 0)
                    {
                        if (request.PageIndex != null && request.PageSize != null)
                        {
                            if (request.Fields is null)
                            {
                                var result = await _baseService.SearchIncludeChildrenPaging(searchConditions, request.PageSize.Value,
                                    request.PageIndex.Value, sortDTO, companyCode, _dataLevel);

                                var total = result.Total;
                                var results = result.Data;


                                response.Data = results;
                                response.Total = total;
                            }
                            else
                            {
                                var result = await _baseService.SearchIncludeChildrenPagingFields(searchConditions, request.PageSize.Value,
                                        request.PageIndex.Value, request.Fields, sortDTO, companyCode, _dataLevel);

                                var total = result.Total;
                                var results = result.Data;

                                response.Data = results;
                                response.Total = total;
                            }

                        }
                        else
                        {
                            dynamic results = request.Fields is null ?
                                await _baseService.SearchIncludeChildren(searchConditions, sortDTO, companyCode, _dataLevel) :
                                await _baseService.SearchIncludeChildrenFieldAsync(searchConditions, request.Fields, sortDTO, companyCode, _dataLevel);

                            var total = results.Count;

                            response.Data = results;
                            response.Code = _resultCode.Ok.Code;
                            response.Total = total;
                        }

                        response.Code = _resultCode.Ok.Code;
                        response.Message = _resultCode.Ok.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// Search Single Async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> SearchSingleAsync([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();

            try
            {
                if (request.Searching is null && request.SortDTO is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var searchConditions = request.Searching;

                    string companyCode = null;

                    if (request.PostObject != null)
                    {
                        companyCode = request.PostObject is string ? request.PostObject as string : null;
                    }

                    var sortDTO = request.SortDTO;

                    if (searchConditions != null && searchConditions.Count > 0)
                    {
                        dynamic results = await _baseService.SearchSingleAsync(searchConditions, request.Fields, sortDTO, companyCode, _dataLevel);
                        response.Data = results;
                    }

                    response.Code = _resultCode.Ok.Code;
                    response.Message = _resultCode.Ok.Message;

                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// Count Async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> CountAsync([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();

            try
            {
                if (request.Searching is null && request.SortDTO is null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    var searchConditions = request.Searching;

                    string companyCode = null;

                    if (request.PostObject != null)
                    {
                        companyCode = request.PostObject is string ? request.PostObject as string : null;
                    }

                    if (searchConditions != null && searchConditions.Count > 0)
                    {
                        dynamic results = await _baseService.CountAsync(searchConditions, _dataLevel, companyCode);
                        response.Data = results;
                    }

                    response.Code = _resultCode.Ok.Code;
                    response.Message = _resultCode.Ok.Message;

                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }
    }
}