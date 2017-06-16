<Query Kind="Program">
  <Connection>
    <ID>c6a60f90-584f-46bd-ab2b-e2fae4717c41</ID>
    <Persist>true</Persist>
    <Server>localhost</Server>
    <Database>Playground</Database>
    <ShowServer>true</ShowServer>
  </Connection>
  <Output>DataGrids</Output>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Framework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Tasks.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Utilities.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.DataAnnotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.Protocols.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.RegularExpressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <GACReference>System.IO.Compression, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</GACReference>
  <GACReference>System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</GACReference>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Configuration</Namespace>
  <Namespace>System.Diagnostics</Namespace>
  <Namespace>System.Diagnostics.Contracts</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.IO.Compression</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Linq.Expressions</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Reflection</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Threading</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
  <Namespace>System.Xml.Linq</Namespace>
  <Namespace>System.Xml.XPath</Namespace>
</Query>

void Main()
{
    var g = new GoogleGeocodingService {Proxy = "wps-proxy.zurich.com"};
    var code = g.GetGeocode("toronto, canada");
    Console.WriteLine(code.ToString());
}


/// <summary>
/// A generic implementation to call Geocoding Services. Currently only supports XML format.
/// </summary>
public class GoogleGeocodingService
{
    private const string ZeroResults = "ZERO_RESULTS";
    private const string OverQueryLimit = "OVER_QUERY_LIMIT";
    private const string RequestDenied = "REQUEST_DENIED";
    private const string RequestBaseUri = "http://maps.googleapis.com/maps/api/geocode/xml";

    private readonly string clientId;
    private readonly string cryptoKey;

    public string Proxy { get; set; }

    public GoogleGeocodingService()
    {

    }

    public GoogleGeocodingService(string clientId, string cryptoKey)
        : this()
    {
        this.clientId = clientId;
        this.cryptoKey = cryptoKey;
    }

    public Geocode GetGeocode(string address)
    {
        if (address == null)
            throw new ArgumentNullException("address");
        return ParseGeocodes(GetRawLocationDataAsync(address).Result);
    }

    /// <summary>
    /// Asynchronous method to get geocodes for a single address.
    /// </summary>
    /// <param name="address">Address for which to get the geocodes.</param>
    /// <returns>Geocodes for the stated address.</returns>
    public async Task<Geocode> GetGeocodeAsync(string address)
    {
        //address.DefaultEncodingToUtf8();
        if (address == null)
            throw new ArgumentNullException("address");
        return ParseGeocodes(await GetRawLocationDataAsync(address));
    }

    /// <summary>
    /// Get the XDocument that is returned by the webservice.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<XDocument> GetRawLocationDataAsync(string address)
    {
        var request = GetGeocodingRequest(address);
        using (var response = await request.GetResponseAsync())
        {
            return XDocument.Load(response.GetResponseStream());
        }
    }

    /// <summary>
    /// Create a valid request URI (incl. signing if clientId and cryptoKey provided in constructor) from a provided address.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public Uri GetRequestUri(string address)
    {
        var uriBuilder = new UriBuilder(RequestBaseUri)
        {
            Query = "address=" + address
        };
        return GetSignedUri(uriBuilder.Uri);
    }

    /// <summary>
    /// Gets the signed URI if clientId and cryptoKey provided in constructor, else returns the provided URI.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    private Uri GetSignedUri(Uri uri)
    {
        if (string.IsNullOrWhiteSpace(cryptoKey)
            || string.IsNullOrWhiteSpace(clientId))
            return uri; // throw new InvalidOperationException("Cannot sign the Url");
        try
        {
            var builder = new UriBuilder(SetClient(uri));
            var signature = GetUriSignature(builder.Uri);
            builder.Query = builder.Query.TrimStart('?') + "&signature=" + signature;
            return builder.Uri;
        }
        catch (Exception)
        {
            return uri;
        }
    }

    /// <summary>
    /// Sets the "client" parameters in the query string of the URI.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    private Uri SetClient(Uri uri)
    {
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (string.IsNullOrWhiteSpace(query["client"]))
        {
            var builder = new UriBuilder(uri);
            builder.Query = builder.Query.TrimStart('?') + "&client=" + clientId;
            return builder.Uri;
        }
        return uri;
    }

