// See https://aka.ms/new-console-template for more information
using HititNDCv201;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Channels;
using System.Xml.Schema;
using System.Xml.Serialization;

Console.WriteLine("Hello, World!");
Uri uri = new Uri("https://book-htt.crane.aero/cranendc/v20.1/CraneNDCService");
var binding = new BasicHttpsBinding();
binding.MaxReceivedMessageSize = 1000000;
CraneNDCServiceClient client = new CraneNDCServiceClient(binding, new EndpointAddress(uri));

using (new OperationContextScope(client.InnerChannel))
{
    HttpRequestMessageProperty prop = new HttpRequestMessageProperty();
    prop.Headers["username"] = "**username**";
    prop.Headers["password"] = "**password**";
    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = prop;

    HititNDCv201.doAirShoppingRequest request = new HititNDCv201.doAirShoppingRequest();
    request.MessageDoc = new HititNDCv201.MessageDocType2();
    request.MessageDoc.Name = "NDC GATEWAY";
    request.MessageDoc.RefVersionNumber = 20.1M;

    request.Party = new PartyType1();
    request.Party.Sender = new SenderType1();
    var travelAgency = new HititNDCv201.TravelAgencyType1();
    travelAgency.AgencyID = "OBILET";
    request.Party.Sender.Item = travelAgency;

    request.Request = new RequestType1();
    request.Request.FlightRequest = new FlightRequestType();
    HititNDCv201.OriginDestCriteriaType odCriteria = new HititNDCv201.OriginDestCriteriaType();
    odCriteria.OriginDepCriteria = new OriginDepCriteriaType();
    odCriteria.OriginDepCriteria.IATA_LocationCode = "SAW";
    odCriteria.OriginDepCriteria.Date = DateTime.ParseExact("2024-01-26", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    odCriteria.DestArrivalCriteria = new DestArrivalCriteriaType();
    odCriteria.DestArrivalCriteria.IATA_LocationCode = "ESB";
    
    odCriteria.PreferredCabinType = new CabinTypeType[1];
    odCriteria.PreferredCabinType[0] = new CabinTypeType();
    odCriteria.PreferredCabinType[0].CabinTypeCode = "Y";

    request.Request.FlightRequest.Items = new object[] { odCriteria };

    request.Request.Paxs = new PaxType[1];
    request.Request.Paxs[0] = new PaxType();
    request.Request.Paxs[0].PaxID = "SH1";
    request.Request.Paxs[0].PTC = "ADT";

    request.Request.ResponseParameters = new ResponseParametersType();
    request.Request.ResponseParameters.CurParameter = new CurParameterType[1];
    request.Request.ResponseParameters.CurParameter[0] = new CurParameterType();

    request.Request.ResponseParameters.CurParameter[0].RequestedCurCode = "TRY";
    request.Request.ResponseParameters.LangUsage = new LangUsageType[1];
    request.Request.ResponseParameters.LangUsage[0] = new LangUsageType();
    request.Request.ResponseParameters.LangUsage[0].LangCode = "EN";


    try
    {
        MethodInfo method = typeof(XmlSerializer).GetMethod("set_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        method.Invoke(null, new object[] { 1 });
        HititNDCv201.doAirShoppingResponse response = client.doAirShopping(request);        
        if (response.Items[0].GetType().Name.Equals("HititNDCv201.ErrorType1"))
        {
            var error = (HititNDCv201.ErrorType1)response.Items[0];
            Console.WriteLine("Got Error: " + error.Code + " - " + error.DescText);
        }
        else
        {
            var res = (HititNDCv201.ResponseType1) response.Items[0];
            var offers = res.OffersGroup.CarrierOffers[0].Offer;
            foreach (var offer in offers)
            {
                var total = offer.TotalPrice.TotalAmount;
                Console.WriteLine("OfferId " + offerId(offer) + ": " + total.Value + " " + total.CurCode );
            }
        }
    } catch (Exception e)
    {
        Console.WriteLine("Exception in service call");
        Console.WriteLine(e);
    }
    Console.WriteLine("Service called!");
}

string offerId(HititNDCv201.OfferType offer)
{
    if (offer.OfferID.Length < 20) 
    {
        return offer.OfferID;
    } else
    {
        return offer.OfferID.Substring(0,20) + "...(truncated)";

    }
}
