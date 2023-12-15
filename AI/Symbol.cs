using System.Collections.ObjectModel;
using MarketInfo;

namespace AI;

public class Symbol
{
    private readonly Dictionary<TimeFrame, BasicNetwork> buyNetworks = [];
    private readonly Dictionary<TimeFrame, BasicNetwork> sellNetworks = [];

    public Symbol(string name)
    {
        Name = name;
        Load();
        BuyNetworks = buyNetworks.AsReadOnly();
        SellNetworks = sellNetworks.AsReadOnly();
    }


    public string Name { get; }
    public ReadOnlyDictionary<TimeFrame, BasicNetwork> BuyNetworks { get; }
    public ReadOnlyDictionary<TimeFrame, BasicNetwork> SellNetworks { get; }



    private void Load()
    {

    }
}