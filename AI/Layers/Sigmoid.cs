namespace AI.Layers;
public class Sigmoid(int size) : Layer(size)
{
    internal override float Activation(float x)
    {
        return 1 / (1 + (float)Math.Exp(-x));
    }
    internal override float ActivationDerivative(float x)
    {
        var z = Activation(x);
        return z * (1 - z);
    }
}