    /// <summary>
    /// Returns the Base64 encoded signature for the provided URI (signature depends on cryptoKey and clientId).
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    private string GetUriSignature(Uri uri)
    {
        if (string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(cryptoKey))
        {
            throw new Exception("No ClientId or CryptoKey provided, cannot sign request.");
        }
        var cryptoKeyBytes = Convert.FromBase64String(cryptoKey.Replace("-", "+").Replace("_", "/"));
        byte[] hash;
        using (var algorithm = new HMACSHA1(cryptoKeyBytes))
        {
            hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(uri.PathAndQuery));
        }
        var signature = Convert.ToBase64String(hash);
        signature = signature.Replace("+", "-").Replace("/", "_");
        return signature;
    }

    /// <summary>
    /// Get the HttpWebRequest for a provided address.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public HttpWebRequest GetGeocodingRequest(string address)
    {
        var requestUri = GetRequestUri(address);
        return (HttpWebRequest)WebRequest.Create(requestUri);
    }

    /// <summary>
    /// Parses the response XDocument into a Geocode object.
    /// </summary>
    /// <param name="xdoc"></param>
    /// <returns></returns>
    private static Geocode ParseGeocodes(XNode xdoc)
    {
        var rootElement = xdoc.XPathSelectElement("GeocodeResponse");
        var status = rootElement.XPathSelectElement("status").Value;
        if (status.Equals(ZeroResults))
        {
            //throw new NotFoundGeocodingException("No geocodes found");
        }
        if (status.Equals(OverQueryLimit))
        {
            //throw new OverQueryLimitGeocodingException("Over query limit: " + rootElement.XPathSelectElement("error_message").Value);
        }
        if (status.Equals(RequestDenied))
        {
            //throw new RequestDeniedGeocodingException("Request denied: " + rootElement.XPathSelectElement("error_message").Value);
        }

        var result = xdoc.XPathSelectElement("GeocodeResponse/result");
        var streetNumber = SafeGetValue(result, "address_component/type[text()='street_number']/../long_name");
        var streetName = SafeGetValue(result, "address_component/type[text()='route']/../long_name");
        var city = SafeGetValue(result, "address_component/type[text()='locality']/../long_name");
        var county = SafeGetValue(result, "address_component/type[text()='administrative_area_level_2']/../long_name");
        var stateName = SafeGetValue(result, "address_component/type[text()='administrative_area_level_1']/../long_name");
        var stateCode = SafeGetValue(result, "address_component/type[text()='administrative_area_level_1']/../short_name");
        var country = SafeGetValue(result, "address_component/type[text()='country']/../long_name");
        var isoCountryCode = SafeGetValue(result, "address_component/type[text()='country']/../short_name");
        var postalCode = SafeGetValue(result, "address_component/type[text()='postal_code']/../long_name");
        var formattedAddress = SafeGetValue(result, "formatted_address");
        var placeId = SafeGetValue(result, "place_id");

        var geometry = result.XPathSelectElement("geometry");
        var longitude = double.Parse(SafeGetValue(geometry, "location/lng"));
        var latitude = double.Parse(SafeGetValue(geometry, "location/lat"));
        var locationType = SafeGetValue(geometry, "location_type");

        return new Geocode(longitude, latitude, locationType, formattedAddress, placeId, city, county, stateName, stateCode, country, isoCountryCode, postalCode, streetName, streetNumber);
    }

    private static string SafeGetValue(XNode element, string xpath)
    {
        var node = element.XPathSelectElement(xpath);
        return node != null ? node.Value : "";
    }
}

public enum GoogleGeocodeMatchLevel
{
    Approximate,
    GeometricCenter,
    RangeInterpolated,
    Rooftop
}

[Serializable]
public class Geocode : Coordinate
{
    /// <summary>
    /// The match level of the Geocode (see google geocode reference). Also known as Match level.
    /// From the Google Geocoding API at https://developers.google.com/maps/documentation/geocoding/intro:
    /// - "ROOFTOP" indicates that the returned result is a precise geocode for which we have location information accurate down to street address precision.
    /// - "RANGE_INTERPOLATED" indicates that the returned result reflects an approximation (usually on a road) interpolated between two precise points (such as intersections). Interpolated results are generally returned when rooftop geocodes are unavailable for a street address.
    /// - "GEOMETRIC_CENTER" indicates that the returned result is the geometric center of a result such as a polyline (for example, a street) or polygon (region).
    /// - "APPROXIMATE" indicates that the returned result is approximate.
    /// </summary>
    public string MatchLevel { get; set; }

    /// <summary>
    /// The formatted address the geocode was requested for.
    /// </summary>
    public string Address { get; set; }

    public string PlaceId { get; set; }

    public string City { get; set; }

    public string County { get; set; }

    public string StateName { get; set; }

    public string StateCode { get; set; }

    public string Country { get; set; }

    public string IsoCountryCode { get; set; }

    public string PostalCode { get; set; }

    public string StreetNumber { get; set; }

    public string StreetName { get; set; }

    public Geocode()
    {

    }

    public Geocode(double latitude, double longitude,
        string matchLevel, string address, string placeId, string city, string county, string stateName, string stateCode, string country,
        string isoCountryCode, string postalCode, string streetName, string streetNumber)
        : base(latitude, longitude)
    {
        MatchLevel = matchLevel;
        Address = address;
        PlaceId = placeId;
        City = city;
        County = county;
        StateName = stateName;
        StateCode = stateCode;
        Country = country;
        IsoCountryCode = isoCountryCode;
        PostalCode = postalCode;
        StreetName = streetName;
        StreetNumber = streetNumber;
    }

    public override string ToString()
    {
        return Address;
    }
}