using Bfs.Iop.Admin.Testautomation.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Testautomation.Shared;

internal sealed class DeleteTests
{
    private const string _bearerPrefix = "Bearer";
    private readonly string _datasetsAddress = "api/Datasets/";
    private readonly string _search = "api/Search?query=";
    private readonly string _dataServiceAddress = "api/DataServices/";
    private readonly string _publicServiceAddress = "api/PublicServices/";
    private readonly string _deleteDcatCatalogRecordsAddress = "api/DcatCatalogs/{id}/records/{recordId}";
    private readonly string _searchCatalogRecord = "api/DcatCatalogs/records/from-resource/{}";
    private readonly string _bearerToken;
    private readonly string _searchQuery;
    private readonly string _baseDcatUrl;

    public DeleteTests(string baseDcatUrl, string bearerToken, string searchQuery)
    {
        _baseDcatUrl = baseDcatUrl;
        _bearerToken = bearerToken;
        _searchQuery = searchQuery;
    }

    public async Task<CleanupResult> CleanupDatasetAndDistributionAndDcatCatalogRecord()
    {
        bool deleteCatalogRecordOk = false;
        DcatCatalogRecordModel? catalogRecord = null;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_bearerPrefix, _bearerToken);

            var result = await DeleteDataset(httpClient);

            if (!string.IsNullOrEmpty(result.Id))
            {
                catalogRecord = await ReadCatalogRecord(httpClient, result.Id);
            }

            if (catalogRecord is not null)
            {
                var uriBuilder = new UriBuilder(_baseDcatUrl)
                {
                    Path = _deleteDcatCatalogRecordsAddress.Replace("{id}", catalogRecord.DcatCatalogId.ToString()).Replace("{recordId}", catalogRecord.Id.ToString())
                };
                var url = uriBuilder.Uri;

                var responseRecords = await httpClient.DeleteAsync(url);
                if (responseRecords != null)
                {
                    deleteCatalogRecordOk = responseRecords.StatusCode == System.Net.HttpStatusCode.NoContent;
                    TestContext.Out.WriteLine($"catalogRecord '{catalogRecord.Id}' is removed from DcatCatalog '{catalogRecord.DcatCatalogId}'");
                }
            }

