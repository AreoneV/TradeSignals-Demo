namespace AI.Errors;
public class CrossEntropy : IError
{
    /// <summary>
    /// Вычесление ошибки одной пары (правильное предсказание - предсказание сети) нейросети между правильными предсказаниями и предсказаниями нашей сети 
    /// </summary>
    /// <param name="target">Прявильные предсказания на которые надо ровняться</param>
    /// <param name="output">Предсказания нашей нейронной сети</param>
    /// <returns></returns>
    public static double Error(float target, float output)
    {
        //расчет кросэнтропии
        return -1 * (target * Math.Log(output) + (1 - target) * Math.Log(1 - output));
    }

    /// <summary>
    /// Вычесление ошибки нейросети между правильными предсказаниями и предсказаниями нашей сети
    /// </summary>
    /// <param name="targets">Прявильные предсказания на которые надо ровняться</param>
    /// <param name="outputs">Предсказания нашей нейронной сети</param>
    /// <returns>Полную ошибку</returns>
    public float TotalError(float[] targets, float[] outputs)
    {
        double c = 0;
        double n = targets.Length;
        //расчет кросэнтропии
        for(int i = 0; i < n; i++)
        {
            c += targets[i] * Math.Log(outputs[i]) + (1 - targets[i]) * Math.Log(1 - outputs[i]);
        }
        c *= -(1.0 / n);
        return (float)c;
    }
}
