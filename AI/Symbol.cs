using System.Collections.ObjectModel;
using AI.Initialization;
using AI.Layers;
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
        if(!Directory.Exists("Weights")) return;

        foreach (string file in Directory.GetFiles("Weights"))
        {
            var arr = file.Split('-');

            if(arr.Length != 3 || arr[0] != Name) continue;

            if(!Enum.TryParse(typeof(TimeFrame), arr[2], out object otf)) continue;

            var tf = (TimeFrame)otf;

            BasicNetwork nn = new BasicNetwork(Service.InputsBars * 4);
            nn.AddLayer(new ReLU(32), new RandomWeights());
            nn.AddLayer(new ReLU(16), new RandomWeights());
            nn.AddLayer(new ReLU(8), new RandomWeights());
            nn.AddLayer(new Sigmoid(1), new RandomWeights());

            nn.LoadWeights(file);

            switch (arr[1])
            {
                case "Buy":
                    buyNetworks.Add(tf, nn);
                    continue;
                case "Sell":
                    sellNetworks.Add(tf, nn);
                    break;
            }
        }
    }
}