            return new CleanupResult
            {
                Success = result.Success && deleteCatalogRecordOk,
                Message = result.Success && deleteCatalogRecordOk ? "Deletion successful" : "Something was not deleted"
            };
        }
        catch (Exception ex)
        {
            return new CleanupResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<CleanupResult> CleanupDataset()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_bearerPrefix, _bearerToken);

            var result = await DeleteDataset(httpClient);
            return new CleanupResult
            {
                Success = result.Success,
                Message = result.Message
            };
        }
        catch (Exception ex)
        {
            return new CleanupResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<CleanupResult> CleanupChannels()
    {
        try
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_bearerPrefix, _bearerToken);

            var result = await DeleteDataset(httpClient);
            return new CleanupResult
            {
                Success = result.Success,
                Message = result.Message
            };
        }
        catch (Exception ex)
        {
            return new CleanupResult { Success = false, Message = ex.Message };
        }
    }


    #region Delete

    private async Task<DeleteResult> DeleteDataset(HttpClient httpClient)
    {
        var result = new DeleteResult();

        result.Id = await ReadCatalogModelId(httpClient, _searchQuery, SearchResourceType.Dataset);

        if (!string.IsNullOrEmpty(result.Id))
        {
            var responseDataset = await httpClient.DeleteAsync(_baseDcatUrl + _datasetsAddress + result.Id);
            result.Ok = responseDataset.StatusCode == System.Net.HttpStatusCode.NoContent;
            Console.WriteLine($"dataset {result.Id} delete status: {responseDataset.StatusCode}");
        }
        else
        {
            result.Success = false;
            result.Message = $"Cannot find dataset: {_searchQuery}";
            return result;
        }

        result.Success = result.Ok;
        result.Message = result.Ok ? "Deletion successful" : "Dataset was not deleted";
        return result;
    }

    public async Task DeleteDataService()
    {
        string dataServiceId = string.Empty;
        bool dataServiceOk = false;

        try
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_bearerPrefix, _bearerToken);

            dataServiceId = await ReadCatalogModelId(httpClient, _searchQuery, SearchResourceType.DataService);

            if (!string.IsNullOrEmpty(dataServiceId))
            {
                var responseDataService = await httpClient.DeleteAsync(_baseDcatUrl + _dataServiceAddress + dataServiceId);
                if (responseDataService != null)
                {
                    dataServiceOk = responseDataService.StatusCode == System.Net.HttpStatusCode.NoContent;
                    TestContext.Out.WriteLine($"dataservice {dataServiceId} is removed");
                }
            }

            Assert.That(dataServiceOk, Is.True, "Something was not deleted, please check the log for details");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Error when deleting a data service: {ex.Message}");
        }
    }

    public async Task DeletePublicService()
    {
        try
        {
            bool publicServiceOk = false;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_bearerPrefix, _bearerToken);

            var publicServiceId = await ReadCatalogModelId(httpClient, _searchQuery, SearchResourceType.PublicService);

            if (!string.IsNullOrEmpty(publicServiceId))
            {
                var responsePublicService = await httpClient.DeleteAsync(_baseDcatUrl + _publicServiceAddress + publicServiceId);
                if (responsePublicService != null)
                {
                    publicServiceOk = responsePublicService.StatusCode == System.Net.HttpStatusCode.NoContent;
                    TestContext.Out.WriteLine($"public service {publicServiceId} is removed");
                }
            }

            Assert.That(publicServiceOk, Is.True, "Something was not deleted, please check the log for details");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Error when deleting public service: {ex.Message}");
        }
    }

    #endregion  Delete

    #region Read

    private async Task<string> ReadCatalogModelId(HttpClient httpClient, string searchQuery, SearchResourceType expectedObjectType)
    {
        var s = _baseDcatUrl + _search + searchQuery;
        var responseCatalogModel = await httpClient.GetAsync(s);

        if (responseCatalogModel.IsSuccessStatusCode)
        {
            var content = await responseCatalogModel.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            options.Converters.Add(new JsonStringEnumConverter());

            var models = JsonSerializer.Deserialize<SearchResultModel[]>(content, options);

            if (models is not null && models.Length > 0 && models.First().Type == expectedObjectType)
            {
                return models.First().Id.ToString();
            }
            else
            {
                TestContext.Out.WriteLine($"{expectedObjectType} {searchQuery} could not be found.");
            }
        }
        else
        {
            Assert.Fail($"Error when retrieving the data record:  {responseCatalogModel.ReasonPhrase}");
        }

        return string.Empty;
    }

    private async Task<DcatCatalogRecordModel?> ReadCatalogRecord(HttpClient httpClient, string datasetId)
    {
        var searchCatalogRecord = _searchCatalogRecord.Replace("{}", datasetId);
        var responseCatalogRecord = await httpClient.GetAsync(_baseDcatUrl + searchCatalogRecord);

        if (responseCatalogRecord.IsSuccessStatusCode)
        {
            var content = await responseCatalogRecord.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            options.Converters.Add(new JsonStringEnumConverter());

            var recordModels = JsonSerializer.Deserialize<List<DcatCatalogRecordModel>>(content, options);

            if (recordModels != null && recordModels.Count != 0)
            {
                return recordModels.First();
            }
            else
            {
                TestContext.Out.WriteLine($"Catalog record for the dataset '{datasetId}' could not be deserialized.");
            }
        }
        else
        {
            Assert.Fail($"Error when retrieving the data record:  {responseCatalogRecord.ReasonPhrase}");
        }

        return null;
    }
    #endregion Read
}