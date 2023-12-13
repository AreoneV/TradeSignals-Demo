using AI.Initialization;
using AI.Layers;

namespace AI;
public class BasicNetwork(int inputs)
{
    internal readonly int inputs = inputs;
    internal List<Layer> layers = [];

    public void AddLayer(Layer layer, IInitialization initialization)
    {
        layer.InitializeWeights(layers.Count == 0 ? inputs : layers.Last().Size, initialization);
        layers.Add(layer);
    }



    public void SaveWeights(string file)
    {
        var fs = new FileStream(file, FileMode.Create, FileAccess.Write);
        BinaryWriter w = new(fs);

        foreach(var layer in layers)
        {
            layer.SaveWeights(w);
        }
        w.Close();
    }
    public void LoadWeights(string file)
    {
        var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        BinaryReader w = new(fs);

        foreach(var layer in layers)
        {
            layer.LoadWeights(w);
        }
        w.Close();
    }


    public float[] Predict(float[] inputs)
    {
        float[] result = inputs;
        for(int i = 0; i < layers.Count; i++)
        {
            result = layers[i].FeedForward(result);
        }
        return result;
    }


}
