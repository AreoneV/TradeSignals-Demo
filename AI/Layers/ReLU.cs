namespace AI.Layers;
public class ReLU(int size) : Layer(size)
{
    internal override float Activation(float x)
    {
        return Math.Max(0.0f, x);
    }

    internal override float ActivationDerivative(float x)
    {
        return x > 0 ? 1 : 0;
    }
}
