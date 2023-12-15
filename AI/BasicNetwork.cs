using AI.Initialization;
using AI.Layers;

namespace AI;
public class BasicNetwork(int inputs)
{
    internal readonly int Inputs = inputs;
    internal List<Layer> Layers = [];

    public void AddLayer(Layer layer, IInitialization initialization)
    {
        layer.InitializeWeights(Layers.Count == 0 ? Inputs : Layers.Last().Size, initialization);
        Layers.Add(layer);
    }



    public void SaveWeights(string file)
    {
        var fs = new FileStream(file, FileMode.Create, FileAccess.Write);
        BinaryWriter w = new(fs);

        foreach(var layer in Layers)
        {
            layer.SaveWeights(w);
        }
        w.Close();
    }
    public void LoadWeights(string file)
    {
        var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        BinaryReader w = new(fs);

        foreach(var layer in Layers)
        {
            layer.LoadWeights(w);
        }
        w.Close();
    }


    public float[] Predict(float[] inputData)
    {
        return Layers.Aggregate(inputData, (current, t) => t.FeedForward(current));
    }


}
