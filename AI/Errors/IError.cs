namespace AI.Errors;
public interface IError
{
    /// <summary>
    /// Вычесление ошибки нейросети между правильными предсказаниями и предсказаниями нашей сети
    /// </summary>
    /// <param name="targets">Прявильные предсказания на которые надл ровняться</param>
    /// <param name="outputs">Предсказания нашей нейронной сети</param>
    /// <returns>Полную ошибку</returns>
    float TotalError(float[] targets, float[] outputs);
}
