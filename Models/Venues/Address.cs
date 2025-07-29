using System.Text.Json.Serialization;

namespace Sonic.Models;

public class Address
{
    [JsonIgnore]
    public string Name => ToString();
    public required string StreetNumber { get; set; }
    public string? Route { get; set; }
    public string? Locality { get; set; }
    public string? County { get; set; }
    public string ? Township { get; set; }
    public State State { get => StateEnumExtensions.FromValue(StateCode); }
    public StateCode StateCode { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? MapsLink { get; set; }

    public override string ToString()
    {
        return $"{StreetNumber} {Route}, {Locality}, {StateCode}, {State} {ZipCode}, {Country}";
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum State
{
    Alabama, Alaska, Arizona, Arkansas, California,
    Colorado, Connecticut, Delaware, Florida, Georgia,
    Hawaii, Idaho, Illinois, Indiana, Iowa,
    Kansas, Kentucky, Louisiana, Maine, Maryland,
    Massachusetts, Michigan, Minnesota, Mississippi, Missouri,
    Montana, Nebraska, Nevada, NewHampshire, NewJersey,
    NewMexico, NewYork, NorthCarolina, NorthDakota, Ohio,
    Oklahoma, Oregon, Pennsylvania, RhodeIsland, SouthCarolina,
    SouthDakota, Tennessee, Texas, Utah, Vermont,
    Virginia, Washington, WestVirginia, Wisconsin, Wyoming
}

public class StateEnumExtensions
{
    public static State FromValue(StateCode value)
    {
        return value switch
        {
            StateCode.AL => State.Alabama,StateCode.AK => State.Alaska,StateCode.AZ => State.Arizona,StateCode.AR => State.Arkansas,StateCode.CA => State.California,
            StateCode.CO => State.Colorado,StateCode.CT => State.Connecticut,StateCode.DE => State.Delaware,StateCode.FL => State.Florida,StateCode.GA => State.Georgia,
            StateCode.HI => State.Hawaii,StateCode.ID => State.Idaho,StateCode.IL => State.Illinois,StateCode.IN => State.Indiana,StateCode.IA => State.Iowa,
            StateCode.KS => State.Kansas,StateCode.KY => State.Kentucky,StateCode.LA => State.Louisiana,StateCode.ME => State.Maine,StateCode.MD => State.Maryland,
            StateCode.MA => State.Massachusetts,StateCode.MI => State.Michigan,StateCode.MN => State.Minnesota,StateCode.MS => State.Mississippi,StateCode.MO => State.Missouri,
            StateCode.MT => State.Montana,StateCode.NE => State.Nebraska,StateCode.NV => State.Nevada,StateCode.NH => State.NewHampshire,StateCode.NJ => State.NewJersey,
            StateCode.NM => State.NewMexico,StateCode.NY => State.NewYork,StateCode.NC => State.NorthCarolina,StateCode.ND => State.NorthDakota,StateCode.OH => State.Ohio,
            StateCode.OK => State.Oklahoma,StateCode.OR => State.Oregon,StateCode.PA => State.Pennsylvania,StateCode.RI => State.RhodeIsland,StateCode.SC => State.SouthCarolina,
            StateCode.SD => State.SouthDakota,StateCode.TN => State.Tennessee,StateCode.TX => State.Texas,StateCode.UT => State.Utah,StateCode.VT => State.Vermont,
            StateCode.VA => State.Virginia,StateCode.WA => State.Washington,StateCode.WV => State.WestVirginia,StateCode.WI => State.Wisconsin,StateCode.WY => State.Wyoming,
            _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown state code: {value}")
        };
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StateCode
{
    AL, AK, AZ, AR, CA,
    CO, CT, DE, FL, GA,
    HI, ID, IL, IN, IA,
    KS, KY, LA, ME, MD,
    MA, MI, MN, MS, MO,
    MT, NE, NV, NH, NJ,
    NM, NY, NC, ND, OH,
    OK, OR, PA, RI, SC,
    SD, TN, TX, UT, VT,
    VA, WA, WV, WI, WY
}
